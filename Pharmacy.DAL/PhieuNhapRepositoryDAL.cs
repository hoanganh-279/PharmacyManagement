using Microsoft.Data.SqlClient;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.DAL;

public class PhieuNhapRepositoryDAL
{
    private readonly DbContextDAL _db;

    public PhieuNhapRepositoryDAL(DbContextDAL db) => _db = db;

    public IReadOnlyList<IdTenDTO> LayKhoHoatDong()
    {
        const string sql = "SELECT MaKho AS Id, TenKho AS Ten FROM Kho WHERE TrangThai = 1 ORDER BY TenKho;";
        return LayIdTen(sql);
    }

    public IReadOnlyList<IdTenDTO> LayNhaCungCapHoatDong()
    {
        const string sql = "SELECT MaNhaCungCap AS Id, TenNhaCungCap AS Ten FROM NhaCungCap WHERE TrangThai = 1 ORDER BY TenNhaCungCap;";
        return LayIdTen(sql);
    }

    public IReadOnlyList<IdTenDTO> LayThuKhoHoatDong()
    {
        const string sql = """
            SELECT nv.MaNhanVien AS Id, nv.HoTen + N' - NV' + CAST(nv.MaNhanVien AS NVARCHAR(10)) AS Ten
            FROM NhanVien nv
            INNER JOIN VaiTro vt ON nv.MaVaiTro = vt.MaVaiTro
            WHERE nv.TrangThai = 1
              AND vt.TenVaiTro IN (N'Admin', N'Quản lý', N'Nhân viên kho')
            ORDER BY nv.HoTen;
            """;
        return LayIdTen(sql);
    }

