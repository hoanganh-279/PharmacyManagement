// File: Pharmacy.BLL/DoanhThuBLL.cs
// Đặt vào project Pharmacy.BLL
// Xử lý nghiệp vụ, lọc, validate trước khi trả về UI

using Pharmacy.DAL;
using Pharmacy.DTO.Views;

namespace Pharmacy.BLL
{
    public class DoanhThuBLL
    {
        private readonly DoanhThuDAL _dal = new();

        // ---------- Lấy danh sách doanh thu (có lọc trạng thái) ----------
        public List<DoanhThuDTO> LayDoanhThu(
            DateTime tuNgay,
            DateTime denNgay,
            string? trangThai = null)
        {
            if (tuNgay > denNgay)
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");

            var ds = _dal.LayDoanhThuTheoKhoangNgay(tuNgay, denNgay);

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                ds = ds.Where(x => x.TrangThai == trangThai).ToList();

            return ds;
        }

        // ---------- Tổng kết ----------
        public TongKetDoanhThuDTO LayTongKet(DateTime tuNgay, DateTime denNgay)
            => _dal.LayTongKet(tuNgay, denNgay);

        // ---------- Tìm kiếm theo mã / tên ----------
        public List<DoanhThuDTO> TimKiem(
            DateTime tuNgay, DateTime denNgay, string keyword)
        {
            var ds = _dal.LayDoanhThuTheoKhoangNgay(tuNgay, denNgay);
            keyword = keyword.Trim().ToLower();
            return ds.Where(x =>
                x.SoDienThoai.ToLower().Contains(keyword)||
                x.MaHoaDon.ToLower().Contains(keyword) ||
                x.TenKhachHang.ToLower().Contains(keyword) ||
                x.TenNhanVien.ToLower().Contains(keyword)
            ).ToList();
        }

        // ---------- Xuất DataTable cho DataGridView ----------
        public System.Data.DataTable ToDataTable(List<DoanhThuDTO> ds)
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Mã HĐ", typeof(string));
            dt.Columns.Add("Ngày bán", typeof(string));
            dt.Columns.Add("Khách hàng", typeof(string));
            dt.Columns.Add("Số điện thoại", typeof(string));
            dt.Columns.Add("Nhân viên", typeof(string));
            dt.Columns.Add("Tổng tiền", typeof(decimal));
            dt.Columns.Add("Giảm giá", typeof(decimal));
            dt.Columns.Add("Thanh toán", typeof(decimal));
            dt.Columns.Add("Lợi nhuận", typeof(decimal));
            dt.Columns.Add("Trạng thái", typeof(string));

            foreach (var r in ds)
                dt.Rows.Add(
    r.MaHoaDon,
    r.NgayBan.ToString("dd/MM/yyyy HH:mm"),
    r.TenKhachHang,
    r.SoDienThoai,
    r.TenNhanVien,
    r.TongTien,
    r.TienGiam,
    r.ThanhToan,
    r.LoiNhuan,
    r.TrangThai);

            return dt;
        }
    }
}