using Microsoft.Data.SqlClient;
using Pharmacy.DTO;

namespace Pharmacy.DAL;

public class VaiTroRepositoryDAL
{
    private readonly DbContextDAL _db;

    public VaiTroRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<VaiTroDTO> LayTatCa()
    {
        const string sql = "SELECT MaVaiTro, TenVaiTro, MoTa, TrangThai FROM VaiTro WHERE TrangThai = 1 ORDER BY MaVaiTro;";
        var list = new List<VaiTroDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new VaiTroDTO
            {
                MaVaiTro = rd.GetInt32("MaVaiTro"),
                TenVaiTro = rd.GetString("TenVaiTro"),
                MoTa = rd.GetNullableString("MoTa"),
                TrangThai = rd.GetBoolean("TrangThai")
            });
        }

        return list;
    }
}
