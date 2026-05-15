/*
  Thứ tự triển khai CSDL (khớp project_Context.md):
  1) PharmacyManagement.sql — tạo DB, bảng, chỉ mục, thủ tục, dữ liệu mẫu
  2) Trigger_PharmacyManagemnt.sql — cột bổ sung (nếu thiếu), trigger, migration trạng thái phiếu
  3) View_PharmacyManagement.sql — view báo cáo / dashboard
*/
CREATE DATABASE PharmacyManagement;
GO

USE PharmacyManagement;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
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

CREATE TABLE Kho (
    MaKho INT IDENTITY(1,1) PRIMARY KEY,
    TenKho NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    TrangThai BIT DEFAULT 1
);

CREATE TABLE NhaCungCap (
    MaNhaCungCap INT IDENTITY(1,1) PRIMARY KEY,
    TenNhaCungCap NVARCHAR(150) NOT NULL,
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(255),
    TrangThai BIT DEFAULT 1
);

CREATE TABLE DanhMucDQG (
    MaDQG INT IDENTITY(1,1) PRIMARY KEY,
    MaDQGDonVi VARCHAR(50),
    TenHangHoa NVARCHAR(150) NOT NULL,
    SoDangKy VARCHAR(50),
    HoatChatChinh NVARCHAR(255),
    HoatChatDangKy NVARCHAR(255),
    HamLuong NVARCHAR(100),
    DongGoi NVARCHAR(150),
    HangSanXuat NVARCHAR(150),
    NuocSanXuat NVARCHAR(100),
    DonViTinh NVARCHAR(30),
    TrangThai BIT DEFAULT 1
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
    MaDQG INT NULL,
    SoDangKy VARCHAR(50) NULL,
    HangSanXuat NVARCHAR(150) NULL,
    NuocSanXuat NVARCHAR(100) NULL,
    DongGoi NVARCHAR(150) NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Thuoc_NhomThuoc 
    FOREIGN KEY (MaNhomThuoc) REFERENCES NhomThuoc(MaNhomThuoc),

    CONSTRAINT FK_Thuoc_DanhMucDQG
    FOREIGN KEY (MaDQG) REFERENCES DanhMucDQG(MaDQG),

    CONSTRAINT CK_Thuoc_GiaNhap CHECK (GiaNhap >= 0),
    CONSTRAINT CK_Thuoc_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_Thuoc_SoLuongTon CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_Thuoc_TonToiThieu CHECK (TonToiThieu >= 0)
);

CREATE TABLE PhieuNhap (
    MaPhieuNhap INT IDENTITY(1,1) PRIMARY KEY,
    NgayNhap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNhanVien INT NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(255),
    SoHoaDon VARCHAR(50) NULL,
    NgayHoaDon DATE NULL,
    LoaiPhieuNhap NVARCHAR(50) NULL,
    MaKho INT NULL,
    MaNhaCungCap INT NULL,
    PhuongTienVanChuyen NVARCHAR(100) NULL,
    DonViVanChuyen NVARCHAR(150) NULL,
    NguoiGiaoHang NVARCHAR(100) NULL,
    VAT DECIMAL(5,2) DEFAULT 0,
    ChietKhau DECIMAL(18,2) DEFAULT 0,
    CongNo DECIMAL(18,2) DEFAULT 0,
    TrangThai NVARCHAR(30) DEFAULT N'Đang lập',

    CONSTRAINT FK_PhieuNhap_NhanVien 
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),

    CONSTRAINT FK_PhieuNhap_Kho
    FOREIGN KEY (MaKho) REFERENCES Kho(MaKho),

    CONSTRAINT FK_PhieuNhap_NhaCungCap
    FOREIGN KEY (MaNhaCungCap) REFERENCES NhaCungCap(MaNhaCungCap),

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
    GiaBan DECIMAL(18,2) DEFAULT 0,
    SoLo VARCHAR(50) NULL,
    ViTri NVARCHAR(100) NULL,
    GhiChu NVARCHAR(255) NULL,
    /* VAT dòng (%): nếu NULL, áp dụng VAT % trên cột PhieuNhap.VAT khi tính CongNo */
    VAT DECIMAL(5,2) NULL,

    CONSTRAINT FK_CTPN_PhieuNhap 
    FOREIGN KEY (MaPhieuNhap) REFERENCES PhieuNhap(MaPhieuNhap),

    CONSTRAINT FK_CTPN_Thuoc 
    FOREIGN KEY (MaThuoc) REFERENCES Thuoc(MaThuoc),

    CONSTRAINT CK_CTPN_SoLuongNhap CHECK (SoLuongNhap > 0),
    CONSTRAINT CK_CTPN_DonGiaNhap CHECK (DonGiaNhap >= 0)
);