    public PhieuNhapDTO? LayPhieu(int maPhieuNhap)
    {
        const string sql = """
            SELECT MaPhieuNhap, NgayNhap, MaNhanVien, SoHoaDon, NgayHoaDon, LoaiPhieuNhap, MaKho, MaNhaCungCap,
                   PhuongTienVanChuyen, DonViVanChuyen, NguoiGiaoHang, VAT, ChietKhau, CongNo, TongTien, GhiChu, TrangThai
            FROM PhieuNhap WHERE MaPhieuNhap = @MaPhieuNhap;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", maPhieuNhap);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? MapPhieu(rd) : null;
    }

    public string? LayTrangThai(int maPhieuNhap)
    {
        const string sql = "SELECT TrangThai FROM PhieuNhap WHERE MaPhieuNhap = @MaPhieuNhap;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", maPhieuNhap);
        cn.Open();
        var o = cmd.ExecuteScalar();
        return o is null or DBNull ? null : Convert.ToString(o, null);
    }

    public int TaoPhieu(PhieuNhapDTO dto)
    {
        const string sql = """
            INSERT INTO PhieuNhap (
                NgayNhap, MaNhanVien, SoHoaDon, NgayHoaDon, LoaiPhieuNhap, MaKho, MaNhaCungCap,
                PhuongTienVanChuyen, DonViVanChuyen, NguoiGiaoHang, VAT, ChietKhau, CongNo, GhiChu, TrangThai)
            VALUES (
                @NgayNhap, @MaNhanVien, @SoHoaDon, @NgayHoaDon, @LoaiPhieuNhap, @MaKho, @MaNhaCungCap,
                @PhuongTienVanChuyen, @DonViVanChuyen, @NguoiGiaoHang, @VAT, @ChietKhau, @CongNo, @GhiChu, @TrangThai);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        AddPhieuParams(cmd, dto);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    public void CapNhatPhieu(PhieuNhapDTO dto)
    {
        const string sql = """
            UPDATE PhieuNhap SET
                NgayNhap = @NgayNhap, MaNhanVien = @MaNhanVien, SoHoaDon = @SoHoaDon, NgayHoaDon = @NgayHoaDon,
                LoaiPhieuNhap = @LoaiPhieuNhap, MaKho = @MaKho, MaNhaCungCap = @MaNhaCungCap,
                PhuongTienVanChuyen = @PhuongTienVanChuyen, DonViVanChuyen = @DonViVanChuyen,
                NguoiGiaoHang = @NguoiGiaoHang, VAT = @VAT, ChietKhau = @ChietKhau, CongNo = @CongNo,
                GhiChu = @GhiChu, TrangThai = @TrangThai
            WHERE MaPhieuNhap = @MaPhieuNhap;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", dto.MaPhieuNhap);
        AddPhieuParams(cmd, dto);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void DatTrangThai(int maPhieuNhap, string trangThai)
    {
        const string sql = "UPDATE PhieuNhap SET TrangThai = @TrangThai WHERE MaPhieuNhap = @MaPhieuNhap;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", maPhieuNhap);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<DanhSachHangNhapKhoViewDTO> LayChiTietPhieu(int maPhieuNhap)
    {
        const string sql = """
            SELECT MaPhieuNhap, MaCTPN, MaThuoc, TenThuoc, DonViTinh, SoLuongNhap, DonGiaNhap, GiaBan,
                   ThanhTien, SoLo, HanSuDung, SoNgayConHan, ViTri, GhiChu, VATDongPhanTram, TrangThaiPhieu
            FROM vw_DanhSachHangNhapKho
            WHERE MaPhieuNhap = @MaPhieuNhap
            ORDER BY MaCTPN;
            """;

        var list = new List<DanhSachHangNhapKhoViewDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", maPhieuNhap);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DanhSachHangNhapKhoViewDTO
            {
                MaPhieuNhap = rd.GetInt32("MaPhieuNhap"),
                MaCTPN = rd.GetInt32("MaCTPN"),
                MaThuoc = rd.GetInt32("MaThuoc"),
                TenThuoc = rd.GetString("TenThuoc"),
                DonViTinh = rd.GetString("DonViTinh"),
                SoLuongNhap = rd.GetInt32("SoLuongNhap"),
                DonGiaNhap = rd.GetDecimal("DonGiaNhap"),
                GiaBan = rd.GetDecimal("GiaBan"),
                ThanhTien = rd.GetDecimal("ThanhTien"),
                SoLo = rd.GetNullableString("SoLo"),
                HanSuDung = rd.GetNullableDateTime("HanSuDung"),
                SoNgayConHan = rd.GetNullableInt32("SoNgayConHan"),
                ViTri = rd.GetNullableString("ViTri"),
                GhiChu = rd.GetNullableString("GhiChu"),
                VATDongPhanTram = rd.GetNullableDecimal("VATDongPhanTram"),
                TrangThaiPhieu = rd.GetString("TrangThaiPhieu")
            });
        }

        return list;
    }

    public int ThemChiTiet(ChiTietPhieuNhapDTO ct)
    {
        const string sql = """
            INSERT INTO ChiTietPhieuNhap (
                MaPhieuNhap, MaThuoc, SoLuongNhap, DonGiaNhap, HanSuDung, GiaBan, SoLo, ViTri, GhiChu, VAT)
            VALUES (
                @MaPhieuNhap, @MaThuoc, @SoLuongNhap, @DonGiaNhap, @HanSuDung, @GiaBan, @SoLo, @ViTri, @GhiChu, @VAT);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaPhieuNhap", ct.MaPhieuNhap);
        cmd.Parameters.AddWithValue("@MaThuoc", ct.MaThuoc);
        cmd.Parameters.AddWithValue("@SoLuongNhap", ct.SoLuongNhap);
        cmd.Parameters.AddWithValue("@DonGiaNhap", ct.DonGiaNhap);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)ct.HanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GiaBan", ct.GiaBan);
        cmd.Parameters.AddWithValue("@SoLo", (object?)ct.SoLo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ViTri", (object?)ct.ViTri ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)ct.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VAT", (object?)ct.VAT ?? DBNull.Value);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    public void CapNhatChiTiet(ChiTietPhieuNhapDTO ct)
    {
        const string sql = """
            UPDATE ChiTietPhieuNhap SET
                SoLuongNhap = @SoLuongNhap,
                DonGiaNhap = @DonGiaNhap,
                HanSuDung = @HanSuDung,
                GiaBan = @GiaBan,
                SoLo = @SoLo,
                ViTri = @ViTri,
                GhiChu = @GhiChu,
                VAT = @VAT
            WHERE MaCTPN = @MaCTPN;
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaCTPN", ct.MaCTPN);
        cmd.Parameters.AddWithValue("@SoLuongNhap", ct.SoLuongNhap);
        cmd.Parameters.AddWithValue("@DonGiaNhap", ct.DonGiaNhap);
        cmd.Parameters.AddWithValue("@HanSuDung", (object?)ct.HanSuDung ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GiaBan", ct.GiaBan);
        cmd.Parameters.AddWithValue("@SoLo", (object?)ct.SoLo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ViTri", (object?)ct.ViTri ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)ct.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VAT", (object?)ct.VAT ?? DBNull.Value);
        cn.Open();
        if (cmd.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("Không tìm thấy dòng chi tiết phiếu nhập.");
    }

    public void XoaChiTiet(int maCtpn)
    {
        const string sql = "DELETE FROM ChiTietPhieuNhap WHERE MaCTPN = @MaCTPN;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaCTPN", maCtpn);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public int? LayMaThuocTheoMaDqg(int maDqg)
    {
        const string sql = "SELECT MaThuoc FROM Thuoc WHERE MaDQG = @MaDQG;";
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaDQG", maDqg);
        cn.Open();
        var o = cmd.ExecuteScalar();
        return o is null or DBNull ? null : Convert.ToInt32(o, null);
    }

    public int TaoThuocTuDqg(int maDqg, int maNhomThuoc, bool choPhepLienThong, decimal giaNhap, decimal giaBan)
    {
        const string sql = """
            INSERT INTO Thuoc (
                TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, SoLuongTon, TonToiThieu,
                MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi, ChoPhepLienThong, TrangThai)
            SELECT
                d.TenHangHoa, d.HoatChatChinh, d.HamLuong, ISNULL(d.DonViTinh, N'Viên'),
                @GiaNhap, @GiaBan, 0, 20, @MaNhomThuoc, d.MaDQG, d.SoDangKy,
                d.HangSanXuat, d.NuocSanXuat, d.DongGoi, @ChoPhepLienThong, 1
            FROM DanhMucDQG d
            WHERE d.MaDQG = @MaDQG;
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@MaDQG", maDqg);
        cmd.Parameters.AddWithValue("@MaNhomThuoc", maNhomThuoc);
        cmd.Parameters.AddWithValue("@ChoPhepLienThong", choPhepLienThong);
        cmd.Parameters.AddWithValue("@GiaNhap", giaNhap);
        cmd.Parameters.AddWithValue("@GiaBan", giaBan);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar(), null);
    }

