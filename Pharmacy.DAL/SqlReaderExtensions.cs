using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL;

internal static class SqlReaderExtensions
{
    public static string GetString(this SqlDataReader r, string column) =>
        r[column] as string ?? string.Empty;

    public static string? GetNullableString(this SqlDataReader r, string column) =>
        r[column] is DBNull ? null : r[column] as string;

    public static int GetInt32(this SqlDataReader r, string column) => Convert.ToInt32(r[column], null);

    public static int? GetNullableInt32(this SqlDataReader r, string column) =>
        r[column] is DBNull ? null : Convert.ToInt32(r[column], null);

    public static bool GetBoolean(this SqlDataReader r, string column) => Convert.ToBoolean(r[column], null);

    public static decimal GetDecimal(this SqlDataReader r, string column) => Convert.ToDecimal(r[column], null);

    public static decimal? GetNullableDecimal(this SqlDataReader r, string column) =>
        r[column] is DBNull ? null : Convert.ToDecimal(r[column], null);

    public static DateTime GetDateTime(this SqlDataReader r, string column) => Convert.ToDateTime(r[column], null);

    public static DateTime? GetNullableDateTime(this SqlDataReader r, string column) =>
        r[column] is DBNull ? null : Convert.ToDateTime(r[column], null);
}
