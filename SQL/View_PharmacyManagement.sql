USE PharmacyManagement;
GO

/*
  Thứ tự triển khai: 1) PharmacyManagement.sql  2) Trigger_PharmacyManagemnt.sql  3) View_PharmacyManagement.sql (file này)
*/

/* vw_PhieuNhapKho — Xem toàn bộ phiếu nhập kho (lớp trên bảng PhieuNhap). Dùng khi muốn truy vấn thống nhất qua view hoặc sau này mở rộng cột/lọc mà không đổi code ứng dụng. */
CREATE OR ALTER VIEW dbo.vw_PhieuNhapKho AS
SELECT * FROM dbo.PhieuNhap;
GO

/* vw_DanhSachThuoc — Danh mục thuốc hiển thị đầy đủ: nhóm thuốc, mã DQG, giá, tồn, HSD và trạng thái kinh doanh (ngừng bán / hết hàng / tồn thấp / đang bán). Dùng cho màn tra cứu thuốc và lưới chọn hàng. */
CREATE OR ALTER VIEW dbo.vw_DanhSachThuoc AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    t.HoatChat,
    t.HamLuong,
    t.DonViTinh,
    nt.TenNhomThuoc,
    dqg.MaDQGDonVi,
    t.SoDangKy,
    t.HangSanXuat,
    t.NuocSanXuat,
    t.DongGoi,
    t.GiaNhap,
    t.GiaBan,
    t.SoLuongTon,
    t.TonToiThieu,
    t.HanSuDung,
    CASE
        WHEN t.TrangThai = 0 THEN N'Ngừng bán'
        WHEN t.SoLuongTon = 0 THEN N'Hết hàng'
        WHEN t.SoLuongTon < t.TonToiThieu THEN N'Tồn thấp'
        ELSE N'Đang bán'
    END AS TrangThai
FROM dbo.Thuoc t
JOIN dbo.NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc
LEFT JOIN dbo.DanhMucDQG dqg ON t.MaDQG = dqg.MaDQG;
GO

/* vw_TraCuuDanhMucDQG — Tra cứu danh mục chuẩn DQG (chỉ bản ghi đang hiệu lực); đối chiếu đã/chưa nhập kho (liên kết Thuoc) và tồn. Dùng khi thêm hàng hoặc đồng bộ theo DQG. */
CREATE OR ALTER VIEW dbo.vw_TraCuuDanhMucDQG AS
SELECT
    dqg.MaDQG,
    dqg.MaDQGDonVi,
    dqg.TenHangHoa,
    dqg.SoDangKy,
    dqg.HoatChatChinh,
    dqg.HoatChatDangKy,
    dqg.HamLuong,
    dqg.DongGoi,
    dqg.HangSanXuat,
    dqg.NuocSanXuat,
    dqg.DonViTinh,
    CASE WHEN t.MaThuoc IS NULL THEN N'Chưa có trong kho' ELSE N'Đã có trong kho' END AS TrangThaiNhapKho,
    t.MaThuoc,
    t.SoLuongTon
FROM dbo.DanhMucDQG dqg
LEFT JOIN dbo.Thuoc t ON dqg.MaDQG = t.MaDQG
WHERE dqg.TrangThai = 1;
GO

/* vw_TonKho — Tồn kho theo mặt hàng (Thuoc): nhóm thuốc, số lượng, ngưỡng tối thiểu, HSD và trạng thái tồn tổng quát. Dùng báo cáo tồn và dashboard liên quan tồn. */
CREATE OR ALTER VIEW dbo.vw_TonKho AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    nt.TenNhomThuoc,
    t.DonViTinh,
    t.SoLuongTon,
    t.TonToiThieu,
    t.HanSuDung,
    CASE
        WHEN t.TrangThai = 0 THEN N'Ngừng bán'
        WHEN t.SoLuongTon = 0 THEN N'Hết hàng'
        WHEN t.SoLuongTon < t.TonToiThieu THEN N'Tồn thấp'
        ELSE N'Còn hàng'
    END AS TrangThaiTonKho
FROM dbo.Thuoc t
JOIN dbo.NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc;
GO

