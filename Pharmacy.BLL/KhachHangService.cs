<<<<<<< HEAD
using Pharmacy.Common;
=======
﻿using Pharmacy.Common;
>>>>>>> c178570feb4e8edc1d85abcf5c1940dbf983f787
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class KhachHangService
{
    private readonly KhachHangRepositoryDAL _khachHang;

    public KhachHangService(DbContextDAL db) =>
        _khachHang = new KhachHangRepositoryDAL(db);

    public KhachHangDTO? TraCuuTheoCccd(string? cccd)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        if (!Validator.TryNormalizeCccd(cccd, out var normalized))
            throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
        return _khachHang.LayTheoCccd(normalized);
    }

    public void ThemKhachHang(KhachHangDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        if (!Validator.TryNormalizeCccd(dto.CCCD, out var cccd))
            throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
        if (Validator.IsNullOrWhiteSpace(dto.HoTen))
            throw new ArgumentException("Họ tên khách hàng là bắt buộc.");
        if (!Validator.IsPhoneOptional(dto.SoDienThoai))
            throw new ArgumentException("Số điện thoại không hợp lệ.");
        if (_khachHang.LayTheoCccd(cccd) is not null)
            throw new InvalidOperationException("CCCD đã tồn tại trong hệ thống.");

        dto.CCCD = cccd;
        _khachHang.ThemMoi(dto);
    }

    public IReadOnlyList<LichSuMuaHangDTO> LayLichSuMuaHang(string? cccd, int top = 100)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi);
        if (!Validator.TryNormalizeCccd(cccd, out var normalized))
            throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
        return _khachHang.LayLichSuMuaHang(normalized, top);
    }
}
