using Microsoft.Data.SqlClient;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class HoaDonRepositoryDAL
{
    private readonly DbContextDAL _db;

    public HoaDonRepositoryDAL(DbContextDAL db) => _db = db;

    /// <summary>Tạo hóa đơn nhiều dòng trong một transaction (trigger FEFO trên ChiTietHoaDon).</summary>
    public int TaoDonHang(
        int maNhanVien,
        string? cccd,
        decimal giamGia,
        string hinhThucThanhToan,
        IReadOnlyList<DonHangGioHangDTO> chiTiet)
    {
        if (chiTiet.Count == 0)
            throw new InvalidOperationException("Giỏ hàng trống.");

        using var cn = _db.CreateConnection();
        cn.Open();
        using var tx = cn.BeginTransaction();

        try
        {
            int maHoaDon;
            using (var cmdHd = new SqlCommand("""
                INSERT INTO HoaDon (MaNhanVien, CCCD, TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai)
                VALUES (@MaNhanVien, @CCCD, 0, @GiamGia, 0, @HinhThucThanhToan, N'Hoàn thành');
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """, cn, tx))
            {
                cmdHd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                cmdHd.Parameters.AddWithValue("@CCCD", (object?)cccd ?? DBNull.Value);
                cmdHd.Parameters.AddWithValue("@GiamGia", giamGia);
                cmdHd.Parameters.AddWithValue("@HinhThucThanhToan", hinhThucThanhToan);
                maHoaDon = Convert.ToInt32(cmdHd.ExecuteScalar(), null);
            }

            foreach (var line in chiTiet)
            {
                using var cmdCt = new SqlCommand("""
                    INSERT INTO ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
                    VALUES (@MaHoaDon, @MaThuoc, @SoLuongBan, @DonGiaBan);
                    """, cn, tx);
                cmdCt.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                cmdCt.Parameters.AddWithValue("@MaThuoc", line.MaThuoc);
                cmdCt.Parameters.AddWithValue("@SoLuongBan", line.SoLuong);
                cmdCt.Parameters.AddWithValue("@DonGiaBan", line.DonGia);
                cmdCt.ExecuteNonQuery();
            }

            decimal tongTien;
            decimal thanhTien;
            using (var cmdRead = new SqlCommand("""
                SELECT TongTien, ThanhTien FROM HoaDon WHERE MaHoaDon = @MaHoaDon;
                """, cn, tx))
            {
                cmdRead.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                using var rd = cmdRead.ExecuteReader();
                if (!rd.Read())
                    throw new InvalidOperationException("Không đọc được hóa đơn vừa tạo.");
                tongTien = rd.GetDecimal("TongTien");
                thanhTien = rd.GetDecimal("ThanhTien");
            }

            if (thanhTien < 0)
                throw new InvalidOperationException("Giảm giá không được lớn hơn tổng tiền hàng.");

            using (var cmdAudit = new SqlCommand("""
                INSERT INTO AuditLog (MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung)
                VALUES (@MaNhanVien, N'Kê đơn bán thuốc', N'HoaDon', @MaBanGhi, @NoiDung);
                """, cn, tx))
            {
                cmdAudit.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                cmdAudit.Parameters.AddWithValue("@MaBanGhi", maHoaDon.ToString());
                cmdAudit.Parameters.AddWithValue("@NoiDung",
                    $"Hóa đơn #{maHoaDon} | {chiTiet.Count} dòng | Tổng {tongTien:N0} | Thanh toán {thanhTien:N0}");
                cmdAudit.ExecuteNonQuery();
            }

            tx.Commit();
            return maHoaDon;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>Gọi sp_BanThuoc — một dòng chi tiết / một hóa đơn theo script SQL hiện tại.</summary>
    public int BanThuoc(
        int maNhanVien,
        int maThuoc,
        int soLuongBan,
        string? cccd,
        decimal giamGia,
        string? hinhThucThanhToan)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand("sp_BanThuoc", cn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
        cmd.Parameters.AddWithValue("@MaThuoc", maThuoc);
        cmd.Parameters.AddWithValue("@SoLuongBan", soLuongBan);
        cmd.Parameters.AddWithValue("@CCCD", (object?)cccd ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GiamGia", giamGia);
        cmd.Parameters.AddWithValue("@HinhThucThanhToan", (object?)hinhThucThanhToan ?? DBNull.Value);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            throw new InvalidOperationException("sp_BanThuoc không trả về MaHoaDon.");

        return rd.GetInt32("MaHoaDon");
    }
}
