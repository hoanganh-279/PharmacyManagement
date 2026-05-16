namespace Pharmacy.DTO;

public class HoaDonDTO
{
    public int MaHoaDon { get; set; }
    public DateTime NgayLap { get; set; }
    public int MaNhanVien { get; set; }
    public string? CCCD { get; set; }
    public decimal TongTien { get; set; }
    public decimal GiamGia { get; set; }
    public decimal ThanhTien { get; set; }
    public string? HinhThucThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}
