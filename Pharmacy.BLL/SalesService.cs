using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;
using Pharmacy.DTO.Views;
using static Pharmacy.DTO.Views.ThuocKeDonViewDTO;
namespace Pharmacy.BLL;

public class SalesService
{
    private readonly HoaDonRepositoryDAL _hoaDon;
    private readonly ThuocRepositoryDAL _thuoc;

    public SalesService(DbContextDAL db)
    {
        _hoaDon = new HoaDonRepositoryDAL(db);
        _thuoc = new ThuocRepositoryDAL(db);
    }

    public IReadOnlyList<ThuocKeDonViewDTO> TimKiemThuocBan(string? tuKhoa)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        return _thuoc.TimKiemThuocKeDon(tuKhoa);
    }

    /// <summary>Xác nhận đơn hàng từ giỏ — tạo HoaDon + ChiTietHoaDon, trừ tồn theo FEFO (trigger DB).</summary>
    public int XacNhanDonHang(
        string? cccd,
        decimal giamGia,
        string? hinhThucThanhToan,
         List<DonHangGioHangDTO> gioHang)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        if (!UserSession.MaNhanVien.HasValue)
            throw new InvalidOperationException("Thiếu mã nhân viên phiên làm việc.");
        if (gioHang is null || gioHang.Count == 0)
            throw new ArgumentException("Giỏ hàng trống, không thể xác nhận.");
        if (!Validator.IsNonNegativeDecimal(giamGia))
            throw new ArgumentException("Giảm giá không hợp lệ.");

        string? cccdParam = null;
        if (!string.IsNullOrWhiteSpace(cccd))
        {
            if (!Validator.TryNormalizeCccd(cccd, out var normalized))
                throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
            cccdParam = normalized;
        }

        var tongHang = 0m;
        foreach (var line in gioHang)
        {
            if (!Validator.IsPositiveInt(line.SoLuong))
                throw new ArgumentException($"Số lượng thuốc «{line.TenThuoc}» phải > 0.");
            if (line.SoLuong > line.TonToiDa)
                throw new ArgumentException($"«{line.TenThuoc}» vượt tồn kho ({line.TonToiDa}).");
            if (line.DonGia < 0)
                throw new ArgumentException($"Đơn giá «{line.TenThuoc}» không hợp lệ.");
            tongHang += line.ThanhTien;
        }

        if (giamGia > tongHang)
            throw new ArgumentException("Giảm giá không được lớn hơn tổng tiền hàng.");

        var ht = string.IsNullOrWhiteSpace(hinhThucThanhToan) ? "Tiền mặt" : hinhThucThanhToan.Trim();
        return _hoaDon.TaoDonHang(
            UserSession.MaNhanVien.Value,
            cccdParam,
            giamGia,
            ht,
            gioHang);
    }

    /// <summary>Bán một dòng thuốc / một hóa đơn (theo sp_BanThuoc trong SQL).</summary>
    public int BanThuoc(
        int maThuoc,
        int soLuongBan,
        string? cccd = null,
        decimal giamGia = 0,
        string? hinhThucThanhToan = null)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        if (!UserSession.MaNhanVien.HasValue)
            throw new InvalidOperationException("Thiếu mã nhân viên phiên làm việc.");
        if (!Validator.IsPositiveInt(soLuongBan))
            throw new ArgumentException("Số lượng bán phải > 0.");
        if (!Validator.IsNonNegativeDecimal(giamGia))
            throw new ArgumentException("Giảm giá không hợp lệ.");

        string? cccdParam = null;
        if (!string.IsNullOrWhiteSpace(cccd))
        {
            if (!Validator.TryNormalizeCccd(cccd, out var normalized))
                throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
            cccdParam = normalized;
        }

        return _hoaDon.BanThuoc(
            UserSession.MaNhanVien.Value,
            maThuoc,
            soLuongBan,
            cccdParam,
            giamGia,
            string.IsNullOrWhiteSpace(hinhThucThanhToan) ? "Tiền mặt" : hinhThucThanhToan);
    }
}
