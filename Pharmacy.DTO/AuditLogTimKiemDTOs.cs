using Pharmacy.DTO.Views;

namespace Pharmacy.DTO;

/// <summary>Tham số lọc & phân trang nhật ký Audit (menu 8).</summary>
public sealed class AuditLogTimKiemThamSo
{
    public DateTime? TuNgay { get; init; }
    /// <summary>Ngày kết thúc (bao gồm cả ngày).</summary>
    public DateTime? DenNgay { get; init; }
    public int? MaNhanVien { get; init; }
    /// <summary>Giá trị <c>HanhDong</c> đầy đủ trong DB, hoặc <c>null</c> / rỗng = tất cả.</summary>
    public string? HanhDong { get; init; }
    public string? TuKhoa { get; init; }
    /// <summary>Trang bắt đầu từ 1.</summary>
    public int Trang { get; init; } = 1;
    public int KichThuocTrang { get; init; } = 25;
}

/// <summary>Kết quả một trang tra cứu audit.</summary>
public sealed class AuditLogTrangDTO
{
    public IReadOnlyList<AuditLogChiTietViewDTO> Items { get; init; } = Array.Empty<AuditLogChiTietViewDTO>();
    public long TongSoBanGhi { get; init; }
}

/// <summary>Nhân viên xuất hiện trong nhật ký (cho combo lọc).</summary>
public sealed class AuditLogNguoiTomTatDTO
{
    public int MaNhanVien { get; init; }
    public string HoTen { get; init; } = string.Empty;
}

/// <summary>Chỉ số tổng quan màn Audit (thẻ cuối trang).</summary>
public sealed class AuditLogThongKeManHinhDTO
{
    public decimal DungLuongBangMb { get; init; }
    public long TongBanGhiToanCuc { get; init; }
    public long CanhBaoNhayCam24h { get; init; }
    /// <summary>0–100, theo bộ lọc hiện tại.</summary>
    public decimal TyLeThaoTacNhayCamTrongBoLoc { get; init; }
}
