using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL;

public class PhieuNhapRepositoryDAL
{
    private readonly DbContextDAL _db;

    public PhieuNhapRepositoryDAL(DbContextDAL db) => _db = db;

    /// <summary>Gọi sp_NhapKho — transaction và audit nằm trong CSDL.</summary>
    public int NhapKho(
        int maNhanVien,
        int maThuoc,
        int soLuongNhap,
        decimal donGiaNhap,
        DateTime? hanSuDung,
        string? nhaCungCap,
        string? ghiChu)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand("sp_NhapKho", cn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        cmd.Parameters.AddWithValue("@SoLuongNhap", soLuongNhap);
        cmd.Parameters.AddWithValue("@DonGiaNhap", donGiaNhap);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)hanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NhaCungCap", (object?)nhaCungCap ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)ghiChu ?? DBNull.Value);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            throw new InvalidOperationException("sp_NhapKho không trả về MaPhieuNhap.");

        return rd.GetInt32("MaPhieuNhap");
    }
}