/* vw_TonKhoTheoLo — Tồn chi tiết theo lô (LoThuoc) và kho: số lô, HSD, số ngày còn lại, giá, vị trí và trạng thái lô (hết hàng / hết hạn / sắp hết hạn). Dùng FEFO, kiểm kê theo lô và báo cáo kho. */
CREATE OR ALTER VIEW dbo.vw_TonKhoTheoLo AS
SELECT
    lt.MaLoThuoc,
    t.MaThuoc,
    t.TenThuoc,
    nt.TenNhomThuoc,
    k.MaKho,
    k.TenKho,
    lt.SoLo,
    lt.HanSuDung,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) AS SoNgayConLai,
    lt.SoLuongTon,
    lt.GiaNhap,
    lt.GiaBan,
    lt.ViTri,
    CASE
        WHEN lt.SoLuongTon = 0 THEN N'Hết hàng'
        WHEN lt.HanSuDung < CAST(GETDATE() AS DATE) THEN N'Hết hạn'
        WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) <= 90 THEN N'Sắp hết hạn'
        ELSE N'Còn hàng'
    END AS TrangThaiLo
FROM dbo.LoThuoc lt
JOIN dbo.Thuoc t ON lt.MaThuoc = t.MaThuoc
JOIN dbo.Kho k ON lt.MaKho = k.MaKho
JOIN dbo.NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc;
GO

/* vw_ThuocSapHetHan — Cảnh báo hạn dùng: gộp mức thuốc (HSD trên Thuoc) và mức lô (LoThuoc), chỉ bản ghi còn tồn và HSD trong khoảng cảnh báo (~90 ngày). Cột TheoLo phân biệt nguồn dòng. Phục vụ FEFO và báo cáo cảnh báo (menu 7a). */
CREATE OR ALTER VIEW dbo.vw_ThuocSapHetHan AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    t.DonViTinh,
    t.SoLuongTon,
    t.HanSuDung,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), t.HanSuDung) AS SoNgayConLai,
    CASE
        WHEN t.HanSuDung < CAST(GETDATE() AS DATE) THEN N'Đã hết hạn'
        WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), t.HanSuDung) <= 90 THEN N'Sắp hết hạn'
        ELSE N'Còn hạn'
    END AS TrangThaiHanDung,
    CAST(0 AS BIT) AS TheoLo
FROM dbo.Thuoc t
WHERE t.HanSuDung IS NOT NULL
  AND t.SoLuongTon > 0
  AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), t.HanSuDung) <= 90
UNION ALL
SELECT
    lt.MaThuoc,
    t.TenThuoc,
    t.DonViTinh,
    lt.SoLuongTon,
    lt.HanSuDung,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) AS SoNgayConLai,
    CASE
        WHEN lt.HanSuDung < CAST(GETDATE() AS DATE) THEN N'Đã hết hạn'
        WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) <= 90 THEN N'Sắp hết hạn'
        ELSE N'Còn hạn'
    END AS TrangThaiHanDung,
    CAST(1 AS BIT) AS TheoLo
FROM dbo.LoThuoc lt
JOIN dbo.Thuoc t ON lt.MaThuoc = t.MaThuoc
WHERE lt.SoLuongTon > 0
  AND ISNULL(lt.TrangThai, 1) = 1
  AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) <= 90;
GO

/* vw_CanhBaoHanTheoLo — Lọc từ vw_TonKhoTheoLo các lô còn tồn và đang hết hạn hoặc sắp hết hạn. Dùng trực tiếp cho màn cảnh báo theo lô. */
CREATE OR ALTER VIEW dbo.vw_CanhBaoHanTheoLo AS
SELECT *
FROM dbo.vw_TonKhoTheoLo
WHERE TrangThaiLo IN (N'Sắp hết hạn', N'Hết hạn')
  AND SoLuongTon > 0;
GO

/* vw_ThuocTonThap — Thuốc đang kinh doanh có tồn dưới ngưỡng tối thiểu. Dùng cảnh báo nhập hàng và báo cáo tồn thấp. */
CREATE OR ALTER VIEW dbo.vw_ThuocTonThap AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    t.DonViTinh,
    t.SoLuongTon,
    t.TonToiThieu,
    t.HanSuDung
FROM dbo.Thuoc t
WHERE t.TrangThai = 1
  AND t.SoLuongTon < t.TonToiThieu;
GO

