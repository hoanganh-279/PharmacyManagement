namespace Pharmacy.DTO;

public class PhieuNhapDTO
{
    public int MaPhieuNhap { get; set; }
    public DateTime NgayNhap { get; set; }
    public int MaNhanVien { get; set; }
    public string? NhaCungCap { get; set; }
    public decimal TongTien { get; set; }
    public string? GhiChu { get; set; }
}
