using System.Globalization;
using System.Text.RegularExpressions;

namespace Pharmacy.Common;

public static class Validator
{
    public static bool IsNullOrWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value);

    public static bool IsPositiveInt(int value) => value > 0;

    public static bool IsNonNegativeDecimal(decimal value) => value >= 0;

    private static readonly Regex PhoneRegex = new(@"^[\d\s\+\-\(\)]{8,15}$", RegexOptions.Compiled);

    public static bool IsPhoneOptional(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone.Trim());

    public static bool TryParseDecimalInvariant(string? text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public static bool TryParseIntInvariant(string? text, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}
