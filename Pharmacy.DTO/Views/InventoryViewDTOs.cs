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
    public string TrangThaiPhieu { get; set; } = string.Empty;
}

public class TraCuuDqgViewDTO
{
    public int MaDQG { get; set; }
    public string? MaDQGDonVi { get; set; }
    public string TenHangHoa { get; set; } = string.Empty;
    public string? SoDangKy { get; set; }
    public string? HoatChatChinh { get; set; }
    public string? HamLuong { get; set; }
    public string? DongGoi { get; set; }
    public string? HangSanXuat { get; set; }
    public string? NuocSanXuat { get; set; }
    public string? DonViTinh { get; set; }
    public string TrangThaiNhapKho { get; set; } = string.Empty;
    public int? MaThuoc { get; set; }
    public int? SoLuongTon { get; set; }
}
