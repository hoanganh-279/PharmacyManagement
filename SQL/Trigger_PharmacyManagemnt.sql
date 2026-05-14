
USE PharmacyManagement;
GO

-- Trigger cập nhật tổng tiền hóa đơn
CREATE TRIGGER trg_UpdateTongTienHoaDon
ON ChiTietHoaDon
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE hd
    SET 
        TongTien = ISNULL((
            SELECT SUM(ThanhTien)
            FROM ChiTietHoaDon ct
            WHERE ct.MaHoaDon = hd.MaHoaDon
        ), 0),
        ThanhTien = ISNULL((
            SELECT SUM(ThanhTien)
            FROM ChiTietHoaDon ct
            WHERE ct.MaHoaDon = hd.MaHoaDon
        ), 0) - hd.GiamGia
    FROM HoaDon hd
    WHERE hd.MaHoaDon IN (
        SELECT MaHoaDon FROM inserted
        UNION
        SELECT MaHoaDon FROM deleted
    );
END;
GO

-- Trigger ghi log khi sửa giá thuốc
CREATE TRIGGER trg_Audit_UpdateGiaThuoc
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
    WHERE i.GiaNhap <> d.GiaNhap 
       OR i.GiaBan <> d.GiaBan;
END;
GO

-- Trigger cập nhật tổng tiền phiếu nhập
CREATE TRIGGER trg_UpdateTongTienPhieuNhap
ON ChiTietPhieuNhap
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE pn
    SET TongTien = ISNULL((
        SELECT SUM(ThanhTien)
        FROM ChiTietPhieuNhap ct
        WHERE ct.MaPhieuNhap = pn.MaPhieuNhap
    ), 0)
    FROM PhieuNhap pn
    WHERE pn.MaPhieuNhap IN (
        SELECT MaPhieuNhap FROM inserted
        UNION
        SELECT MaPhieuNhap FROM deleted
    );
END;
GO

-- Chặn bán thuốc vượt tồn
CREATE TRIGGER trg_CheckTonKho_BeforeBan
ON ChiTietHoaDon
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Thuoc t ON i.MaThuoc = t.MaThuoc
        WHERE i.SoLuongBan > t.SoLuongTon
    )
    BEGIN
        THROW 52001, N'Không đủ tồn kho để bán.', 1;
    END

    INSERT INTO ChiTietHoaDon(MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan)
    SELECT MaHoaDon, MaThuoc, SoLuongBan, DonGiaBan
    FROM inserted;
END;
GO

-- Chặn bán thuốc hết hạn
CREATE TRIGGER trg_CheckHanSuDung_BeforeBan
ON ChiTietHoaDon
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Thuoc t ON i.MaThuoc = t.MaThuoc
        WHERE t.HanSuDung IS NOT NULL
          AND t.HanSuDung < CAST(GETDATE() AS DATE)
    )
    BEGIN
        ROLLBACK;
        THROW 52002, N'Thuốc đã hết hạn, không được bán.', 1;
    END
END;
GO

-- Tự trừ tồn kho sau khi bán
CREATE TRIGGER trg_TruTonKho_AfterBan
ON ChiTietHoaDon
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t
    SET t.SoLuongTon = t.SoLuongTon - i.SoLuongBan
    FROM Thuoc t
    JOIN inserted i ON t.MaThuoc = i.MaThuoc;
END;
GO

-- Tự tăng tồn kho sau khi nhập
CREATE TRIGGER trg_TangTonKho_AfterNhap
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE t
    SET 
        t.SoLuongTon = t.SoLuongTon + i.SoLuongNhap,
        t.GiaNhap = i.DonGiaNhap,
        t.HanSuDung = i.HanSuDung
    FROM Thuoc t
    JOIN inserted i ON t.MaThuoc = i.MaThuoc;
END;
GO

-- Ghi log khi thêm thuốc
CREATE TRIGGER trg_Audit_InsertThuoc
ON Thuoc
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLog(HanhDong, TenBang, MaBanGhi, NoiDung)
    SELECT 
        N'Thêm thuốc',
        N'Thuoc',
        CAST(MaThuoc AS NVARCHAR(50)),
        N'Thêm thuốc: ' + TenThuoc
    FROM inserted;
END;
GO

-- Ghi log khi xóa mềm thuốc
CREATE TRIGGER trg_Audit_DeleteSoftThuoc
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
    WHERE d.TrangThai = 1 AND i.TrangThai = 0;
END;
GO

-- Ghi log khi tạo hóa đơn
CREATE TRIGGER trg_Audit_InsertHoaDon
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


