using Microsoft.Data.SqlClient;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class ThuocRepositoryDAL
{
    private readonly DbContextDAL _db;

    public ThuocRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<DanhSachThuocViewDTO> LayTuViewDanhSach()
    {
        const string sql = "SELECT * FROM vw_DanhSachThuoc ORDER BY TenThuoc;";
        var list = new List<DanhSachThuocViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DanhSachThuocViewDTO
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
            });
        }

        return list;
    }

    public ThuocDTO? LayTheoMa(int maThuoc)
    {
        const string sql = """
            SELECT MaThuoc, TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, SoLuongTon, TonToiThieu,
                   HanSuDung, MaNhomThuoc, TrangThai, NgayTao
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

    public int Them(ThuocDTO t)
    {
        const string sql = """
            INSERT INTO Thuoc(TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, SoLuongTon, TonToiThieu, HanSuDung, MaNhomThuoc, TrangThai)
            OUTPUT INSERTED.MaThuoc
            VALUES (@TenThuoc, @HoatChat, @HamLuong, @DonViTinh, @GiaNhap, @GiaBan, @SoLuongTon, @TonToiThieu, @HanSuDung, @MaNhomThuoc, @TrangThai);
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
                GiaNhap=@GiaNhap, GiaBan=@GiaBan, TonToiThieu=@TonToiThieu, HanSuDung=@HanSuDung, MaNhomThuoc=@MaNhomThuoc, TrangThai=@TrangThai
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
        TrangThai = rd.GetBoolean("TrangThai"),
        NgayTao = rd.GetDateTime("NgayTao")
    };
}
