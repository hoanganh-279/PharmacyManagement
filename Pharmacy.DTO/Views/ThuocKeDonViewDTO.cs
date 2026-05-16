namespace Pharmacy.DTO.Views
{
    public class ThuocKeDonViewDTO
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public string HoatChat { get; set; } = string.Empty;
        public string HamLuong { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = string.Empty;

        public decimal GiaBan { get; set; }
        public int TonLoConHan { get; set; }

        public DateTime? HanSuDung { get; set; }   // ✅ FIX CS0117

        public string TrangThai
        {
            get
            {
                if (TonLoConHan <= 0) return "Hết hàng";
                if (TonLoConHan < 10) return "Tồn thấp";
                return "Còn hàng";
            }
        }
    }

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
}