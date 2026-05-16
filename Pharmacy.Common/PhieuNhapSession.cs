<<<<<<< HEAD
namespace Pharmacy.Common;
=======
﻿namespace Pharmacy.Common;
>>>>>>> c178570feb4e8edc1d85abcf5c1940dbf983f787

/// <summary>Phiếu nhập đang thao tác trên shell (giữ giữa menu 2a ↔ 2b).</summary>
public static class PhieuNhapSession
{
    public static int? MaPhieuNhap { get; private set; }

    public static string MaPhieuHienThi =>
        MaPhieuNhap is int id ? $"PN{id:D5}" : "—";

    public static void SetPhieu(int maPhieuNhap) => MaPhieuNhap = maPhieuNhap;

    public static void Clear() => MaPhieuNhap = null;
}
