namespace Pharmacy.DTO;

public class NhomThuocDTO
{
    public int MaNhomThuoc { get; set; }
    public string TenNhomThuoc { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThai { get; set; }
}
