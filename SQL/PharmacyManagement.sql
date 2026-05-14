CREATE DATABASE PharmacyManagement;
GO

USE PharmacyManagement;
GO

CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL UNIQUE,
    MoTa NVARCHAR(255),
    TrangThai BIT NOT NULL DEFAULT 1
);

CREATE TABLE NhanVien (
    MaNhanVien INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhauHash VARCHAR(255) NOT NULL,
    SoDienThoai VARCHAR(15),
    Email VARCHAR(100),
    MaVaiTro INT NOT NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_NhanVien_VaiTro 
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);

CREATE TABLE NhomThuoc (
    MaNhomThuoc INT IDENTITY(1,1) PRIMARY KEY,
    TenNhomThuoc NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(255),
    TrangThai BIT NOT NULL DEFAULT 1
);

CREATE TABLE Thuoc (
    MaThuoc INT IDENTITY(1,1) PRIMARY KEY,
    TenThuoc NVARCHAR(150) NOT NULL,
    HoatChat NVARCHAR(150),
    HamLuong NVARCHAR(50),
    DonViTinh NVARCHAR(30) NOT NULL,
    GiaNhap DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaBan DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon INT NOT NULL DEFAULT 0,
    TonToiThieu INT NOT NULL DEFAULT 0,
    HanSuDung DATE NULL,
    MaNhomThuoc INT NOT NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Thuoc_NhomThuoc 
    FOREIGN KEY (MaNhomThuoc) REFERENCES NhomThuoc(MaNhomThuoc),

    CONSTRAINT CK_Thuoc_GiaNhap CHECK (GiaNhap >= 0),
    CONSTRAINT CK_Thuoc_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_Thuoc_SoLuongTon CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_Thuoc_TonToiThieu CHECK (TonToiThieu >= 0)
);

CREATE TABLE PhieuNhap (
    MaPhieuNhap INT IDENTITY(1,1) PRIMARY KEY,
    NgayNhap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNhanVien INT NOT NULL,
    NhaCungCap NVARCHAR(150),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(255),

    CONSTRAINT FK_PhieuNhap_NhanVien 
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),

    CONSTRAINT CK_PhieuNhap_TongTien CHECK (TongTien >= 0)
);

CREATE TABLE ChiTietPhieuNhap (
    MaCTPN INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuNhap INT NOT NULL,
    MaThuoc INT NOT NULL,
    SoLuongNhap INT NOT NULL,
    DonGiaNhap DECIMAL(18,2) NOT NULL,
    ThanhTien AS (SoLuongNhap * DonGiaNhap) PERSISTED,
    HanSuDung DATE NULL,

    CONSTRAINT FK_CTPN_PhieuNhap 
    FOREIGN KEY (MaPhieuNhap) REFERENCES PhieuNhap(MaPhieuNhap),

    CONSTRAINT FK_CTPN_Thuoc 
    FOREIGN KEY (MaThuoc) REFERENCES Thuoc(MaThuoc),

    CONSTRAINT CK_CTPN_SoLuongNhap CHECK (SoLuongNhap > 0),
    CONSTRAINT CK_CTPN_DonGiaNhap CHECK (DonGiaNhap >= 0)
);

CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNhanVien INT NOT NULL,
    TenKhachHang NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiamGia DECIMAL(18,2) NOT NULL DEFAULT 0,
    ThanhTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    HinhThucThanhToan NVARCHAR(50) DEFAULT N'Tiền mặt',
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Hoàn thành',

    CONSTRAINT FK_HoaDon_NhanVien 
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),

    CONSTRAINT CK_HoaDon_TongTien CHECK (TongTien >= 0),
    CONSTRAINT CK_HoaDon_GiamGia CHECK (GiamGia >= 0),
    CONSTRAINT CK_HoaDon_ThanhTien CHECK (ThanhTien >= 0)
);