    /// <summary>Gọi sp_NhapKho — luồng nhập nhanh một dòng (giữ tương thích cũ).</summary>
    public int NhapKho(
        int maNhanVien,
        int maThuoc,
        int soLuongNhap,
        decimal donGiaNhap,
        DateTime? hanSuDung,
        string? nhaCungCap,
        string? ghiChu,
        int? maKho = null,
        string? soLo = null,
        decimal? giaBanDong = null,
        string? viTri = null)
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
        cmd.Parameters.AddWithValue("@MaKho", (object?)maKho ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoLo", (object?)soLo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GiaBanDong", (object?)giaBanDong ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ViTri", (object?)viTri ?? DBNull.Value);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            throw new InvalidOperationException("sp_NhapKho không trả về MaPhieuNhap.");

        return rd.GetInt32("MaPhieuNhap");
    }

    private static void AddPhieuParams(SqlCommand cmd, PhieuNhapDTO dto)
    {
        cmd.Parameters.AddWithValue("@NgayNhap", dto.NgayNhap);
        cmd.Parameters.AddWithValue("@MaNhanVien", dto.MaNhanVien);
        cmd.Parameters.AddWithValue("@SoHoaDon", (object?)dto.SoHoaDon ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NgayHoaDon", (object?)dto.NgayHoaDon ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LoaiPhieuNhap", (object?)dto.LoaiPhieuNhap ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaKho", (object?)dto.MaKho ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaNhaCungCap", (object?)dto.MaNhaCungCap ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PhuongTienVanChuyen", (object?)dto.PhuongTienVanChuyen ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DonViVanChuyen", (object?)dto.DonViVanChuyen ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NguoiGiaoHang", (object?)dto.NguoiGiaoHang ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VAT", dto.VAT);
        cmd.Parameters.AddWithValue("@ChietKhau", dto.ChietKhau);
        cmd.Parameters.AddWithValue("@CongNo", dto.CongNo);
        cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", dto.TrangThai);
    }

    private static PhieuNhapDTO MapPhieu(SqlDataReader rd) => new()
    {
        MaPhieuNhap = rd.GetInt32("MaPhieuNhap"),
        NgayNhap = rd.GetDateTime("NgayNhap"),
        MaNhanVien = rd.GetInt32("MaNhanVien"),
        SoHoaDon = rd.GetNullableString("SoHoaDon"),
        NgayHoaDon = rd.GetNullableDateTime("NgayHoaDon"),
        LoaiPhieuNhap = rd.GetNullableString("LoaiPhieuNhap"),
        MaKho = rd.GetNullableInt32("MaKho"),
        MaNhaCungCap = rd.GetNullableInt32("MaNhaCungCap"),
        PhuongTienVanChuyen = rd.GetNullableString("PhuongTienVanChuyen"),
        DonViVanChuyen = rd.GetNullableString("DonViVanChuyen"),
        NguoiGiaoHang = rd.GetNullableString("NguoiGiaoHang"),
        VAT = rd.GetDecimal("VAT"),
        ChietKhau = rd.GetDecimal("ChietKhau"),
        CongNo = rd.GetDecimal("CongNo"),
        TongTien = rd.GetDecimal("TongTien"),
        GhiChu = rd.GetNullableString("GhiChu"),
        TrangThai = rd.GetString("TrangThai")
    };

    private IReadOnlyList<IdTenDTO> LayIdTen(string sql)
    {
        var list = new List<IdTenDTO>();
        using var cn = _db.CreateConnection();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new IdTenDTO
            {
                Id = rd.GetInt32("Id"),
                Ten = rd.GetString("Ten")
            });
        }

        return list;
    }
}