/* vw_LapPhieuNhapKho — Danh sách phiếu nhập kho kèm nhân viên, kho, NCC, tiền, VAT, chiết khấu, công nợ và số dòng chi tiết. Dùng màn “Thông tin / lập phiếu nhập kho” (menu 2a). */
CREATE OR ALTER VIEW dbo.vw_LapPhieuNhapKho AS
SELECT
    pn.MaPhieuNhap,
    pn.NgayNhap,
    pn.SoHoaDon,
    pn.NgayHoaDon,
    pn.LoaiPhieuNhap,
    nv.HoTen AS NhanVienLap,
    k.TenKho,
    ncc.TenNhaCungCap,
    pn.PhuongTienVanChuyen,
    pn.DonViVanChuyen,
    pn.NguoiGiaoHang,
    pn.TongTien AS TongTienHang,
    pn.VAT,
    pn.ChietKhau,
    pn.CongNo AS TongThanhToan,
    pn.TrangThai,
    pn.GhiChu,
    COUNT(ct.MaCTPN) AS SoDongHang
FROM dbo.PhieuNhap pn
JOIN dbo.NhanVien nv ON pn.MaNhanVien = nv.MaNhanVien
LEFT JOIN dbo.Kho k ON pn.MaKho = k.MaKho
LEFT JOIN dbo.NhaCungCap ncc ON pn.MaNhaCungCap = ncc.MaNhaCungCap
LEFT JOIN dbo.ChiTietPhieuNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
GROUP BY
    pn.MaPhieuNhap, pn.NgayNhap, pn.SoHoaDon, pn.NgayHoaDon, pn.LoaiPhieuNhap,
    nv.HoTen, k.TenKho, ncc.TenNhaCungCap, pn.PhuongTienVanChuyen,
    pn.DonViVanChuyen, pn.NguoiGiaoHang, pn.TongTien, pn.VAT, pn.ChietKhau,
    pn.CongNo, pn.TrangThai, pn.GhiChu;
GO

/* vw_DanhSachHangNhapKho — Chi tiết từng dòng hàng trên phiếu nhập (thuốc, DQG, số lượng, đơn giá, lô, HSD, vị trí…). Dùng màn “Danh sách hàng nhập kho” (menu 2b). */
CREATE OR ALTER VIEW dbo.vw_DanhSachHangNhapKho AS
SELECT
    pn.MaPhieuNhap,
    pn.NgayNhap,
    pn.SoHoaDon,
    pn.TrangThai AS TrangThaiPhieu,
    nv.HoTen AS NhanVienNhap,
    k.TenKho,
    ncc.TenNhaCungCap,
    ct.MaCTPN,
    t.MaThuoc,
    t.TenThuoc,
    dqg.MaDQGDonVi,
    t.SoDangKy,
    t.HoatChat,
    t.HamLuong,
    t.DonViTinh,
    ct.SoLuongNhap,
    ct.DonGiaNhap,
    ct.GiaBan,
    ct.ThanhTien,
    ct.VAT AS VATDongPhanTram,
    ct.SoLo,
    ct.HanSuDung,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), ct.HanSuDung) AS SoNgayConHan,
    ct.ViTri,
    ct.GhiChu
FROM dbo.PhieuNhap pn
JOIN dbo.NhanVien nv ON pn.MaNhanVien = nv.MaNhanVien
JOIN dbo.ChiTietPhieuNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
JOIN dbo.Thuoc t ON ct.MaThuoc = t.MaThuoc
LEFT JOIN dbo.DanhMucDQG dqg ON t.MaDQG = dqg.MaDQG
LEFT JOIN dbo.Kho k ON pn.MaKho = k.MaKho
LEFT JOIN dbo.NhaCungCap ncc ON pn.MaNhaCungCap = ncc.MaNhaCungCap;
GO

/* vw_LichSuNhapKho — Lịch sử nhập kho theo phiếu: số mặt hàng, tổng số lượng nhập, tiền hàng và thanh toán. Dùng tra cứu và báo cáo nhập. */
CREATE OR ALTER VIEW dbo.vw_LichSuNhapKho AS
SELECT
    pn.MaPhieuNhap,
    pn.NgayNhap,
    pn.SoHoaDon,
    nv.HoTen AS NhanVienNhap,
    k.TenKho,
    ncc.TenNhaCungCap,
    COUNT(ct.MaCTPN) AS SoMatHang,
    SUM(ct.SoLuongNhap) AS TongSoLuongNhap,
    pn.TongTien AS TongTienHang,
    pn.VAT,
    pn.ChietKhau,
    pn.CongNo AS TongThanhToan,
    pn.TrangThai
