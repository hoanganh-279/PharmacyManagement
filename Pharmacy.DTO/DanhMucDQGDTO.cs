namespace Pharmacy.DTO;

/// <summary>
/// Bản ghi danh mục Dược Quốc Gia (DQG) — tra cứu khi thêm hàng / nhập kho.
/// </summary>
public class DanhMucDQGDTO
{
    public int MaDQG { get; set; }
    public string? MaDQGDonVi { get; set; }
    public string TenHangHoa { get; set; } = string.Empty;
    public string? SoDangKy { get; set; }
    public string? HoatChatChinh { get; set; }
    public string? HoatChatDangKy { get; set; }
    public string? HamLuong { get; set; }
    public string? DongGoi { get; set; }
    public string? HangSanXuat { get; set; }
    public string? NuocSanXuat { get; set; }
    public string? DonViTinh { get; set; }
    public bool TrangThai { get; set; } = true;
}