CREATE TABLE LoThuoc (
    MaLoThuoc INT IDENTITY(1,1) PRIMARY KEY,
    MaThuoc INT NOT NULL,
    MaKho INT NOT NULL,
    SoLo VARCHAR(50) NOT NULL,
    HanSuDung DATE NOT NULL,
    SoLuongTon INT NOT NULL DEFAULT 0,
    GiaNhap DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaBan DECIMAL(18,2) NOT NULL DEFAULT 0,
    ViTri NVARCHAR(100),
    TrangThai BIT DEFAULT 1,

    CONSTRAINT FK_LoThuoc_Thuoc
    FOREIGN KEY (MaThuoc) REFERENCES Thuoc(MaThuoc),

    CONSTRAINT FK_LoThuoc_Kho
    FOREIGN KEY (MaKho) REFERENCES Kho(MaKho),

    CONSTRAINT CK_LoThuoc_SoLuongTon CHECK (SoLuongTon >= 0)
);

/*
  Luồng CSDL khách hàng / hóa đơn (CCCD làm định danh):
  1) KhachHang: PRIMARY KEY = CCCD (12 chữ số). Một CCCD = một hồ sơ duy nhất.
  2) Tra cứu: SELECT * FROM KhachHang WHERE CCCD = @cccd → hiển thị HoTen, SĐT, Địa chỉ trên GUI.
  3) Chưa có CCCD: INSERT KhachHang rồi dùng CCCD vừa tạo cho các hóa đơn sau.
  4) HoaDon: chỉ lưu CCCD (FK → KhachHang). Không denormalize tên/SĐT; JOIN hoặc VIEW khi in/ báo cáo.
  5) Trigger trg_HoaDon_KiemTraCCCD: khi INSERT/UPDATE HoaDon có CCCD → khách phải tồn tại và TrangThai = 1.
  6) Lịch sử mua: vw_LichSuMuaHangTheoCCCD / vw_HoaDon_ThongTinKhachHang WHERE CCCD = @cccd.
*/
CREATE TABLE KhachHang (
    CCCD CHAR(12) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15) NULL,
    NgaySinh DATE NULL,
    DiaChi NVARCHAR(255) NULL,
    GhiChu NVARCHAR(255) NULL,
    TrangThai BIT NOT NULL CONSTRAINT DF_KhachHang_TrangThai DEFAULT 1,
    NgayTao DATETIME NOT NULL CONSTRAINT DF_KhachHang_NgayTao DEFAULT GETDATE(),

    CONSTRAINT PK_KhachHang PRIMARY KEY (CCCD),
    CONSTRAINT CK_KhachHang_CCCD
        CHECK (CCCD NOT LIKE '%[^0-9]%' AND LEN(CCCD) = 12),
    CONSTRAINT CK_KhachHang_HoTen
        CHECK (LEN(LTRIM(RTRIM(HoTen))) > 0)
);

CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap DATETIME NOT NULL CONSTRAINT DF_HoaDon_NgayLap DEFAULT GETDATE(),
    MaNhanVien INT NOT NULL,
    CCCD CHAR(12) NULL,
    TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_TongTien DEFAULT 0,
    GiamGia DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_GiamGia DEFAULT 0,
    ThanhTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_ThanhTien DEFAULT 0,
    HinhThucThanhToan NVARCHAR(50) NOT NULL CONSTRAINT DF_HoaDon_HinhThucTT DEFAULT N'Tiền mặt',
    TrangThai NVARCHAR(30) NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT N'Hoàn thành',

    CONSTRAINT FK_HoaDon_NhanVien 
        FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien),

    CONSTRAINT FK_HoaDon_KhachHang
        FOREIGN KEY (CCCD) REFERENCES KhachHang(CCCD),

    CONSTRAINT CK_HoaDon_CCCD
        CHECK (CCCD IS NULL OR (CCCD NOT LIKE '%[^0-9]%' AND LEN(CCCD) = 12)),
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

