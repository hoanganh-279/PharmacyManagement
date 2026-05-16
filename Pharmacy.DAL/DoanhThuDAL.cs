using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL
{
    public class DoanhThuDAL
    {
        private static readonly string _connStr;

        static DoanhThuDAL()
        {
            _connStr = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .GetConnectionString("PharmacyManagement")!;
        }

        private SqlConnection GetConnection() => new SqlConnection(_connStr);

        public List<DoanhThuDTO> LayDoanhThuTheoKhoangNgay(DateTime tuNgay, DateTime denNgay)
        {
            var result = new List<DoanhThuDTO>();

            const string sql = @"
                SELECT 
                    hd.NgayLap,
                    hd.MaHoaDon,
                    ISNULL(kh.HoTen, N'Khách lẻ') AS TenKhachHang,
                    ISNULL(nv.HoTen, N'') AS TenNhanVien,
                    ISNULL(hd.CCCD, '') AS SoDienThoai,           -- Dùng CCCD tạm thời, sau có thể join lấy SĐT
                    hd.TongTien,
                    ISNULL(hd.GiamGia, 0) AS TienGiam,
                    hd.ThanhTien AS ThanhToan,
                    0 AS LoiNhuan,                                 -- Tạm tính 0, sau sẽ cải tiến
                    hd.TrangThai
                FROM HoaDon hd
                LEFT JOIN KhachHang kh ON kh.CCCD = hd.CCCD
                LEFT JOIN NhanVien nv ON nv.MaNhanVien = hd.MaNhanVien
                WHERE hd.NgayLap BETWEEN @TuNgay AND @DenNgay
                ORDER BY hd.NgayLap DESC";

            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
            cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

            conn.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new DoanhThuDTO
                {
                    MaHoaDon = reader["MaHoaDon"].ToString()!,
                    NgayBan = Convert.ToDateTime(reader["NgayLap"]),
                    TenKhachHang = reader["TenKhachHang"].ToString()!,
                    TenNhanVien = reader["TenNhanVien"].ToString()!,
                    SoDienThoai = reader["SoDienThoai"].ToString() ?? "",
                    TongTien = Convert.ToDecimal(reader["TongTien"]),
                    TienGiam = Convert.ToDecimal(reader["TienGiam"]),
                    ThanhToan = Convert.ToDecimal(reader["ThanhToan"]),
                    LoiNhuan = Convert.ToDecimal(reader["LoiNhuan"]),
                    TrangThai = reader["TrangThai"].ToString()!
                });
            }

            return result;
        }

        public TongKetDoanhThuDTO LayTongKet(DateTime tuNgay, DateTime denNgay)
        {
            const string sql = @"
                SELECT 
                    ISNULL(SUM(TongTien), 0) AS TongDoanhThu,
                    ISNULL(SUM(ThanhTien), 0) AS TongThanhToan,
                    COUNT(*) AS TongDonHang
                FROM HoaDon
                WHERE NgayLap BETWEEN @TuNgay AND @DenNgay";

            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
            cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date.AddDays(1).AddSeconds(-1));

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                decimal tongDoanhThu = Convert.ToDecimal(reader["TongDoanhThu"]);

                return new TongKetDoanhThuDTO
                {
                    TongDoanhThu = tongDoanhThu,
                    TongDonHang = Convert.ToInt32(reader["TongDonHang"]),
                    TongLoiNhuan = tongDoanhThu * 0.25m
                };
            }

            return new TongKetDoanhThuDTO();
        }
    }
}