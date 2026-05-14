namespace Pharmacy.DTO;

public class NhanVienDTO
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string? MatKhauHash { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public int MaVaiTro { get; set; }
    public string? TenVaiTro { get; set; }
    public bool TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
}

/// <summary>Kết quả gọi sp_DangNhap.</summary>
public class NhanVienDangNhapDTO
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauHash { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;
}
