namespace Pharmacy.Common;

/// <summary>
/// Tính giá bán lẻ gợi ý theo mức thặng giá tối đa (đơn vị đóng gói nhỏ nhất) — Thông tư BYT.
/// </summary>
public static class GiaBanBoYTeHelper
{
    public const decimal Nguong5Nghin = 5_000m;
    public const decimal Nguong100Nghin = 100_000m;
    public const decimal Nguong1Trieu = 1_000_000m;

    /// <summary>Trả về % thặng giá tối đa áp dụng; 0 nếu giá nhập ≤ 5.000đ (ngoài ba bậc quy định).</summary>
    public static decimal LayTyLeMarkupPhanTram(decimal giaNhapDonVi)
    {
        if (giaNhapDonVi <= Nguong5Nghin)
            return 10m;
        if (giaNhapDonVi <= Nguong100Nghin)
            return 10m;
        if (giaNhapDonVi <= Nguong1Trieu)
            return 7m;
        return 5m;
    }

    /// <summary>Giá bán gợi ý = giá nhập × (1 + %/100), làm tròn lên đồng.</summary>
    public static decimal TinhGiaBanGoiY(decimal giaNhapDonVi)
    {
        if (giaNhapDonVi <= 0)
            return 0m;

        var tyLe = LayTyLeMarkupPhanTram(giaNhapDonVi);
        if (tyLe <= 0)
            return giaNhapDonVi;

        return Math.Ceiling(giaNhapDonVi * (1m + tyLe / 100m));
    }
}
