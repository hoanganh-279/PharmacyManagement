using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL;

public class DbContextDAL
{
    public string ConnectionString { get; }

    public DbContextDAL(string? connectionString = null)
    {
        ConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? DefaultConnectionString
            : connectionString;
    }

    public static string DefaultConnectionString { get; set; } =
        "Server=(localdb)\\mssqllocaldb;Database=PharmacyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    // =========================
    // ExecuteQuery
    // =========================
    public DataTable ExecuteQuery(
        string query,
        SqlParameter[]? parameters = null)
    {
        using SqlConnection conn = CreateConnection();

        using SqlCommand cmd =
            new SqlCommand(query, conn);

        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }

        SqlDataAdapter adapter =
            new SqlDataAdapter(cmd);

        DataTable dt = new DataTable();

        conn.Open();

        adapter.Fill(dt);

        return dt;
    }

    // =========================
    // ExecuteNonQuery
    // =========================
    public int ExecuteNonQuery(
        string query,
        SqlParameter[]? parameters = null)
    {
        using SqlConnection conn = CreateConnection();

        using SqlCommand cmd =
            new SqlCommand(query, conn);

        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }

        conn.Open();

        return cmd.ExecuteNonQuery();
    }

    // =========================
    // ExecuteScalar
    // =========================
    public object? ExecuteScalar(
        string query,
        SqlParameter[]? parameters = null)
    {
        using SqlConnection conn = CreateConnection();

        using SqlCommand cmd =
            new SqlCommand(query, conn);

        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }

        conn.Open();

        return cmd.ExecuteScalar();
    }
}