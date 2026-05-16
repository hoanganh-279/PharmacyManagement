#nullable disable
using System.Drawing.Drawing2D;
using System.Globalization;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Report;

/// <summary>
/// Báo cáo 7b — Báo cáo thuốc cho Admin / Quản lý.
/// Bốn tab: Danh mục · Tồn kho · Bán chạy · Lịch sử (nhập/bán).
/// </summary>
public partial class FrmBaoCaoThuoc : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    private static readonly Color Muted = Color.FromArgb(97, 97, 97);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);
    private static readonly Color WarnOrange = Color.FromArgb(251, 140, 0);
    private static readonly Color DangerRed = Color.FromArgb(211, 47, 47);
    private static readonly Color CardBorder = Color.FromArgb(228, 234, 229);
    private static readonly Color BgSoft = Color.FromArgb(245, 247, 246);
    private static readonly Color RowAlt = Color.FromArgb(250, 252, 250);
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly ReportService _reportService = new(new DbContextDAL());
    private readonly MedicineService _medicineService = new(new DbContextDAL());

    private Panel _header;
    private TabControl _tabs;
    private TextBox _txtTimKiem;
    private ComboBox _cboTopBanChay;
    private Label _lblStatus;
    private Label _lblRowsInfo;
    private Button _btnRefresh;
    private Button _btnXuat;

    private Label _lblKpiSoLuong;
    private Label _lblKpiTong;
    private Label _lblKpiSub;
    private Label _lblKpiTitle;
    private Panel _kpiCard;

    private DataGridView _gridDanhMuc;
    private DataGridView _gridTonKho;
    private DataGridView _gridBanChay;
    private DataGridView _gridLichSuNhap;
    private DataGridView _gridLichSuBan;
    private TabControl _subTabsLichSu;

    private IReadOnlyList<DanhSachThuocViewDTO> _danhMucGoc = Array.Empty<DanhSachThuocViewDTO>();
    private IReadOnlyList<TonKhoViewDTO> _tonKhoGoc = Array.Empty<TonKhoViewDTO>();
    private IReadOnlyList<ThuocBanChayViewDTO> _banChayGoc = Array.Empty<ThuocBanChayViewDTO>();
    private IReadOnlyList<LichSuNhapKhoViewDTO> _lichSuNhapGoc = Array.Empty<LichSuNhapKhoViewDTO>();
    private IReadOnlyList<LichSuBanHangViewDTO> _lichSuBanGoc = Array.Empty<LichSuBanHangViewDTO>();

    public FrmBaoCaoThuoc()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmBaoCaoThuoc_Load;
    }

    private void FrmBaoCaoThuoc_Load(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            SetStatus("Phiên đăng nhập không hợp lệ.", DangerRed);
            return;
        }
        if (UserSession.TenVaiTro is not (VaiTroTen.Admin or VaiTroTen.QuanLy))
        {
            SetStatus("Tài khoản hiện tại không có quyền xem báo cáo thuốc.", WarnOrange);
            _tabs.Enabled = false;
            return;
        }
        TaiTatCaTabs();
    }

    private void BuildLayout()
    {
        BuildHeader();
        BuildTabs();
        BuildFooter();
        _tabs.BringToFront();
    }

    private void BuildHeader()
    {
        _header = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = Color.White };
        _header.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        };

        _header.Controls.Add(new Label
        {
            Text = "Báo cáo thuốc",
            AutoSize = true,
            Location = new Point(28, 14),
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Ink
        });
        _header.Controls.Add(new Label
        {
            Text = "Danh mục · Tồn kho · Bán chạy · Lịch sử nhập / bán — dành cho Admin / Quản lý.",
            AutoSize = true,
            Location = new Point(30, 50),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted
        });

        _txtTimKiem = new TextBox
        {
            PlaceholderText = "Lọc theo tên / hoạt chất / nhóm thuốc / nhà cung cấp...",
            Width = 380,
            Height = 32,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(28, 100),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        _txtTimKiem.TextChanged += (_, _) => ApDungLocHienHanh();

        _cboTopBanChay = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(420, 100),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        _cboTopBanChay.Items.AddRange(["Top 10 bán chạy", "Top 20 bán chạy", "Toàn bộ"]);
        _cboTopBanChay.SelectedIndex = 0;
        _cboTopBanChay.SelectedIndexChanged += (_, _) => TaiTabBanChay();

        _btnRefresh = MakeOutlineButton("↻ Tải lại", (_, _) => TaiTatCaTabs());
        _btnRefresh.Size = new Size(120, 34);
        _btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _btnXuat = MakePrimaryButton("Xuất Excel (CSV)", BtnXuat_Click);
        _btnXuat.Size = new Size(170, 34);
        _btnXuat.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _kpiCard = BuildKpiCard();
        _kpiCard.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _header.Resize += (_, _) =>
        {
            _btnXuat.Location = new Point(_header.Width - _btnXuat.Width - 28, 20);
            _btnRefresh.Location = new Point(_btnXuat.Left - _btnRefresh.Width - 10, 20);
            _kpiCard.Location = new Point(_header.Width - _kpiCard.Width - 28, 64);
        };

        _header.Controls.Add(_txtTimKiem);
        _header.Controls.Add(_cboTopBanChay);
        _header.Controls.Add(_btnRefresh);
        _header.Controls.Add(_btnXuat);
        _header.Controls.Add(_kpiCard);

        Controls.Add(_header);
    }

    private Panel BuildKpiCard()
    {
        var p = new Panel
        {
            Size = new Size(360, 72),
            BackColor = Color.White,
            Padding = new Padding(14, 8, 14, 8)
        };
        p.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = RoundRect(rect, 8);
            using var fill = new SolidBrush(MintBg);
            e.Graphics.FillPath(fill, path);
            using var pen = new Pen(Color.FromArgb(180, 210, 182), 1);
            e.Graphics.DrawPath(pen, path);
        };

        _lblKpiTitle = new Label
        {
            Text = "Danh mục thuốc",
            AutoSize = true,
            Location = new Point(14, 6),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = PrimaryDark,
            BackColor = Color.Transparent
        };
        _lblKpiSoLuong = new Label
        {
            Text = "—",
            AutoSize = true,
            Location = new Point(14, 24),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Ink,
            BackColor = Color.Transparent
        };
        _lblKpiTong = new Label
        {
            Text = "Tổng giá trị: —",
            AutoSize = true,
            Location = new Point(170, 28),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = PrimaryDark,
            BackColor = Color.Transparent
        };
        _lblKpiSub = new Label
        {
            Text = "thuốc đang quản lý",
            AutoSize = true,
            Location = new Point(170, 50),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Muted,
            BackColor = Color.Transparent
        };

        p.Controls.Add(_lblKpiTitle);
        p.Controls.Add(_lblKpiSoLuong);
        p.Controls.Add(_lblKpiTong);
        p.Controls.Add(_lblKpiSub);
        return p;
    }

    private void BuildTabs()
    {
        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ItemSize = new Size(190, 38),
            SizeMode = TabSizeMode.Fixed,
            Padding = new Point(16, 8)
        };
        _tabs.SelectedIndexChanged += (_, _) =>
        {
            _txtTimKiem.Visible = _tabs.SelectedIndex != 2;
            _cboTopBanChay.Visible = _tabs.SelectedIndex == 2;
            ApDungLocHienHanh();
            CapNhatKpiTheoTab();
        };

        _gridDanhMuc = BuildGridDanhMuc();
        _gridTonKho = BuildGridTonKho();
        _gridBanChay = BuildGridBanChay();
        _gridLichSuNhap = BuildGridLichSuNhap();
        _gridLichSuBan = BuildGridLichSuBan();

        var tab1 = NewTab("  Danh mục thuốc"); tab1.Controls.Add(WrapGridInCard(_gridDanhMuc));
        var tab2 = NewTab("  Tồn kho"); tab2.Controls.Add(WrapGridInCard(_gridTonKho));
        var tab3 = NewTab("  Bán chạy"); tab3.Controls.Add(WrapGridInCard(_gridBanChay));

        _subTabsLichSu = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.75F),
            ItemSize = new Size(160, 32),
            SizeMode = TabSizeMode.Fixed
        };
        _subTabsLichSu.SelectedIndexChanged += (_, _) =>
        {
            ApDungLocHienHanh();
            CapNhatKpiTheoTab();
        };
        var subNhap = NewTab("  Nhập kho"); subNhap.Controls.Add(WrapGridInCard(_gridLichSuNhap));
        var subBan = NewTab("  Bán hàng"); subBan.Controls.Add(WrapGridInCard(_gridLichSuBan));
        _subTabsLichSu.TabPages.Add(subNhap);
        _subTabsLichSu.TabPages.Add(subBan);

        var tab4 = NewTab("  Lịch sử");
        tab4.Controls.Add(_subTabsLichSu);

        _tabs.TabPages.Add(tab1);
        _tabs.TabPages.Add(tab2);
        _tabs.TabPages.Add(tab3);
        _tabs.TabPages.Add(tab4);

        Controls.Add(_tabs);
        _cboTopBanChay.Visible = false;
    }

    private void BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };
        _lblStatus = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 0, 28, 0),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9.5F),
            Text = "Sẵn sàng."
        };
        _lblRowsInfo = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Right,
            Width = 260,
            Padding = new Padding(0, 0, 28, 0),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
            Text = ""
        };
        footer.Controls.Add(_lblStatus);
        footer.Controls.Add(_lblRowsInfo);
        Controls.Add(footer);
    }

    private static TabPage NewTab(string text) =>
        new(text) { BackColor = BgSoft, Padding = new Padding(16) };

    private static Panel WrapGridInCard(DataGridView grid)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, card.Width - 1, card.Height - 1));
        };
        grid.Dock = DockStyle.Fill;
        card.Controls.Add(grid);
        return card;
    }

    private DataGridView NewGrid()
    {
        return new DataGridView
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = CardBorder,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 34 },
            Font = new Font("Segoe UI", 9.75F),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = RowAlt },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MintBg,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                Padding = new Padding(10, 8, 10, 8),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = MintBg,
                SelectionForeColor = Ink,
                Padding = new Padding(8, 4, 8, 4)
            }
        };
    }

    private DataGridView BuildGridDanhMuc()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaThuoc", HeaderText = "Mã", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", FillWeight = 200 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoatChat", HeaderText = "Hoạt chất", FillWeight = 150 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HamLuong", HeaderText = "Hàm lượng", FillWeight = 80 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonViTinh", HeaderText = "ĐVT", FillWeight = 60 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenNhomThuoc", HeaderText = "Nhóm", FillWeight = 130 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiaNhap", HeaderText = "Giá nhập", FillWeight = 90 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "GiaBan", HeaderText = "Giá bán", FillWeight = 90 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongTon", HeaderText = "Tồn", FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "Trạng thái", FillWeight = 110 });

        g.Columns["GiaNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["GiaBan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView BuildGridTonKho()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaThuoc", HeaderText = "Mã", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", FillWeight = 220 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenNhomThuoc", HeaderText = "Nhóm", FillWeight = 140 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonViTinh", HeaderText = "ĐVT", FillWeight = 60 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongTon", HeaderText = "Tồn", FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TonToiThieu", HeaderText = "Tối thiểu", FillWeight = 90 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HanSuDung", HeaderText = "HSD", FillWeight = 100 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThaiTonKho", HeaderText = "Tình trạng", FillWeight = 130 });
        g.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["TonToiThieu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView BuildGridBanChay()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hang", HeaderText = "#", FillWeight = 40 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", FillWeight = 240 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenNhomThuoc", HeaderText = "Nhóm", FillWeight = 140 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongSoLuongBan", HeaderText = "SL đã bán", FillWeight = 100 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongDoanhThu", HeaderText = "Doanh thu", FillWeight = 130 });
        g.Columns["Hang"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        g.Columns["Hang"].DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        g.Columns["TongSoLuongBan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["TongDoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView BuildGridLichSuNhap()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaPN", HeaderText = "Phiếu", FillWeight = 60 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayNhap", HeaderText = "Ngày nhập", FillWeight = 110 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NhanVienNhap", HeaderText = "Thủ kho", FillWeight = 140 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NhaCungCap", HeaderText = "Nhà cung cấp", FillWeight = 150 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Thuốc", FillWeight = 200 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongNhap", HeaderText = "SL", FillWeight = 60 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonGiaNhap", HeaderText = "Đơn giá", FillWeight = 100 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "Thành tiền", FillWeight = 120 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HanSuDung", HeaderText = "HSD", FillWeight = 100 });
        g.Columns["SoLuongNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["DonGiaNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView BuildGridLichSuBan()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHD", HeaderText = "HĐ", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayLap", HeaderText = "Ngày lập", FillWeight = 120 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NhanVienBan", HeaderText = "Dược sĩ", FillWeight = 130 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenKhachHang", HeaderText = "Khách hàng", FillWeight = 150 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Thuốc", FillWeight = 200 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongBan", HeaderText = "SL", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonGiaBan", HeaderText = "Đơn giá", FillWeight = 90 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThanhTien", HeaderText = "Thành tiền", FillWeight = 110 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "Trạng thái", FillWeight = 110 });
        g.Columns["SoLuongBan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["DonGiaBan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private void TaiTatCaTabs()
    {
        try
        {
            TaiTabDanhMuc();
            TaiTabTonKho();
            TaiTabBanChay();
            TaiTabLichSu();
            SetStatus($"Tải dữ liệu báo cáo lúc {DateTime.Now:HH:mm:ss}.", Muted);
            CapNhatKpiTheoTab();
        }
        catch (UnauthorizedAccessException ex)
        {
            SetStatus(ex.Message, DangerRed);
        }
        catch (Exception ex)
        {
            SetStatus("Lỗi tải báo cáo: " + ex.Message, DangerRed);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void TaiTabDanhMuc()
    {
        _danhMucGoc = _medicineService.LayDanhSachThuoc();
        ApDungLocDanhMuc();
    }

    private void TaiTabTonKho()
    {
        _tonKhoGoc = _reportService.LayTonKho();
        ApDungLocTonKho();
    }

    private void TaiTabBanChay()
    {
        _banChayGoc = _reportService.LayThuocBanChay();
        ApDungBanChay();
    }

    private void TaiTabLichSu()
    {
        _lichSuNhapGoc = _reportService.LayLichSuNhapKho(top: 200);
        try
        {
            _lichSuBanGoc = _reportService.LayLichSuBanHang(top: 200);
        }
        catch (UnauthorizedAccessException)
        {
            _lichSuBanGoc = Array.Empty<LichSuBanHangViewDTO>();
        }
        ApDungLocLichSu();
    }

    private void ApDungLocHienHanh()
    {
        switch (_tabs.SelectedIndex)
        {
            case 0: ApDungLocDanhMuc(); break;
            case 1: ApDungLocTonKho(); break;
            case 2: ApDungBanChay(); break;
            case 3: ApDungLocLichSu(); break;
        }
        CapNhatKpiTheoTab();
    }

    private void ApDungLocDanhMuc()
    {
        _gridDanhMuc.Rows.Clear();
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<DanhSachThuocViewDTO> nguon = _danhMucGoc;
        if (kw.Length > 0)
        {
            nguon = _danhMucGoc.Where(t =>
                Contains(t.TenThuoc, kw) ||
                Contains(t.HoatChat, kw) ||
                Contains(t.HamLuong, kw) ||
                Contains(t.TenNhomThuoc, kw));
        }

        foreach (var t in nguon)
        {
            var idx = _gridDanhMuc.Rows.Add(
                t.MaThuoc,
                t.TenThuoc,
                t.HoatChat ?? "—",
                t.HamLuong ?? "—",
                t.DonViTinh,
                t.TenNhomThuoc,
                t.GiaNhap.ToString("C0", Vi),
                t.GiaBan.ToString("C0", Vi),
                t.SoLuongTon.ToString("N0", Vi),
                t.TrangThai);
            ApplyStatusBadge(_gridDanhMuc.Rows[idx].Cells["TrangThai"], t.TrangThai);
        }
    }

    private void ApDungLocTonKho()
    {
        _gridTonKho.Rows.Clear();
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<TonKhoViewDTO> nguon = _tonKhoGoc;
        if (kw.Length > 0)
        {
            nguon = _tonKhoGoc.Where(t =>
                Contains(t.TenThuoc, kw) ||
                Contains(t.TenNhomThuoc, kw) ||
                Contains(t.DonViTinh, kw));
        }

        foreach (var t in nguon)
        {
            var idx = _gridTonKho.Rows.Add(
                t.MaThuoc,
                t.TenThuoc,
                t.TenNhomThuoc,
                t.DonViTinh,
                t.SoLuongTon.ToString("N0", Vi),
                t.TonToiThieu.ToString("N0", Vi),
                t.HanSuDung?.ToString("dd/MM/yyyy", Vi) ?? "—",
                t.TrangThaiTonKho);
            ApplyStatusBadge(_gridTonKho.Rows[idx].Cells["TrangThaiTonKho"], t.TrangThaiTonKho);
            if (t.SoLuongTon < t.TonToiThieu)
            {
                var c = _gridTonKho.Rows[idx].Cells["SoLuongTon"];
                c.Style.ForeColor = t.SoLuongTon == 0 ? DangerRed : WarnOrange;
                c.Style.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            }
        }
    }

    private void ApDungBanChay()
    {
        var top = _cboTopBanChay.SelectedIndex switch
        {
            0 => _banChayGoc.Take(10).ToList(),
            1 => _banChayGoc.Take(20).ToList(),
            _ => _banChayGoc.ToList()
        };

        _gridBanChay.Rows.Clear();
        var rank = 1;
        foreach (var t in top)
        {
            var idx = _gridBanChay.Rows.Add(
                rank,
                t.TenThuoc,
                t.TenNhomThuoc,
                t.TongSoLuongBan.ToString("N0", Vi),
                t.TongDoanhThu.ToString("C0", Vi));
            if (rank <= 3)
            {
                var hangCell = _gridBanChay.Rows[idx].Cells["Hang"];
                hangCell.Style.ForeColor = Color.White;
                hangCell.Style.BackColor = rank switch
                {
                    1 => Color.FromArgb(212, 175, 55),
                    2 => Color.FromArgb(160, 160, 160),
                    _ => Color.FromArgb(176, 141, 87)
                };
                hangCell.Style.SelectionBackColor = hangCell.Style.BackColor;
                hangCell.Style.SelectionForeColor = Color.White;
                hangCell.Style.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            rank++;
        }
    }

    private void ApDungLocLichSu()
    {
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();

        _gridLichSuNhap.Rows.Clear();
        IEnumerable<LichSuNhapKhoViewDTO> nhap = _lichSuNhapGoc;
        if (kw.Length > 0)
        {
            nhap = _lichSuNhapGoc.Where(n =>
                Contains(n.TenThuoc, kw) ||
                Contains(n.NhanVienNhap, kw) ||
                Contains(n.NhaCungCap, kw));
        }
        foreach (var n in nhap)
        {
            _gridLichSuNhap.Rows.Add(
                "#" + n.MaPhieuNhap.ToString("D5", Vi),
                n.NgayNhap.ToString("dd/MM/yyyy HH:mm", Vi),
                n.NhanVienNhap,
                n.NhaCungCap ?? "—",
                n.TenThuoc,
                n.SoLuongNhap.ToString("N0", Vi),
                n.DonGiaNhap.ToString("C0", Vi),
                n.ThanhTien.ToString("C0", Vi),
                n.HanSuDung?.ToString("dd/MM/yyyy", Vi) ?? "—");
        }

        _gridLichSuBan.Rows.Clear();
        IEnumerable<LichSuBanHangViewDTO> ban = _lichSuBanGoc;
        if (kw.Length > 0)
        {
            ban = _lichSuBanGoc.Where(b =>
                Contains(b.TenThuoc, kw) ||
                Contains(b.NhanVienBan, kw) ||
                Contains(b.TenKhachHang, kw));
        }
        foreach (var b in ban)
        {
            var idx = _gridLichSuBan.Rows.Add(
                "#" + b.MaHoaDon.ToString("D5", Vi),
                b.NgayLap.ToString("dd/MM/yyyy HH:mm", Vi),
                b.NhanVienBan,
                b.TenKhachHang ?? "Khách lẻ",
                b.TenThuoc,
                b.SoLuongBan.ToString("N0", Vi),
                b.DonGiaBan.ToString("C0", Vi),
                b.ThanhTien.ToString("C0", Vi),
                UnicodeTextHelper.TryRepairMojibakeForDisplay(b.TrangThai));
            ApplyStatusBadge(_gridLichSuBan.Rows[idx].Cells["TrangThai"], b.TrangThai);
        }
    }

    private void CapNhatKpiTheoTab()
    {
        if (_lblKpiSoLuong is null) return;
        var grid = TabHienHanhGrid();
        var rowCount = grid?.Rows.Count ?? 0;

        switch (_tabs.SelectedIndex)
        {
            case 0:
                _lblKpiTitle.Text = "Danh mục thuốc";
                _lblKpiSub.Text = "thuốc đang quản lý";
                _lblKpiSoLuong.Text = rowCount.ToString("N0", Vi);
                _lblKpiTong.Text = "Tổng giá trị: " + TongGiaTriDanhMuc().ToString("C0", Vi);
                break;
            case 1:
                _lblKpiTitle.Text = "Tồn kho";
                _lblKpiSub.Text = "SKU có dữ liệu tồn";
                _lblKpiSoLuong.Text = rowCount.ToString("N0", Vi);
                _lblKpiTong.Text = "Tổng tồn: " + TongTonKho().ToString("N0", Vi);
                break;
            case 2:
                _lblKpiTitle.Text = "Bán chạy";
                _lblKpiSub.Text = "thuốc đứng top doanh thu";
                _lblKpiSoLuong.Text = rowCount.ToString("N0", Vi);
                _lblKpiTong.Text = "Tổng DT: " + TongDoanhThuBanChay().ToString("C0", Vi);
                break;
            case 3:
                if (_subTabsLichSu?.SelectedIndex == 1)
                {
                    _lblKpiTitle.Text = "Lịch sử bán";
                    _lblKpiSub.Text = "dòng bán hàng (top 200)";
                    _lblKpiSoLuong.Text = rowCount.ToString("N0", Vi);
                    _lblKpiTong.Text = "Doanh thu: " + TongDoanhThuBan().ToString("C0", Vi);
                }
                else
                {
                    _lblKpiTitle.Text = "Lịch sử nhập";
                    _lblKpiSub.Text = "dòng nhập kho (top 200)";
                    _lblKpiSoLuong.Text = rowCount.ToString("N0", Vi);
                    _lblKpiTong.Text = "Tổng nhập: " + TongTienNhap().ToString("C0", Vi);
                }
                break;
        }

        _lblRowsInfo.Text = rowCount > 0 ? $"Đang hiển thị {rowCount:N0} dòng." : "Không có dữ liệu.";
    }

    private decimal TongGiaTriDanhMuc()
    {
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<DanhSachThuocViewDTO> nguon = _danhMucGoc;
        if (kw.Length > 0)
            nguon = _danhMucGoc.Where(t =>
                Contains(t.TenThuoc, kw) ||
                Contains(t.HoatChat, kw) ||
                Contains(t.HamLuong, kw) ||
                Contains(t.TenNhomThuoc, kw));
        return nguon.Sum(t => t.GiaBan * t.SoLuongTon);
    }

    private long TongTonKho()
    {
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<TonKhoViewDTO> nguon = _tonKhoGoc;
        if (kw.Length > 0)
            nguon = _tonKhoGoc.Where(t => Contains(t.TenThuoc, kw) || Contains(t.TenNhomThuoc, kw) || Contains(t.DonViTinh, kw));
        return nguon.Sum(t => (long)t.SoLuongTon);
    }

    private decimal TongDoanhThuBanChay()
    {
        var top = _cboTopBanChay.SelectedIndex switch
        {
            0 => _banChayGoc.Take(10),
            1 => _banChayGoc.Take(20),
            _ => _banChayGoc
        };
        return top.Sum(t => t.TongDoanhThu);
    }

    private decimal TongTienNhap()
    {
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<LichSuNhapKhoViewDTO> nguon = _lichSuNhapGoc;
        if (kw.Length > 0)
            nguon = _lichSuNhapGoc.Where(n =>
                Contains(n.TenThuoc, kw) || Contains(n.NhanVienNhap, kw) || Contains(n.NhaCungCap, kw));
        return nguon.Sum(n => n.ThanhTien);
    }

    private decimal TongDoanhThuBan()
    {
        var kw = (_txtTimKiem?.Text ?? string.Empty).Trim();
        IEnumerable<LichSuBanHangViewDTO> nguon = _lichSuBanGoc;
        if (kw.Length > 0)
            nguon = _lichSuBanGoc.Where(b =>
                Contains(b.TenThuoc, kw) || Contains(b.NhanVienBan, kw) || Contains(b.TenKhachHang, kw));
        return nguon.Sum(b => b.ThanhTien);
    }

    private static bool Contains(string s, string kw)
        => !string.IsNullOrEmpty(s) && s.Contains(kw, StringComparison.OrdinalIgnoreCase);

    private static void ApplyStatusBadge(DataGridViewCell cell, string status)
    {
        if (string.IsNullOrEmpty(status)) return;
        Color bg, fg;
        var s = UnicodeTextHelper.TryRepairMojibakeForDisplay(status);

        if (s.Contains("Ngừng", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Hủy", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Hết", StringComparison.OrdinalIgnoreCase))
        {
            bg = Color.FromArgb(252, 230, 230);
            fg = DangerRed;
        }
        else if (s.Contains("Tồn thấp", StringComparison.OrdinalIgnoreCase)
                 || s.Contains("Sắp", StringComparison.OrdinalIgnoreCase))
        {
            bg = Color.FromArgb(255, 240, 220);
            fg = WarnOrange;
        }
        else if (s.Contains("Hoàn", StringComparison.OrdinalIgnoreCase)
                 || s.Contains("Đang bán", StringComparison.OrdinalIgnoreCase)
                 || s.Contains("Đang", StringComparison.OrdinalIgnoreCase)
                 || s.Contains("Còn", StringComparison.OrdinalIgnoreCase))
        {
            bg = MintBg;
            fg = Primary;
        }
        else
        {
            bg = Color.FromArgb(238, 240, 241);
            fg = Muted;
        }
        cell.Style.BackColor = bg;
        cell.Style.SelectionBackColor = bg;
        cell.Style.ForeColor = fg;
        cell.Style.SelectionForeColor = fg;
        cell.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private void BtnXuat_Click(object sender, EventArgs e)
    {
        var grid = TabHienHanhGrid();
        if (grid is null || grid.Rows.Count == 0)
        {
            SetStatus("Không có dữ liệu để xuất.", WarnOrange);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"BaoCaoThuoc_{TenTabHienHanh()}_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            Title = "Xuất báo cáo (CSV)"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            using var sw = new System.IO.StreamWriter(dlg.FileName, false, new System.Text.UTF8Encoding(true));
            var headers = grid.Columns.Cast<DataGridViewColumn>().Select(c => Escape(c.HeaderText));
            sw.WriteLine(string.Join(",", headers));
            foreach (DataGridViewRow row in grid.Rows)
            {
                var cells = row.Cells.Cast<DataGridViewCell>().Select(c => Escape(c.Value?.ToString() ?? ""));
                sw.WriteLine(string.Join(",", cells));
            }
            SetStatus("Đã xuất " + dlg.FileName, Primary);
        }
        catch (Exception ex)
        {
            SetStatus("Lỗi xuất file: " + ex.Message, DangerRed);
        }
    }

    private string TenTabHienHanh() => _tabs.SelectedIndex switch
    {
        0 => "DanhMuc",
        1 => "TonKho",
        2 => "BanChay",
        3 => _subTabsLichSu.SelectedIndex == 0 ? "LichSuNhap" : "LichSuBan",
        _ => "BaoCao"
    };

    private DataGridView TabHienHanhGrid()
    {
        return _tabs.SelectedIndex switch
        {
            0 => _gridDanhMuc,
            1 => _gridTonKho,
            2 => _gridBanChay,
            3 => _subTabsLichSu.SelectedIndex == 0 ? _gridLichSuNhap : _gridLichSuBan,
            _ => null
        };
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;
        var need = s.Contains(',') || s.Contains('"') || s.Contains('\n');
        var escaped = s.Replace("\"", "\"\"");
        return need ? $"\"{escaped}\"" : escaped;
    }

    private static GraphicsPath RoundRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static Button MakePrimaryButton(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = PrimaryDark;
        b.Click += onClick;
        return b;
    }

    private static Button MakeOutlineButton(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Color.White,
            ForeColor = PrimaryDark,
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        b.FlatAppearance.MouseOverBackColor = MintBg;
        b.Click += onClick;
        return b;
    }

    private void SetStatus(string text, Color color)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = color;
    }
}
