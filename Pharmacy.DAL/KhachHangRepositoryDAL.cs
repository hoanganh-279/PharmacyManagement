using Pharmacy.DTO;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DAL
{
    public class KhachHangRepositoryDAL
    {
        private readonly DbContextDAL _db = new DbContextDAL();

        public DataTable GetAll()
        {
            string query = "SELECT * FROM KhachHang";
            return _db.ExecuteQuery(query);
        }

        public bool Insert(KhachHangDTO kh)
        {
            string query = @"
                INSERT INTO KhachHang
                (
                    CCCD,
                    HoTen,
                    SoDienThoai,
                    NgaySinh,
                    DiaChi,
                    GhiChu
                )
                VALUES
                (
                    @CCCD,
                    @HoTen,
                    @SoDienThoai,
                    @NgaySinh,
                    @DiaChi,
                    @GhiChu
                )";

            SqlParameter[] parameters =
            {
                new SqlParameter("@CCCD", kh.CCCD),
                new SqlParameter("@HoTen", kh.HoTen),
                new SqlParameter("@SoDienThoai", kh.SoDienThoai),
                new SqlParameter("@NgaySinh", kh.NgaySinh),
                new SqlParameter("@DiaChi", kh.DiaChi),
                new SqlParameter("@GhiChu", kh.GhiChu)
            };

            return _db.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}