USE PharmacyManagement;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
  Thứ tự chạy script (đồng bộ project_Context.md):
  1) SQL/PharmacyManagement.sql  — DB, bảng, index, SP, seed
  2) SQL/Trigger_PharmacyManagemnt.sql — bảng/cột thiếu (nâng cấp), trigger, migration
  3) SQL/View_PharmacyManagement.sql — view

  File trigger duy nhất trong repo: Trigger_PharmacyManagemnt.sql (đường dẫn SQL/ hoặc SQL\ trùng tệp trên Windows).
*/

/* --- Nâng cấp schema (DB đã tồn tại từ bản cũ) --- */
IF OBJECT_ID(N'dbo.ChiTietHoaDon_PhanBoLo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon_PhanBoLo (
        MaPhanBo INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaCTHD INT NOT NULL,
        MaLoThuoc INT NOT NULL,
        SoLuongXuat INT NOT NULL,
        CONSTRAINT FK_PhanBoLo_CTHD
            FOREIGN KEY (MaCTHD) REFERENCES dbo.ChiTietHoaDon(MaCTHD) ON DELETE CASCADE,
        CONSTRAINT FK_PhanBoLo_LoThuoc
            FOREIGN KEY (MaLoThuoc) REFERENCES dbo.LoThuoc(MaLoThuoc),
        CONSTRAINT CK_PhanBoLo_SoLuongXuat CHECK (SoLuongXuat > 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_PhanBoLo_MaCTHD' AND object_id = OBJECT_ID(N'dbo.ChiTietHoaDon_PhanBoLo')
)
    CREATE INDEX IX_PhanBoLo_MaCTHD ON dbo.ChiTietHoaDon_PhanBoLo(MaCTHD);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_PhanBoLo_MaLoThuoc' AND object_id = OBJECT_ID(N'dbo.ChiTietHoaDon_PhanBoLo')
)
    CREATE INDEX IX_PhanBoLo_MaLoThuoc ON dbo.ChiTietHoaDon_PhanBoLo(MaLoThuoc);
GO

IF COL_LENGTH(N'dbo.ChiTietPhieuNhap', N'VAT') IS NULL
    ALTER TABLE dbo.ChiTietPhieuNhap ADD VAT DECIMAL(5,2) NULL;
GO

/* --- Nâng cấp: KhachHang(CCCD PK) + HoaDon.CCCD FK (thay MaKhachHang / snapshot) --- */
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HoaDon_KhachHang' AND parent_object_id = OBJECT_ID(N'dbo.HoaDon'))
    ALTER TABLE dbo.HoaDon DROP CONSTRAINT FK_HoaDon_KhachHang;
GO

IF COL_LENGTH(N'dbo.HoaDon', N'MaKhachHang') IS NOT NULL
    ALTER TABLE dbo.HoaDon DROP COLUMN MaKhachHang;
GO

IF COL_LENGTH(N'dbo.HoaDon', N'TenKhachHang') IS NOT NULL
    ALTER TABLE dbo.HoaDon DROP COLUMN TenKhachHang;
GO

IF COL_LENGTH(N'dbo.HoaDon', N'SoDienThoai') IS NOT NULL
    ALTER TABLE dbo.HoaDon DROP COLUMN SoDienThoai;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDon_MaKhachHang' AND object_id = OBJECT_ID(N'dbo.HoaDon'))
    DROP INDEX IX_HoaDon_MaKhachHang ON dbo.HoaDon;
GO

IF COL_LENGTH(N'dbo.KhachHang', N'MaKhachHang') IS NOT NULL
BEGIN
    DROP TABLE dbo.KhachHang;
END;
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang (
        CCCD CHAR(12) NOT NULL,
        HoTen NVARCHAR(100) NOT NULL,
        SoDienThoai VARCHAR(15) NULL,
        NgaySinh DATE NULL,
        DiaChi NVARCHAR(255) NULL,
        GhiChu NVARCHAR(255) NULL,
        TrangThai BIT NOT NULL CONSTRAINT DF_KhachHang_TrangThai DEFAULT 1,
        NgayTao DATETIME NOT NULL CONSTRAINT DF_KhachHang_NgayTao DEFAULT GETDATE(),
        CONSTRAINT PK_KhachHang PRIMARY KEY (CCCD),
        CONSTRAINT CK_KhachHang_CCCD CHECK (CCCD NOT LIKE '%[^0-9]%' AND LEN(CCCD) = 12),
        CONSTRAINT CK_KhachHang_HoTen CHECK (LEN(LTRIM(RTRIM(HoTen))) > 0)
    );
