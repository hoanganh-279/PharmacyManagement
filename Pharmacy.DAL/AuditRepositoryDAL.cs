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
            SELECT TOP ({top}) MaLog, ThoiGian, NhanVien, HanhDong, TenBang, MaBanGhi, NoiDung, DiaChiMay
            FROM vw_AuditLogChiTiet
            ORDER BY ThoiGian DESC;
            """;

        var list = new List<AuditLogChiTietViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new AuditLogChiTietViewDTO
            {
                MaLog = rd.GetInt32("MaLog"),
                ThoiGian = rd.GetDateTime("ThoiGian"),
                NhanVien = rd.GetNullableString("NhanVien"),
                HanhDong = rd.GetString("HanhDong"),
                TenBang = rd.GetNullableString("TenBang"),
                MaBanGhi = rd.GetNullableString("MaBanGhi"),
                NoiDung = rd.GetNullableString("NoiDung"),
                DiaChiMay = rd.GetNullableString("DiaChiMay")
            });
        }

        return list;
    }

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
