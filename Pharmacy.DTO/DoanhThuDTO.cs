// File: Pharmacy.DTO/Views/DoanhThuDTO.cs
// Đặt vào thư mục Views trong project Pharmacy.DTO (cùng chỗ với HoaDonDTO, ThuocDTO...)

namespace Pharmacy.DTO.Views
{
    /// <summary>
    /// DTO tổng hợp doanh thu theo ngày/tháng – dùng cho báo cáo Finance
    /// </summary>
    public class DoanhThuDTO
    {
        public DateTime NgayBan { get; set; }
        public string MaHoaDon { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public decimal TienGiam { get; set; }
        public decimal ThanhToan { get; set; }
        public decimal LoiNhuan { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = "";
    }

    /// <summary>
    /// DTO tổng kết chỉ số chính trên dashboard
    /// </summary>
    public class TongKetDoanhThuDTO
    {
        public decimal TongDoanhThu { get; set; }
        public decimal TongLoiNhuan { get; set; }
        public int TongDonHang { get; set; }
        public decimal TyLeLoiNhuan =>
            TongDoanhThu == 0 ? 0 : Math.Round(TongLoiNhuan / TongDoanhThu * 100, 1);
    }
}