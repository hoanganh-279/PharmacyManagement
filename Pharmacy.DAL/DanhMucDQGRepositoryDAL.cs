using Microsoft.Data.SqlClient;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

/// <summary>
/// Repository cho danh mục Dược Quốc Gia (DQG). Đọc trực tiếp bảng DanhMucDQG khi cần
/// thông tin gốc, và đọc view vw_TraCuuDanhMucDQG khi tra cứu để gắn vào thuốc.
/// </summary>
public class DanhMucDQGRepositoryDAL
{
    private readonly DbContextDAL _db;

    public DanhMucDQGRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<TraCuuDanhMucDQGViewDTO> TraCuu(string? tuKhoa = null, int top = 200)
    {
        var hasKeyword = !string.IsNullOrWhiteSpace(tuKhoa);
        var sql = hasKeyword
            ? $"""
                SELECT TOP ({top}) *
                FROM vw_TraCuuDanhMucDQG
                WHERE TenHangHoa LIKE @kw
                   OR ISNULL(MaDQGDonVi, N'') LIKE @kw
                   OR ISNULL(SoDangKy, N'') LIKE @kw
                   OR ISNULL(HoatChatChinh, N'') LIKE @kw
                ORDER BY TenHangHoa;
              """
            : $"SELECT TOP ({top}) * FROM vw_TraCuuDanhMucDQG ORDER BY TenHangHoa;";

        var list = new List<TraCuuDanhMucDQGViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        if (hasKeyword)
            cmd.Parameters.AddWithValue("@kw", "%" + tuKhoa!.Trim() + "%");

        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new TraCuuDanhMucDQGViewDTO
            {
                MaDQG = rd.GetInt32("MaDQG"),
                MaDQGDonVi = rd.GetNullableString("MaDQGDonVi"),
                TenHangHoa = rd.GetString("TenHangHoa"),
                SoDangKy = rd.GetNullableString("SoDangKy"),
                HoatChatChinh = rd.GetNullableString("HoatChatChinh"),
                HoatChatDangKy = rd.GetNullableString("HoatChatDangKy"),
                HamLuong = rd.GetNullableString("HamLuong"),
                DongGoi = rd.GetNullableString("DongGoi"),
                HangSanXuat = rd.GetNullableString("HangSanXuat"),
                NuocSanXuat = rd.GetNullableString("NuocSanXuat"),
                DonViTinh = rd.GetNullableString("DonViTinh"),
                TrangThaiNhapKho = rd.GetString("TrangThaiNhapKho"),
                MaThuoc = rd.GetNullableInt32("MaThuoc"),
                SoLuongTon = rd.GetNullableInt32("SoLuongTon")
            });
        }

        return list;
    }

    public DanhMucDQGDTO? LayTheoMa(int maDQG)
    {
        const string sql = """
            SELECT MaDQG, MaDQGDonVi, TenHangHoa, SoDangKy, HoatChatChinh, HoatChatDangKy,
                   HamLuong, DongGoi, HangSanXuat, NuocSanXuat, DonViTinh, ISNULL(TrangThai, 1) AS TrangThai
            FROM DanhMucDQG WHERE MaDQG = @MaDQG;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaDQG", maDQG);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return null;

        return new DanhMucDQGDTO
        {
            MaDQG = rd.GetInt32("MaDQG"),
            MaDQGDonVi = rd.GetNullableString("MaDQGDonVi"),
            TenHangHoa = rd.GetString("TenHangHoa"),
            SoDangKy = rd.GetNullableString("SoDangKy"),
            HoatChatChinh = rd.GetNullableString("HoatChatChinh"),
            HoatChatDangKy = rd.GetNullableString("HoatChatDangKy"),
            HamLuong = rd.GetNullableString("HamLuong"),
            DongGoi = rd.GetNullableString("DongGoi"),
            HangSanXuat = rd.GetNullableString("HangSanXuat"),
            NuocSanXuat = rd.GetNullableString("NuocSanXuat"),
            DonViTinh = rd.GetNullableString("DonViTinh"),
            TrangThai = rd.GetBoolean("TrangThai")
        };
    }
}
