namespace Pharmacy.DTO.Views;

public class LichSuMuaHangDTO
{
    public string CCCD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public int MaHoaDon { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal ThanhTienHoaDon { get; set; }
    public string TrangThaiHoaDon { get; set; } = string.Empty;
    public string TenThuoc { get; set; } = string.Empty;
    public int SoLuongBan { get; set; }
    public decimal DonGiaBan { get; set; }
    public decimal ThanhTienDong { get; set; }
}