/* Phân bổ xuất bán theo lô (FEFO) — một dòng ChiTietHoaDon có thể tách nhiều lô */
CREATE TABLE ChiTietHoaDon_PhanBoLo (
    MaPhanBo INT IDENTITY(1,1) PRIMARY KEY,
    MaCTHD INT NOT NULL,
    MaLoThuoc INT NOT NULL,
    SoLuongXuat INT NOT NULL,

    CONSTRAINT FK_PhanBoLo_CTHD
        FOREIGN KEY (MaCTHD) REFERENCES ChiTietHoaDon(MaCTHD) ON DELETE CASCADE,
    CONSTRAINT FK_PhanBoLo_LoThuoc
        FOREIGN KEY (MaLoThuoc) REFERENCES LoThuoc(MaLoThuoc),
    CONSTRAINT CK_PhanBoLo_SoLuongXuat CHECK (SoLuongXuat > 0)
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

CREATE INDEX IX_Thuoc_MaDQG
ON Thuoc(MaDQG);

CREATE INDEX IX_PhieuNhap_NgayNhap 
ON PhieuNhap(NgayNhap);

CREATE INDEX IX_PhieuNhap_MaNhanVien 
ON PhieuNhap(MaNhanVien);

CREATE INDEX IX_PhieuNhap_MaKho
ON PhieuNhap(MaKho);

CREATE INDEX IX_PhieuNhap_MaNhaCungCap
ON PhieuNhap(MaNhaCungCap);

CREATE INDEX IX_LoThuoc_MaThuoc
ON LoThuoc(MaThuoc);

CREATE INDEX IX_LoThuoc_MaKho
ON LoThuoc(MaKho);

/* PK clustered trên CCCD — tra cứu theo CCCD dùng clustered index sẵn có */
CREATE INDEX IX_KhachHang_HoTen
ON KhachHang(HoTen)
INCLUDE (SoDienThoai, DiaChi, TrangThai);

CREATE INDEX IX_KhachHang_SoDienThoai
ON KhachHang(SoDienThoai)
WHERE SoDienThoai IS NOT NULL;

CREATE INDEX IX_HoaDon_NgayLap 
ON HoaDon(NgayLap);

CREATE INDEX IX_HoaDon_MaNhanVien 
ON HoaDon(MaNhanVien);

CREATE INDEX IX_HoaDon_CCCD
ON HoaDon(CCCD)
INCLUDE (NgayLap, ThanhTien, TrangThai)
WHERE CCCD IS NOT NULL;

CREATE INDEX IX_CTHD_MaThuoc 
ON ChiTietHoaDon(MaThuoc);

CREATE INDEX IX_CTPN_MaPhieuNhap
ON ChiTietPhieuNhap(MaPhieuNhap);

CREATE INDEX IX_PhieuNhap_TrangThai
ON PhieuNhap(TrangThai);

CREATE INDEX IX_LoThuoc_FEFO
ON LoThuoc(MaThuoc, HanSuDung)
INCLUDE (SoLuongTon, MaLoThuoc, MaKho)
WHERE TrangThai = 1 AND SoLuongTon > 0;

CREATE INDEX IX_PhanBoLo_MaCTHD
ON ChiTietHoaDon_PhanBoLo(MaCTHD);

CREATE INDEX IX_PhanBoLo_MaLoThuoc
ON ChiTietHoaDon_PhanBoLo(MaLoThuoc);

CREATE INDEX IX_AuditLog_ThoiGian 
ON AuditLog(ThoiGian);
GO

/* Hằng trạng thái phiếu nhập (NCHAR — tránh lệch encoding khi chạy script/trigger trên Windows) */
CREATE OR ALTER FUNCTION dbo.fn_TrangThai_DangLap()
RETURNS NVARCHAR(30) WITH SCHEMABINDING
AS
BEGIN
    RETURN NCHAR(272) + NCHAR(97) + NCHAR(110) + NCHAR(103) + N' ' + NCHAR(108) + NCHAR(7853) + NCHAR(112);
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_TrangThai_DaNhapKho()
RETURNS NVARCHAR(30) WITH SCHEMABINDING
AS
BEGIN
    RETURN NCHAR(272) + NCHAR(227) + N' nh' + NCHAR(7853) + N'p kho';
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_TrangThai_Luu()
RETURNS NVARCHAR(30) WITH SCHEMABINDING
AS
BEGIN
    RETURN NCHAR(76) + NCHAR(432) + NCHAR(117);
END;
GO

-- Procedure đăng nhập
CREATE OR ALTER PROCEDURE dbo.sp_DangNhap
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

-- Procedure nhập kho: lập phiếu (Đang lập) -> chi tiết -> xác nhận Đã nhập kho (trigger cộng LoThuoc)
CREATE OR ALTER PROCEDURE dbo.sp_NhapKho
    @MaNhanVien INT,
    @MaThuoc INT,
    @SoLuongNhap INT,
    @DonGiaNhap DECIMAL(18,2),
    @HanSuDung DATE,
    @NhaCungCap NVARCHAR(150) = NULL,
    @GhiChu NVARCHAR(255) = NULL,
    @MaKho INT = NULL,
    @SoLo NVARCHAR(50) = NULL,
    @GiaBanDong DECIMAL(18,2) = NULL,
    @ViTri NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @SoLuongNhap <= 0
            THROW 50001, N'Số lượng nhập phải lớn hơn 0.', 1;

        IF @DonGiaNhap < 0
            THROW 50002, N'Đơn giá nhập không hợp lệ.', 1;

        IF @HanSuDung IS NULL
            THROW 50003, N'Hạn sử dụng bắt buộc khi nhập kho theo lô.', 1;

        IF @HanSuDung <= CAST(GETDATE() AS DATE)
            THROW 50004, N'Hạn sử dụng phải lớn hơn ngày hiện tại.', 1;

        IF NOT EXISTS (SELECT 1 FROM Thuoc WHERE MaThuoc = @MaThuoc AND TrangThai = 1)
            THROW 50005, N'Thuốc không tồn tại hoặc đã ngừng kinh doanh.', 1;

        IF @MaKho IS NULL
            SELECT TOP (1) @MaKho = MaKho FROM Kho WHERE ISNULL(TrangThai, 1) = 1 ORDER BY MaKho;

        IF @MaKho IS NULL OR NOT EXISTS (SELECT 1 FROM Kho WHERE MaKho = @MaKho)
            THROW 50006, N'Chưa có kho hợp lệ (MaKho).', 1;

        IF NULLIF(LTRIM(RTRIM(@SoLo)), N'') IS NULL
            SET @SoLo = CONCAT(N'NK-AUTO-', CONVERT(VARCHAR(8), GETDATE(), 112), N'-', ABS(CHECKSUM(NEWID())) % 1000000);

        DECLARE @MaPhieuNhap INT;
        DECLARE @MaNhaCungCap INT = NULL;
        DECLARE @GiaBanThuoc DECIMAL(18,2);

        IF NULLIF(LTRIM(RTRIM(@NhaCungCap)), N'') IS NOT NULL
        BEGIN
            SELECT @MaNhaCungCap = MaNhaCungCap
            FROM NhaCungCap
            WHERE TenNhaCungCap = @NhaCungCap;

            IF @MaNhaCungCap IS NULL
            BEGIN
                INSERT INTO NhaCungCap (TenNhaCungCap)
                VALUES (@NhaCungCap);
                SET @MaNhaCungCap = SCOPE_IDENTITY();
            END
        END

        SELECT @GiaBanThuoc = ISNULL(@GiaBanDong, GiaBan) FROM Thuoc WHERE MaThuoc = @MaThuoc;

        INSERT INTO PhieuNhap (MaNhanVien, MaNhaCungCap, MaKho, GhiChu, TrangThai)
        VALUES (@MaNhanVien, @MaNhaCungCap, @MaKho, @GhiChu, dbo.fn_TrangThai_DangLap());

        SET @MaPhieuNhap = SCOPE_IDENTITY();

        INSERT INTO ChiTietPhieuNhap (
            MaPhieuNhap,
            MaThuoc,
            SoLuongNhap,
            DonGiaNhap,
            HanSuDung,
            GiaBan,
            SoLo,
            ViTri
        )
        VALUES (
            @MaPhieuNhap,
            @MaThuoc,
            @SoLuongNhap,
            @DonGiaNhap,
            @HanSuDung,
            ISNULL(@GiaBanThuoc, 0),
            @SoLo,
            @ViTri
        );

        /* Trigger trg_NhapKho_KhiHoanTat cộng LoThuoc + đồng bộ Thuoc; trg_UpdateTongTienPhieuNhap cập nhật tổng */
        UPDATE PhieuNhap
        SET TrangThai = dbo.fn_TrangThai_DaNhapKho()
        WHERE MaPhieuNhap = @MaPhieuNhap;

        COMMIT;

        SELECT @MaPhieuNhap AS MaPhieuNhap;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

-- Procedure bán thuốc có transaction
CREATE OR ALTER PROCEDURE dbo.sp_BanThuoc
    @MaNhanVien INT,
    @MaThuoc INT,
    @SoLuongBan INT,
    @CCCD CHAR(12) = NULL,
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

        SET @CCCD = NULLIF(LTRIM(RTRIM(@CCCD)), '');

        IF @CCCD IS NOT NULL
        BEGIN
            IF @CCCD LIKE '%[^0-9]%' OR LEN(@CCCD) <> 12
                THROW 51008, N'CCCD phải gồm đúng 12 chữ số.', 1;

            IF NOT EXISTS (
                SELECT 1 FROM dbo.KhachHang kh
                WHERE kh.CCCD = @CCCD AND kh.TrangThai = 1
            )
                THROW 51007, N'CCCD chưa có trong danh mục khách hàng hoặc khách đã ngừng.', 1;
        END

        DECLARE @GiaBan DECIMAL(18,2);
        DECLARE @TonLoHopLe INT;

        SELECT @GiaBan = GiaBan
        FROM Thuoc
        WHERE MaThuoc = @MaThuoc
          AND TrangThai = 1;

        IF @GiaBan IS NULL
            THROW 51003, N'Thuốc không tồn tại hoặc đã ngừng kinh doanh.', 1;

        SELECT @TonLoHopLe = ISNULL(SUM(SoLuongTon), 0)
        FROM LoThuoc
        WHERE MaThuoc = @MaThuoc
          AND SoLuongTon > 0
          AND HanSuDung >= CAST(GETDATE() AS DATE)
          AND ISNULL(TrangThai, 1) = 1;

        IF @TonLoHopLe < @SoLuongBan
            THROW 51005, N'Không đủ tồn kho theo lô (còn hạn) để bán.', 1;

        DECLARE @MaHoaDon INT;
        DECLARE @TongTien DECIMAL(18,2);
        DECLARE @ThanhTien DECIMAL(18,2);

        SET @TongTien = @SoLuongBan * @GiaBan;
        SET @ThanhTien = @TongTien - @GiamGia;

        IF @ThanhTien < 0
            THROW 51006, N'Giảm giá không được lớn hơn tổng tiền.', 1;

        INSERT INTO HoaDon(
            MaNhanVien,
            CCCD,
            TongTien,
            GiamGia,
            ThanhTien,
            HinhThucThanhToan,
            TrangThai
        )
        VALUES (
            @MaNhanVien,
            @CCCD,
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

        /* Tồn theo lô: INSTEAD OF + AFTER trigger trên ChiTietHoaDon (FEFO) */

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
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* Trigger: khi gắn CCCD trên hóa đơn, khách phải tồn tại và còn hiệu lực (bổ sung FK) */
CREATE OR ALTER TRIGGER dbo.trg_HoaDon_KiemTraCCCD
ON dbo.HoaDon
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.CCCD IS NOT NULL
          AND NOT EXISTS (
                SELECT 1
                FROM dbo.KhachHang kh
                WHERE kh.CCCD = i.CCCD
                  AND kh.TrangThai = 1
          )
    )
    BEGIN
        THROW 51010, N'CCCD trên hóa đơn phải tồn tại trong KhachHang và TrangThai = 1.', 1;
    END
END;
GO

USE PharmacyManagement;
GO

/* ========================================================================
   DỮ LIỆU MẪU CHUYÊN NGHIỆP CHO PharmacyManagement
   - Chống trùng dữ liệu khi chạy lại script
   - Có dữ liệu Danh mục Dược Quốc Gia
   - Có phiếu nhập kho đúng luồng: lập phiếu -> thêm hàng nhập -> tra DQG
   ======================================================================== */
USE PharmacyManagement;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* 1. Danh mục nền */
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE TenVaiTro = N'Admin')
        INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES (N'Admin', N'Quản trị toàn hệ thống');
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE TenVaiTro = N'QuanLy')
        INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES (N'QuanLy', N'Quản lý nhà thuốc');
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE TenVaiTro = N'DuocSi')
        INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES (N'DuocSi', N'Dược sĩ bán thuốc, tư vấn thuốc');
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE TenVaiTro = N'NhanVienKho')
        INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES (N'NhanVienKho', N'Nhân viên lập phiếu nhập kho, kiểm kho');

    IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Giảm đau - Hạ sốt')
        INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa) VALUES (N'Giảm đau - Hạ sốt', N'Thuốc giảm đau, hạ sốt thông dụng');
    IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Kháng sinh')
        INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa) VALUES (N'Kháng sinh', N'Thuốc kháng sinh dùng theo đơn');
    IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Vitamin - Khoáng chất')
        INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa) VALUES (N'Vitamin - Khoáng chất', N'Vitamin và khoáng chất');
    IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Tiêu hóa')
        INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa) VALUES (N'Tiêu hóa', N'Thuốc hỗ trợ tiêu hóa, dạ dày');
    IF NOT EXISTS (SELECT 1 FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Dị ứng')
        INSERT INTO dbo.NhomThuoc (TenNhomThuoc, MoTa) VALUES (N'Dị ứng', N'Thuốc kháng histamin, chống dị ứng');

    IF NOT EXISTS (SELECT 1 FROM dbo.Kho WHERE TenKho = N'Kho chính')
        INSERT INTO dbo.Kho (TenKho, DiaChi) VALUES (N'Kho chính', N'Khu vực bảo quản chính của nhà thuốc');
    IF NOT EXISTS (SELECT 1 FROM dbo.Kho WHERE TenKho = N'Quầy bán')
        INSERT INTO dbo.Kho (TenKho, DiaChi) VALUES (N'Quầy bán', N'Khu vực quầy bán lẻ');

    IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE TenNhaCungCap = N'Công ty Dược Minh Anh')
        INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi) VALUES (N'Công ty Dược Minh Anh', '02839990001', N'TP. Hồ Chí Minh');
    IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungCap WHERE TenNhaCungCap = N'Đại lý Dược Phúc An')
        INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, DiaChi) VALUES (N'Đại lý Dược Phúc An', '02438880002', N'Hà Nội');

    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE TenDangNhap = 'admin')
        INSERT INTO dbo.NhanVien (HoTen, TenDangNhap, MatKhauHash, SoDienThoai, Email, MaVaiTro)
        SELECT N'Quản trị viên', 'admin', '123456', '0900000000', 'admin@pharmacy.local', MaVaiTro FROM dbo.VaiTro WHERE TenVaiTro = N'Admin';
    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE TenDangNhap = 'duocsi01')
        INSERT INTO dbo.NhanVien (HoTen, TenDangNhap, MatKhauHash, SoDienThoai, Email, MaVaiTro)
        SELECT N'Nguyễn Văn A', 'duocsi01', '123456', '0911111111', 'duocsi01@pharmacy.local', MaVaiTro FROM dbo.VaiTro WHERE TenVaiTro = N'DuocSi';
    IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien WHERE TenDangNhap = 'kho01')
        INSERT INTO dbo.NhanVien (HoTen, TenDangNhap, MatKhauHash, SoDienThoai, Email, MaVaiTro)
        SELECT N'Trần Thị B', 'kho01', '123456', '0922222222', 'kho01@pharmacy.local', MaVaiTro FROM dbo.VaiTro WHERE TenVaiTro = N'NhanVienKho';

    /* 1b. Khách hàng mẫu — CCCD là PK (có thể trùng họ tên, không trùng CCCD) */
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001234')
        INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
        VALUES ('079085001234', N'Nguyễn Văn An', '0901000001', '1985-03-12', N'Quận 1, TP.HCM');
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001235')
        INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
        VALUES ('079085001235', N'Nguyễn Văn An', '0901000002', '1992-07-25', N'Quận 7, TP.HCM');
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001236')
        INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
        VALUES ('079085001236', N'Trần Thị Mai', '0912000003', '1978-11-08', N'Quận 3, TP.HCM');
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001237')
        INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
        VALUES ('079085001237', N'Lê Hoàng Nam', '0988000004', '2000-01-15', N'Quận Bình Thạnh, TP.HCM');

    /* 2. Danh mục thuốc Dược Quốc Gia mẫu */
    DECLARE @DQG TABLE(
        MaDQGDonVi VARCHAR(50), TenHangHoa NVARCHAR(150), SoDangKy VARCHAR(50),
        HoatChatChinh NVARCHAR(255), HoatChatDangKy NVARCHAR(255), HamLuong NVARCHAR(100),
        DongGoi NVARCHAR(150), HangSanXuat NVARCHAR(150), NuocSanXuat NVARCHAR(100), DonViTinh NVARCHAR(30)
    );

    INSERT INTO @DQG VALUES
    ('DQG00012947', N'Harcotin', 'VD-21602-14', N'Atorvastatin', N'Atorvastatin calcium 10mg', N'10mg', N'Hộp 10 vỉ x 10 viên', N'Xí nghiệp dược phẩm 150', N'Việt Nam', N'Viên'),
    ('DQG00024581', N'Paracetamol STADA', 'VD-31452-19', N'Paracetamol', N'Paracetamol 500mg', N'500mg', N'Hộp 10 vỉ x 10 viên nén', N'STADA Việt Nam', N'Việt Nam', N'Viên'),
    ('DQG00038741', N'Amoxicillin 500', 'VD-22871-15', N'Amoxicillin', N'Amoxicillin trihydrate 500mg', N'500mg', N'Hộp 10 vỉ x 10 viên nang', N'Công ty cổ phần dược Hậu Giang', N'Việt Nam', N'Viên'),
    ('DQG00048125', N'Cefixim 200', 'VD-27911-17', N'Cefixime', N'Cefixime 200mg', N'200mg', N'Hộp 2 vỉ x 10 viên', N'Pymepharco', N'Việt Nam', N'Viên'),
    ('DQG00051263', N'Augmentin 625mg', 'VN-18932-15', N'Amoxicillin + Acid clavulanic', N'Amoxicillin 500mg + Clavulanic acid 125mg', N'625mg', N'Hộp 2 vỉ x 7 viên', N'GlaxoSmithKline', N'Anh', N'Viên'),
    ('DQG00062314', N'Panadol Extra', 'VN-10231-10', N'Paracetamol + Caffeine', N'Paracetamol 500mg + Caffeine 65mg', N'565mg', N'Hộp 15 vỉ x 12 viên', N'GlaxoSmithKline', N'Anh', N'Viên'),
    ('DQG00073492', N'Metformin 500mg', 'VD-19832-13', N'Metformin hydrochloride', N'Metformin hydrochloride 500mg', N'500mg', N'Hộp 10 vỉ x 10 viên', N'US Pharma USA', N'Việt Nam', N'Viên'),
    ('DQG00084521', N'Diamicron MR', 'VN-17742-14', N'Gliclazide', N'Gliclazide 30mg', N'30mg', N'Hộp 2 vỉ x 15 viên', N'Les Laboratoires Servier', N'Pháp', N'Viên'),
    ('DQG00095632', N'Voltaren 50mg', 'VN-13542-11', N'Diclofenac', N'Diclofenac sodium 50mg', N'50mg', N'Hộp 2 vỉ x 10 viên', N'Novartis', N'Thụy Sĩ', N'Viên'),
    ('DQG00106743', N'Alpha Choay', 'VN-15236-12', N'Alpha chymotrypsin', N'Alpha chymotrypsin 4.2mg', N'4.2mg', N'Hộp 2 vỉ x 10 viên', N'Sanofi', N'Pháp', N'Viên'),
    ('DQG00117854', N'Clarithromycin 500mg', 'VD-25231-16', N'Clarithromycin', N'Clarithromycin 500mg', N'500mg', N'Hộp 1 vỉ x 10 viên', N'Imexpharm', N'Việt Nam', N'Viên'),
    ('DQG00128965', N'Zinnat 500mg', 'VN-19452-15', N'Cefuroxime', N'Cefuroxime axetil 500mg', N'500mg', N'Hộp 2 vỉ x 5 viên', N'GlaxoSmithKline', N'Anh', N'Viên'),
    ('DQG00139076', N'Loratadin 10mg', 'VD-30145-18', N'Loratadine', N'Loratadine 10mg', N'10mg', N'Hộp 10 vỉ x 10 viên', N'Dược phẩm Hà Tây', N'Việt Nam', N'Viên'),
    ('DQG00140187', N'Cetirizine STADA', 'VD-22117-15', N'Cetirizine', N'Cetirizine dihydrochloride 10mg', N'10mg', N'Hộp 2 vỉ x 10 viên', N'STADA Việt Nam', N'Việt Nam', N'Viên'),
    ('DQG00151298', N'Oresol', 'VD-11245-10', N'Glucose + Sodium chloride + Potassium chloride', N'Oresol pha dung dịch uống', N'27.9g', N'Hộp 20 gói', N'Dược phẩm OPC', N'Việt Nam', N'Gói'),
    ('DQG00162409', N'Vitamin C 500mg', 'VD-20871-14', N'Acid ascorbic', N'Acid ascorbic 500mg', N'500mg', N'Hộp 10 vỉ x 10 viên', N'Dược phẩm Nam Hà', N'Việt Nam', N'Viên'),
    ('DQG00173510', N'Omeprazole 20mg', 'VD-31542-19', N'Omeprazole', N'Omeprazole 20mg', N'20mg', N'Hộp 3 vỉ x 10 viên', N'Hassan-Dermapharm', N'Việt Nam', N'Viên'),
    ('DQG00184621', N'Efferalgan 500mg', 'VN-12452-11', N'Paracetamol', N'Paracetamol 500mg', N'500mg', N'Hộp 4 vỉ x 4 viên sủi', N'UPSA SAS', N'Pháp', N'Viên sủi'),
    ('DQG00195732', N'Bromhexin 8mg', 'VD-17854-12', N'Bromhexine hydrochloride', N'Bromhexine hydrochloride 8mg', N'8mg', N'Hộp 10 vỉ x 10 viên', N'Traphaco', N'Việt Nam', N'Viên'),
    ('DQG00206843', N'Methylprednisolone 16mg', 'VD-28654-18', N'Methylprednisolone', N'Methylprednisolone 16mg', N'16mg', N'Hộp 3 vỉ x 10 viên', N'Pfizer', N'Mỹ', N'Viên');

    MERGE dbo.DanhMucDQG AS target
    USING @DQG AS src
       ON target.MaDQGDonVi = src.MaDQGDonVi
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MaDQGDonVi, TenHangHoa, SoDangKy, HoatChatChinh, HoatChatDangKy, HamLuong, DongGoi, HangSanXuat, NuocSanXuat, DonViTinh)
        VALUES (src.MaDQGDonVi, src.TenHangHoa, src.SoDangKy, src.HoatChatChinh, src.HoatChatDangKy, src.HamLuong, src.DongGoi, src.HangSanXuat, src.NuocSanXuat, src.DonViTinh)
    WHEN MATCHED THEN
        UPDATE SET TenHangHoa = src.TenHangHoa, SoDangKy = src.SoDangKy, HoatChatChinh = src.HoatChatChinh,
                   HoatChatDangKy = src.HoatChatDangKy, HamLuong = src.HamLuong, DongGoi = src.DongGoi,
                   HangSanXuat = src.HangSanXuat, NuocSanXuat = src.NuocSanXuat, DonViTinh = src.DonViTinh;

    /* 3. Thuốc nội bộ liên kết DQG */
    DECLARE @NhomGiamDau INT = (SELECT MaNhomThuoc FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Giảm đau - Hạ sốt');
    DECLARE @NhomKhangSinh INT = (SELECT MaNhomThuoc FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Kháng sinh');
    DECLARE @NhomVitamin INT = (SELECT MaNhomThuoc FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Vitamin - Khoáng chất');
    DECLARE @NhomDiUng INT = (SELECT MaNhomThuoc FROM dbo.NhomThuoc WHERE TenNhomThuoc = N'Dị ứng');

    IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc WHERE TenThuoc = N'Paracetamol STADA')
        INSERT INTO dbo.Thuoc (TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, TonToiThieu, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi)
        SELECT TenHangHoa, HoatChatChinh, HamLuong, DonViTinh, 700, 1000, 30, @NhomGiamDau, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi FROM dbo.DanhMucDQG WHERE MaDQGDonVi = 'DQG00024581';
    IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc WHERE TenThuoc = N'Amoxicillin 500')
        INSERT INTO dbo.Thuoc (TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, TonToiThieu, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi)
        SELECT TenHangHoa, HoatChatChinh, HamLuong, DonViTinh, 1500, 2500, 20, @NhomKhangSinh, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi FROM dbo.DanhMucDQG WHERE MaDQGDonVi = 'DQG00038741';
    IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc WHERE TenThuoc = N'Vitamin C 500mg')
        INSERT INTO dbo.Thuoc (TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, TonToiThieu, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi)
        SELECT TenHangHoa, HoatChatChinh, HamLuong, DonViTinh, 500, 900, 50, @NhomVitamin, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi FROM dbo.DanhMucDQG WHERE MaDQGDonVi = 'DQG00162409';
    IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc WHERE TenThuoc = N'Loratadin 10mg')
        INSERT INTO dbo.Thuoc (TenThuoc, HoatChat, HamLuong, DonViTinh, GiaNhap, GiaBan, TonToiThieu, MaNhomThuoc, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi)
        SELECT TenHangHoa, HoatChatChinh, HamLuong, DonViTinh, 600, 1200, 20, @NhomDiUng, MaDQG, SoDangKy, HangSanXuat, NuocSanXuat, DongGoi FROM dbo.DanhMucDQG WHERE MaDQGDonVi = 'DQG00139076';

    /* 4. Phiếu nhập kho mẫu: Đang lập -> chi tiết (lô/HSD) -> Đã nhập kho (trigger cộng LoThuoc) */
    IF NOT EXISTS (SELECT 1 FROM dbo.PhieuNhap WHERE SoHoaDon = 'PNK-20260514-001')
    BEGIN
        DECLARE @MaNVKho INT = (SELECT MaNhanVien FROM dbo.NhanVien WHERE TenDangNhap = 'kho01');
        DECLARE @MaKho INT = (SELECT MaKho FROM dbo.Kho WHERE TenKho = N'Kho chính');
        DECLARE @MaNCC INT = (SELECT MaNhaCungCap FROM dbo.NhaCungCap WHERE TenNhaCungCap = N'Công ty Dược Minh Anh');
        DECLARE @PN INT;

        INSERT INTO dbo.PhieuNhap (
            NgayNhap, MaNhanVien, SoHoaDon, NgayHoaDon, LoaiPhieuNhap, MaKho, MaNhaCungCap,
            PhuongTienVanChuyen, DonViVanChuyen, NguoiGiaoHang, VAT, ChietKhau, GhiChu, TrangThai
        )
        VALUES (
            '2026-05-14T08:30:00', @MaNVKho, 'PNK-20260514-001', '2026-05-14', N'Nhập hàng nhà cung cấp', @MaKho, @MaNCC,
            N'Xe tải', N'Giao hàng nhanh', N'Lê Văn Giao', 8, 50000,
            N'Phiếu nhập mẫu cho menu Lập phiếu nhập kho', dbo.fn_TrangThai_Luu()
        );
        SET @PN = SCOPE_IDENTITY();

        INSERT INTO dbo.ChiTietPhieuNhap (MaPhieuNhap, MaThuoc, SoLuongNhap, DonGiaNhap, HanSuDung, GiaBan, SoLo, ViTri, GhiChu, VAT)
        SELECT @PN, MaThuoc, 120, 700, '2027-08-30', 1000, 'LO-PARA-0827', N'Kệ A1', N'Thuốc đã có trong hệ thống', NULL FROM dbo.Thuoc WHERE TenThuoc = N'Paracetamol STADA'
        UNION ALL
        SELECT @PN, MaThuoc, 80, 1500, '2027-06-15', 2500, 'LO-AMOX-0627', N'Kệ B2', N'Thuốc liên kết từ Danh mục DQG', NULL FROM dbo.Thuoc WHERE TenThuoc = N'Amoxicillin 500'
        UNION ALL
        SELECT @PN, MaThuoc, 150, 500, '2028-01-10', 900, 'LO-VITC-0128', N'Kệ C1', N'Thuốc mới thêm từ Danh mục DQG', NULL FROM dbo.Thuoc WHERE TenThuoc = N'Vitamin C 500mg';

        UPDATE dbo.PhieuNhap SET TrangThai = dbo.fn_TrangThai_DaNhapKho() WHERE MaPhieuNhap = @PN;
    END

    COMMIT TRANSACTION;
    PRINT N'Đã nạp dữ liệu mẫu PharmacyManagement thành công.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

USE PharmacyManagement;
GO
DECLARE @hs DATE = DATEADD(YEAR, 1, GETDATE());
EXEC dbo.sp_NhapKho
    @MaNhanVien   = 3,      -- MaNhanVien thật (vd. kho01)
    @MaThuoc      = 1,
    @SoLuongNhap  = 10,
    @DonGiaNhap   = 700,
    @HanSuDung    = @hs,
    @NhaCungCap   = N'Công ty Dược Minh Anh',
    @MaKho        = 1,
    @SoLo         = N'LO-TEST-01';

	DROP PROCEDURE IF EXISTS dbo.sp_NhapKho;
GO