CREATE TABLE ChiTietHoaDon (
    MaCTHD INT IDENTITY(1,1) PRIMARY KEY,
    MaHoaDon INT NOT NULL,
    MaThuoc INT NOT NULL,
    SoLuongBan INT NOT NULL,
    DonGiaBan DECIMAL(18,2) NOT NULL,
    ThanhTien AS (SoLuongBan * DonGiaBan) PERSISTED,

    CONSTRAINT FK_CTHD_HoaDon 
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),

    CONSTRAINT FK_CTHD_Thuoc 
    FOREIGN KEY (MaThuoc) REFERENCES Thuoc(MaThuoc),

    CONSTRAINT CK_CTHD_SoLuongBan CHECK (SoLuongBan > 0),
    CONSTRAINT CK_CTHD_DonGiaBan CHECK (DonGiaBan >= 0)
);

CREATE TABLE AuditLog (
    MaLog INT IDENTITY(1,1) PRIMARY KEY,
    ThoiGian DATETIME NOT NULL DEFAULT GETDATE(),
    MaNhanVien INT NULL,
    HanhDong NVARCHAR(100) NOT NULL,
    TenBang NVARCHAR(100),
    MaBanGhi NVARCHAR(50),
    NoiDung NVARCHAR(MAX),
    DiaChiMay NVARCHAR(100),

    CONSTRAINT FK_AuditLog_NhanVien 
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
);

-- INDEX --
CREATE INDEX IX_NhanVien_TenDangNhap 
ON NhanVien(TenDangNhap);

CREATE INDEX IX_NhanVien_MaVaiTro 
ON NhanVien(MaVaiTro);

CREATE INDEX IX_Thuoc_TenThuoc 
ON Thuoc(TenThuoc);

CREATE INDEX IX_Thuoc_NhomThuoc 
ON Thuoc(MaNhomThuoc);

CREATE INDEX IX_Thuoc_HanSuDung 
ON Thuoc(HanSuDung);

CREATE INDEX IX_Thuoc_TonKho 
ON Thuoc(SoLuongTon);

CREATE INDEX IX_PhieuNhap_NgayNhap 
ON PhieuNhap(NgayNhap);

CREATE INDEX IX_PhieuNhap_MaNhanVien 
ON PhieuNhap(MaNhanVien);

CREATE INDEX IX_HoaDon_NgayLap 
ON HoaDon(NgayLap);

CREATE INDEX IX_HoaDon_MaNhanVien 
ON HoaDon(MaNhanVien);

CREATE INDEX IX_CTHD_MaThuoc 
ON ChiTietHoaDon(MaThuoc);

CREATE INDEX IX_AuditLog_ThoiGian 
ON AuditLog(ThoiGian);

-- Procedure đăng nhập
CREATE PROCEDURE sp_DangNhap
    @TenDangNhap VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        nv.MaNhanVien,
        nv.HoTen,
        nv.TenDangNhap,
        nv.MatKhauHash,
        vt.TenVaiTro
    FROM NhanVien nv
    JOIN VaiTro vt ON nv.MaVaiTro = vt.MaVaiTro
    WHERE nv.TenDangNhap = @TenDangNhap
      AND nv.TrangThai = 1;
END;
GO

