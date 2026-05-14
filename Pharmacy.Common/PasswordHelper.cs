namespace Pharmacy.Common;

/// <summary>
/// Mật khẩu: ưu tiên BCrypt; nếu CSDL còn chuỗi thuần (dữ liệu mẫu cũ) thì so khớp trực tiếp.
/// </summary>
public static class PasswordHelper
{
    public static string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 11);

    public static bool Verify(string plainPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);

        return string.Equals(plainPassword, storedHash, StringComparison.Ordinal);
    }
}
