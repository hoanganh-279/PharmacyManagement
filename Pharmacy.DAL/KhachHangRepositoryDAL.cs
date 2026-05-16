<<<<<<< HEAD
using Microsoft.Data.SqlClient;
=======
﻿using Microsoft.Data.SqlClient;
>>>>>>> c178570feb4e8edc1d85abcf5c1940dbf983f787
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class KhachHangRepositoryDAL
{
    private readonly DbContextDAL _db;

    public KhachHangRepositoryDAL(DbContextDAL db) => _db = db;

    public KhachHangDTO? LayTheoCccd(string cccd)
    {
        const string sql = """
            SELECT CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi, GhiChu, TrangThai, NgayTao
            FROM KhachHang
            WHERE CCCD = @CCCD AND TrangThai = 1;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@CCCD", cccd);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? Map(rd) : null;
    }

    public void ThemMoi(KhachHangDTO dto)
    {
        const string sql = """
            INSERT INTO KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi, GhiChu)
            VALUES (@CCCD, @HoTen, @SoDienThoai, @NgaySinh, @DiaChi, @GhiChu);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@CCCD", dto.CCCD.Trim());
        cmd.Parameters.AddWithValue("@HoTen", dto.HoTen.Trim());
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)dto.SoDienThoai?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NgaySinh", (object?)dto.NgaySinh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DiaChi", (object?)dto.DiaChi?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu?.Trim() ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<LichSuMuaHangDTO> LayLichSuMuaHang(string cccd, int top = 100)
    {
        const string sql = """
            SELECT TOP (@Top)
                CCCD, HoTen, MaHoaDon, NgayLap, ThanhTienHoaDon, TrangThaiHoaDon,
                TenThuoc, SoLuongBan, DonGiaBan, ThanhTienDong
            FROM vw_LichSuMuaHangTheoCCCD
            WHERE CCCD = @CCCD
            ORDER BY NgayLap DESC, MaHoaDon DESC;
            """;

        var list = new List<LichSuMuaHangDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Top", top);
        cmd.Parameters.AddWithValue("@CCCD", cccd);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new LichSuMuaHangDTO
            {
                CCCD = rd.GetString("CCCD"),
                HoTen = rd.GetString("HoTen"),
                MaHoaDon = rd.GetInt32("MaHoaDon"),
                NgayLap = rd.GetDateTime("NgayLap"),
                ThanhTienHoaDon = rd.GetDecimal("ThanhTienHoaDon"),
                TrangThaiHoaDon = rd.GetString("TrangThaiHoaDon"),
                TenThuoc = rd.GetString("TenThuoc"),
                SoLuongBan = rd.GetInt32("SoLuongBan"),
                DonGiaBan = rd.GetDecimal("DonGiaBan"),
                ThanhTienDong = rd.GetDecimal("ThanhTienDong")
            });
        }

        return list;
    }

    private static KhachHangDTO Map(SqlDataReader rd) => new()
    {
        CCCD = rd.GetString("CCCD"),
        HoTen = rd.GetString("HoTen"),
        SoDienThoai = rd.GetNullableString("SoDienThoai"),
        NgaySinh = rd.GetNullableDateTime("NgaySinh"),
        DiaChi = rd.GetNullableString("DiaChi"),
        GhiChu = rd.GetNullableString("GhiChu"),
        TrangThai = rd.GetBoolean("TrangThai"),
        NgayTao = rd.GetDateTime("NgayTao")
    };
}
