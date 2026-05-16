using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class InventoryService
{
    private readonly PhieuNhapRepositoryDAL _phieuNhap;
    private readonly DanhMucDqgRepositoryDAL _dqg;
    private readonly NhomThuocRepositoryDAL _nhomThuoc;

    public InventoryService(DbContextDAL db)
    {
        _phieuNhap = new PhieuNhapRepositoryDAL(db);
        _dqg = new DanhMucDqgRepositoryDAL(db);
        _nhomThuoc = new NhomThuocRepositoryDAL(db);
    }

    private static void RequireKhoRole() =>
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);

    private static void EnsurePhieuChuaNhapKho(string? trangThai)
    {
        if (string.Equals(trangThai, PhieuNhapTrangThai.DaNhapKho, StringComparison.Ordinal))
            throw new InvalidOperationException("Phiếu đã nhập kho — không thể chỉnh sửa.");
    }

    public IReadOnlyList<IdTenDTO> LayKho() { RequireKhoRole(); return _phieuNhap.LayKhoHoatDong(); }
    public IReadOnlyList<IdTenDTO> LayNhaCungCap() { RequireKhoRole(); return _phieuNhap.LayNhaCungCapHoatDong(); }
    public IReadOnlyList<IdTenDTO> LayThuKho() { RequireKhoRole(); return _phieuNhap.LayThuKhoHoatDong(); }
    public IReadOnlyList<NhomThuocDTO> LayNhomThuoc() { RequireKhoRole(); return _nhomThuoc.LayTatCa(); }

    public PhieuNhapDTO? LayPhieuHienTai()
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            return null;
        return _phieuNhap.LayPhieu(PhieuNhapSession.MaPhieuNhap.Value);
    }

    public PhieuNhapDTO? LayPhieu(int maPhieuNhap)
    {
        RequireKhoRole();
        return _phieuNhap.LayPhieu(maPhieuNhap);
    }

    /// <summary>Lưu thông tin phiếu (bước 1) — chưa cộng tồn.</summary>
    public int LuuThongTinPhieu(PhieuNhapDTO dto, bool quanLyCongNo, bool chuyenBuoc2 = false)
    {
        RequireKhoRole();
        if (!UserSession.MaNhanVien.HasValue)
            throw new InvalidOperationException("Thiếu mã nhân viên phiên làm việc.");

        if (Validator.IsNullOrWhiteSpace(dto.SoHoaDon))
            throw new ArgumentException("Vui lòng nhập số hóa đơn.");
        if (!dto.MaKho.HasValue)
            throw new ArgumentException("Vui lòng chọn kho nhập.");
        if (!dto.MaNhaCungCap.HasValue)
            throw new ArgumentException("Vui lòng chọn nhà cung cấp.");

        dto.MaNhanVien = dto.MaNhanVien > 0 ? dto.MaNhanVien : UserSession.MaNhanVien.Value;
        dto.TrangThai = chuyenBuoc2 ? PhieuNhapTrangThai.Luu : PhieuNhapTrangThai.DangLap;
        if (!quanLyCongNo)
            dto.CongNo = 0;

        int ma;
        if (dto.MaPhieuNhap > 0)
        {
            var cur = _phieuNhap.LayPhieu(dto.MaPhieuNhap)
                      ?? throw new InvalidOperationException("Không tìm thấy phiếu nhập.");
            EnsurePhieuChuaNhapKho(cur.TrangThai);
            _phieuNhap.CapNhatPhieu(dto);
            ma = dto.MaPhieuNhap;
        }
        else
        {
            ma = _phieuNhap.TaoPhieu(dto);
        }

        PhieuNhapSession.SetPhieu(ma);
        return ma;
    }

    public IReadOnlyList<DanhSachHangNhapKhoViewDTO> LayChiTietPhieuHienTai()
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            return Array.Empty<DanhSachHangNhapKhoViewDTO>();
        return _phieuNhap.LayChiTietPhieu(PhieuNhapSession.MaPhieuNhap.Value);
    }

    public IReadOnlyList<TraCuuDqgViewDTO> TraCuuDqg(string? tuKhoa) =>
        _dqg.TraCuu(tuKhoa);

    public TraCuuDqgViewDTO? LayDqg(int maDqg)
    {
        RequireKhoRole();
        return _dqg.LayTheoMa(maDqg);
    }

    /// <summary>Thêm dòng chi tiết từ DQG hoặc thuốc đã có — chưa cộng tồn (Workflow §4–5).</summary>
    public void ThemChiTietTuDqg(
        int maDqg,
        int maNhomThuoc,
        bool choPhepLienThong,
        int soLuongNhap,
        string donViNhap,
        string soLo,
        DateTime hanSuDung,
        decimal? vatDong,
        decimal donGiaNhap,
        decimal giaBan,
        string? viTri,
        string? ghiChu,
        bool khongQuanLyLoHan)
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            throw new InvalidOperationException("Chưa có phiếu nhập — hãy lưu thông tin phiếu trước.");

        var maPhieu = PhieuNhapSession.MaPhieuNhap.Value;
        var trangThai = _phieuNhap.LayTrangThai(maPhieu);
        EnsurePhieuChuaNhapKho(trangThai);

        if (!Validator.IsPositiveInt(soLuongNhap))
            throw new ArgumentException("Số lượng nhập phải > 0.");
        if (!Validator.IsNonNegativeDecimal(donGiaNhap))
            throw new ArgumentException("Giá nhập không hợp lệ.");
        if (!khongQuanLyLoHan && Validator.IsNullOrWhiteSpace(soLo))
            throw new ArgumentException("Vui lòng nhập số lô.");

        var maThuoc = _phieuNhap.LayMaThuocTheoMaDqg(maDqg);
        if (!maThuoc.HasValue)
        {
            maThuoc = _phieuNhap.TaoThuocTuDqg(maDqg, maNhomThuoc, choPhepLienThong, donGiaNhap, giaBan);
        }

        var ct = new ChiTietPhieuNhapDTO
        {
            MaPhieuNhap = maPhieu,
            MaThuoc = maThuoc.Value,
            SoLuongNhap = soLuongNhap,
            DonGiaNhap = donGiaNhap,
            GiaBan = giaBan,
            SoLo = khongQuanLyLoHan ? null : soLo.Trim(),
            HanSuDung = khongQuanLyLoHan ? null : hanSuDung.Date,
            ViTri = viTri,
            GhiChu = ghiChu,
            VAT = vatDong
        };
        _phieuNhap.ThemChiTiet(ct);
    }

    public void XoaChiTiet(int maCtpn)
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            return;
        EnsurePhieuChuaNhapKho(_phieuNhap.LayTrangThai(PhieuNhapSession.MaPhieuNhap.Value));
        _phieuNhap.XoaChiTiet(maCtpn);
    }

    /// <summary>Lưu phiếu ở trạng thái Lưu — vẫn chưa cộng tồn.</summary>
    public void LuuPhieuTam()
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            throw new InvalidOperationException("Chưa có phiếu nhập.");
        var ma = PhieuNhapSession.MaPhieuNhap.Value;
        EnsurePhieuChuaNhapKho(_phieuNhap.LayTrangThai(ma));
        _phieuNhap.DatTrangThai(ma, PhieuNhapTrangThai.Luu);
    }

    /// <summary>Xác nhận nhập kho — trigger CSDL cộng tồn (Workflow §5 bước 4).</summary>
    public void HoanTatNhapKho()
    {
        RequireKhoRole();
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
            throw new InvalidOperationException("Chưa có phiếu nhập.");

        var ma = PhieuNhapSession.MaPhieuNhap.Value;
        var trangThai = _phieuNhap.LayTrangThai(ma);
        EnsurePhieuChuaNhapKho(trangThai);

        var chiTiet = _phieuNhap.LayChiTietPhieu(ma);
        if (chiTiet.Count == 0)
            throw new InvalidOperationException("Phiếu chưa có dòng hàng — không thể nhập kho.");

        _phieuNhap.DatTrangThai(ma, PhieuNhapTrangThai.DaNhapKho);
        PhieuNhapSession.Clear();
    }

    public void HuyPhieuHienTai()
    {
        RequireKhoRole();
        PhieuNhapSession.Clear();
    }

    /// <summary>Nhập kho nhanh một dòng qua sp_NhapKho (tương thích cũ).</summary>
    public int NhapKho(
        int maThuoc,
        int soLuongNhap,
        decimal donGiaNhap,
        DateTime? hanSuDung,
        string? nhaCungCap,
        string? ghiChu,
        int? maKho = null,
        string? soLo = null,
        decimal? giaBanDong = null,
        string? viTri = null)
    {
        RequireKhoRole();
        if (!UserSession.MaNhanVien.HasValue)
            throw new InvalidOperationException("Thiếu mã nhân viên phiên làm việc.");
        if (!Validator.IsPositiveInt(soLuongNhap))
            throw new ArgumentException("Số lượng nhập phải > 0.");
        if (!Validator.IsNonNegativeDecimal(donGiaNhap))
            throw new ArgumentException("Đơn giá nhập không hợp lệ.");

        return _phieuNhap.NhapKho(
            UserSession.MaNhanVien.Value,
            maThuoc,
            soLuongNhap,
            donGiaNhap,
            hanSuDung,
            nhaCungCap,
            ghiChu,
            maKho,
            soLo,
            giaBanDong,
            viTri);
    }
}