-- Procedure nhập kho có transaction
CREATE PROCEDURE sp_NhapKho
    @MaNhanVien INT,
    @MaThuoc INT,
    @SoLuongNhap INT,
    @DonGiaNhap DECIMAL(18,2),
    @HanSuDung DATE,
    @NhaCungCap NVARCHAR(150),
    @GhiChu NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @SoLuongNhap <= 0
            THROW 50001, N'Số lượng nhập phải lớn hơn 0.', 1;

        IF @DonGiaNhap < 0
            THROW 50002, N'Đơn giá nhập không hợp lệ.', 1;

        IF @HanSuDung IS NOT NULL AND @HanSuDung <= CAST(GETDATE() AS DATE)
            THROW 50003, N'Hạn sử dụng phải lớn hơn ngày hiện tại.', 1;

        DECLARE @MaPhieuNhap INT;

        INSERT INTO PhieuNhap(MaNhanVien, NhaCungCap, GhiChu)
        VALUES (@MaNhanVien, @NhaCungCap, @GhiChu);

        SET @MaPhieuNhap = SCOPE_IDENTITY();

        INSERT INTO ChiTietPhieuNhap(
            MaPhieuNhap, 
            MaThuoc, 
            SoLuongNhap, 
            DonGiaNhap, 
            HanSuDung
        )
        VALUES (
            @MaPhieuNhap, 
            @MaThuoc, 
            @SoLuongNhap, 
            @DonGiaNhap, 
            @HanSuDung
        );

        UPDATE Thuoc
        SET 
            SoLuongTon = SoLuongTon + @SoLuongNhap,
            GiaNhap = @DonGiaNhap,
            HanSuDung = @HanSuDung
        WHERE MaThuoc = @MaThuoc;

        INSERT INTO AuditLog(MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung)
        VALUES (
            @MaNhanVien,
            N'Nhập kho',
            N'PhieuNhap',
            CAST(@MaPhieuNhap AS NVARCHAR(50)),
            N'Nhập thuốc mã ' + CAST(@MaThuoc AS NVARCHAR(50)) 
            + N', số lượng ' + CAST(@SoLuongNhap AS NVARCHAR(50))
        );

        COMMIT;

        SELECT @MaPhieuNhap AS MaPhieuNhap;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

-- Procedure bán thuốc có transaction
CREATE PROCEDURE sp_BanThuoc
    @MaNhanVien INT,
    @MaThuoc INT,
    @SoLuongBan INT,
    @TenKhachHang NVARCHAR(100) = NULL,
    @SoDienThoai VARCHAR(15) = NULL,
    @GiamGia DECIMAL(18,2) = 0,
    @HinhThucThanhToan NVARCHAR(50) = N'Tiền mặt'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @SoLuongBan <= 0
            THROW 51001, N'Số lượng bán phải lớn hơn 0.', 1;

        IF @GiamGia < 0
            THROW 51002, N'Giảm giá không hợp lệ.', 1;

        DECLARE @SoLuongTon INT;
        DECLARE @GiaBan DECIMAL(18,2);
        DECLARE @HanSuDung DATE;

        SELECT 
            @SoLuongTon = SoLuongTon,
            @GiaBan = GiaBan,
            @HanSuDung = HanSuDung
        FROM Thuoc
        WHERE MaThuoc = @MaThuoc
          AND TrangThai = 1;

        IF @SoLuongTon IS NULL
            THROW 51003, N'Thuốc không tồn tại.', 1;

        IF @HanSuDung IS NOT NULL AND @HanSuDung < CAST(GETDATE() AS DATE)
            THROW 51004, N'Thuốc đã hết hạn, không được bán.', 1;

        IF @SoLuongTon < @SoLuongBan
            THROW 51005, N'Không đủ tồn kho để bán.', 1;

        DECLARE @MaHoaDon INT;
        DECLARE @TongTien DECIMAL(18,2);
        DECLARE @ThanhTien DECIMAL(18,2);

        SET @TongTien = @SoLuongBan * @GiaBan;
        SET @ThanhTien = @TongTien - @GiamGia;

        IF @ThanhTien < 0
            THROW 51006, N'Giảm giá không được lớn hơn tổng tiền.', 1;

        INSERT INTO HoaDon(
            MaNhanVien,
            TenKhachHang,
            SoDienThoai,
            TongTien,
            GiamGia,
            ThanhTien,
            HinhThucThanhToan,
            TrangThai
        )
        VALUES (
            @MaNhanVien,
            @TenKhachHang,
            @SoDienThoai,
            @TongTien,
            @GiamGia,
            @ThanhTien,
            @HinhThucThanhToan,
            N'Hoàn thành'
        );

        SET @MaHoaDon = SCOPE_IDENTITY();

        INSERT INTO ChiTietHoaDon(
            MaHoaDon,
            MaThuoc,
            SoLuongBan,
            DonGiaBan
        )
        VALUES (
            @MaHoaDon,
            @MaThuoc,
            @SoLuongBan,
            @GiaBan
        );

        UPDATE Thuoc
        SET SoLuongTon = SoLuongTon - @SoLuongBan
        WHERE MaThuoc = @MaThuoc;

        INSERT INTO AuditLog(MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung)
        VALUES (
            @MaNhanVien,
            N'Bán thuốc',
            N'HoaDon',
            CAST(@MaHoaDon AS NVARCHAR(50)),
            N'Bán thuốc mã ' + CAST(@MaThuoc AS NVARCHAR(50)) 
            + N', số lượng ' + CAST(@SoLuongBan AS NVARCHAR(50))
        );

        COMMIT;

        SELECT @MaHoaDon AS MaHoaDon;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END;
