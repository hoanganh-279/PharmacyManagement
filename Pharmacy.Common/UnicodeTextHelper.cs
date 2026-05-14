using System.Text;

namespace Pharmacy.Common;

/// <summary>
/// Xử lý chuỗi tiếng Việt khi CSDL/script bị lưu sai (UTF-8 đọc nhầm Latin-1 hoặc Windows-1252, ví dụ "Thá»«", "HoÃ n thÃ nh").
/// </summary>
public static class UnicodeTextHelper
{
    /// <summary>
    /// Biến thể khi chuỗi Unicode đúng bị mã UTF-8 rồi từng byte hiểu là Latin-1 (dùng trong SQL IN (...)).
    /// </summary>
    public static string Utf8BytesMisreadAsLatin1(string utf16)
    {
        if (string.IsNullOrEmpty(utf16))
            return utf16;
        return Encoding.Latin1.GetString(Encoding.UTF8.GetBytes(utf16));
    }

    /// <summary>
    /// Chuỗi chuẩn + các biến thể mojibake (tối đa 2 lớp Latin-1) để dùng trong IN (...).
    /// </summary>
    public static IReadOnlyList<string> DistinctMojibakeAliases(string canonical)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var list = new List<string>(4);
        void Add(string? s)
        {
            if (string.IsNullOrEmpty(s) || !seen.Add(s))
                return;
            list.Add(s);
        }

        Add(canonical);
        var a = Utf8BytesMisreadAsLatin1(canonical);
        Add(a);
        if (a != canonical)
        {
            var b = Utf8BytesMisreadAsLatin1(a);
            Add(b);
        }

        foreach (var c in Utf8BytesMisreadAsCp1252Aliases(canonical))
            Add(c);

        return list;
    }

    /// <summary>Biến thể UTF-8 → byte → hiểu nhầm CP1252 (một số máy/client).</summary>
    private static IEnumerable<string> Utf8BytesMisreadAsCp1252Aliases(string canonical)
    {
        string? one = null;
        try
        {
            one = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(canonical));
        }
        catch (ArgumentException)
        {
            yield break;
        }

        if (!string.IsNullOrEmpty(one) && one != canonical)
        {
            yield return one;
            string? two;
            try
            {
                two = Encoding.GetEncoding(1252).GetString(Encoding.UTF8.GetBytes(one));
            }
            catch (ArgumentException)
            {
                yield break;
            }

            if (!string.IsNullOrEmpty(two) && two != one)
                yield return two;
        }
    }

    /// <summary>Chuẩn hóa chuỗi hiển thị: thử Latin-1 và CP1252, chọn bản ít dấu hiệu mojibake nhất.</summary>
    public static string TryRepairMojibakeForDisplay(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return s ?? "";

        var cur = s;
        for (var pass = 0; pass < 3; pass++)
        {
            var next = PickBestRepairCandidate(cur);
            if (next == cur)
                break;
            cur = next;
        }

        return cur;
    }

    /// <summary>Tương thích tên cũ — nội bộ gọi <see cref="TryRepairMojibakeForDisplay"/>.</summary>
    public static string TryRepairIsoLatin1MojibakeFromUtf8(string? s) =>
        TryRepairMojibakeForDisplay(s);

    private static string PickBestRepairCandidate(string s)
    {
        var best = s;
        var bestScore = MojibakeScore(s);

        foreach (var candidate in EnumerateSingleStepRepairs(s))
        {
            var sc = MojibakeScore(candidate);
            if (sc < bestScore)
            {
                best = candidate;
                bestScore = sc;
            }
        }

        return best;
    }

    private static IEnumerable<string> EnumerateSingleStepRepairs(string s)
    {
        foreach (var enc in new Encoding[] { Encoding.Latin1, GetEncoding1252() })
        {
            string? r = null;
            try
            {
                r = Encoding.UTF8.GetString(enc.GetBytes(s));
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(r) && !r.Contains('\uFFFD', StringComparison.Ordinal) && r != s)
                yield return r;
        }
    }

    private static Encoding GetEncoding1252()
    {
        try
        {
            return Encoding.GetEncoding(1252);
        }
        catch (ArgumentException)
        {
            return Encoding.Latin1;
        }
        catch (NotSupportedException)
        {
            return Encoding.Latin1;
        }
    }

    private static int MojibakeScore(string s)
    {
        var score = 0;
        foreach (var c in s)
        {
            if (c is 'Ã' or 'Ä' or 'Å' or 'Ð' or 'Ý')
                score += 4;
            if (c is '»' or '¼' or '½' or '¾' or '¢' or '¤' or '¦' or '§')
                score += 3;
            if (c == '\uFFFD')
                score += 10;
            if (c == 'Â')
                score += 2;
        }

        return score;
    }
}
