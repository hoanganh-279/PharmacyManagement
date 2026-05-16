#nullable disable
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Helpers;

/// <summary>Quy tắc tra cứu thuốc thống nhất giữa «Quản lý thuốc» và «Danh sách nhập kho».</summary>
internal enum MedicineSearchMode
{
    /// <summary>Ô trống = hiển thị toàn bộ danh mục trên lưới.</summary>
    DanhMuc,

    /// <summary>Chọn thuốc để thêm vào phiếu — bắt buộc từ khóa; loại «Ngừng bán».</summary>
    ChonNhapKho
}

internal readonly struct MedicineSearchParseResult
{
    public bool CoTheTim { get; init; }
    public string? TuKhoaTim { get; init; }
    public string ThongBaoGoiY { get; init; }
}

internal static class InventoryMedicineSearchKit
{
    public const int MinDoDaiTuKhoa = 2;
    public const int GioiHanChonNhapKho = 12;
    public const string PlaceholderTimThuoc = "Tìm theo tên, hoạt chất, số đăng ký DQG, mã DQG...";

    public static MedicineSearchParseResult PhanTichTuKhoa(string? raw, MedicineSearchMode mode)
    {
        var text = raw?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return mode == MedicineSearchMode.DanhMuc
                ? new MedicineSearchParseResult
                {
                    CoTheTim = true,
                    TuKhoaTim = null,
                    ThongBaoGoiY = "Đang hiển thị toàn bộ danh mục thuốc."
                }
                : new MedicineSearchParseResult
                {
                    CoTheTim = false,
                    TuKhoaTim = null,
                    ThongBaoGoiY = $"Nhập ít nhất {MinDoDaiTuKhoa} ký tự rồi bấm «Tìm kiếm» hoặc Enter."
                };
        }

        if (text.Length < MinDoDaiTuKhoa)
        {
            return new MedicineSearchParseResult
            {
                CoTheTim = false,
                TuKhoaTim = null,
                ThongBaoGoiY = $"Nhập thêm để đủ {MinDoDaiTuKhoa} ký tự (hiện {text.Length} ký tự)."
            };
        }

        return new MedicineSearchParseResult
        {
            CoTheTim = true,
            TuKhoaTim = text,
            ThongBaoGoiY = string.Empty
        };
    }

    public static IReadOnlyList<DanhSachThuocViewDTO> LocChoNhapKho(IEnumerable<DanhSachThuocViewDTO> nguon)
    {
        return nguon
            .Where(t => !string.Equals(t.TrangThai, "Ngừng bán", StringComparison.OrdinalIgnoreCase))
            .Take(GioiHanChonNhapKho)
            .ToList();
    }

    public static string ThongBaoKetQuaDanhMuc(string? tuKhoa, int soDong)
    {
        if (string.IsNullOrEmpty(tuKhoa))
            return soDong > 0
                ? $"Hiển thị {soDong} thuốc trong danh mục."
                : "Danh mục thuốc đang trống.";

        if (soDong > 0)
            return $"Tìm thấy {soDong} thuốc khớp «{tuKhoa}».";

        return $"Không có thuốc khớp «{tuKhoa}». Kiểm tra chính tả hoặc thêm thuốc qua «Thêm hàng hóa».";
    }

    public static string ThongBaoKetQuaChonNhapKho(string tuKhoa, int soDong, int gioiHan = GioiHanChonNhapKho)
    {
        if (soDong == 0)
            return "Không tìm thấy trong danh mục. Thuốc chưa có — dùng «+ Thêm từ danh mục DQG» hoặc menu «Thêm hàng hóa».";

        var gioiHanText = soDong >= gioiHan
            ? $" (hiển thị tối đa {gioiHan} kết quả đầu tiên)"
            : string.Empty;
        return $"Tìm thấy {soDong} thuốc khớp «{tuKhoa}»{gioiHanText} — bấm chọn để nhập lô, số lượng:";
    }

    public static void GanSuKienTimKiem(TextBox txt, Button btnTim, Action thucHienTim, Action xoaTimKiem = null)
    {
        if (string.IsNullOrEmpty(txt.PlaceholderText))
            txt.PlaceholderText = PlaceholderTimThuoc;

        txt.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                thucHienTim();
                return;
            }

            if (e.KeyCode == Keys.Escape && xoaTimKiem is not null)
            {
                e.SuppressKeyPress = true;
                xoaTimKiem();
            }
        };

        btnTim.Click += (_, _) => thucHienTim();
    }
}
