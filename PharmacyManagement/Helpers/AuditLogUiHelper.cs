#nullable disable
namespace PharmacyManagement.Helpers;

/// <summary>Ánh xạ phân hệ + màu nhãn thao tác cho màn Audit log (nhất quán với trigger CSDL).</summary>
internal static class AuditLogUiHelper
{
    public static string PhanHeTuBang(string tenBang, string hanhDong)
    {
        if (!string.IsNullOrWhiteSpace(tenBang))
        {
            return tenBang switch
            {
                "PhieuNhap" or "ChiTietPhieuNhap" => "Kho / Nhập hàng",
                "Thuoc" => "Kho / Danh mục",
                "HoaDon" => "Bán hàng",
                "ChiTietHoaDon" => "Bán hàng",
                "NhanVien" or "VaiTro" => "Quản trị / Bảo mật",
                _ => tenBang
            };
        }

        if (hanhDong.Contains("Nhập kho", StringComparison.Ordinal))
            return "Kho / Nhập hàng";
        if (hanhDong.Contains("Bán thuốc", StringComparison.Ordinal) || hanhDong.Contains("hóa đơn", StringComparison.OrdinalIgnoreCase))
            return "Bán hàng";

        return "Hệ thống";
    }

    /// <summary>Màu chữ nhãn hành động (tương thích chế độ sáng).</summary>
    public static Color MauChuHanhDong(string hanhDong)
    {
        if (string.IsNullOrEmpty(hanhDong))
            return InventoryUiKit.Muted;

        if (hanhDong.Contains("Nhập kho", StringComparison.Ordinal))
            return Color.FromArgb(27, 94, 32);
        if (hanhDong.Contains("Xóa", StringComparison.Ordinal))
            return InventoryUiKit.Danger;
        if (hanhDong.Contains("giá", StringComparison.OrdinalIgnoreCase) || hanhDong.Contains("Cập nhật giá", StringComparison.Ordinal))
            return InventoryUiKit.Warn;
        if (hanhDong.Contains("Bán thuốc", StringComparison.Ordinal))
            return Color.FromArgb(0, 121, 107);
        if (hanhDong.Contains("hóa đơn", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb(13, 71, 161);
        if (hanhDong.Contains("Thêm", StringComparison.Ordinal))
            return Color.FromArgb(25, 118, 210);
        if (hanhDong.Contains("phân quyền", StringComparison.OrdinalIgnoreCase) || hanhDong.Contains("Đăng nhập", StringComparison.Ordinal))
            return Color.FromArgb(0, 131, 143);

        return InventoryUiKit.Ink;
    }
}
