using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Sales;

public partial class FrmKeDonBanThuoc : Form
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly Color Warn = Color.FromArgb(251, 140, 0);
    private static readonly Color Danger = Color.FromArgb(211, 47, 47);

    private readonly SalesService _sales = new(new DbContextDAL());
    private readonly KhachHangService _khachHang = new(new DbContextDAL());

    private readonly List<ThuocKeDonViewDTO> _thuocCache = new();
    private readonly List<DonHangGioHangDTO> _gioHang = new();

    private KhachHangDTO? _khachHienTai;
    private bool _khachLe;
    private bool _choPhepSuaKhach;

    public FrmKeDonBanThuoc()
    {
        InitializeComponent();
        WireEvents();
        SetupGrids();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        TaiDanhSachThuoc();
        CapNhatTongTien();
        DatTrangThaiKhach("Tra cứu CCCD hoặc chọn Khách lẻ");
    }

    private void WireEvents()
    {
        btnTraCuuKhach.Click += (_, _) => TraCuuKhach();
        btnKhachLe.Click += (_, _) => ChonKhachLe();
        btnTaoKhach.Click += (_, _) => LuuKhachMoi();
        btnTimThuoc.Click += (_, _) => TaiDanhSachThuoc();

        txtTimThuoc.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TaiDanhSachThuoc();
            }
        };

        dgvThuoc.SelectionChanged += (_, _) => CapNhatGoiYSoLuong();
        dgvThuoc.CellFormatting += DgvThuoc_CellFormatting;

        btnThemGio.Click += (_, _) => ThemVaoGio();
        btnXoaDong.Click += (_, _) => XoaDongGio();
        numGiamGia.ValueChanged += (_, _) => CapNhatTongTien();
        btnXacNhan.Click += (_, _) => XacNhanBan();
        btnLamMoi.Click += (_, _) => LamMoiDon();
        chkNgaySinh.CheckedChanged += chkNgaySinh_CheckedChanged;
    }

    private void SetupGrids()
    {
        dgvThuoc.AutoGenerateColumns = false;
        dgvThuoc.Columns.Clear();

        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.TenThuoc), "Tên thuốc", 160, 40));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.HoatChat), "Hoạt chất", 120, 22));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.HamLuong), "Hàm lượng", 90, 14));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.DonViTinh), "ĐVT", 52, 8));
        dgvThuoc.Columns.Add(CotSo(nameof(ThuocKeDonViewDTO.GiaBan), "Giá bán", 88, 12, "N0"));
        dgvThuoc.Columns.Add(CotSo(nameof(ThuocKeDonViewDTO.TonLoConHan), "Tồn lô", 58, 10, "N0"));

        dgvGioHang.AutoGenerateColumns = false;
        dgvGioHang.Columns.Clear();

        dgvGioHang.Columns.Add(CotChu(nameof(DonHangGioHangDTO.TenThuoc), "Thuốc", 140, 45));
        dgvGioHang.Columns.Add(CotChu(nameof(DonHangGioHangDTO.DonViTinh), "ĐVT", 48, 10));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.SoLuong), "SL", 44, 10, "N0"));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.DonGia), "Đơn giá", 88, 18, "N0"));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.ThanhTien), "Thành tiền", 96, 22, "N0"));

        ApDungStyleLuoi(dgvThuoc);
        ApDungStyleLuoi(dgvGioHang);
    }

    private static DataGridViewTextBoxColumn CotChu(string prop, string header, int minWidth, int fillWeight) =>
        new()
        {
            DataPropertyName = prop,
            HeaderText = header,
            MinimumWidth = minWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                WrapMode = DataGridViewTriState.True,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            }
        };

    private static DataGridViewTextBoxColumn CotSo(string prop, string header, int minWidth, int fillWeight, string format) =>
        new()
        {
            DataPropertyName = prop,
            HeaderText = header,
            MinimumWidth = minWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = format,
                WrapMode = DataGridViewTriState.False,
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };

    private static void ApDungStyleLuoi(DataGridView g)
    {
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        g.RowTemplate.MinimumHeight = 34;
        g.ColumnHeadersHeight = 40;
        g.EnableHeadersVisualStyles = false;

        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 125, 50);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
    }

    private void TraCuuKhach()
    {
        if (!Validator.TryNormalizeCccd(txtCccd.Text, out var cccd))
        {
            MessageBox.Show("CCCD phải 12 số");
            return;
        }

        txtCccd.Text = cccd;

        var kh = _khachHang.TraCuuTheoCccd(cccd);

        if (kh is null)
        {
            _khachHienTai = null;
            _choPhepSuaKhach = true;
            GanThongTinKhachForm(new KhachHangDTO { CCCD = cccd });
            DatTrangThaiKhach("Chưa có khách");
            return;
        }

        _khachHienTai = kh;
        _choPhepSuaKhach = false;
        GanThongTinKhachForm(kh);
        DatTrangThaiKhach($"Đã tìm: {kh.HoTen}");
    }

    private void ChonKhachLe()
    {
        _khachLe = true;
        _khachHienTai = null;
        _choPhepSuaKhach = false;
        DatTrangThaiKhach("Khách lẻ");
    }

    private void LuuKhachMoi()
    {
        var dto = new KhachHangDTO
        {
            CCCD = txtCccd.Text.Trim(),
            HoTen = txtHoTen.Text.Trim()
        };

        _khachHang.ThemKhachHang(dto);
        _khachHienTai = _khachHang.TraCuuTheoCccd(dto.CCCD);
        _choPhepSuaKhach = false;

        GanThongTinKhachForm(_khachHienTai!);
    }

    private void GanThongTinKhachForm(KhachHangDTO kh)
    {
        txtCccd.Text = kh.CCCD;
        txtHoTen.Text = kh.HoTen;
    }

    private void DatTrangThaiKhach(string text)
        => lblKhachTrangThai.Text = text;

    private void TaiDanhSachThuoc()
    {
        _thuocCache.Clear();
        _thuocCache.AddRange(_sales.TimKiemThuocBan(txtTimThuoc.Text.Trim()));

        dgvThuoc.DataSource = null;
        dgvThuoc.DataSource = _thuocCache;
    }

    private ThuocKeDonViewDTO? ThuocDangChon()
        => dgvThuoc.CurrentRow?.DataBoundItem as ThuocKeDonViewDTO;

    private void CapNhatGoiYSoLuong()
    {
        var t = ThuocDangChon();
        if (t is null) return;

        numSoLuong.Maximum = Math.Max(1, t.TonLoConHan);
    }

    private void ThemVaoGio() { }
    private void XoaDongGio() { }
    private void CapNhatTongTien() { }
    private void XacNhanBan() { }
    private void LamMoiDon() { }

    private void DgvThuoc_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
    }

    private void chkNgaySinh_CheckedChanged(object sender, EventArgs e)
    {
        dtpNgaySinh.Enabled = chkNgaySinh.Checked;
    }
}