GO

USE PharmacyManagement;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @MaNVKho INT;
DECLARE @MaNVDuoc INT;
DECLARE @MaNVAdmin INT;
DECLARE @T1 INT;
DECLARE @T2 INT;
DECLARE @T3 INT;
DECLARE @T4 INT;
DECLARE @GhiChuMau NVARCHAR(255) = N'[ALN sample 11-13/05/2026]';
DECLARE @Pn INT;
DECLARE @Hd INT;

/* ------------------------------------------------------------------------- */
/* Khối A — Danh mục (style giống PharmacyManagement.sql, idempotent)        */
/* ------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro)
BEGIN
    INSERT INTO dbo.VaiTro (TenVaiTro, MoTa)
    VALUES
        (N'Admin', N'Quản trị toàn hệ thống'),
        (N'QuanLy', N'Quản lý nhà thuốc'),
        (N'DuocSi', N'Nhân viên bán thuốc'),
        (N'NhanVienKho', N'Nhân viên kho');

    PRINT N'Khối A: Đã chèn VaiTro.';
END
ELSE
    PRINT N'Khối A: Bảng VaiTro đã có dữ liệu — bỏ qua INSERT VaiTro.';

IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc)
BEGIN
    INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa)
    VALUES
        (N'Giảm đau - Hạ sốt', N'Nhóm thuốc giảm đau, hạ sốt'),
        (N'Kháng sinh', N'Nhóm thuốc kháng sinh'),
        (N'Vitamin', N'Nhóm vitamin và khoáng chất'),
        (N'Thực phẩm chức năng', N'Sản phẩm hỗ trợ sức khỏe');

    PRINT N'Khối A: Đã chèn NhomThuoc.';
END
ELSE
    PRINT N'Khối A: Bảng NhomThuoc đã có dữ liệu — bỏ qua INSERT NhomThuoc.';

IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien)
BEGIN
    INSERT INTO dbo.NhanVien (
        HoTen,
        TenDangNhap,
        MatKhauHash,
        SoDienThoai,
        Email,
        MaVaiTro
    )
    VALUES
        (N'Quản trị viên', N'admin', N'123456', N'0900000000', N'admin@pharmacy.com', 1),
        (N'Nguyễn Văn A', N'duocsi01', N'123456', N'0911111111', N'duocsi01@pharmacy.com', 3),
        (N'Trần Thị B', N'kho01', N'123456', N'0922222222', N'kho01@pharmacy.com', 4);

    PRINT N'Khối A: Đã chèn NhanVien.';
END
ELSE
    PRINT N'Khối A: Bảng NhanVien đã có dữ liệu — bỏ qua INSERT NhanVien.';

IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc)
BEGIN
    INSERT INTO dbo.Thuoc (
        TenThuoc,
        HoatChat,
        HamLuong,
        DonViTinh,
        GiaNhap,
        GiaBan,
        SoLuongTon,
        TonToiThieu,
        HanSuDung,
        MaNhomThuoc
    )
    VALUES
        (N'Panadol 500mg', N'Paracetamol', N'500mg', N'Viên', 800, 1200, 100, 20, CAST(N'2026-12-31' AS DATE), 1),
        (N'Hapacol 500mg', N'Paracetamol', N'500mg', N'Viên', 700, 1000, 150, 30, CAST(N'2026-10-30' AS DATE), 1),
        (N'Amoxicillin 500mg', N'Amoxicillin', N'500mg', N'Viên', 1500, 2500, 80, 20, CAST(N'2026-08-15' AS DATE), 2),
        (N'Vitamin C 500mg', N'Ascorbic Acid', N'500mg', N'Viên', 500, 900, 200, 50, CAST(N'2027-01-01' AS DATE), 3);

    PRINT N'Khối A: Đã chèn Thuoc.';
END
ELSE
    PRINT N'Khối A: Bảng Thuoc đã có dữ liệu — bỏ qua INSERT Thuoc.';

/* ------------------------------------------------------------------------- */
/* Khối B — Nghiệp vụ 11–13/05/2026 (phiếu nhập + hóa đơn)                   */
/* ------------------------------------------------------------------------- */