END;
GO

IF COL_LENGTH(N'dbo.HoaDon', N'CCCD') IS NULL
    ALTER TABLE dbo.HoaDon ADD CCCD CHAR(12) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HoaDon_KhachHang')
BEGIN
    ALTER TABLE dbo.HoaDon WITH NOCHECK ADD CONSTRAINT FK_HoaDon_KhachHang
        FOREIGN KEY (CCCD) REFERENCES dbo.KhachHang(CCCD);
    ALTER TABLE dbo.HoaDon CHECK CONSTRAINT FK_HoaDon_KhachHang;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_HoaDon_CCCD')
    ALTER TABLE dbo.HoaDon ADD CONSTRAINT CK_HoaDon_CCCD
        CHECK (CCCD IS NULL OR (CCCD NOT LIKE '%[^0-9]%' AND LEN(CCCD) = 12));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_KhachHang_HoTen' AND object_id = OBJECT_ID(N'dbo.KhachHang'))
    CREATE INDEX IX_KhachHang_HoTen ON dbo.KhachHang(HoTen) INCLUDE (SoDienThoai, DiaChi, TrangThai);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDon_CCCD' AND object_id = OBJECT_ID(N'dbo.HoaDon'))
    CREATE INDEX IX_HoaDon_CCCD ON dbo.HoaDon(CCCD) INCLUDE (NgayLap, ThanhTien, TrangThai) WHERE CCCD IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001234')
    INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
    VALUES ('079085001234', N'Nguyễn Văn An', '0901000001', '1985-03-12', N'Quận 1, TP.HCM');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001235')
    INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
    VALUES ('079085001235', N'Nguyễn Văn An', '0901000002', '1992-07-25', N'Quận 7, TP.HCM');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001236')
    INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
    VALUES ('079085001236', N'Trần Thị Mai', '0912000003', '1978-11-08', N'Quận 3, TP.HCM');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD = '079085001237')
    INSERT INTO dbo.KhachHang (CCCD, HoTen, SoDienThoai, NgaySinh, DiaChi)
    VALUES ('079085001237', N'Lê Hoàng Nam', '0988000004', '2000-01-15', N'Quận Bình Thạnh, TP.HCM');
GO

IF OBJECT_ID(N'dbo.trg_HoaDon_KiemTraCCCD', N'TR') IS NULL
    EXEC(N'
CREATE TRIGGER dbo.trg_HoaDon_KiemTraCCCD
ON dbo.HoaDon
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.CCCD IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM dbo.KhachHang kh WHERE kh.CCCD = i.CCCD AND kh.TrangThai = 1)
    )
        THROW 51010, N''CCCD trên hóa đơn phải tồn tại trong KhachHang và TrangThai = 1.'', 1;
END;
');
GO

IF OBJECT_ID(N'dbo.sp_BanThuoc', N'P') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.parameters p
        WHERE p.object_id = OBJECT_ID(N'dbo.sp_BanThuoc') AND p.name = N'@CCCD'
   )
    DROP PROCEDURE dbo.sp_BanThuoc;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_CTPN_MaPhieuNhap' AND object_id = OBJECT_ID(N'dbo.ChiTietPhieuNhap')
)
    CREATE INDEX IX_CTPN_MaPhieuNhap ON dbo.ChiTietPhieuNhap(MaPhieuNhap);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_PhieuNhap_TrangThai' AND object_id = OBJECT_ID(N'dbo.PhieuNhap')
)
    CREATE INDEX IX_PhieuNhap_TrangThai ON dbo.PhieuNhap(TrangThai);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_LoThuoc_FEFO' AND object_id = OBJECT_ID(N'dbo.LoThuoc')
)
    CREATE INDEX IX_LoThuoc_FEFO ON dbo.LoThuoc(MaThuoc, HanSuDung)
    INCLUDE (SoLuongTon, MaLoThuoc, MaKho)
    WHERE TrangThai = 1 AND SoLuongTon > 0;
GO

/* =========================================================
   BỔ SUNG CỘT PHỤC VỤ CHỨC NĂNG THÊM HÀNG HÓA / THÊM THUỐC
   - Nếu thuốc thuộc danh mục DQG: ChoPhepLienThong = 1
   - Nếu hàng hóa/thuốc tự nhập thủ công: ChoPhepLienThong = 0
   ========================================================= */
