using Pharmacy.Common;
using Pharmacy.DAL;

namespace Pharmacy.BLL;

public class SalesService
{
    private readonly HoaDonRepositoryDAL _hoaDon;

    public SalesService(DbContextDAL db) =>
        _hoaDon = new HoaDonRepositoryDAL(db);

    /// <summary>Bán một dòng thuốc / một hóa đơn (theo sp_BanThuoc trong SQL).</summary>
    public int BanThuoc(
        int maThuoc,
        int soLuongBan,
        string? tenKhachHang,
        string? soDienThoai,
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
        if (!Validator.IsPhoneOptional(soDienThoai))
            throw new ArgumentException("Số điện thoại không hợp lệ.");

        return _hoaDon.BanThuoc(
            UserSession.MaNhanVien.Value,
            maThuoc,
            soLuongBan,
            tenKhachHang,
            soDienThoai,
            giamGia,
            string.IsNullOrWhiteSpace(hinhThucThanhToan) ? "Tiền mặt" : hinhThucThanhToan);
    }
}
