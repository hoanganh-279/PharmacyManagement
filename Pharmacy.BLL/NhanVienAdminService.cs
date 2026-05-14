using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;

namespace Pharmacy.BLL;

/// <summary>Quản lý nhân viên — theo ma trận quyền chỉ Admin.</summary>
public class NhanVienAdminService
{
    private readonly NhanVienRepositoryDAL _nhanVien;
    private readonly VaiTroRepositoryDAL _vaiTro;

    public NhanVienAdminService(DbContextDAL db)
    {
        _nhanVien = new NhanVienRepositoryDAL(db);
        _vaiTro = new VaiTroRepositoryDAL(db);
    }

    public IReadOnlyList<NhanVienDTO> LayTatCa()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin);
        return _nhanVien.LayTatCa();
    }

    public IReadOnlyList<VaiTroDTO> LayVaiTro()
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin);
        return _vaiTro.LayTatCa();
    }

    public int Them(NhanVienDTO dto, string matKhauPlain)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin);
        if (Validator.IsNullOrWhiteSpace(dto.TenDangNhap) || Validator.IsNullOrWhiteSpace(matKhauPlain))
            throw new ArgumentException("Tên đăng nhập và mật khẩu là bắt buộc.");
        if (Validator.IsNullOrWhiteSpace(dto.HoTen))
            throw new ArgumentException("Họ tên là bắt buộc.");

        var hash = PasswordHelper.Hash(matKhauPlain);
        return _nhanVien.Them(dto, hash);
    }

    public void CapNhat(NhanVienDTO dto, string? matKhauMoiPlain)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin);
        if (dto.MaNhanVien <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.");

        var hash = string.IsNullOrEmpty(matKhauMoiPlain) ? null : PasswordHelper.Hash(matKhauMoiPlain);
        _nhanVien.CapNhat(dto, hash);
    }
}