IF EXISTS (
    SELECT 1
    FROM dbo.PhieuNhap
    WHERE GhiChu = @GhiChuMau
)
BEGIN
    PRINT N'Khối B: Đã có phiếu nhập ghi chú mẫu — không chèn lại dữ liệu 11–13/05.';
END
ELSE
BEGIN
    SET @MaNVKho =
        (SELECT MaNhanVien FROM dbo.NhanVien WHERE TenDangNhap = N'kho01');
    SET @MaNVDuoc =
        (SELECT MaNhanVien FROM dbo.NhanVien WHERE TenDangNhap = N'duocsi01');
    SET @MaNVAdmin =
        (SELECT MaNhanVien FROM dbo.NhanVien WHERE TenDangNhap = N'admin');

    SET @T1 = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Panadol 500mg');
    SET @T2 = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Hapacol 500mg');
    SET @T3 = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Amoxicillin 500mg');
    SET @T4 = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Vitamin C 500mg');

    IF @MaNVKho IS NULL
        OR @MaNVDuoc IS NULL
        OR @T1 IS NULL
        OR @T2 IS NULL
        OR @T3 IS NULL
        OR @T4 IS NULL
    BEGIN
        RAISERROR(
            N'Khối B: Thiếu nhân viên (kho01/duocsi01) hoặc thuốc seed (Panadol/Hapacol/Amoxicillin/Vitamin C). Hoàn tất Khối A hoặc chạy PharmacyManagement.sql.',
            16,
            1
        );
    END
    ELSE
    BEGIN
        IF @MaNVAdmin IS NULL
            SET @MaNVAdmin = @MaNVDuoc;

        BEGIN TRANSACTION;

        BEGIN TRY
            INSERT INTO dbo.PhieuNhap (NgayNhap, MaNhanVien, NhaCungCap, TongTien, GhiChu)
            VALUES (CAST(N'2026-05-11T08:15:00' AS DATETIME), @MaNVKho, N'Công ty Dược Minh Anh', 0, @GhiChuMau);

            SET @Pn = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietPhieuNhap (MaPhieuNhap, MaThuoc, SoLuongNhap, DonGiaNhap, HanSuDung)
            VALUES
                (@Pn, @T1, 220, 810.00, CAST(N'2027-08-01' AS DATE)),
                (@Pn, @T2, 160, 705.00, CAST(N'2027-06-15' AS DATE)),
                (@Pn, @T3, 90, 1490.00, CAST(N'2027-03-20' AS DATE)),
                (@Pn, @T4, 120, 495.00, CAST(N'2028-01-10' AS DATE));

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-11T09:40:00' AS DATETIME), @MaNVDuoc, N'Lê Thị Lan', N'0912000101',
                0, 0, 0, N'Tiền mặt', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES
                (@Hd, @T1, 12, 1200.00),
                (@Hd, @T4, 8, 900.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-11T14:05:00' AS DATETIME), @MaNVDuoc, N'Phạm Văn Hùng', N'0912000102',
                0, 5000.00, 0, N'Chuyển khoản', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES
                (@Hd, @T2, 20, 1000.00),
                (@Hd, @T3, 6, 2500.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-11T16:50:00' AS DATETIME), @MaNVDuoc, N'Đơn hủy (mẫu)', N'0912000199',
                185000.00, 0, 185000.00, N'Tiền mặt', N'Đã hủy'
            );

            INSERT INTO dbo.PhieuNhap (NgayNhap, MaNhanVien, NhaCungCap, TongTien, GhiChu)
            VALUES (CAST(N'2026-05-12T07:45:00' AS DATETIME), @MaNVKho, N'Đại lý Dược Phúc An', 0, @GhiChuMau);

            SET @Pn = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietPhieuNhap (MaPhieuNhap, MaThuoc, SoLuongNhap, DonGiaNhap, HanSuDung)
            VALUES
                (@Pn, @T1, 80, 805.00, CAST(N'2027-09-01' AS DATE)),
                (@Pn, @T4, 60, 500.00, CAST(N'2028-02-01' AS DATE));

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-12T10:20:00' AS DATETIME), @MaNVDuoc, N'Hoàng Minh Tuấn', N'0912000201',
                0, 0, 0, N'Tiền mặt', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES (@Hd, @T1, 18, 1200.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-12T15:30:00' AS DATETIME), @MaNVAdmin, N'Nguyễn Thảo My', N'0912000202',
                0, 0, 0, N'QR Pay', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES
                (@Hd, @T2, 14, 1000.00),
                (@Hd, @T4, 22, 900.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-12T18:00:00' AS DATETIME), @MaNVDuoc, N'Khách chờ (mẫu)', N'0912000298',
                45000.00, 0, 45000.00, N'Tiền mặt', N'Chờ thanh toán'
            );

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-13T08:50:00' AS DATETIME), @MaNVDuoc, N'Trần Quốc Bảo', N'0912000301',
                0, 0, 0, N'Tiền mặt', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES
                (@Hd, @T3, 10, 2500.00),
                (@Hd, @T1, 6, 1200.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-13T12:10:00' AS DATETIME), @MaNVDuoc, N'Võ Thị Mai', N'0912000302',
                0, 3000.00, 0, N'Tiền mặt', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES (@Hd, @T4, 30, 900.00);

            INSERT INTO dbo.HoaDon (
                NgayLap, MaNhanVien, TenKhachHang, SoDienThoai,
                TongTien, GiamGia, ThanhTien, HinhThucThanhToan, TrangThai
            )
            VALUES (
                CAST(N'2026-05-13T19:25:00' AS DATETIME), @MaNVAdmin, N'Cửa hàng ABC (mẫu)', N'0912000303',
                0, 0, 0, N'Chuyển khoản', N'Hoàn thành'
            );
            SET @Hd = SCOPE_IDENTITY();
            INSERT INTO dbo.ChiTietHoaDon (MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
            VALUES
                (@Hd, @T1, 25, 1200.00),
                (@Hd, @T2, 12, 1000.00),
                (@Hd, @T3, 4, 2500.00);

            COMMIT TRANSACTION;
            PRINT N'Khối B: Đã nạp phiếu nhập + hóa đơn mẫu 11–13/05/2026.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            THROW;
        END CATCH;
    END;
END;

PRINT N'';
PRINT N'Kiểm tra: SELECT * FROM vw_DoanhThuTheoNgay WHERE Ngay BETWEEN ''2026-05-11'' AND ''2026-05-13'';';
GO

/*
--------------------------------------------------------------------------------
Encoding / Unicode (tiếng Việt)
--------------------------------------------------------------------------------
- Lưu và chạy script SQL dưới dạng UTF-8 (khuyến nghị có BOM) để literal N''...'' không bị sai byte.
- Trạng thái hóa đơn chuẩn: N''Hoàn thành'', N''Đã hủy'', N''Chờ thanh toán''.
- Ứng dụng (DAL) so khớp thêm biến thể mojibake cho ''Hoàn thành'' khi dữ liệu cũ lưu sai UTF-8/Latin-1;
  BLL chuẩn hóa chuỗi hiển thị dashboard (UnicodeTextHelper).
--------------------------------------------------------------------------------
*/
GO
