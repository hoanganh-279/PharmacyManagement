using Pharmacy.Common;
using Pharmacy.DAL;

namespace Pharmacy.BLL;

public class InventoryService
{
    private readonly PhieuNhapRepositoryDAL _phieuNhap;

    public InventoryService(DbContextDAL db) =>
        _phieuNhap = new PhieuNhapRepositoryDAL(db);

    /// <summary>Nhập kho qua sp_NhapKho.</summary>
    public int NhapKho(
        int maThuoc,
        int soLuongNhap,
        decimal donGiaNhap,
        DateTime? hanSuDung,
        string? nhaCungCap,
        string? ghiChu)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
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
            ghiChu);
    }
}
