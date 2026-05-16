namespace Pharmacy.DTO;

public class ChiTietPhieuNhapDTO
{
    public int MaCTPN { get; set; }
    public int MaPhieuNhap { get; set; }
    public int MaThuoc { get; set; }
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
    public DateTime? HanSuDung { get; set; }
    public decimal GiaBan { get; set; }
    public string? SoLo { get; set; }
    public string? ViTri { get; set; }
    public string? GhiChu { get; set; }
    public decimal? VAT { get; set; }
}
