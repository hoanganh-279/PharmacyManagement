using Pharmacy.Common;

namespace PharmacyManagement.Helpers;

/// <summary>
/// Chuẩn hóa chữ trên avatar, nhãn vai trò và màu theo quyền (đồng bộ seed SQL TenVaiTro).
/// </summary>
public static class UserDisplayHelper
{
    public static string GetAvatarInitials(string? hoTen, string? tenDangNhap)
    {
        if (!string.IsNullOrWhiteSpace(hoTen))
        {
            var parts = hoTen.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var a = char.ToUpperInvariant(NormalizeLetter(parts[0][0]));
                var b = char.ToUpperInvariant(NormalizeLetter(parts[^1][0]));
                return $"{a}{b}";
            }

            if (parts.Length == 1)
            {
                var w = parts[0];
                if (w.Length >= 2)
                    return $"{char.ToUpperInvariant(NormalizeLetter(w[0]))}{char.ToUpperInvariant(NormalizeLetter(w[1]))}";
                if (w.Length == 1)
                    return $"{char.ToUpperInvariant(NormalizeLetter(w[0]))}";
            }
        }

        if (!string.IsNullOrWhiteSpace(tenDangNhap))
        {
            var t = tenDangNhap.Trim();
            if (t.Length >= 2)
                return t[..2].ToUpperInvariant();
            return t.ToUpperInvariant();
        }

        return "?";
    }

    /// <summary>Mã gắn với vai trò (logo chữ theo quyền).</summary>
    public static string GetRoleBadgeLetters(string? tenVaiTro) => tenVaiTro switch
    {
        VaiTroTen.Admin => "AD",
        VaiTroTen.QuanLy => "QL",
        VaiTroTen.DuocSi => "DS",
        VaiTroTen.NhanVienKho => "KH",
        _ => "NV"
    };

    public static string GetVaiTroDisplayName(string? tenVaiTro) => tenVaiTro switch
    {
        VaiTroTen.Admin => "Quản trị viên",
        VaiTroTen.QuanLy => "Quản lý nhà thuốc",
        VaiTroTen.DuocSi => "Dược sĩ / bán hàng",
        VaiTroTen.NhanVienKho => "Nhân viên kho",
        _ => string.IsNullOrWhiteSpace(tenVaiTro) ? "Nhân viên" : tenVaiTro
    };

    public static Color GetAvatarBackColor(string? tenVaiTro) => tenVaiTro switch
    {
        VaiTroTen.Admin => Color.FromArgb(27, 94, 32),
        VaiTroTen.QuanLy => Color.FromArgb(46, 125, 50),
        VaiTroTen.DuocSi => Color.FromArgb(56, 142, 60),
        VaiTroTen.NhanVienKho => Color.FromArgb(0, 121, 107),
        _ => Color.FromArgb(46, 125, 50)
    };

    private static char NormalizeLetter(char c) => char.IsLetter(c) ? c : '?';
}
