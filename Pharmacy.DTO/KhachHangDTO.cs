namespace Pharmacy.DTO;

public class KhachHangDTO
{
    public string CCCD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? DiaChi { get; set; }
    public string? GhiChu { get; set; }
    public bool TrangThai { get; set; } = true;
    public DateTime NgayTao { get; set; }
}
