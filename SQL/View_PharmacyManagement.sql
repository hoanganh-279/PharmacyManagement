USE PharmacyManagement;
GO

-- View t?n kho
CREATE VIEW vw_TonKho AS
SELECT 
    t.MaThuoc,
    t.TenThuoc,
    nt.TenNhomThuoc,
    t.DonViTinh,
    t.SoLuongTon,
    t.TonToiThieu,
    t.HanSuDung,
    CASE
        WHEN t.SoLuongTon = 0 THEN N'H?t hàng'
        WHEN t.SoLuongTon < t.TonToiThieu THEN N'T?n th?p'
        ELSE N'Còn hàng'
    END AS TrangThaiTonKho
FROM Thuoc t
JOIN NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc
WHERE t.TrangThai = 1;
GO

-- View thu?c s?p h?t h?n
CREATE VIEW vw_ThuocSapHetHan AS
SELECT 
    MaThuoc,
    TenThuoc,
    DonViTinh,
    SoLuongTon,
    HanSuDung,
    DATEDIFF(DAY, GETDATE(), HanSuDung) AS SoNgayConLai,
    CASE
        WHEN HanSuDung < CAST(GETDATE() AS DATE) THEN N'?ã h?t h?n'
        WHEN DATEDIFF(DAY, GETDATE(), HanSuDung) <= 90 THEN N'S?p h?t h?n'
        ELSE N'Còn h?n'
    END AS TrangThaiHanDung
FROM Thuoc
WHERE HanSuDung IS NOT NULL
  AND SoLuongTon > 0
  AND DATEDIFF(DAY, GETDATE(), HanSuDung) <= 90;
GO

--View doanh thu theo ngày
CREATE VIEW vw_DoanhThuTheoNgay AS
SELECT 
    CAST(NgayLap AS DATE) AS Ngay,
    COUNT(MaHoaDon) AS SoHoaDon,
    SUM(TongTien) AS TongTienHang,
    SUM(GiamGia) AS TongGiamGia,
    SUM(ThanhTien) AS DoanhThu
FROM HoaDon
WHERE TrangThai = N'Hoàn thành'
GROUP BY CAST(NgayLap AS DATE);
GO

--View doanh thu theo nhân viên
CREATE VIEW vw_DoanhThuNhanVien AS
SELECT 
    nv.MaNhanVien,
    nv.HoTen,
    COUNT(hd.MaHoaDon) AS SoHoaDon,
    SUM(hd.ThanhTien) AS DoanhThu
FROM HoaDon hd
JOIN NhanVien nv ON hd.MaNhanVien = nv.MaNhanVien
WHERE hd.TrangThai = N'Hoàn thành'
GROUP BY nv.MaNhanVien, nv.HoTen;
GO

-- View thu?c bán ch?y
CREATE VIEW vw_ThuocBanChay AS
SELECT 
    t.MaThuoc,
    t.TenThuoc,
    nt.TenNhomThuoc,
    SUM(ct.SoLuongBan) AS TongSoLuongBan,
    SUM(ct.ThanhTien) AS TongDoanhThu
FROM ChiTietHoaDon ct
JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc
JOIN NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc
JOIN HoaDon hd ON ct.MaHoaDon = hd.MaHoaDon
WHERE hd.TrangThai = N'Hoàn thành'
GROUP BY t.MaThuoc, t.TenThuoc, nt.TenNhomThuoc;
GO

-- View danh sách thu?c ??y ??
CREATE VIEW vw_DanhSachThuoc AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    t.HoatChat,
    t.HamLuong,
    t.DonViTinh,
    nt.TenNhomThuoc,
    t.GiaNhap,
    t.GiaBan,
    t.SoLuongTon,
    t.TonToiThieu,
    t.HanSuDung,
    CASE 
        WHEN t.TrangThai = 1 THEN N'?ang bán'
        ELSE N'Ng?ng bán'
    END AS TrangThai
