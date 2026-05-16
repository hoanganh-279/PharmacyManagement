#nullable disable
using Pharmacy.Common;

namespace PharmacyManagement.Helpers;

/// <summary>Gắn tự động điền giá bán khi đổi giá nhập; người dùng vẫn sửa tay ô giá bán.</summary>
internal static class InventoryGiaBanUiHelper
{
    public static void GanTuDongGiaBanTheoBoYTe(
        NumericUpDown numGiaNhap,
        NumericUpDown numGiaBan,
        bool apDungNgayKhiKhoiTao = false)
    {
        void CapNhatGiaBan()
        {
            var goiY = GiaBanBoYTeHelper.TinhGiaBanGoiY(numGiaNhap.Value);
            if (goiY < numGiaBan.Minimum)
                goiY = numGiaBan.Minimum;
            if (goiY > numGiaBan.Maximum)
                goiY = numGiaBan.Maximum;
            numGiaBan.Value = goiY;
        }

        numGiaNhap.ValueChanged += (_, _) => CapNhatGiaBan();
        if (apDungNgayKhiKhoiTao)
            CapNhatGiaBan();
    }
}
