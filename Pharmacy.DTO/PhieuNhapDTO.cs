namespace Pharmacy.DTO;

public class PhieuNhapDTO
{
    public int MaPhieuNhap { get; set; }
    public DateTime NgayNhap { get; set; }
    public int MaNhanVien { get; set; }
    public string? SoHoaDon { get; set; }
    public DateTime? NgayHoaDon { get; set; }
    public string? LoaiPhieuNhap { get; set; }
    public int? MaKho { get; set; }
    public int? MaNhaCungCap { get; set; }
    public string? PhuongTienVanChuyen { get; set; }
    public string? DonViVanChuyen { get; set; }
    public string? NguoiGiaoHang { get; set; }
    public decimal VAT { get; set; }
    public decimal ChietKhau { get; set; }
    public decimal CongNo { get; set; }
    public decimal TongTien { get; set; }
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "Đang lập";
}
