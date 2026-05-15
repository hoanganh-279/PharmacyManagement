using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL;

public class MedicineService
{
    private readonly ThuocRepositoryDAL _thuoc;
    private readonly NhomThuocRepositoryDAL _nhom;
    private readonly DanhMucDQGRepositoryDAL _dqg;

    public MedicineService(DbContextDAL db)
    {
        _thuoc = new ThuocRepositoryDAL(db);
        _nhom = new NhomThuocRepositoryDAL(db);
        _dqg = new DanhMucDQGRepositoryDAL(db);
    }

    public IReadOnlyList<DanhSachThuocViewDTO> LayDanhSachThuoc()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _thuoc.LayTuViewDanhSach();
    }

    public ThuocDTO? LayChiTietThuoc(int maThuoc)
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _thuoc.LayTheoMa(maThuoc);
    }

    public int ThemThuoc(ThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        ValidateThuoc(dto);
        if (_thuoc.TonTaiTheoTen(dto.TenThuoc, dto.MaDQG))
            throw new ArgumentException("Đã tồn tại thuốc có cùng tên hoặc cùng mã DQG.");

        return _thuoc.Them(dto);
    }

    public void CapNhatThuoc(ThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        if (dto.MaThuoc <= 0)
            throw new ArgumentException("Mã thuốc không hợp lệ.");
        ValidateThuoc(dto);
        if (_thuoc.TonTaiTheoTen(dto.TenThuoc, dto.MaDQG, dto.MaThuoc))
            throw new ArgumentException("Đã tồn tại thuốc khác có cùng tên hoặc cùng mã DQG.");

        _thuoc.CapNhat(dto);
    }

    private static void ValidateThuoc(ThuocDTO dto)
    {
        if (Validator.IsNullOrWhiteSpace(dto.TenThuoc))
            throw new ArgumentException("Tên thuốc là bắt buộc.");
        if (Validator.IsNullOrWhiteSpace(dto.DonViTinh))
            throw new ArgumentException("Đơn vị tính là bắt buộc.");
        if (!Validator.IsNonNegativeDecimal(dto.GiaNhap) || !Validator.IsNonNegativeDecimal(dto.GiaBan))
            throw new ArgumentException("Giá nhập / giá bán không được âm.");
        if (dto.GiaBan > 0 && dto.GiaBan < dto.GiaNhap)
            throw new ArgumentException("Giá bán nên lớn hơn hoặc bằng giá nhập.");
        if (dto.TonToiThieu < 0 || dto.SoLuongTon < 0)
            throw new ArgumentException("Tồn kho không được âm.");
        if (dto.HanSuDung is { } hsd && hsd.Date < DateTime.Today)
            throw new ArgumentException("Hạn sử dụng phải lớn hơn hoặc bằng ngày hiện tại.");
    }

    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

    /// <summary>Tra cứu danh mục Dược Quốc Gia phục vụ thêm hàng / nhập kho.</summary>
    public IReadOnlyList<TraCuuDanhMucDQGViewDTO> TraCuuDQG(string? tuKhoa = null, int top = 200)
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);

        try
        {
            // THỬ GỌI API TRƯỚC (Ưu tiên số 1)
            return TraCuuDQGTuApi(tuKhoa, top);
        }
        catch
        {
            // NẾU LỖI (mất mạng, URL sai, API yêu cầu Token) -> FALLBACK VỀ DATABASE NỘI BỘ
            return _dqg.TraCuu(tuKhoa, top);
        }
    }

    private IReadOnlyList<TraCuuDanhMucDQGViewDTO> TraCuuDQGTuApi(string? tuKhoa, int top)
    {
        // 1. Cấu hình URL API của Cục Quản Lý Dược (Bộ Y Tế) hoặc Drugbank.vn
        // URL giả định (vì hệ thống quốc gia yêu cầu đăng ký Token mới cho gọi API thật):
        string apiUrl = "https://api.dichvucong.dav.gov.vn/api/public/thuoc?keyword=" + tuKhoa;

        // 2. Gọi HTTP GET
        var responseJson = _httpClient.GetStringAsync(apiUrl).GetAwaiter().GetResult();

        // 3. Deserialize JSON trả về thành danh sách TraCuuDanhMucDQGViewDTO
        // var data = JsonSerializer.Deserialize<...>(responseJson);
        // return data;

        // Do chưa có Token/URL chính thức nên cố tình Throw để hệ thống tự chạy về Database cục bộ
        throw new NotImplementedException("Cần đăng ký API Cục Quản Lý Dược để sử dụng thực tế.");
    }

    public DanhMucDQGDTO? LayDQGTheoMa(int maDQG)
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        return _dqg.LayTheoMa(maDQG);
    }

    public void NgungKinhDoanh(int maThuoc)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho);
        _thuoc.DatTrangThai(maThuoc, trangThai: false);
    }

    public IReadOnlyList<NhomThuocDTO> LayNhomThuoc()
    {
        BllAuthorization.RequireAnyRole(
            VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho);
        return _nhom.LayTatCa();
    }

    public int ThemNhom(NhomThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        if (Validator.IsNullOrWhiteSpace(dto.TenNhomThuoc))
            throw new ArgumentException("Tên nhóm thuốc là bắt buộc.");
        return _nhom.Them(dto);
    }

    public void CapNhatNhom(NhomThuocDTO dto)
    {
        BllAuthorization.RequireAnyRole(VaiTroTen.Admin, VaiTroTen.QuanLy);
        if (dto.MaNhomThuoc <= 0 || Validator.IsNullOrWhiteSpace(dto.TenNhomThuoc))
            throw new ArgumentException("Nhóm thuốc không hợp lệ.");
        _nhom.CapNhat(dto);
    }
}
