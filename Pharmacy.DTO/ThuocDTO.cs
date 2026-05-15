namespace Pharmacy.DTO;

public class ThuocDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string? HoatChat { get; set; }
    public string? HamLuong { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int TonToiThieu { get; set; }
    public DateTime? HanSuDung { get; set; }
    public int MaNhomThuoc { get; set; }

    public int? MaDQG { get; set; }
    public string? SoDangKy { get; set; }
    public string? HangSanXuat { get; set; }
    public string? NuocSanXuat { get; set; }
    public string? DongGoi { get; set; }

    public bool TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
}