FROM dbo.PhieuNhap pn
JOIN dbo.NhanVien nv ON pn.MaNhanVien = nv.MaNhanVien
LEFT JOIN dbo.Kho k ON pn.MaKho = k.MaKho
LEFT JOIN dbo.NhaCungCap ncc ON pn.MaNhaCungCap = ncc.MaNhaCungCap
LEFT JOIN dbo.ChiTietPhieuNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
GROUP BY pn.MaPhieuNhap, pn.NgayNhap, pn.SoHoaDon, nv.HoTen, k.TenKho, ncc.TenNhaCungCap,
         pn.TongTien, pn.VAT, pn.ChietKhau, pn.CongNo, pn.TrangThai;
GO

/* vw_LichSuBanHang — Lịch sử bán: hóa đơn, nhân viên, khách, từng dòng chi tiết bán và số dòng phân bổ lô. Dùng tra cứu bán hàng và đối soát hóa đơn. */
CREATE OR ALTER VIEW dbo.vw_LichSuBanHang AS
SELECT
    hd.MaHoaDon,
    hd.NgayLap,
    nv.HoTen AS NhanVienBan,
    hd.TenKhachHang,
    hd.SoDienThoai,
    t.MaThuoc,
    t.TenThuoc,
    ct.MaCTHD,
    ct.SoLuongBan,
    ct.DonGiaBan,
    ct.ThanhTien,
    (SELECT COUNT(*) FROM dbo.ChiTietHoaDon_PhanBoLo p WHERE p.MaCTHD = ct.MaCTHD) AS SoDongPhanBoLo,
    hd.GiamGia,
    hd.HinhThucThanhToan,
    hd.TrangThai
FROM dbo.HoaDon hd
JOIN dbo.NhanVien nv ON hd.MaNhanVien = nv.MaNhanVien
JOIN dbo.ChiTietHoaDon ct ON hd.MaHoaDon = ct.MaHoaDon
JOIN dbo.Thuoc t ON ct.MaThuoc = t.MaThuoc;
GO

/* vw_ChiTietBanTheoLo — Chi tiết xuất bán theo từng lô (ChiTietHoaDon_PhanBoLo): số lô, HSD, số lượng xuất. Dùng truy vết FEFO và kiểm tra phân bổ lô trên hóa đơn. */
CREATE OR ALTER VIEW dbo.vw_ChiTietBanTheoLo AS
SELECT
    ct.MaCTHD,
    ct.MaHoaDon,
    ct.MaThuoc,
    p.MaLoThuoc,
    lt.SoLo,
    lt.HanSuDung,
    p.SoLuongXuat
FROM dbo.ChiTietHoaDon_PhanBoLo p
JOIN dbo.ChiTietHoaDon ct ON p.MaCTHD = ct.MaCTHD
JOIN dbo.LoThuoc lt ON p.MaLoThuoc = lt.MaLoThuoc;
GO

/* vw_DoanhThuTheoNgay — Tổng hợp doanh thu theo ngày (chỉ hóa đơn hoàn thành): số hóa đơn, tiền hàng, giảm giá, thành tiền. Dùng biểu đồ và báo cáo theo ngày. */
CREATE OR ALTER VIEW dbo.vw_DoanhThuTheoNgay AS
SELECT
    CAST(NgayLap AS DATE) AS Ngay,
    COUNT(MaHoaDon) AS SoHoaDon,
    SUM(TongTien) AS TongTienHang,
    SUM(GiamGia) AS TongGiamGia,
    SUM(ThanhTien) AS DoanhThu
FROM dbo.HoaDon
WHERE TrangThai = N'Hoàn thành'
GROUP BY CAST(NgayLap AS DATE);
GO

/* vw_DoanhThuTheoThang — Tổng hợp doanh thu theo tháng/năm (hóa đơn hoàn thành). Dùng báo cáo xu hướng và so sánh theo tháng. */
CREATE OR ALTER VIEW dbo.vw_DoanhThuTheoThang AS
SELECT
    YEAR(NgayLap) AS Nam,
    MONTH(NgayLap) AS Thang,
    COUNT(MaHoaDon) AS SoHoaDon,
    SUM(TongTien) AS TongTienHang,
    SUM(GiamGia) AS TongGiamGia,
    SUM(ThanhTien) AS DoanhThu
FROM dbo.HoaDon
WHERE TrangThai = N'Hoàn thành'
GROUP BY YEAR(NgayLap), MONTH(NgayLap);
GO

