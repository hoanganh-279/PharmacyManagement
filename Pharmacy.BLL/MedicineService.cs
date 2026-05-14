using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class MedicineService
{
    private readonly ThuocRepositoryDAL _thuoc;
    private readonly NhomThuocRepositoryDAL _nhom;

    public MedicineService(DbContextDAL db)
    {
        _thuoc = new ThuocRepositoryDAL(db);
        _nhom = new NhomThuocRepositoryDAL(db);
    }

    public IReadOnlyList<DanhSachThuocViewDTO> LayDanhSachThuoc()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _thuoc.LayTuViewDanhSach();
    }

    public ThuocDTO? LayChiTietThuoc(int maThuoc)
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _thuoc.LayTheoMa(maThuoc);
    }

    public int ThemThuoc(ThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        if (Validator.IsNullOrWhiteSpace(dto.TenThuoc) || Validator.IsNullOrWhiteSpace(dto.DonViTinh))
            throw new ArgumentException("Tên thuốc và đơn vị tính là bắt buộc.");
        if (!Validator.IsNonNegativeDecimal(dto.GiaNhap) || !Validator.IsNonNegativeDecimal(dto.GiaBan))
            throw new ArgumentException("Giá không hợp lệ.");
        if (dto.TonToiThieu < 0 || dto.SoLuongTon < 0)
            throw new ArgumentException("Tồn kho không hợp lệ.");

        return _thuoc.Them(dto);
    }

    public void CapNhatThuoc(ThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        if (dto.MaThuoc <= 0)
            throw new ArgumentException("Mã thuốc không hợp lệ.");
        if (Validator.IsNullOrWhiteSpace(dto.TenThuoc) || Validator.IsNullOrWhiteSpace(dto.DonViTinh))
            throw new ArgumentException("Tên thuốc và đơn vị tính là bắt buộc.");
        if (!Validator.IsNonNegativeDecimal(dto.GiaNhap) || !Validator.IsNonNegativeDecimal(dto.GiaBan))
            throw new ArgumentException("Giá không hợp lệ.");

        _thuoc.CapNhat(dto);
    }

    public void NgungKinhDoanh(int maThuoc)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        _thuoc.DatTrangThai(maThuoc, trangThai: false);
    }

    public IReadOnlyList<NhomThuocDTO> LayNhomThuoc()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _nhom.LayTatCa();
    }

    public int ThemNhom(NhomThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        if (Validator.IsNullOrWhiteSpace(dto.TenNhomThuoc))
            throw new ArgumentException("Tên nhóm thuốc là bắt buộc.");
        return _nhom.Them(dto);
    }

    public void CapNhatNhom(NhomThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        if (dto.MaNhomThuoc <= 0 || Validator.IsNullOrWhiteSpace(dto.TenNhomThuoc))
            throw new ArgumentException("Nhóm thuốc không hợp lệ.");
        _nhom.CapNhat(dto);
    }
}
