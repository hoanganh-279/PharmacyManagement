using Microsoft.Data.SqlClient;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class DanhMucDqgRepositoryDAL
{
    private readonly DbContextDAL _db;

    public DanhMucDqgRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<TraCuuDqgViewDTO> TraCuu(string? tuKhoa, int top = 50)
    {
        const string sql = """
            SELECT TOP (@Top) *
            FROM vw_TraCuuDanhMucDQG
            WHERE (@TuKhoa IS NULL OR LTRIM(RTRIM(@TuKhoa)) = N''
                OR TenHangHoa LIKE N'%' + @TuKhoa + N'%'
                OR SoDangKy LIKE N'%' + @TuKhoa + N'%'
                OR HoatChatChinh LIKE N'%' + @TuKhoa + N'%'
                OR MaDQGDonVi LIKE N'%' + @TuKhoa + N'%')
            ORDER BY TenHangHoa;
            """;

        var list = new List<TraCuuDqgViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Top", top);
        cmd.Parameters.AddWithValue("@TuKhoa", string.IsNullOrWhiteSpace(tuKhoa) ? DBNull.Value : tuKhoa.Trim());
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            list.Add(Map(rd));

        return list;
    }

    public TraCuuDqgViewDTO? LayTheoMa(int maDqg)
    {
        const string sql = "SELECT * FROM vw_TraCuuDanhMucDQG WHERE MaDQG = @MaDQG;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaDQG", maDqg);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? Map(rd) : null;
    }

    private static TraCuuDqgViewDTO Map(SqlDataReader rd) => new()
    {
        MaDQG = rd.GetInt32("MaDQG"),
        MaDQGDonVi = rd.GetNullableString("MaDQGDonVi"),
        TenHangHoa = rd.GetString("TenHangHoa"),
        SoDangKy = rd.GetNullableString("SoDangKy"),
        HoatChatChinh = rd.GetNullableString("HoatChatChinh"),
        HamLuong = rd.GetNullableString("HamLuong"),
        DongGoi = rd.GetNullableString("DongGoi"),
        HangSanXuat = rd.GetNullableString("HangSanXuat"),
        NuocSanXuat = rd.GetNullableString("NuocSanXuat"),
        DonViTinh = rd.GetNullableString("DonViTinh"),
        TrangThaiNhapKho = rd.GetString("TrangThaiNhapKho"),
        MaThuoc = rd.GetNullableInt32("MaThuoc"),
        SoLuongTon = rd.GetNullableInt32("SoLuongTon")
    };
}
