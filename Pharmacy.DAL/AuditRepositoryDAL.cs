using Microsoft.Data.SqlClient;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class AuditRepositoryDAL
{
    private readonly DbContextDAL _db;

    public AuditRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<AuditLogChiTietViewDTO> LayTuView(int top = 500)
    {
        var sql = $"""
            SELECT TOP ({top}) MaLog, ThoiGian, MaNhanVien, NhanVien, HanhDong, TenBang, MaBanGhi, NoiDung, DiaChiMay
            FROM vw_AuditLogChiTiet
            ORDER BY ThoiGian DESC;
            """;

        var list = new List<AuditLogChiTietViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(DocMotDong(rd));

        return list;
    }

    public long DemTheoBoLoc(AuditLogTimKiemThamSo th)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(BuildSqlDem(), cn);
        GanThamSoLoc(cmd, th);
        cn.Open();
        var o = cmd.ExecuteScalar();
        return o is null or DBNull ? 0L : Convert.ToInt64(o, null);
    }

    public IReadOnlyList<AuditLogChiTietViewDTO> TimPhanTrang(AuditLogTimKiemThamSo th)
    {
        var skip = Math.Max(0, (th.Trang - 1) * th.KichThuocTrang);
        var take = Math.Clamp(th.KichThuocTrang, 1, 500);

        var sql = $"""
            {BuildSqlSelectCoWhere()}
            ORDER BY v.ThoiGian DESC
            OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY;
            """;

        var list = new List<AuditLogChiTietViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        GanThamSoLoc(cmd, th);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(DocMotDong(rd));

        return list;
    }

    /// <summary>Xuất / xem nhanh — tối đa <paramref name="gioiHan"/> dòng, cùng bộ lọc.</summary>
    public IReadOnlyList<AuditLogChiTietViewDTO> LayTheoBoLocToiDa(AuditLogTimKiemThamSo th, int gioiHan = 10_000)
    {
        gioiHan = Math.Clamp(gioiHan, 1, 50_000);
        var sql = $"""
            SELECT TOP ({gioiHan})
                v.MaLog, v.ThoiGian, v.MaNhanVien, v.NhanVien, v.HanhDong, v.TenBang, v.MaBanGhi, v.NoiDung, v.DiaChiMay
            {BuildSqlFromWhere()}
            ORDER BY v.ThoiGian DESC;
            """;

        var list = new List<AuditLogChiTietViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        GanThamSoLoc(cmd, th);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(DocMotDong(rd));

        return list;
    }

    public IReadOnlyList<string> LayDanhSachHanhDongPhanBiet()
    {
        const string sql = """
            SELECT DISTINCT HanhDong
            FROM dbo.AuditLog
            ORDER BY HanhDong;
            """;

        var list = new List<string>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(rd.GetString("HanhDong"));

        return list;
    }

    public IReadOnlyList<AuditLogNguoiTomTatDTO> LayNguoiCoTrongNhatKy()
    {
        const string sql = """
            SELECT DISTINCT al.MaNhanVien, nv.HoTen
            FROM dbo.AuditLog al
            INNER JOIN dbo.NhanVien nv ON al.MaNhanVien = nv.MaNhanVien
            ORDER BY nv.HoTen;
            """;

        var list = new List<AuditLogNguoiTomTatDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new AuditLogNguoiTomTatDTO
            {
                MaNhanVien = rd.GetInt32("MaNhanVien"),
                HoTen = rd.GetString("HoTen")
            });
        }

        return list;
    }

    public AuditLogThongKeManHinhDTO LayThongKeManHinh(AuditLogTimKiemThamSo boLocGiongLuoi)
    {
        decimal dungLuongMb = 0;
        long tongToanCuc = 0;
        using (var cn = _db.CreateConnection())
        {
            cn.Open();
            using (var cmd = new SqlCommand("""
                SELECT CAST(SUM(a.total_pages) * 8.0 / 1024.0 AS DECIMAL(18, 2))
                FROM sys.tables t
                JOIN sys.indexes i ON t.object_id = i.object_id
                JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
                JOIN sys.allocation_units a ON p.partition_id = a.container_id
                WHERE t.name = N'AuditLog' AND i.index_id <= 1;
                """, cn))
            {
                var o = cmd.ExecuteScalar();
                if (o is not null and not DBNull)
                    dungLuongMb = Convert.ToDecimal(o, null);
            }

            using (var cmd = new SqlCommand("SELECT COUNT_BIG(*) FROM dbo.AuditLog;", cn))
            {
                var o = cmd.ExecuteScalar();
                if (o is not null and not DBNull)
                    tongToanCuc = Convert.ToInt64(o, null);
            }
        }

        var canhBao24h = DemNhayCam24Gio();

        var tongTrongLoc = DemTheoBoLoc(boLocGiongLuoi);
        long nhayCamTrongLoc = 0;
        if (tongTrongLoc > 0)
            nhayCamTrongLoc = DemTheoBoLocVaNhayCam(boLocGiongLuoi);

        var tyLe = tongTrongLoc <= 0
            ? 0m
            : Math.Round(nhayCamTrongLoc * 100m / tongTrongLoc, 1);

        return new AuditLogThongKeManHinhDTO
        {
            DungLuongBangMb = dungLuongMb,
            TongBanGhiToanCuc = tongToanCuc,
            CanhBaoNhayCam24h = canhBao24h,
            TyLeThaoTacNhayCamTrongBoLoc = tyLe
        };
    }

    private long DemNhayCam24Gio()
    {
        const string sql = """
            SELECT COUNT_BIG(*)
            FROM dbo.AuditLog
            WHERE ThoiGian >= DATEADD(HOUR, -24, GETDATE())
              AND (
                    HanhDong IN (N'Xóa mềm thuốc', N'Cập nhật giá thuốc')
                    OR TenBang IN (N'NhanVien', N'VaiTro')
                  );
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        var o = cmd.ExecuteScalar();
        return o is null or DBNull ? 0L : Convert.ToInt64(o, null);
    }

    private long DemTheoBoLocVaNhayCam(AuditLogTimKiemThamSo th)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(BuildSqlDem() + """

              AND (
                    v.HanhDong IN (N'Xóa mềm thuốc', N'Cập nhật giá thuốc')
                    OR v.HanhDong LIKE N'%Xóa%'
                  );
            """, cn);
        GanThamSoLoc(cmd, th);
        cn.Open();
        var o = cmd.ExecuteScalar();
        return o is null or DBNull ? 0L : Convert.ToInt64(o, null);
    }

    private static string BuildSqlFromWhere() => """

            FROM vw_AuditLogChiTiet v
            WHERE (@TuNgay IS NULL OR v.ThoiGian >= @TuNgay)
              AND (@DenNgay IS NULL OR v.ThoiGian < DATEADD(DAY, 1, CAST(@DenNgay AS DATE)))
              AND (@MaNV IS NULL OR v.MaNhanVien = @MaNV)
              AND (@HanhDong IS NULL OR @HanhDong = N'' OR v.HanhDong = @HanhDong)
              AND (
                    @TuKhoa IS NULL
                    OR LEN(LTRIM(RTRIM(@TuKhoa))) = 0
                    OR CHARINDEX(@TuKhoa, ISNULL(v.NoiDung, N'')) > 0
                    OR CHARINDEX(@TuKhoa, ISNULL(v.MaBanGhi, N'')) > 0
                    OR CHARINDEX(@TuKhoa, ISNULL(v.TenBang, N'')) > 0
                    OR CHARINDEX(@TuKhoa, ISNULL(v.NhanVien, N'')) > 0
                    OR CHARINDEX(@TuKhoa, ISNULL(v.HanhDong, N'')) > 0
                  )
            """;

    private static string BuildSqlDem() => """
            SELECT COUNT_BIG(*)
            """ + BuildSqlFromWhere();

    private static string BuildSqlSelectCoWhere() => """
            SELECT
                v.MaLog,
                v.ThoiGian,
                v.MaNhanVien,
                v.NhanVien,
                v.HanhDong,
                v.TenBang,
                v.MaBanGhi,
                v.NoiDung,
                v.DiaChiMay
            """ + BuildSqlFromWhere();

    private static void GanThamSoLoc(SqlCommand cmd, AuditLogTimKiemThamSo th)
    {
        cmd.Parameters.Add("@TuNgay", System.Data.SqlDbType.DateTime2);
        cmd.Parameters["@TuNgay"].Value = th.TuNgay.HasValue ? th.TuNgay.Value : DBNull.Value;

        cmd.Parameters.Add("@DenNgay", System.Data.SqlDbType.Date);
        cmd.Parameters["@DenNgay"].Value = th.DenNgay.HasValue ? th.DenNgay.Value.Date : DBNull.Value;

        cmd.Parameters.Add("@MaNV", System.Data.SqlDbType.Int);
        cmd.Parameters["@MaNV"].Value = th.MaNhanVien.HasValue ? th.MaNhanVien.Value : DBNull.Value;

        cmd.Parameters.Add("@HanhDong", System.Data.SqlDbType.NVarChar, 100);
        cmd.Parameters["@HanhDong"].Value =
            string.IsNullOrWhiteSpace(th.HanhDong) ? DBNull.Value : th.HanhDong.Trim();

        cmd.Parameters.Add("@TuKhoa", System.Data.SqlDbType.NVarChar, 200);
        cmd.Parameters["@TuKhoa"].Value =
            string.IsNullOrWhiteSpace(th.TuKhoa) ? DBNull.Value : th.TuKhoa.Trim();
    }

    private static AuditLogChiTietViewDTO DocMotDong(SqlDataReader rd) => new()
    {
        MaLog = rd.GetInt32("MaLog"),
        ThoiGian = rd.GetDateTime("ThoiGian"),
        MaNhanVien = rd.GetNullableInt32("MaNhanVien"),
        NhanVien = rd.GetNullableString("NhanVien"),
        HanhDong = rd.GetString("HanhDong"),
        TenBang = rd.GetNullableString("TenBang"),
        MaBanGhi = rd.GetNullableString("MaBanGhi"),
        NoiDung = rd.GetNullableString("NoiDung"),
        DiaChiMay = rd.GetNullableString("DiaChiMay")
    };

    public IReadOnlyList<AuditLogDTO> LayBangGoc(int top = 500)
    {
        var sql = $"""
            SELECT TOP ({top}) MaLog, ThoiGian, MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung, DiaChiMay
            FROM AuditLog
            ORDER BY ThoiGian DESC;
            """;

        var list = new List<AuditLogDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new AuditLogDTO
            {
                MaLog = rd.GetInt32("MaLog"),
                ThoiGian = rd.GetDateTime("ThoiGian"),
                MaNhanVien = rd.GetNullableInt32("MaNhanVien"),
                HanhDong = rd.GetString("HanhDong"),
                TenBang = rd.GetNullableString("TenBang"),
                MaBanGhi = rd.GetNullableString("MaBanGhi"),
                NoiDung = rd.GetNullableString("NoiDung"),
                DiaChiMay = rd.GetNullableString("DiaChiMay")
            });
        }

        return list;
    }

    public void Ghi(int? maNhanVien, string hanhDong, string? tenBang, string? maBanGhi, string? noiDung, string? diaChiMay = null)
    {
        const string sql = """
            INSERT INTO AuditLog(MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung, DiaChiMay)
            VALUES (@MaNhanVien, @HanhDong, @TenBang, @MaBanGhi, @NoiDung, @DiaChiMay);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaNhanVien", (object?)maNhanVien ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HanhDong", hanhDong);
        cmd.Parameters.AddWithValue("@TenBang", (object?)tenBang ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaBanGhi", (object?)maBanGhi ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NoiDung", (object?)noiDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DiaChiMay", (object?)diaChiMay ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }
}