IF COL_LENGTH('Thuoc', 'ChoPhepLienThong') IS NULL
    ALTER TABLE Thuoc ADD ChoPhepLienThong BIT NOT NULL CONSTRAINT DF_Thuoc_ChoPhepLienThong DEFAULT 0;
GO

IF COL_LENGTH('Thuoc', 'TenVietTat') IS NULL
    ALTER TABLE Thuoc ADD TenVietTat NVARCHAR(100) NULL;
GO

IF COL_LENGTH('Thuoc', 'TuKhoa') IS NULL
    ALTER TABLE Thuoc ADD TuKhoa NVARCHAR(255) NULL;
GO

IF COL_LENGTH('Thuoc', 'GhiChu') IS NULL
    ALTER TABLE Thuoc ADD GhiChu NVARCHAR(255) NULL;
GO

IF COL_LENGTH('Thuoc', 'CoBarcode') IS NULL
    ALTER TABLE Thuoc ADD CoBarcode BIT NOT NULL CONSTRAINT DF_Thuoc_CoBarcode DEFAULT 0;
GO

IF COL_LENGTH('Thuoc', 'CanNhapKho') IS NULL
    ALTER TABLE Thuoc ADD CanNhapKho BIT NOT NULL CONSTRAINT DF_Thuoc_CanNhapKho DEFAULT 1;
GO

IF COL_LENGTH('Thuoc', 'ThuocBiDinhChiLuuHanh') IS NULL
    ALTER TABLE Thuoc ADD ThuocBiDinhChiLuuHanh BIT NOT NULL CONSTRAINT DF_Thuoc_BiDinhChi DEFAULT 0;
GO

/* =========================================================
   1. KIỂM TRA THÔNG TIN THUỐC / HÀNG HÓA KHI THÊM HOẶC SỬA
   ========================================================= */
CREATE OR ALTER TRIGGER trg_CheckThongTinThuoc
ON Thuoc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE TenThuoc IS NULL OR LTRIM(RTRIM(TenThuoc)) = N''
    )
    BEGIN
        THROW 53001, N'Tên hàng hóa/thuốc không được để trống.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE DonViTinh IS NULL OR LTRIM(RTRIM(DonViTinh)) = N''
    )
    BEGIN
        THROW 53002, N'Đơn vị tính không được để trống.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE MaNhomThuoc IS NULL
    )
    BEGIN
        THROW 53003, N'Vui lòng chọn nhóm hàng hóa/nhóm thuốc.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE GiaNhap < 0 OR GiaBan < 0 OR SoLuongTon < 0 OR TonToiThieu < 0
    )
    BEGIN
        THROW 53004, N'Giá, tồn kho và tồn tối thiểu không được âm.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE HanSuDung IS NOT NULL
          AND HanSuDung < CAST(GETDATE() AS DATE)
    )
    BEGIN
        THROW 53005, N'Hạn sử dụng không được nhỏ hơn ngày hiện tại.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE ThuocBiDinhChiLuuHanh = 1 AND TrangThai = 1
    )
    BEGIN
        THROW 53006, N'Thuốc bị đình chỉ lưu hành không được để trạng thái đang kinh doanh.', 1;
    END;
END;
GO

/* =========================================================
   2. KIỂM TRA THUỐC LIÊN THÔNG DQG
   - Tích ChoPhepLienThong: bắt buộc có MaDQG, Số đăng ký, hoạt chất, hàm lượng
   - Không tích: cho phép nhập thủ công, không bắt buộc DQG
   ========================================================= */
CREATE OR ALTER TRIGGER trg_CheckThuoc_LienThongDQG
ON Thuoc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE ChoPhepLienThong = 1
          AND (
                MaDQG IS NULL
                OR SoDangKy IS NULL OR LTRIM(RTRIM(SoDangKy)) = ''
                OR HoatChat IS NULL OR LTRIM(RTRIM(HoatChat)) = N''
                OR HamLuong IS NULL OR LTRIM(RTRIM(HamLuong)) = N''
              )
    )
    BEGIN
        THROW 53010, N'Thuốc liên thông DQG phải có Mã DQG, Số đăng ký, Hoạt chất và Hàm lượng.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN DanhMucDQG dqg ON i.MaDQG = dqg.MaDQG
        WHERE i.ChoPhepLienThong = 1
          AND dqg.MaDQG IS NULL
    )
    BEGIN
        THROW 53011, N'Mã DQG không tồn tại trong DanhMucDQG.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Thuoc t ON t.MaDQG = i.MaDQG
        WHERE i.ChoPhepLienThong = 1
          AND i.MaDQG IS NOT NULL
          AND t.MaThuoc <> i.MaThuoc
          AND t.TrangThai = 1
    )
    BEGIN
        THROW 53012, N'Thuốc thuộc mã DQG này đã tồn tại trong hệ thống.', 1;
    END;
