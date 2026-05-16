using System.Globalization;
using System.Text.RegularExpressions;

namespace Pharmacy.Common;

public static class Validator
{
    public static bool IsNullOrWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value);

    public static bool IsPositiveInt(int value) => value > 0;

    public static bool IsNonNegativeDecimal(decimal value) => value >= 0;

    private static readonly Regex PhoneRegex = new(@"^[\d\s\+\-\(\)]{8,15}$", RegexOptions.Compiled);
    private static readonly Regex CccdRegex = new(@"^\d{12}$", RegexOptions.Compiled);

    public static bool IsPhoneOptional(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone.Trim());

    /// <summary>Chuẩn hóa CCCD 12 chữ số (bỏ khoảng trắng). Trả về false nếu không hợp lệ.</summary>
    public static bool TryNormalizeCccd(string? cccd, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(cccd))
            return false;
        normalized = new string(cccd.Where(char.IsDigit).ToArray());
        return CccdRegex.IsMatch(normalized);
    }

    public static bool IsCccdOptional(string? cccd) =>
        string.IsNullOrWhiteSpace(cccd) || TryNormalizeCccd(cccd, out _);

    public static bool TryParseDecimalInvariant(string? text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public static bool TryParseIntInvariant(string? text, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}
