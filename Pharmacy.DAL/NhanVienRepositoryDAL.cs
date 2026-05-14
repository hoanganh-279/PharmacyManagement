using Microsoft.Data.SqlClient;
using Pharmacy.DTO;

namespace Pharmacy.DAL;

public class NhanVienRepositoryDAL
{
    private readonly DbContextDAL _db;

    public NhanVienRepositoryDAL(DbContextDAL db) => _db = db;

    public NhanVienDangNhapDTO? LayChoDangNhap(string tenDangNhap)
    {
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand("sp_DangNhap", cn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            return null;

        return new NhanVienDangNhapDTO
        {
            MaNhanVien = rd.GetInt32("MaNhanVien"),
            HoTen = rd.GetString("HoTen"),
            TenDangNhap = rd.GetString("TenDangNhap"),
            MatKhauHash = rd.GetString("MatKhauHash"),
            TenVaiTro = rd.GetString("TenVaiTro")
        };
    }

    public IReadOnlyList<NhanVienDTO> LayTatCa()
    {
        const string sql = """
            SELECT nv.MaNhanVien, nv.HoTen, nv.TenDangNhap, nv.SoDienThoai, nv.Email,
                   nv.MaVaiTro, vt.TenVaiTro, nv.TrangThai, nv.NgayTao
            FROM NhanVien nv
            JOIN VaiTro vt ON nv.MaVaiTro = vt.MaVaiTro
            ORDER BY nv.MaNhanVien;
            """;

        var list = new List<NhanVienDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new NhanVienDTO
            {
                MaNhanVien = rd.GetInt32("MaNhanVien"),
                HoTen = rd.GetString("HoTen"),
                TenDangNhap = rd.GetString("TenDangNhap"),
                SoDienThoai = rd.GetNullableString("SoDienThoai"),
                Email = rd.GetNullableString("Email"),
                MaVaiTro = rd.GetInt32("MaVaiTro"),
                TenVaiTro = rd.GetNullableString("TenVaiTro"),
                TrangThai = rd.GetBoolean("TrangThai"),
                NgayTao = rd.GetDateTime("NgayTao")
            });
        }

        return list;
    }

    public int Them(NhanVienDTO nv, string matKhauHash)
    {
        const string sql = """
            INSERT INTO NhanVien(HoTen, TenDangNhap, MatKhauHash, SoDienThoai, Email, MaVaiTro, TrangThai)
            OUTPUT INSERTED.MaNhanVien
            VALUES (@HoTen, @TenDangNhap, @MatKhauHash, @SoDienThoai, @Email, @MaVaiTro, @TrangThai);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
        cmd.Parameters.AddWithValue("@TenDangNhap", nv.TenDangNhap);
        cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)nv.SoDienThoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)nv.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaVaiTro", nv.MaVaiTro);
        cmd.Parameters.AddWithValue("@TrangThai", nv.TrangThai);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    public void CapNhat(NhanVienDTO nv, string? matKhauHashMoi)
    {
        var sql = matKhauHashMoi is null
            ? """
              UPDATE NhanVien SET HoTen=@HoTen, SoDienThoai=@SoDienThoai, Email=@Email, MaVaiTro=@MaVaiTro, TrangThai=@TrangThai
              WHERE MaNhanVien=@MaNhanVien;
              """
            : """
              UPDATE NhanVien SET HoTen=@HoTen, SoDienThoai=@SoDienThoai, Email=@Email, MaVaiTro=@MaVaiTro, TrangThai=@TrangThai,
                  MatKhauHash=@MatKhauHash
              WHERE MaNhanVien=@MaNhanVien;
              """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaNhanVien", nv.MaNhanVien);
        cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)nv.SoDienThoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)nv.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaVaiTro", nv.MaVaiTro);
        cmd.Parameters.AddWithValue("@TrangThai", nv.TrangThai);
        if (matKhauHashMoi is not null)
            cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHashMoi);
        cn.Open();
        cmd.ExecuteNonQuery();
    }
}