END;
GO

/* =========================================================
   3. TỰ ĐỒNG BỘ THÔNG TIN THUỐC TỪ DANH MỤC DQG
   Chỉ áp dụng khi ChoPhepLienThong = 1
   ========================================================= */
CREATE OR ALTER TRIGGER trg_DongBoThongTinThuoc_TuDQG
ON Thuoc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t
    SET
        t.SoDangKy = ISNULL(NULLIF(t.SoDangKy, ''), dqg.SoDangKy),
        t.HoatChat = ISNULL(NULLIF(t.HoatChat, N''), dqg.HoatChatChinh),
        t.HamLuong = ISNULL(NULLIF(t.HamLuong, N''), dqg.HamLuong),
        t.DonViTinh = ISNULL(NULLIF(t.DonViTinh, N''), dqg.DonViTinh),
        t.HangSanXuat = ISNULL(NULLIF(t.HangSanXuat, N''), dqg.HangSanXuat),
        t.NuocSanXuat = ISNULL(NULLIF(t.NuocSanXuat, N''), dqg.NuocSanXuat),
        t.DongGoi = ISNULL(NULLIF(t.DongGoi, N''), dqg.DongGoi)
    FROM Thuoc t
    JOIN inserted i ON t.MaThuoc = i.MaThuoc
    JOIN DanhMucDQG dqg ON i.MaDQG = dqg.MaDQG
    WHERE i.ChoPhepLienThong = 1;
END;
GO

/* =========================================================
   4. CẬP NHẬT TỔNG TIỀN HÓA ĐƠN BÁN
   ========================================================= */
CREATE OR ALTER TRIGGER trg_UpdateTongTienHoaDon
ON ChiTietHoaDon
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE hd
    SET 
        TongTien = ISNULL(x.TongTien, 0),
        ThanhTien = ISNULL(x.TongTien, 0) - ISNULL(hd.GiamGia, 0)
    FROM HoaDon hd
    LEFT JOIN (
        SELECT MaHoaDon, SUM(ThanhTien) AS TongTien
        FROM ChiTietHoaDon
        GROUP BY MaHoaDon
    ) x ON hd.MaHoaDon = x.MaHoaDon
    WHERE hd.MaHoaDon IN (
        SELECT MaHoaDon FROM inserted
        UNION
        SELECT MaHoaDon FROM deleted
    );
END;
GO

/* =========================================================
   5. CẬP NHẬT TỔNG TIỀN PHIẾU NHẬP
   Danh sách hàng nhập chỉ là chi tiết phiếu, chưa cộng kho
   ========================================================= */
CREATE OR ALTER TRIGGER trg_UpdateTongTienPhieuNhap
ON ChiTietPhieuNhap
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN PhieuNhap pn ON i.MaPhieuNhap = pn.MaPhieuNhap
        WHERE pn.TrangThai = dbo.fn_TrangThai_DaNhapKho()
        UNION
        SELECT 1
        FROM deleted d
        JOIN PhieuNhap pn ON d.MaPhieuNhap = pn.MaPhieuNhap
        WHERE pn.TrangThai = dbo.fn_TrangThai_DaNhapKho()
    )
    BEGIN
        THROW 52010, N'Phiếu đã nhập kho, không được sửa chi tiết phiếu nhập.', 1;
    END;

    UPDATE pn
    SET
        TongTien = ISNULL(v.TongTienHang, 0),
        /* CongNo: tiền hàng + VAT (ưu tiên VAT % từng dòng nếu có ít nhất một dòng khai VAT; không thì VAT % trên phiếu) - ChietKhau */
        CongNo = ISNULL(v.TongTienHang, 0)
            + CASE
                  WHEN ISNULL(v.CoVATDong, 0) = 1 THEN ISNULL(v.TienVATDong, 0)
                  ELSE ROUND(ISNULL(v.TongTienHang, 0) * ISNULL(pn.VAT, 0) / 100.0, 2)
              END
            - ISNULL(pn.ChietKhau, 0)
    FROM PhieuNhap pn
    LEFT JOIN (
        SELECT
            ct.MaPhieuNhap,
            SUM(ct.ThanhTien) AS TongTienHang,
            MAX(CASE WHEN ct.VAT IS NOT NULL THEN 1 ELSE 0 END) AS CoVATDong,
            SUM(ROUND(ct.ThanhTien * ISNULL(ct.VAT, 0) / 100.0, 2)) AS TienVATDong
        FROM ChiTietPhieuNhap ct
        GROUP BY ct.MaPhieuNhap
    ) v ON pn.MaPhieuNhap = v.MaPhieuNhap
    WHERE pn.MaPhieuNhap IN (
        SELECT MaPhieuNhap FROM inserted
        UNION
        SELECT MaPhieuNhap FROM deleted
    );
