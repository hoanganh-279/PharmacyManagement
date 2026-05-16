namespace Pharmacy.DTO.Views;

public class DanhSachHangNhapKhoViewDTO
{
    public int MaPhieuNhap { get; set; }
    public int MaCTPN { get; set; }
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public decimal ThanhTien { get; set; }
    public string? SoLo { get; set; }
    public DateTime? HanSuDung { get; set; }
    public int? SoNgayConHan { get; set; }
    public string? ViTri { get; set; }
    public string? GhiChu { get; set; }
    public decimal? VATDongPhanTram { get; set; }
    public string TrangThaiPhieu { get; set; } = string.Empty;
}
