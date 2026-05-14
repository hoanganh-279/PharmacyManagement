namespace Pharmacy.DTO;

public class AuditLogDTO
{
    public int MaLog { get; set; }
    public DateTime ThoiGian { get; set; }
    public int? MaNhanVien { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string? TenBang { get; set; }
    public string? MaBanGhi { get; set; }
    public string? NoiDung { get; set; }
    public string? DiaChiMay { get; set; }
}