END;
GO

/* =========================================================
   6. KIỂM TRA DỮ LIỆU CHI TIẾT PHIẾU NHẬP
   ========================================================= */
CREATE OR ALTER TRIGGER trg_CheckChiTietPhieuNhap
ON ChiTietPhieuNhap
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongNhap <= 0)
    BEGIN
        THROW 52011, N'Số lượng nhập phải lớn hơn 0.', 1;
    END;

    IF EXISTS (SELECT 1 FROM inserted WHERE DonGiaNhap < 0 OR ISNULL(GiaBan, 0) < 0)
    BEGIN
        THROW 52012, N'Giá nhập và giá bán không được âm.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE HanSuDung IS NOT NULL
          AND HanSuDung < CAST(GETDATE() AS DATE)
    )
    BEGIN
        THROW 52013, N'Hạn sử dụng của thuốc nhập không được nhỏ hơn ngày hiện tại.', 1;
    END;
END;
GO

/* =========================================================
   7. NHẬP KHO KHI PHIẾU CHUYỂN SANG TRẠNG THÁI ĐÃ NHẬP KHO
   ========================================================= */
CREATE OR ALTER TRIGGER trg_NhapKho_KhiHoanTat
ON PhieuNhap
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.MaPhieuNhap = d.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND ISNULL(d.TrangThai, N'') <> dbo.fn_TrangThai_DaNhapKho()
    )
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN ChiTietPhieuNhap ct ON i.MaPhieuNhap = ct.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND ct.MaCTPN IS NULL
    )
    BEGIN
        THROW 52020, N'Không thể nhập kho vì phiếu chưa có danh sách hàng nhập.', 1;
    END;

    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND i.MaKho IS NULL
    )
    BEGIN
        THROW 52021, N'Không thể nhập kho vì chưa chọn kho nhập.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN ChiTietPhieuNhap ct ON i.MaPhieuNhap = ct.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND (
                ct.SoLo IS NULL OR LTRIM(RTRIM(ct.SoLo)) = ''
                OR ct.HanSuDung IS NULL
              )
    )
    BEGIN
        THROW 52022, N'Không thể nhập kho vì có thuốc chưa nhập số lô hoặc hạn sử dụng.', 1;
    END;

    ;WITH NguonNhap AS (
        SELECT
            ct.MaThuoc,
            i.MaKho,
            ct.SoLo,
            ct.HanSuDung,
            SUM(ct.SoLuongNhap) AS SoLuongNhap,
            MAX(ct.DonGiaNhap) AS DonGiaNhap,
            MAX(ISNULL(ct.GiaBan, 0)) AS GiaBan,
            MAX(ct.ViTri) AS ViTri
        FROM inserted i
        JOIN deleted d ON i.MaPhieuNhap = d.MaPhieuNhap
        JOIN ChiTietPhieuNhap ct ON i.MaPhieuNhap = ct.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND ISNULL(d.TrangThai, N'') <> dbo.fn_TrangThai_DaNhapKho()
        GROUP BY ct.MaThuoc, i.MaKho, ct.SoLo, ct.HanSuDung
    )
    UPDATE lt
    SET
        lt.SoLuongTon = lt.SoLuongTon + n.SoLuongNhap,
        lt.GiaNhap = n.DonGiaNhap,
        lt.GiaBan = n.GiaBan,
        lt.ViTri = n.ViTri,
        lt.TrangThai = 1
    FROM LoThuoc lt
    JOIN NguonNhap n
        ON lt.MaThuoc = n.MaThuoc
       AND lt.MaKho = n.MaKho
       AND lt.SoLo = n.SoLo
       AND lt.HanSuDung = n.HanSuDung;

    ;WITH NguonNhap AS (
        SELECT
            ct.MaThuoc,
            i.MaKho,
            ct.SoLo,
            ct.HanSuDung,
            SUM(ct.SoLuongNhap) AS SoLuongNhap,
            MAX(ct.DonGiaNhap) AS DonGiaNhap,
            MAX(ISNULL(ct.GiaBan, 0)) AS GiaBan,
            MAX(ct.ViTri) AS ViTri
        FROM inserted i
        JOIN deleted d ON i.MaPhieuNhap = d.MaPhieuNhap
        JOIN ChiTietPhieuNhap ct ON i.MaPhieuNhap = ct.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND ISNULL(d.TrangThai, N'') <> dbo.fn_TrangThai_DaNhapKho()
        GROUP BY ct.MaThuoc, i.MaKho, ct.SoLo, ct.HanSuDung
    )
    INSERT INTO LoThuoc (MaThuoc, MaKho, SoLo, HanSuDung, SoLuongTon, GiaNhap, GiaBan, ViTri, TrangThai)
    SELECT n.MaThuoc, n.MaKho, n.SoLo, n.HanSuDung, n.SoLuongNhap, n.DonGiaNhap, n.GiaBan, n.ViTri, 1
    FROM NguonNhap n
    WHERE NOT EXISTS (
        SELECT 1 FROM LoThuoc lt
        WHERE lt.MaThuoc = n.MaThuoc
          AND lt.MaKho = n.MaKho
          AND lt.SoLo = n.SoLo
          AND lt.HanSuDung = n.HanSuDung
    );

    UPDATE t
    SET
        t.SoLuongTon = ISNULL(x.TongTon, 0),
        t.GiaNhap = ISNULL(y.DonGiaNhap, t.GiaNhap),
        t.GiaBan = ISNULL(NULLIF(y.GiaBan, 0), t.GiaBan),
        t.HanSuDung = ISNULL(y.HanSuDung, t.HanSuDung)
    FROM Thuoc t
    JOIN (
        SELECT MaThuoc, SUM(SoLuongTon) AS TongTon
        FROM LoThuoc
        GROUP BY MaThuoc
    ) x ON t.MaThuoc = x.MaThuoc
    OUTER APPLY (
        SELECT TOP 1 ct.DonGiaNhap, ct.GiaBan, ct.HanSuDung
        FROM ChiTietPhieuNhap ct
        JOIN inserted i ON ct.MaPhieuNhap = i.MaPhieuNhap
        WHERE ct.MaThuoc = t.MaThuoc
          AND i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
        ORDER BY ct.MaCTPN DESC
    ) y
    WHERE t.MaThuoc IN (
        SELECT ct.MaThuoc
        FROM ChiTietPhieuNhap ct
        JOIN inserted i ON ct.MaPhieuNhap = i.MaPhieuNhap
        WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
    );

    INSERT INTO AuditLog(MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT
        i.MaNhanVien,
        N'Nhập kho',
        N'PhieuNhap',
        CAST(i.MaPhieuNhap AS NVARCHAR(50)),
        N'Hoàn tất nhập kho phiếu nhập: ' + CAST(i.MaPhieuNhap AS NVARCHAR(50))
    FROM inserted i
    JOIN deleted d ON i.MaPhieuNhap = d.MaPhieuNhap
    WHERE i.TrangThai = dbo.fn_TrangThai_DaNhapKho()
      AND ISNULL(d.TrangThai, N'') <> dbo.fn_TrangThai_DaNhapKho();
END;
GO

/* =========================================================
   8. KHÔNG CHO SỬA PHIẾU ĐÃ NHẬP KHO
   ========================================================= */
CREATE OR ALTER TRIGGER trg_KhongSuaPhieuNhapDaNhapKho
ON PhieuNhap
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.MaPhieuNhap = d.MaPhieuNhap
        WHERE d.TrangThai = dbo.fn_TrangThai_DaNhapKho()
          AND (
                ISNULL(i.SoHoaDon, '') <> ISNULL(d.SoHoaDon, '')
                OR ISNULL(i.MaKho, 0) <> ISNULL(d.MaKho, 0)
                OR ISNULL(i.MaNhaCungCap, 0) <> ISNULL(d.MaNhaCungCap, 0)
                OR ISNULL(i.TongTien, 0) <> ISNULL(d.TongTien, 0)
                OR ISNULL(i.CongNo, 0) <> ISNULL(d.CongNo, 0)
                OR ISNULL(i.TrangThai, '') <> ISNULL(d.TrangThai, '')
              )
    )
    BEGIN
        THROW 52023, N'Phiếu đã nhập kho, không được sửa hoặc đổi trạng thái.', 1;
    END;
