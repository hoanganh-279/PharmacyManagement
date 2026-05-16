using Microsoft.Data.SqlClient;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class ThuocRepositoryDAL
{
    private readonly DbContextDAL _db;

    public ThuocRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<DanhSachThuocViewDTO> TimKiemTuView(string? tuKhoa)
    {
        const string sql = """
            SELECT * FROM vw_DanhSachThuoc
            WHERE (@TuKhoa IS NULL OR LTRIM(RTRIM(@TuKhoa)) = N''
                OR TenThuoc LIKE N'%' + @TuKhoa + N'%'
                OR HoatChat LIKE N'%' + @TuKhoa + N'%'
                OR HamLuong LIKE N'%' + @TuKhoa + N'%'
                OR DonViTinh LIKE N'%' + @TuKhoa + N'%'
                OR TenNhomThuoc LIKE N'%' + @TuKhoa + N'%'
                OR SoDangKy LIKE N'%' + @TuKhoa + N'%'
                OR MaDQGDonVi LIKE N'%' + @TuKhoa + N'%'
                OR CAST(MaThuoc AS NVARCHAR(20)) = @TuKhoa)
            ORDER BY TenThuoc;
            """;

        var list = new List<DanhSachThuocViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@TuKhoa", string.IsNullOrWhiteSpace(tuKhoa) ? DBNull.Value : tuKhoa.Trim());
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(MapDanhSachThuoc(rd));

        return list;
    }

    public IReadOnlyList<DanhSachThuocViewDTO> LayTuViewDanhSach()
    {
        return TimKiemTuView(null);
    }

    /// <summary>Thuốc còn tồn theo lô hợp lệ (còn hạn) — dùng màn kê đơn bán.</summary>
    public IReadOnlyList<ThuocKeDonViewDTO> TimKiemThuocKeDon(string? tuKhoa)
    {
        const string sql = """
            SELECT
                v.MaThuoc,
                v.TenThuoc,
                v.HoatChat,
                v.HamLuong,
                v.DonViTinh,
                v.GiaBan,
                v.HanSuDung,
                v.TrangThai,
                ISNULL(lo.TonLoConHan, 0) AS TonLoConHan
            FROM vw_DanhSachThuoc v
            OUTER APPLY (
                SELECT SUM(lt.SoLuongTon) AS TonLoConHan
                FROM LoThuoc lt
                WHERE lt.MaThuoc = v.MaThuoc
                  AND lt.SoLuongTon > 0
                  AND lt.HanSuDung >= CAST(GETDATE() AS DATE)
                  AND ISNULL(lt.TrangThai, 1) = 1
            ) lo
            WHERE v.TrangThai <> N'Ngừng bán'
              AND ISNULL(lo.TonLoConHan, 0) > 0
              AND (@TuKhoa IS NULL OR LTRIM(RTRIM(@TuKhoa)) = N''
                OR v.TenThuoc LIKE N'%' + @TuKhoa + N'%'
                OR v.HoatChat LIKE N'%' + @TuKhoa + N'%'
                OR v.HamLuong LIKE N'%' + @TuKhoa + N'%'
                OR v.DonViTinh LIKE N'%' + @TuKhoa + N'%')
            ORDER BY v.TenThuoc;
            """;

        var list = new List<ThuocKeDonViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@TuKhoa", string.IsNullOrWhiteSpace(tuKhoa) ? DBNull.Value : tuKhoa.Trim());
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThuocKeDonViewDTO
            {
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                HoatChat = rd.GetNullableString("HoatChat"),
                HamLuong = rd.GetNullableString("HamLuong"),
                DonViTinh = rd.GetString("DonViTinh"),
                GiaBan = rd.GetDecimal("GiaBan"),
                HanSuDung = rd.GetNullableDateTime("HanSuDung"),
                TonLoConHan = rd.GetInt32("TonLoConHan")
            });
        }

        return list;
    }

    private static DanhSachThuocViewDTO MapDanhSachThuoc(SqlDataReader rd) => new()
    {
        MaThuoc = rd.GetInt32("MaThuoc"),
        TenThuoc = rd.GetString("TenThuoc"),
        HoatChat = rd.GetNullableString("HoatChat"),
        HamLuong = rd.GetNullableString("HamLuong"),
        DonViTinh = rd.GetString("DonViTinh"),
        TenNhomThuoc = rd.GetString("TenNhomThuoc"),
        GiaNhap = rd.GetDecimal("GiaNhap"),
        GiaBan = rd.GetDecimal("GiaBan"),
        SoLuongTon = rd.GetInt32("SoLuongTon"),
        TonToiThieu = rd.GetInt32("TonToiThieu"),
        HanSuDung = rd.GetNullableDateTime("HanSuDung"),
        TrangThai = rd.GetString("TrangThai")
    };

    public ThuocDTO? LayTheoMa(int maThuoc)
    {
        const string sql = """
            SELECT MaThuoc, TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, SoLuongTon, TonToiThieu,
                   HanSuDung, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi, TrangThai, NgayTao
            FROM Thuoc WHERE MaThuoc = @MaThuoc;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return null;

        return MapThuoc(rd);
    }

    public bool TonTaiTheoTen(string tenThuoc, int? maDQG, int? loaiTruMa = null)
    {
        const string sql = """
            SELECT TOP 1 1
            FROM Thuoc
            WHERE (LOWER(LTRIM(RTRIM(TenThuoc))) = LOWER(LTRIM(RTRIM(@TenThuoc)))
                   OR (@MaDQG IS NOT NULL AND MaDQG = @MaDQG))
              AND (@LoaiTru IS NULL OR MaThuoc <> @LoaiTru);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@TenThuoc", tenThuoc);
        cmd.Parameters.AddWithValue("@MaDQG", (object?)maDQG ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LoaiTru", (object?)loaiTruMa ?? DBNull.Value);
        cn.Open();
        var result = cmd.ExecuteScalar();
        return result is not null && result is not DBNull;
    }

    public int Them(ThuocDTO t)
    {
        const string sql = """
            INSERT INTO Thuoc(TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, SoLuongTon, TonToiThieu,
                              HanSuDung, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi, TrangThai)
            VALUES (@TenThuoc, @HoatChat, @HamLuong, @DonViTinh, @GiaNhap, @GiaBan, @SoLuongTon, @TonToiThieu,
                    @HanSuDung, @MaNhomThuoc, @MaDQG, @SoDangKy, @HangSanXuat, @NuocSanXuat, @DongGoi, @TrangThai);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        AddThuocParams(cmd, t);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    public void CapNhat(ThuocDTO t)
    {
        const string sql = """
            UPDATE Thuoc SET TenThuoc=@TenThuoc, HoatChat=@HoatChat, HamLuong=@HamLuong, DonViTinh=@DonViTinh,
                GiaNhap=@GiaNhap, GiaBan=@GiaBan, TonToiThieu=@TonToiThieu, HanSuDung=@HanSuDung,
                MaNhomThuoc=@MaNhomThuoc, MaDQG=@MaDQG, SoDangKy=@SoDangKy, HangSanXuat=@HangSanXuat,
                NuocSanXuat=@NuocSanXuat, DongGoi=@DongGoi, TrangThai=@TrangThai
            WHERE MaThuoc=@MaThuoc;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaThuoc", t.MaThuoc);
        AddThuocParams(cmd, t);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void DatTrangThai(int maThuoc, bool trangThai)
    {
        const string sql = "UPDATE Thuoc SET TrangThai=@TrangThai WHERE MaThuoc=@MaThuoc;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private static void AddThuocParams(SqlCommand cmd, ThuocDTO t)
    {
        cmd.Parameters.AddWithValue("@TenThuoc", t.TenThuoc);
        cmd.Parameters.AddWithValue("@HoatChat", (object?)t.HoatChat ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HamLuong", (object?)t.HamLuong ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonViTinh", t.DonViTinh);
        cmd.Parameters.AddWithValue("@GiaNhap", t.GiaNhap);
        cmd.Parameters.AddWithValue("@GiaBan", t.GiaBan);
        cmd.Parameters.AddWithValue("@SoLuongTon", t.SoLuongTon);
        cmd.Parameters.AddWithValue("@TonToiThieu", t.TonToiThieu);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)t.HanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaNhomThuoc", t.MaNhomThuoc);
        cmd.Parameters.AddWithValue("@MaDQG", (object?)t.MaDQG ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoDangKy", (object?)t.SoDangKy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HangSanXuat", (object?)t.HangSanXuat ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NuocSanXuat", (object?)t.NuocSanXuat ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DongGoi", (object?)t.DongGoi ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", t.TrangThai);
    }

    private static ThuocDTO MapThuoc(SqlDataReader rd) => new()
    {
        MaThuoc = rd.GetInt32("MaThuoc"),
        TenThuoc = rd.GetString("TenThuoc"),
        HoatChat = rd.GetNullableString("HoatChat"),
        HamLuong = rd.GetNullableString("HamLuong"),
        DonViTinh = rd.GetString("DonViTinh"),
        GiaNhap = rd.GetDecimal("GiaNhap"),
        GiaBan = rd.GetDecimal("GiaBan"),
        SoLuongTon = rd.GetInt32("SoLuongTon"),
        TonToiThieu = rd.GetInt32("TonToiThieu"),
        HanSuDung = rd.GetNullableDateTime("HanSuDung"),
        MaNhomThuoc = rd.GetInt32("MaNhomThuoc"),
        MaDQG = rd.GetNullableInt32("MaDQG"),
        SoDangKy = rd.GetNullableString("SoDangKy"),
        HangSanXuat = rd.GetNullableString("HangSanXuat"),
        NuocSanXuat = rd.GetNullableString("NuocSanXuat"),
        DongGoi = rd.GetNullableString("DongGoi"),
        TrangThai = rd.GetBoolean("TrangThai"),
        NgayTao = rd.GetDateTime("NgayTao")
    };
}
