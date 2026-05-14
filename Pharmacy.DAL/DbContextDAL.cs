using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL;

public class DbContextDAL
{
    public string ConnectionString { get; }

    public DbContextDAL(string? connectionString = null) =>
        ConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? DefaultConnectionString
            : connectionString;

    /// <summary>Chuỗi mặc định LocalDB — ghi đè khi khởi tạo ứng dụng nếu cần.</summary>
    public static string DefaultConnectionString { get; set; } =
        "Server=(localdb)\\mssqllocaldb;Database=PharmacyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public SqlConnection CreateConnection() => new(ConnectionString);
}