FROM Thuoc t
JOIN NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc;
GO

-- View thu?c t?n th?p
CREATE VIEW vw_ThuocTonThap AS
SELECT
    MaThuoc,
    TenThuoc,
    DonViTinh,
    SoLuongTon,
    TonToiThieu,
    HanSuDung
FROM Thuoc
WHERE TrangThai = 1
  AND SoLuongTon < TonToiThieu;
GO

-- View l?ch s? nh?p kho
CREATE VIEW vw_LichSuNhapKho AS
SELECT
    pn.MaPhieuNhap,
    pn.NgayNhap,
    nv.HoTen AS NhanVienNhap,
    pn.NhaCungCap,
    t.TenThuoc,
    ct.SoLuongNhap,
    ct.DonGiaNhap,
    ct.ThanhTien,
    ct.HanSuDung
FROM PhieuNhap pn
JOIN NhanVien nv ON pn.MaNhanVien = nv.MaNhanVien
JOIN ChiTietPhieuNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc;
GO

-- View l?ch s? bán hàng
CREATE VIEW vw_LichSuBanHang AS
SELECT
    hd.MaHoaDon,
    hd.NgayLap,
    nv.HoTen AS NhanVienBan,
    hd.TenKhachHang,
    hd.SoDienThoai,
    t.TenThuoc,
    ct.SoLuongBan,
    ct.DonGiaBan,
    ct.ThanhTien,
    hd.HinhThucThanhToan,
    hd.TrangThai
FROM HoaDon hd
JOIN NhanVien nv ON hd.MaNhanVien = nv.MaNhanVien
JOIN ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
JOIN Thuoc t ON ct.MaThuoc = t.MaThuoc;
GO

-- View doanh thu theo tháng
CREATE VIEW vw_DoanhThuTheoThang AS
SELECT
    YEAR(NgayLap) AS Nam,
    MONTH(NgayLap) AS Thang,
    COUNT(MaHoaDon) AS SoHoaDon,
    SUM(ThanhTien) AS DoanhThu
FROM HoaDon
WHERE TrangThai = N'Hoàn thành'
GROUP BY YEAR(NgayLap), MONTH(NgayLap);
GO

-- View doanh thu hôm nay
CREATE VIEW vw_DoanhThuHomNay AS
SELECT
    CAST(GETDATE() AS DATE) AS Ngay,
    COUNT(MaHoaDon) AS SoHoaDon,
    ISNULL(SUM(ThanhTien), 0) AS DoanhThu
FROM HoaDon
WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)
  AND TrangThai = N'Hoàn thành';
GO

-- View dashboard t?ng quan
CREATE VIEW vw_DashboardTongQuan AS
SELECT
    (SELECT COUNT(*) FROM HoaDon 
     WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)) AS SoHoaDonHomNay,

    (SELECT ISNULL(SUM(ThanhTien), 0) FROM HoaDon 
     WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)
       AND TrangThai = N'Hoàn thành') AS DoanhThuHomNay,

    (SELECT COUNT(*) FROM Thuoc 
     WHERE SoLuongTon < TonToiThieu 
       AND TrangThai = 1) AS SoThuocTonThap,

    (SELECT COUNT(*) FROM Thuoc 
     WHERE HanSuDung IS NOT NULL
       AND DATEDIFF(DAY, GETDATE(), HanSuDung) <= 90
       AND SoLuongTon > 0) AS SoThuocSapHetHan;
GO

-- View audit log chi ti?t
CREATE VIEW vw_AuditLogChiTiet AS
SELECT
    al.MaLog,
    al.ThoiGian,
    nv.HoTen AS NhanVien,
    al.HanhDong,
    al.TenBang,
    al.MaBanGhi,
    al.NoiDung,
    al.DiaChiMay
FROM AuditLog al
LEFT JOIN NhanVien nv ON al.MaNhanVien = nv.MaNhanVien;
GO
