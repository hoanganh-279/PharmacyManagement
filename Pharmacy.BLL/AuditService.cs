using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class AuditService
{
    private readonly AuditRepositoryDAL _audit;

    public AuditService(DbContextDAL db) =>
        _audit = new AuditRepositoryDAL(db);

    public IReadOnlyList<AuditLogChiTietViewDTO> LayNhatKyChiTiet(int top = 500)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayTuView(top);
    }

    public IReadOnlyList<AuditLogDTO> LayNhatKyBangGoc(int top = 500)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayBangGoc(top);
    }

    public AuditLogTrangDTO TimPhanTrang(AuditLogTimKiemThamSo th)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        var tong = _audit.DemTheoBoLoc(th);
        var items = tong == 0 ? Array.Empty<AuditLogChiTietViewDTO>() : _audit.TimPhanTrang(th);
        return new AuditLogTrangDTO { TongSoBanGhi = tong, Items = items };
    }

    public IReadOnlyList<string> LayDanhSachHanhDong()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayDanhSachHanhDongPhanBiet();
    }

    public IReadOnlyList<AuditLogNguoiTomTatDTO> LayNguoiTrongNhatKy()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayNguoiCoTrongNhatKy();
    }

    public AuditLogThongKeManHinhDTO LayThongKeManHinh(AuditLogTimKiemThamSo boLocGiongLuoi)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayThongKeManHinh(boLocGiongLuoi);
    }

    public IReadOnlyList<AuditLogChiTietViewDTO> LayXuatDuLieu(AuditLogTimKiemThamSo th, int gioiHan = 10_000)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        return _audit.LayTheoBoLocToiDa(th, gioiHan);
    }

    /// <summary>Ghi log từ ứng dụng (ví dụ đổi cấu hình) — không thay thế trigger trên SQL Server.</summary>
    public void GhiThaoTac(string hanhDong, string? tenBang = null, string? maBanGhi = null, string? noiDung = null)
    {
        BllAuthorization.RequireAuthenticated();
        _audit.Ghi(UserSession.MaNhanVien, hanhDong, tenBang, maBanGhi, noiDung, diaChiMay: null);
    }
}
