/*
  Đồng bộ bảng AuditLog với SQL/PharmacyManagement.sql (MaNhanVien, DiaChiMay).
  Chạy khi gặp lỗi: Invalid column name 'MaNhanVien' hoặc 'DiaChiMay' trên màn Audit log.

  Sau khi chạy script này, chạy tiếp khối vw_AuditLogChiTiet trong SQL/View_PharmacyManagement.sql
  để view tham chiếu đúng cột.
*/
SET NOCOUNT ON;
GO

IF COL_LENGTH(N'dbo.AuditLog', N'MaNhanVien') IS NULL
BEGIN
    ALTER TABLE dbo.AuditLog ADD MaNhanVien INT NULL;
END
GO

IF COL_LENGTH(N'dbo.AuditLog', N'DiaChiMay') IS NULL
BEGIN
    ALTER TABLE dbo.AuditLog ADD DiaChiMay NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AuditLog_NhanVien' AND parent_object_id = OBJECT_ID(N'dbo.AuditLog'))
BEGIN
    IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.AuditLog WITH NOCHECK
            ADD CONSTRAINT FK_AuditLog_NhanVien
            FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien);
        -- Bật kiểm tra FK sau khi thêm (tránh lỗi nếu đã có dữ liệu lệch)
        ALTER TABLE dbo.AuditLog CHECK CONSTRAINT FK_AuditLog_NhanVien;
    END
END
GO

PRINT N'Hoàn tất Migration_AuditLog_MaNhanVien_DiaChiMay.sql — nhớ chạy CREATE OR ALTER VIEW dbo.vw_AuditLogChiTiet.';
GO