/* vw_DoanhThuNhanVien — Doanh thu gắn với từng nhân viên (hóa đơn hoàn thành). Dùng xếp hạng và KPI nhân viên bán hàng. */
CREATE OR ALTER VIEW dbo.vw_DoanhThuNhanVien AS
SELECT
    nv.MaNhanVien,
    nv.HoTen,
    COUNT(hd.MaHoaDon) AS SoHoaDon,
    SUM(hd.ThanhTien) AS DoanhThu
FROM dbo.HoaDon hd
JOIN dbo.NhanVien nv ON hd.MaNhanVien = nv.MaNhanVien
WHERE hd.TrangThai = N'Hoàn thành'
GROUP BY nv.MaNhanVien, nv.HoTen;
GO

/* vw_ThuocBanChay — Thống kê thuốc bán chạy: tổng số lượng và doanh thu theo mặt hàng (hóa đơn hoàn thành). Dùng báo cáo thuốc (menu 7b). */
CREATE OR ALTER VIEW dbo.vw_ThuocBanChay AS
SELECT
    t.MaThuoc,
    t.TenThuoc,
    nt.TenNhomThuoc,
    SUM(ct.SoLuongBan) AS TongSoLuongBan,
    SUM(ct.ThanhTien) AS TongDoanhThu
FROM dbo.ChiTietHoaDon ct
JOIN dbo.HoaDon hd ON ct.MaHoaDon = hd.MaHoaDon
JOIN dbo.Thuoc t ON ct.MaThuoc = t.MaThuoc
JOIN dbo.NhomThuoc nt ON t.MaNhomThuoc = nt.MaNhomThuoc
WHERE hd.TrangThai = N'Hoàn thành'
GROUP BY t.MaThuoc, t.TenThuoc, nt.TenNhomThuoc;
GO

/* vw_DoanhThuHomNay — Chỉ số doanh thu trong ngày hiện tại (số hóa đơn và tổng thành tiền, trạng thái hoàn thành). Dùng KPI “hôm nay” trên dashboard. */
CREATE OR ALTER VIEW dbo.vw_DoanhThuHomNay AS
SELECT
    CAST(GETDATE() AS DATE) AS Ngay,
    COUNT(MaHoaDon) AS SoHoaDon,
    ISNULL(SUM(ThanhTien), 0) AS DoanhThu
FROM dbo.HoaDon
WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)
  AND TrangThai = N'Hoàn thành';
GO

/* vw_DashboardTongQuan — Một dòng KPI tổng quan: hóa đơn và doanh thu hôm nay, thuốc tồn thấp, số lô sắp hết hạn (0–90 ngày), phiếu nhập hôm nay, số mã DQG hiệu lực. Dùng tile dashboard (BLL/DAL đọc view này). */
CREATE OR ALTER VIEW dbo.vw_DashboardTongQuan AS
SELECT
    (SELECT COUNT(*) FROM dbo.HoaDon WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)) AS SoHoaDonHomNay,
    (SELECT ISNULL(SUM(ThanhTien), 0) FROM dbo.HoaDon WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE) AND TrangThai = N'Hoàn thành') AS DoanhThuHomNay,
    (SELECT COUNT(*) FROM dbo.Thuoc WHERE SoLuongTon < TonToiThieu AND TrangThai = 1) AS SoThuocTonThap,
    (SELECT COUNT(*)
     FROM dbo.LoThuoc lt
     WHERE lt.SoLuongTon > 0
       AND ISNULL(lt.TrangThai, 1) = 1
       AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.HanSuDung) BETWEEN 0 AND 90) AS SoThuocSapHetHan,
    (SELECT COUNT(*) FROM dbo.PhieuNhap WHERE CAST(NgayNhap AS DATE) = CAST(GETDATE() AS DATE)) AS SoPhieuNhapHomNay,
    (SELECT COUNT(*) FROM dbo.DanhMucDQG WHERE TrangThai = 1) AS SoThuocTrongDQG;
GO

/* vw_AuditLogChiTiet — Nhật ký thao tác (AuditLog) kèm tên nhân viên. Dùng màn Audit log (menu 8) và tra cứu thay đổi dữ liệu. */
CREATE OR ALTER VIEW dbo.vw_AuditLogChiTiet AS
SELECT
    al.MaLog,
    al.ThoiGian,
    nv.HoTen AS NhanVien,
    al.HanhDong,
    al.TenBang,
    al.MaBanGhi,
    al.NoiDung,
    al.DiaChiMay
FROM dbo.AuditLog al
LEFT JOIN dbo.NhanVien nv ON al.MaNhanVien = nv.MaNhanVien;
GO
