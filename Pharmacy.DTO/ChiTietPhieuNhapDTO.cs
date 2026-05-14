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
}
