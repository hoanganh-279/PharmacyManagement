using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL;

public class HoaDonRepositoryDAL
{
    private readonly DbContextDAL _db;

    public HoaDonRepositoryDAL(DbContextDAL db) => _db = db;

    /// <summary>Gọi sp_BanThuoc — một dòng chi tiết / một hóa đơn theo script SQL hiện tại.</summary>
    public int BanThuoc(
        int maNhanVien,
        int maThuoc,
        int soLuongBan,
        string? tenKhachHang,
        string? soDienThoai,
        decimal giamGia,
        string? hinhThucThanhToan)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand("sp_BanThuoc", cn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        cmd.Parameters.AddWithValue("@SoLuongBan", soLuongBan);
        cmd.Parameters.AddWithValue("@TenKhachHang", (object?)tenKhachHang ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)soDienThoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GiamGia", giamGia);
        cmd.Parameters.AddWithValue("@HinhThucThanhToan", (object?)hinhThucThanhToan ?? DBNull.Value);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            throw new InvalidOperationException("sp_BanThuoc không trả về MaHoaDon.");

        return rd.GetInt32("MaHoaDon");
    }
}
