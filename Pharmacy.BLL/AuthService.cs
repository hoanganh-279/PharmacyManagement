using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;

namespace Pharmacy.BLL;

public class AuthService
{
    private readonly NhanVienRepositoryDAL _nhanVien;

    public AuthService(DbContextDAL db) =>
        _nhanVien = new NhanVienRepositoryDAL(db);

    public bool DangNhap(string tenDangNhap, string matKhau)
    {
        if (Validator.IsNullOrWhiteSpace(tenDangNhap) || Validator.IsNullOrWhiteSpace(matKhau))
            return false;

        var row = _nhanVien.LayChoDangNhap(tenDangNhap.Trim());
        if (row is null)
            return false;

        if (!PasswordHelper.Verify(matKhau, row.MatKhauHash))
            return false;

        UserSession.Set(row.MaNhanVien, row.HoTen, row.TenDangNhap, row.TenVaiTro);
        Logger.Info($"Đăng nhập: {row.TenDangNhap} ({row.TenVaiTro})");
        return true;
    }

    public void DangXuat()
    {
        Logger.Info("Đăng xuất.");
        UserSession.Clear();
    }
}
