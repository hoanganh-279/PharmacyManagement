namespace Pharmacy.DTO.Views;

public class TonKhoViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string TenNhomThuoc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public int TonToiThieu { get; set; }
    public DateTime? HanSuDung { get; set; }
    public string TrangThaiTonKho { get; set; } = string.Empty;
}

public class ThuocSapHetHanViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public DateTime HanSuDung { get; set; }
    public int SoNgayConLai { get; set; }
    public string TrangThaiHanDung { get; set; } = string.Empty;
}

public class DoanhThuTheoNgayViewDTO
{
    public DateTime Ngay { get; set; }
    public int SoHoaDon { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal TongGiamGia { get; set; }
    public decimal DoanhThu { get; set; }
}

public class DoanhThuNhanVienViewDTO
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public int SoHoaDon { get; set; }
    public decimal DoanhThu { get; set; }
}

public class ThuocBanChayViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string TenNhomThuoc { get; set; } = string.Empty;
    public decimal TongSoLuongBan { get; set; }
    public decimal TongDoanhThu { get; set; }
}

public class DanhSachThuocViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string? HoatChat { get; set; }
    public string? HamLuong { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    public string TenNhomThuoc { get; set; } = string.Empty;
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int TonToiThieu { get; set; }
    public DateTime? HanSuDung { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class ThuocTonThapViewDTO
{
    public int MaThuoc { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public int TonToiThieu { get; set; }
    public DateTime? HanSuDung { get; set; }
}

public class LichSuNhapKhoViewDTO
{
    public int MaPhieuNhap { get; set; }
    public DateTime NgayNhap { get; set; }
    public string NhanVienNhap { get; set; } = string.Empty;
    public string? NhaCungCap { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
    public DateTime? HanSuDung { get; set; }
}

public class LichSuBanHangViewDTO
{
    public int MaHoaDon { get; set; }
    public DateTime NgayLap { get; set; }
    public string NhanVienBan { get; set; } = string.Empty;
    public string? TenKhachHang { get; set; }
    public string? SoDienThoai { get; set; }
    public string TenThuoc { get; set; } = string.Empty;
    public int SoLuongBan { get; set; }
    public decimal DonGiaBan { get; set; }
    public decimal ThanhTien { get; set; }
    public string? HinhThucThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class DoanhThuTheoThangViewDTO
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int SoHoaDon { get; set; }
    public decimal DoanhThu { get; set; }
}

public class DoanhThuHomNayViewDTO
{
    public DateTime Ngay { get; set; }
    public int SoHoaDon { get; set; }
    public decimal DoanhThu { get; set; }
}

public class DashboardTongQuanViewDTO
{
    public int SoHoaDonHomNay { get; set; }
    public decimal DoanhThuHomNay { get; set; }
    public int SoThuocTonThap { get; set; }
    public int SoThuocSapHetHan { get; set; }
}

public class AuditLogChiTietViewDTO
{
    public int MaLog { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? NhanVien { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string? TenBang { get; set; }
    public string? MaBanGhi { get; set; }
    public string? NoiDung { get; set; }
    public string? DiaChiMay { get; set; }
}
