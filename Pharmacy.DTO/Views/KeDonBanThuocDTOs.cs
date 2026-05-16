namespace Pharmacy.DTO.Views;

/// <summary>Thuốc khả dụng bán — tồn thực tế từ lô còn hạn (LoThuoc).</summary>
public class ThuocKeDonViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string? HoatChat { get; set; }
    public string? HamLuong { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int TonLoConHan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? HanSuDung { get; set; }
}

/// <summary>Dòng giỏ hàng trên màn kê đơn (phiên làm việc, chưa ghi CSDL).</summary>
public class DonHangGioHangDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public int TonToiDa { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}
