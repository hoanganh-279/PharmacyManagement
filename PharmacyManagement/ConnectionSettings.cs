using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Pharmacy.DAL;

namespace PharmacyManagement;

/// <summary>Đọc chuỗi kết nối từ appsettings.json (cùng thư mục exe) và thông báo lỗi SQL thân thiện.</summary>
internal static class ConnectionSettings
{
    /// <summary>Gọi ngay sau ApplicationConfiguration.Initialize(), trước khi mở form đăng nhập.</summary>
    internal static void ApplyFromJsonFile()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
                return;
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var cs))
                return;
            if (!cs.TryGetProperty("PharmacyManagement", out var el))
                return;
            var s = el.GetString();
            if (!string.IsNullOrWhiteSpace(s))
                DbContextDAL.DefaultConnectionString = s.Trim();
        }
        catch
        {
            // Giữ DefaultConnectionString trong DbContextDAL
        }
    }

    internal static string FormatSqlExceptionForUser(SqlException ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Không kết nối được SQL Server hoặc không mở được CSDL «PharmacyManagement».");
        sb.AppendLine();

        if (ex.Number == 4060 || ex.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("• CSDL có thể chưa được tạo trên instance bạn đang dùng.");
            sb.AppendLine("  Chạy script: SQL\\PharmacyManagement.sql (SSMS hoặc sqlcmd), chọn đúng Server khớp chuỗi kết nối.");
            sb.AppendLine();
        }

        if (ex.Number == 18456 || ex.Message.Contains("Login failed", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("• Đăng nhập SQL thất bại: kiểm tra Trusted_Connection / tài khoản SQL trong appsettings.json.");
            sb.AppendLine();
        }

        sb.AppendLine("Chỉnh chuỗi kết nối trong appsettings.json (mục ConnectionStrings:PharmacyManagement), file nằm cùng thư mục với file .exe sau khi build.");
        sb.AppendLine();
        sb.Append("Chi tiết kỹ thuật: ");
        sb.Append(ex.Message);
        return sb.ToString();
    }
}
