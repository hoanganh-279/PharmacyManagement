namespace Pharmacy.Common;

/// <summary>
/// Phiên đăng nhập hiện tại (WinForms có thể gán sau khi đăng nhập thành công).
/// </summary>
public static class UserSession
{
    public static int? MaNhanVien { get; private set; }
    public static string? HoTen { get; private set; }
    public static string? TenDangNhap { get; private set; }
    public static string? TenVaiTro { get; private set; }

    public static bool IsAuthenticated => MaNhanVien.HasValue;

    public static void Set(int maNhanVien, string hoTen, string tenDangNhap, string tenVaiTro)
    {
        MaNhanVien = maNhanVien;
        HoTen = hoTen;
        TenDangNhap = tenDangNhap;
        TenVaiTro = tenVaiTro;
    }

    public static void Clear()
    {
        MaNhanVien = null;
        HoTen = null;
        TenDangNhap = null;
        TenVaiTro = null;
    }
}
