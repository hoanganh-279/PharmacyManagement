using System.Globalization;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class ReportService
{
    private readonly ReportRepositoryDAL _report;

    public ReportService(DbContextDAL db) =>
        _report = new ReportRepositoryDAL(db);

    public IReadOnlyList<TonKhoViewDTO> LayTonKho()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _report.LayTonKho();
    }

    public IReadOnlyList<ThuocSapHetHanViewDTO> LayThuocSapHetHan()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _report.LayThuocSapHetHan();
    }

    public IReadOnlyList<DoanhThuTheoNgayViewDTO> LayDoanhThuTheoNgay()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayDoanhThuTheoNgay();
    }

    public IReadOnlyList<DoanhThuNhanVienViewDTO> LayDoanhThuNhanVien()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayDoanhThuNhanVien();
    }

    public IReadOnlyList<ThuocBanChayViewDTO> LayThuocBanChay()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayThuocBanChay();
    }

    public IReadOnlyList<ThuocTonThapViewDTO> LayThuocTonThap()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _report.LayThuocTonThap();
    }

    public IReadOnlyList<LichSuNhapKhoViewDTO> LayLichSuNhapKho(int? top = 200)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        return _report.LayLichSuNhapKho(top);
    }

    public IReadOnlyList<LichSuBanHangViewDTO> LayLichSuBanHang(int? top = 200)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        return _report.LayLichSuBanHang(top);
    }

    public IReadOnlyList<DoanhThuTheoThangViewDTO> LayDoanhThuTheoThang()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayDoanhThuTheoThang();
    }

    public DoanhThuHomNayViewDTO? LayDoanhThuHomNay()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayDoanhThuHomNay();
    }

    public DashboardTongQuanViewDTO? LayDashboardTongQuan()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _report.LayDashboardTongQuan();
    }

    /// <summary>Dữ liệu dashboard shell: tồn kho cho mọi vai trò; doanh thu / hóa đơn / biểu đồ cho Admin, Quản lý, Dược sĩ.</summary>
    public DashboardHienThiDTO LayDashboardHienThi()
    {
        BllAuthorization.RequireAuthenticated();
        var role = UserSession.TenVaiTro;
        var coTaiChinh = role is VaiTroTen.Admin or VaiTroTen.QuanLy or VaiTroTen.DuocSi;

        var tuanBatDau = NgayDauTuanThuHai(DateTime.Today);
        var tuanKetThuc = tuanBatDau.AddDays(7);

        var ton = _report.LayChiSoTonKhoDashboard();
        var dto = new DashboardHienThiDTO
        {
            CoTaiChinh = coTaiChinh,
            TuanBatDau = tuanBatDau,
            SoThuocTonThap = ton.TonThap,
            SoThuocSapHetHan = ton.SapHetHan,
            SoThuocHetHang = ton.HetHang,
            CanhBao = LayDanhSachCanhBaoDashboard()
        };

        if (!coTaiChinh)
        {
            dto.DoanhThuTheoNgay = Array.Empty<DashboardNgayTrongTuanDTO>();
            dto.PhanBoTrangThaiTuan = Array.Empty<DashboardPhanBoTrangThaiDTO>();
            dto.HoaDonGanDay = Array.Empty<DashboardHoaDonGanDayDTO>();
        }
        else
        {
            var tuanNay = _report.LayTongHopDoanhThuTheoKhoang(tuanBatDau, tuanKetThuc);
            dto.DoanhThuTuanNay = tuanNay.DoanhThu;
            dto.SoHoaDonHoanThanhTuan = tuanNay.SoHoaDonHoanThanh;

            var tuanTruoc = _report.LayTongHopDoanhThuTheoKhoang(tuanBatDau.AddDays(-7), tuanBatDau);
            dto.ChenhLechDoanhThuTuanTruoc = MoTaChenhLechPhanTram(tuanTruoc.DoanhThu, tuanNay.DoanhThu);

            dto.DoanhThuTheoNgay = XayDungDoanhThu7Ngay(tuanBatDau, tuanKetThuc);
            dto.PhanBoTrangThaiTuan = _report.LayPhanBoTrangThaiHoaDonTrongKhoang(tuanBatDau, tuanKetThuc);
            dto.HoaDonGanDay = _report.LayHoaDonTomTatGanDay(8);
        }

        ChuanHoaChuoiDashboard(dto);
        return dto;
    }

    private static void ChuanHoaChuoiDashboard(DashboardHienThiDTO dto)
    {
        foreach (var p in dto.PhanBoTrangThaiTuan)
            p.TrangThai = UnicodeTextHelper.TryRepairMojibakeForDisplay(p.TrangThai);

        foreach (var h in dto.HoaDonGanDay)
        {
            h.TenKhachHang = UnicodeTextHelper.TryRepairMojibakeForDisplay(h.TenKhachHang);
            h.TrangThai = UnicodeTextHelper.TryRepairMojibakeForDisplay(h.TrangThai);
        }

        foreach (var c in dto.CanhBao)
        {
            c.TenThuoc = UnicodeTextHelper.TryRepairMojibakeForDisplay(c.TenThuoc);
            c.MoTa = UnicodeTextHelper.TryRepairMojibakeForDisplay(c.MoTa);
        }
    }

    private IReadOnlyList<DashboardCanhBaoItemDTO> LayDanhSachCanhBaoDashboard()
    {
        var tonThap = LayThuocTonThap().Take(4)
            .Select(t => new DashboardCanhBaoItemDTO { TenThuoc = t.TenThuoc, MoTa = "Tồn thấp" });
        var sapHan = LayThuocSapHetHan().Take(4)
            .Select(t => new DashboardCanhBaoItemDTO
            {
                TenThuoc = t.TenThuoc,
                MoTa = $"Sắp hết hạn ({t.SoNgayConLai} ngày)"
            });

        var merged = new List<DashboardCanhBaoItemDTO>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var x in tonThap.Concat(sapHan))
        {
            if (seen.Add(x.TenThuoc))
                merged.Add(x);
            if (merged.Count >= 6)
                break;
        }

        return merged;
    }

    private IReadOnlyList<DashboardNgayTrongTuanDTO> XayDungDoanhThu7Ngay(DateTime tuanBatDau, DateTime tuanKetThuc)
    {
        var map = _report.LayDoanhThuTheoNgayTrongKhoang(tuanBatDau, tuanKetThuc);
        var list = new List<DashboardNgayTrongTuanDTO>(7);
        for (var i = 0; i < 7; i++)
        {
            var ngay = tuanBatDau.AddDays(i);
            map.TryGetValue(ngay, out var cell);
            list.Add(new DashboardNgayTrongTuanDTO
            {
                Ngay = ngay,
                NhanThu = TenTrongTuanTiengViet(ngay),
                DoanhThu = cell.DoanhThu,
                SoHoaDonHoanThanh = cell.SoHd
            });
        }

        return list;
    }

    private static string TenTrongTuanTiengViet(DateTime ngay) =>
        ngay.DayOfWeek switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            DayOfWeek.Sunday => "CN",
            _ => ""
        };

    private static DateTime NgayDauTuanThuHai(DateTime ngay)
    {
        var d = ngay.Date;
        var daysFromMonday = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return d.AddDays(-daysFromMonday);
    }

    private static string? MoTaChenhLechPhanTram(decimal tuanTruoc, decimal tuanNay)
    {
        if (tuanTruoc <= 0)
            return tuanNay > 0 ? "Tuần trước: 0đ · tuần này tăng" : "So với tuần trước: —";

        var pct = (double)((tuanNay - tuanTruoc) / tuanTruoc * 100m);
        var sign = pct >= 0 ? "+" : "";
        return $"So với tuần trước: {sign}{pct.ToString("F1", CultureInfo.InvariantCulture)}%";
    }
}
