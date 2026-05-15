using Microsoft.Data.SqlClient;
using Pharmacy.Common;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class ReportRepositoryDAL
{
    private readonly DbContextDAL _db;

    /// <summary>Các biến thể chuỗi trạng thái "Hoàn thành" (Unicode chuẩn + mojibake UTF-8/Latin-1) để khớp dữ liệu CSDL cũ.</summary>
    private static readonly IReadOnlyList<string> TrangThaiHoanThanhAliases =
        UnicodeTextHelper.DistinctMojibakeAliases("Hoàn thành");

    public ReportRepositoryDAL(DbContextDAL db) => _db = db;

    private static string SqlTrangThaiHoanThanhInClause(string column) =>
        column + " IN (" + string.Join(",", TrangThaiHoanThanhAliases.Select((_, i) => "@htt" + i)) + ")";

    private static void AddTrangThaiHoanThanhParameters(SqlCommand cmd)
    {
        for (var i = 0; i < TrangThaiHoanThanhAliases.Count; i++)
            cmd.Parameters.AddWithValue("@htt" + i, TrangThaiHoanThanhAliases[i]);
    }

    public IReadOnlyList<TonKhoViewDTO> LayTonKho()
    {
        const string sql = "SELECT * FROM vw_TonKho ORDER BY TenThuoc;";
        var list = new List<TonKhoViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new TonKhoViewDTO
            {
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                TenNhomThuoc = rd.GetString("TenNhomThuoc"),
                DonViTinh = rd.GetString("DonViTinh"),
                SoLuongTon = rd.GetInt32("SoLuongTon"),
                TonToiThieu = rd.GetInt32("TonToiThieu"),
                HanSuDung = rd.GetNullableDateTime("HanSuDung"),
                TrangThaiTonKho = rd.GetString("TrangThaiTonKho")
            });
        }

        return list;
    }

    public IReadOnlyList<ThuocSapHetHanViewDTO> LayThuocSapHetHan()
    {
        const string sql = "SELECT * FROM vw_ThuocSapHetHan ORDER BY HanSuDung;";
        var list = new List<ThuocSapHetHanViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThuocSapHetHanViewDTO
            {
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                DonViTinh = rd.GetString("DonViTinh"),
                SoLuongTon = rd.GetInt32("SoLuongTon"),
                HanSuDung = rd.GetDateTime("HanSuDung"),
                SoNgayConLai = rd.GetInt32("SoNgayConLai"),
                TrangThaiHanDung = rd.GetString("TrangThaiHanDung")
            });
        }

        return list;
    }

    public IReadOnlyList<DoanhThuTheoNgayViewDTO> LayDoanhThuTheoNgay()
    {
        const string sql = "SELECT * FROM vw_DoanhThuTheoNgay ORDER BY Ngay DESC;";
        var list = new List<DoanhThuTheoNgayViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DoanhThuTheoNgayViewDTO
            {
                Ngay = rd.GetDateTime("Ngay"),
                SoHoaDon = rd.GetInt32("SoHoaDon"),
                TongTienHang = rd.GetDecimal("TongTienHang"),
                TongGiamGia = rd.GetDecimal("TongGiamGia"),
                DoanhThu = rd.GetDecimal("DoanhThu")
            });
        }

        return list;
    }

    public IReadOnlyList<DoanhThuNhanVienViewDTO> LayDoanhThuNhanVien()
    {
        const string sql = "SELECT * FROM vw_DoanhThuNhanVien ORDER BY DoanhThu DESC;";
        var list = new List<DoanhThuNhanVienViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DoanhThuNhanVienViewDTO
            {
                MaNhanVien = rd.GetInt32("MaNhanVien"),
                HoTen = rd.GetString("HoTen"),
                SoHoaDon = rd.GetInt32("SoHoaDon"),
                DoanhThu = rd.GetDecimal("DoanhThu")
            });
        }

        return list;
    }

    public IReadOnlyList<ThuocBanChayViewDTO> LayThuocBanChay()
    {
        const string sql = "SELECT * FROM vw_ThuocBanChay ORDER BY TongSoLuongBan DESC;";
        var list = new List<ThuocBanChayViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThuocBanChayViewDTO
            {
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                TenNhomThuoc = rd.GetString("TenNhomThuoc"),
                TongSoLuongBan = rd.GetDecimal("TongSoLuongBan"),
                TongDoanhThu = rd.GetDecimal("TongDoanhThu")
            });
        }

        return list;
    }

    public IReadOnlyList<ThuocTonThapViewDTO> LayThuocTonThap()
    {
        const string sql = "SELECT * FROM vw_ThuocTonThap ORDER BY SoLuongTon;";
        var list = new List<ThuocTonThapViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThuocTonThapViewDTO
            {
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                DonViTinh = rd.GetString("DonViTinh"),
                SoLuongTon = rd.GetInt32("SoLuongTon"),
                TonToiThieu = rd.GetInt32("TonToiThieu"),
                HanSuDung = rd.GetNullableDateTime("HanSuDung")
            });
        }

        return list;
    }

    public IReadOnlyList<LichSuNhapKhoViewDTO> LayLichSuNhapKho(int? top = 200)
    {
        var sql = top.HasValue
            ? $"SELECT TOP ({top.Value}) * FROM vw_DanhSachHangNhapKho ORDER BY NgayNhap DESC;"
            : "SELECT * FROM vw_DanhSachHangNhapKho ORDER BY NgayNhap DESC;";

        var list = new List<LichSuNhapKhoViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new LichSuNhapKhoViewDTO
            {
                MaPhieuNhap = rd.GetInt32("MaPhieuNhap"),
                NgayNhap = rd.GetDateTime("NgayNhap"),
                NhanVienNhap = rd.GetString("NhanVienNhap"),
                NhaCungCap = rd.GetNullableString("TenNhaCungCap"),
                TenThuoc = rd.GetString("TenThuoc"),
                SoLuongNhap = rd.GetInt32("SoLuongNhap"),
                DonGiaNhap = rd.GetDecimal("DonGiaNhap"),
                ThanhTien = rd.GetDecimal("ThanhTien"),
                HanSuDung = rd.GetNullableDateTime("HanSuDung")
            });
        }

        return list;
    }

    public IReadOnlyList<LichSuBanHangViewDTO> LayLichSuBanHang(int? top = 200)
    {
        var sql = top.HasValue
            ? $"SELECT TOP ({top.Value}) * FROM vw_LichSuBanHang ORDER BY NgayLap DESC;"
            : "SELECT * FROM vw_LichSuBanHang ORDER BY NgayLap DESC;";

        var list = new List<LichSuBanHangViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new LichSuBanHangViewDTO
            {
                MaHoaDon = rd.GetInt32("MaHoaDon"),
                NgayLap = rd.GetDateTime("NgayLap"),
                NhanVienBan = rd.GetString("NhanVienBan"),
                TenKhachHang = rd.GetNullableString("HoTen"),
                SoDienThoai = rd.GetNullableString("SoDienThoai"),
                TenThuoc = rd.GetString("TenThuoc"),
                SoLuongBan = rd.GetInt32("SoLuongBan"),
                DonGiaBan = rd.GetDecimal("DonGiaBan"),
                ThanhTien = rd.GetDecimal("ThanhTien"),
                HinhThucThanhToan = rd.GetNullableString("HinhThucThanhToan"),
                TrangThai = rd.GetString("TrangThai")
            });
        }

        return list;
    }

    public IReadOnlyList<DoanhThuTheoThangViewDTO> LayDoanhThuTheoThang()
    {
        const string sql = "SELECT * FROM vw_DoanhThuTheoThang ORDER BY Nam DESC, Thang DESC;";
        var list = new List<DoanhThuTheoThangViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DoanhThuTheoThangViewDTO
            {
                Nam = rd.GetInt32("Nam"),
                Thang = rd.GetInt32("Thang"),
                SoHoaDon = rd.GetInt32("SoHoaDon"),
                DoanhThu = rd.GetDecimal("DoanhThu")
            });
        }

        return list;
    }

    public DoanhThuHomNayViewDTO? LayDoanhThuHomNay()
    {
        const string sql = "SELECT * FROM vw_DoanhThuHomNay;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return null;

        return new DoanhThuHomNayViewDTO
        {
            Ngay = rd.GetDateTime("Ngay"),
            SoHoaDon = rd.GetInt32("SoHoaDon"),
            DoanhThu = rd.GetDecimal("DoanhThu")
        };
    }

    public DashboardTongQuanViewDTO? LayDashboardTongQuan()
    {
        const string sql = "SELECT * FROM vw_DashboardTongQuan;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return null;

        return new DashboardTongQuanViewDTO
        {
            SoHoaDonHomNay = rd.GetInt32("SoHoaDonHomNay"),
            DoanhThuHomNay = rd.GetDecimal("DoanhThuHomNay"),
            SoThuocTonThap = rd.GetInt32("SoThuocTonThap"),
            SoThuocSapHetHan = rd.GetInt32("SoThuocSapHetHan")
        };
    }

    public (int TonThap, int SapHetHan, int HetHang) LayChiSoTonKhoDashboard()
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM Thuoc WHERE TrangThai = 1 AND SoLuongTon > 0 AND SoLuongTon < TonToiThieu) AS TonThap,
                (SELECT COUNT(*) FROM Thuoc WHERE TrangThai = 1 AND HanSuDung IS NOT NULL
                   AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), HanSuDung) <= 90 AND SoLuongTon > 0) AS SapHetHan,
                (SELECT COUNT(*) FROM Thuoc WHERE TrangThai = 1 AND SoLuongTon = 0) AS HetHang;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return (0, 0, 0);

        return (rd.GetInt32("TonThap"), rd.GetInt32("SapHetHan"), rd.GetInt32("HetHang"));
    }

    public (decimal DoanhThu, int SoHoaDonHoanThanh) LayTongHopDoanhThuTheoKhoang(DateTime tuNgay, DateTime denNgay)
    {
        var sql = $"""
            SELECT
                ISNULL(SUM(CASE WHEN {SqlTrangThaiHoanThanhInClause("TrangThai")} THEN ThanhTien ELSE 0 END), 0) AS DoanhThu,
                ISNULL(SUM(CASE WHEN {SqlTrangThaiHoanThanhInClause("TrangThai")} THEN 1 ELSE 0 END), 0) AS SoHD
            FROM HoaDon
            WHERE NgayLap >= @Tu AND NgayLap < @Den;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Tu", tuNgay);
        cmd.Parameters.AddWithValue("@Den", denNgay);
        AddTrangThaiHoanThanhParameters(cmd);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return (0, 0);

        return (rd.GetDecimal("DoanhThu"), rd.GetInt32("SoHD"));
    }

    public IReadOnlyDictionary<DateTime, (decimal DoanhThu, int SoHd)> LayDoanhThuTheoNgayTrongKhoang(
        DateTime tuNgay, DateTime denNgay)
    {
        var sql = $"""
            SELECT CAST(NgayLap AS DATE) AS Ngay,
                   ISNULL(SUM(CASE WHEN {SqlTrangThaiHoanThanhInClause("TrangThai")} THEN ThanhTien ELSE 0 END), 0) AS DoanhThu,
                   ISNULL(SUM(CASE WHEN {SqlTrangThaiHoanThanhInClause("TrangThai")} THEN 1 ELSE 0 END), 0) AS SoHD
            FROM HoaDon
            WHERE NgayLap >= @Tu AND NgayLap < @Den
            GROUP BY CAST(NgayLap AS DATE);
            """;

        var map = new Dictionary<DateTime, (decimal, int)>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Tu", tuNgay);
        cmd.Parameters.AddWithValue("@Den", denNgay);
        AddTrangThaiHoanThanhParameters(cmd);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            var ngay = rd.GetDateTime("Ngay").Date;
            map[ngay] = (rd.GetDecimal("DoanhThu"), rd.GetInt32("SoHD"));
        }

        return map;
    }

    public IReadOnlyList<DashboardPhanBoTrangThaiDTO> LayPhanBoTrangThaiHoaDonTrongKhoang(
        DateTime tuNgay, DateTime denNgay)
    {
        const string sql = """
            SELECT TrangThai,
                   COUNT(*) AS SoLuong,
                   ISNULL(SUM(ThanhTien), 0) AS TongTien
            FROM HoaDon
            WHERE NgayLap >= @Tu AND NgayLap < @Den
            GROUP BY TrangThai
            ORDER BY SoLuong DESC;
            """;

        var list = new List<DashboardPhanBoTrangThaiDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Tu", tuNgay);
        cmd.Parameters.AddWithValue("@Den", denNgay);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DashboardPhanBoTrangThaiDTO
            {
                TrangThai = rd.GetString("TrangThai"),
                SoLuong = rd.GetInt32("SoLuong"),
                TongThanhTien = rd.GetDecimal("TongTien")
            });
        }

        return list;
    }

    public IReadOnlyList<DashboardHoaDonGanDayDTO> LayHoaDonTomTatGanDay(int top)
    {
        var sql = $"""
            SELECT TOP ({top})
                MaHoaDon,
                ISNULL(NULLIF(LTRIM(RTRIM(TenKhachHang)), N''), N'Khách lẻ') AS TenKhachHang,
                NgayLap,
                ThanhTien,
                TrangThai
            FROM HoaDon
            ORDER BY NgayLap DESC;
            """;

        var list = new List<DashboardHoaDonGanDayDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DashboardHoaDonGanDayDTO
            {
                MaHoaDon = rd.GetInt32("MaHoaDon"),
                TenKhachHang = rd.GetString("TenKhachHang"),
                NgayLap = rd.GetDateTime("NgayLap"),
                ThanhTien = rd.GetDecimal("ThanhTien"),
                TrangThai = rd.GetString("TrangThai")
            });
        }

        return list;
    }
}
