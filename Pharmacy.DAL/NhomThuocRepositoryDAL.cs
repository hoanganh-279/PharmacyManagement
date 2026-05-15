using Microsoft.Data.SqlClient;
using Pharmacy.DTO;

namespace Pharmacy.DAL;

public class NhomThuocRepositoryDAL
{
    private readonly DbContextDAL _db;

    public NhomThuocRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<NhomThuocDTO> LayTatCa(bool chiHoatDong = true)
    {
        var sql = chiHoatDong
            ? "SELECT MaNhomThuoc, TenNhomThuoc, MoTa, TrangThai FROM NhomThuoc WHERE TrangThai = 1 ORDER BY TenNhomThuoc;"
            : "SELECT MaNhomThuoc, TenNhomThuoc, MoTa, TrangThai FROM NhomThuoc ORDER BY TenNhomThuoc;";

        var list = new List<NhomThuocDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new NhomThuocDTO
            {
                MaNhomThuoc = rd.GetInt32("MaNhomThuoc"),
                TenNhomThuoc = rd.GetString("TenNhomThuoc"),
                MoTa = rd.GetNullableString("MoTa"),
                TrangThai = rd.GetBoolean("TrangThai")
            });
        }

        return list;
    }

    public int Them(NhomThuocDTO dto)
    {
        const string sql = """
            INSERT INTO NhomThuoc(TenNhomThuoc, MoTa, TrangThai)
            VALUES (@TenNhomThuoc, @MoTa, @TrangThai);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@TenNhomThuoc", dto.TenNhomThuoc);
        cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", dto.TrangThai);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    public void CapNhat(NhomThuocDTO dto)
    {
        const string sql = """
            UPDATE NhomThuoc SET TenNhomThuoc=@TenNhomThuoc, MoTa=@MoTa, TrangThai=@TrangThai
            WHERE MaNhomThuoc=@MaNhomThuoc;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaNhomThuoc", dto.MaNhomThuoc);
        cmd.Parameters.AddWithValue("@TenNhomThuoc", dto.TenNhomThuoc);
        cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", dto.TrangThai);
        cn.Open();
        cmd.ExecuteNonQuery();
    }
}