END;
GO

/* =========================================================
   9. CHẶN BÁN THUỐC VƯỢT TỒN
   ========================================================= */
CREATE OR ALTER TRIGGER trg_CheckTonKho_BeforeBan
ON ChiTietHoaDon
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongBan <= 0 OR DonGiaBan < 0)
    BEGIN
        THROW 52030, N'Số lượng bán phải lớn hơn 0 và đơn giá bán không được âm.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Thuoc t ON i.MaThuoc = t.MaThuoc
        WHERE t.TrangThai = 0 OR t.ThuocBiDinhChiLuuHanh = 1
    )
    BEGIN
        THROW 52031, N'Thuốc đã ngừng kinh doanh hoặc bị đình chỉ lưu hành, không được bán.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT MaThuoc, SUM(SoLuongBan) AS TongBan
            FROM inserted
            GROUP BY MaThuoc
        ) b
        JOIN Thuoc t ON b.MaThuoc = t.MaThuoc
        OUTER APPLY (
            SELECT SUM(SoLuongTon) AS TongTonLoHopLe
            FROM LoThuoc lt
            WHERE lt.MaThuoc = b.MaThuoc
              AND lt.SoLuongTon > 0
              AND lt.HanSuDung >= CAST(GETDATE() AS DATE)
              AND lt.TrangThai = 1
        ) l
        WHERE b.TongBan > ISNULL(l.TongTonLoHopLe, t.SoLuongTon)
    )
    BEGIN
        THROW 52032, N'Không đủ tồn kho hoặc không còn lô thuốc hợp lệ để bán.', 1;
    END;

    INSERT INTO ChiTietHoaDon(MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
    SELECT MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan
    FROM inserted;
END;
GO

/* =========================================================
   10. TRỪ TỒN KHO SAU KHI BÁN
   - Trừ tồn tổng bảng Thuoc
   - Đồng thời trừ tồn theo lô theo nguyên tắc FEFO: lô hết hạn trước bán trước
   ========================================================= */
CREATE OR ALTER TRIGGER trg_TruTonKho_AfterBan
ON ChiTietHoaDon
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t
    SET t.SoLuongTon = t.SoLuongTon - x.TongBan
    FROM Thuoc t
    JOIN (
        SELECT MaThuoc, SUM(SoLuongBan) AS TongBan
        FROM inserted
        GROUP BY MaThuoc
    ) x ON t.MaThuoc = x.MaThuoc;

    DECLARE @MaCTHD INT, @MaThuoc INT, @SoLuongCon INT;
    DECLARE @MaLoThuoc INT, @TonLo INT, @SoLuongTru INT;

    DECLARE curCT CURSOR LOCAL FAST_FORWARD FOR
        SELECT MaCTHD, MaThuoc, SoLuongBan
        FROM inserted
        ORDER BY MaCTHD;

    OPEN curCT;
    FETCH NEXT FROM curCT INTO @MaCTHD, @MaThuoc, @SoLuongCon;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        WHILE @SoLuongCon > 0
        BEGIN
            SELECT TOP (1)
                @MaLoThuoc = MaLoThuoc,
                @TonLo = SoLuongTon
            FROM LoThuoc
            WHERE MaThuoc = @MaThuoc
              AND SoLuongTon > 0
              AND HanSuDung >= CAST(GETDATE() AS DATE)
              AND ISNULL(TrangThai, 1) = 1
            ORDER BY HanSuDung ASC, MaLoThuoc ASC;

            IF @MaLoThuoc IS NULL
                BREAK;

            SET @SoLuongTru = CASE WHEN @TonLo >= @SoLuongCon THEN @SoLuongCon ELSE @TonLo END;

            UPDATE LoThuoc
            SET SoLuongTon = SoLuongTon - @SoLuongTru
            WHERE MaLoThuoc = @MaLoThuoc;

            INSERT INTO dbo.ChiTietHoaDon_PhanBoLo (MaCTHD, MaLoThuoc, SoLuongXuat)
            VALUES (@MaCTHD, @MaLoThuoc, @SoLuongTru);

            SET @SoLuongCon = @SoLuongCon - @SoLuongTru;
            SET @MaLoThuoc = NULL;
        END;

        FETCH NEXT FROM curCT INTO @MaCTHD, @MaThuoc, @SoLuongCon;
    END;

    CLOSE curCT;
    DEALLOCATE curCT;

    INSERT INTO AuditLog(HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT
        N'Bán thuốc',
        N'ChiTietHoaDon',
        CAST(i.MaCTHD AS NVARCHAR(50)),
        N'Bán thuốc mã: ' + CAST(i.MaThuoc AS NVARCHAR(50))
        + N' | Số lượng: ' + CAST(i.SoLuongBan AS NVARCHAR(50))
    FROM inserted i;
END;
GO

/* =========================================================
   11. AUDIT LOG KHI SỬA GIÁ THUỐC
   ========================================================= */
CREATE OR ALTER TRIGGER trg_Audit_UpdateGiaThuoc
ON Thuoc
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLog(HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT 
        N'Cập nhật giá thuốc',
        N'Thuoc',
        CAST(i.MaThuoc AS NVARCHAR(50)),
        N'Thuốc: ' + i.TenThuoc 
        + N' | Giá nhập cũ: ' + CAST(d.GiaNhap AS NVARCHAR(50))
        + N' | Giá nhập mới: ' + CAST(i.GiaNhap AS NVARCHAR(50))
        + N' | Giá bán cũ: ' + CAST(d.GiaBan AS NVARCHAR(50))
        + N' | Giá bán mới: ' + CAST(i.GiaBan AS NVARCHAR(50))
    FROM inserted i
    JOIN deleted d ON i.MaThuoc = d.MaThuoc
    WHERE ISNULL(i.GiaNhap, 0) <> ISNULL(d.GiaNhap, 0)
       OR ISNULL(i.GiaBan, 0) <> ISNULL(d.GiaBan, 0);
END;
GO

/* =========================================================
   12. AUDIT LOG KHI THÊM THUỐC / HÀNG HÓA
   ========================================================= */
CREATE OR ALTER TRIGGER trg_Audit_InsertThuoc
ON Thuoc
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLog(HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT 
        N'Thêm hàng hóa/thuốc',
        N'Thuoc',
        CAST(MaThuoc AS NVARCHAR(50)),
        N'Thêm: ' + TenThuoc
        + CASE WHEN ChoPhepLienThong = 1 THEN N' | Có liên thông DQG' ELSE N' | Nhập thủ công' END
    FROM inserted;
END;
GO

/* =========================================================
   13. AUDIT LOG KHI XÓA MỀM THUỐC
   ========================================================= */
CREATE OR ALTER TRIGGER trg_Audit_DeleteSoftThuoc
ON Thuoc
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLog(HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT 
        N'Xóa mềm thuốc',
        N'Thuoc',
        CAST(i.MaThuoc AS NVARCHAR(50)),
        N'Ngừng kinh doanh thuốc: ' + i.TenThuoc
    FROM inserted i
    JOIN deleted d ON i.MaThuoc = d.MaThuoc
    WHERE d.TrangThai = 1 
      AND i.TrangThai = 0;
END;
GO

/* =========================================================
   14. AUDIT LOG KHI TẠO HÓA ĐƠN
   ========================================================= */
CREATE OR ALTER TRIGGER trg_Audit_InsertHoaDon
ON HoaDon
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLog(MaNhanVien, HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT
        MaNhanVien,
        N'Tạo hóa đơn',
        N'HoaDon',
        CAST(MaHoaDon AS NVARCHAR(50)),
        N'Tạo hóa đơn bán thuốc, tổng tiền: ' + CAST(ThanhTien AS NVARCHAR(50))
    FROM inserted;
END;
GO

/* ========================================================================
   MIGRATION (idempotent): chuẩn hoá trạng thái phiếu nhập legacy "Hoàn thành"
   ======================================================================== */

UPDATE pn
SET TrangThai = dbo.fn_TrangThai_DaNhapKho()
FROM PhieuNhap pn
WHERE pn.TrangThai = N'Hoàn thành'
  AND pn.MaKho IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM ChiTietPhieuNhap ct
      WHERE ct.MaPhieuNhap = pn.MaPhieuNhap
        AND (ct.SoLo IS NULL OR LTRIM(RTRIM(ct.SoLo)) = N'' OR ct.HanSuDung IS NULL)
  )
  AND NOT EXISTS (
      SELECT 1
      FROM ChiTietPhieuNhap ct
      WHERE ct.MaPhieuNhap = pn.MaPhieuNhap
        AND EXISTS (
            SELECT 1
            FROM LoThuoc lt
            WHERE lt.MaThuoc = ct.MaThuoc
              AND lt.MaKho = pn.MaKho
              AND lt.SoLo = ct.SoLo
              AND lt.HanSuDung = ct.HanSuDung
        )
  );

UPDATE dbo.PhieuNhap
SET TrangThai = dbo.fn_TrangThai_Luu()
WHERE TrangThai = N'Hoàn thành';
GO
