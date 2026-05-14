namespace Pharmacy.DTO.Views;

/// <summary>Dữ liệu tổng hợp cho màn Dashboard (WinForms).</summary>
public sealed class DashboardHienThiDTO
{
    /// <summary>Admin / Quản lý / Dược sĩ: có doanh thu, biểu đồ tài chính, hóa đơn.</summary>
    public bool CoTaiChinh { get; set; }

    public DateTime TuanBatDau { get; set; }

    public decimal DoanhThuTuanNay { get; set; }
    public int SoHoaDonHoanThanhTuan { get; set; }

    /// <summary>Mô tả so sánh với tuần trước (phần trăm hoặc ghi chú).</summary>
    public string? ChenhLechDoanhThuTuanTruoc { get; set; }

    public int SoThuocHetHang { get; set; }
    public int SoThuocTonThap { get; set; }
    public int SoThuocSapHetHan { get; set; }

    public IReadOnlyList<DashboardNgayTrongTuanDTO> DoanhThuTheoNgay { get; set; } =
        Array.Empty<DashboardNgayTrongTuanDTO>();

    public IReadOnlyList<DashboardPhanBoTrangThaiDTO> PhanBoTrangThaiTuan { get; set; } =
        Array.Empty<DashboardPhanBoTrangThaiDTO>();

    public IReadOnlyList<DashboardHoaDonGanDayDTO> HoaDonGanDay { get; set; } =
        Array.Empty<DashboardHoaDonGanDayDTO>();

    public IReadOnlyList<DashboardCanhBaoItemDTO> CanhBao { get; set; } =
        Array.Empty<DashboardCanhBaoItemDTO>();
}

public sealed class DashboardNgayTrongTuanDTO
{
    public DateTime Ngay { get; set; }
    public string NhanThu { get; set; } = "";
    public decimal DoanhThu { get; set; }
    public int SoHoaDonHoanThanh { get; set; }
}

public sealed class DashboardPhanBoTrangThaiDTO
{
    public string TrangThai { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal TongThanhTien { get; set; }
}

public sealed class DashboardHoaDonGanDayDTO
{
    public int MaHoaDon { get; set; }
    public string TenKhachHang { get; set; } = "";
    public DateTime NgayLap { get; set; }
    public decimal ThanhTien { get; set; }
    public string TrangThai { get; set; } = "";
}

public sealed class DashboardCanhBaoItemDTO
{
    public string TenThuoc { get; set; } = "";
    public string MoTa { get; set; } = "";
}
