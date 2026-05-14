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

    /// <summary>Ghi log từ ứng dụng (ví dụ đổi cấu hình) — không thay thế trigger trên SQL Server.</summary>
    public void GhiThaoTac(string hanhDong, string? tenBang = null, string? maBanGhi = null, string? noiDung = null)
    {
        BllAuthorization.RequireAuthenticated();
        _audit.Ghi(UserSession.MaNhanVien, hanhDong, tenBang, maBanGhi, noiDung, diaChiMay: null);
    }
}
