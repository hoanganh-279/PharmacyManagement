#nullable disable
using System.Drawing.Drawing2D;
using System.Globalization;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Product;

/// <summary>
/// Màn "Thêm hàng hóa" theo project_Context §2.1 mục 3. Ba tab:
///   1) Thông tin chung — tên, hoạt chất, hàm lượng, đơn vị, nhóm, hãng SX, ghi chú.
///   2) Thuộc tính — giá nhập / giá bán, tồn tối thiểu / hiện tại, HSD, trạng thái.
///   3) Đơn vị / DQG — tra cứu danh mục Dược Quốc Gia, gắn vào thuốc.
/// Form là host trong FrmMain (TopLevel = false). Mọi nghiệp vụ đi qua BLL.
/// </summary>
public partial class FrmThemHangHoa : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    private static readonly Color Muted = Color.FromArgb(97, 97, 97);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);
    private static readonly Color DangerRed = Color.FromArgb(211, 47, 47);
    private static readonly Color WarnOrange = Color.FromArgb(251, 140, 0);
    private static readonly Color CardBorder = Color.FromArgb(228, 234, 229);
    private static readonly Color BgSoft = Color.FromArgb(245, 247, 246);
    private static readonly Color BadgeBg = Color.FromArgb(238, 246, 239);

    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly MedicineService _medicineService = new(new DbContextDAL());

    private Panel _header;
    private Label _lblTitle;
    private Label _lblSubtitle;
    private Panel _badgeDQG;
    private Label _lblBadgeDQG;
    private Panel _badgeLoiNhuan;
    private Label _lblBadgeLoiNhuan;
    private Panel _badgeHSD;
    private Label _lblBadgeHSD;

    private TabControl _tabs;
    private TabPage _tabChung;
    private TabPage _tabThuocTinh;
    private TabPage _tabDonVi;

    private TextBox _txtTenThuoc;
    private TextBox _txtHoatChat;
    private TextBox _txtHamLuong;
    private ComboBox _cboDonViTinh;
    private ComboBox _cboNhomThuoc;
    private TextBox _txtHangSanXuat;
    private TextBox _txtNuocSanXuat;
    private TextBox _txtDongGoi;
    private TextBox _txtSoDangKy;

    private NumericUpDown _numGiaNhap;
    private NumericUpDown _numGiaBan;
    private NumericUpDown _numTonToiThieu;
    private NumericUpDown _numSoLuongTon;
    private DateTimePicker _dtpHanSuDung;
    private CheckBox _chkHanSuDung;
    private CheckBox _chkTrangThai;
    private Label _lblLoiNhuanHint;

    private TextBox _txtTimDQG;
    private Button _btnTimDQG;
    private Button _btnXoaLienKetDQG;
    private DataGridView _gridDQG;
    private Label _lblDqgDaChon;
    private int? _maDQGDaChon;

    private Button _btnLuu;
    private Button _btnReset;
    private Label _lblStatus;

    private IReadOnlyList<NhomThuocDTO> _danhSachNhom = Array.Empty<NhomThuocDTO>();
    private IReadOnlyList<TraCuuDanhMucDQGViewDTO> _ketQuaDQG = Array.Empty<TraCuuDanhMucDQGViewDTO>();

    public FrmThemHangHoa()
    {
        InitializeComponent();
        BuildLayout();
        WireEvents();
    }

    private void WireEvents()
    {
        Load += FrmThemHangHoa_Load;
    }

    private void FrmThemHangHoa_Load(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            SetStatus("Phiên đăng nhập không hợp lệ.", DangerRed);
            return;
        }

        if (UserSession.TenVaiTro is not (VaiTroTen.Admin or VaiTroTen.QuanLy or VaiTroTen.NhanVienKho))
        {
            ToggleEditing(enabled: false);
            SetStatus("Tài khoản hiện tại không có quyền thêm hàng hóa.", WarnOrange);
            return;
        }

        TaiDanhMucNhomThuoc();
        TaiDanhMucDQG(tuKhoa: null);
        ResetForm();
    }

    private void BuildLayout()
    {
        Padding = new Padding(0);

        BuildHeader();
        BuildFooter();

        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ItemSize = new Size(190, 38),
            SizeMode = TabSizeMode.Fixed,
            Padding = new Point(16, 8)
        };

        _tabChung = new TabPage("  Thông tin chung") { BackColor = BgSoft, Padding = new Padding(20) };
        _tabThuocTinh = new TabPage("  Thuộc tính & giá") { BackColor = BgSoft, Padding = new Padding(20) };
        _tabDonVi = new TabPage("  Đơn vị / DQG") { BackColor = BgSoft, Padding = new Padding(20) };

        BuildTabChung();
        BuildTabThuocTinh();
        BuildTabDonVi();

        _tabs.TabPages.Add(_tabChung);
        _tabs.TabPages.Add(_tabThuocTinh);
        _tabs.TabPages.Add(_tabDonVi);

        Controls.Add(_tabs);
        _tabs.BringToFront();
    }

    private void BuildHeader()
    {
        _header = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = Color.White };
        _header.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        };
        _lblTitle = new Label
        {
            Text = "Thêm hàng hóa",
            AutoSize = true,
            Location = new Point(28, 18),
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Ink
        };
        _lblSubtitle = new Label
        {
            Text = "Khai báo thuốc mới — đối chiếu Danh mục Dược Quốc Gia (DQG) khi có thể.",
            AutoSize = true,
            Location = new Point(30, 54),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted
        };

        _badgeDQG = MakeBadge(out _lblBadgeDQG, "DQG: chưa gắn", Muted);
        _badgeLoiNhuan = MakeBadge(out _lblBadgeLoiNhuan, "Biên LN: —", Muted);
        _badgeHSD = MakeBadge(out _lblBadgeHSD, "HSD: chưa đặt", Muted);

        _badgeDQG.Location = new Point(28, 86);
        _badgeLoiNhuan.Location = new Point(_badgeDQG.Right + 10, 86);
        _badgeHSD.Location = new Point(_badgeLoiNhuan.Right + 10, 86);

        _header.Controls.Add(_lblTitle);
        _header.Controls.Add(_lblSubtitle);
        _header.Controls.Add(_badgeDQG);
        _header.Controls.Add(_badgeLoiNhuan);
        _header.Controls.Add(_badgeHSD);

        Controls.Add(_header);
    }

    private void BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 66, BackColor = Color.White };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };

        _lblStatus = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(28, 0),
            Size = new Size(560, 66),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted,
            Text = "Sẵn sàng.",
            AutoEllipsis = true
        };

        _btnReset = MakeOutlineButton("Làm mới", BtnReset_Click);
        _btnReset.Size = new Size(140, 40);
        _btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _btnLuu = MakePrimaryButton("Lưu thuốc", BtnLuu_Click);
        _btnLuu.Size = new Size(180, 40);
        _btnLuu.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        footer.Controls.Add(_lblStatus);
        footer.Controls.Add(_btnReset);
        footer.Controls.Add(_btnLuu);
        footer.Resize += (_, _) =>
        {
            _btnLuu.Location = new Point(footer.Width - _btnLuu.Width - 28, 13);
            _btnReset.Location = new Point(_btnLuu.Left - _btnReset.Width - 10, 13);
            _lblStatus.Width = Math.Max(100, _btnReset.Left - _lblStatus.Left - 10);
        };

        Controls.Add(footer);
    }

    private Panel MakeBadge(out Label label, string initialText, Color accent)
    {
        var panel = new Panel
        {
            Size = new Size(220, 32),
            BackColor = BadgeBg,
            Cursor = Cursors.Default
        };
        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using var path = RoundRect(rect, 16);
            using var fill = new SolidBrush(panel.BackColor);
            g.FillPath(fill, path);
            using var pen = new Pen(CardBorder, 1);
            g.DrawPath(pen, path);
            using var dot = new SolidBrush((Color)panel.Tag);
            g.FillEllipse(dot, 12, panel.Height / 2 - 5, 10, 10);
        };
        panel.Tag = accent;

        label = new Label
        {
            Text = initialText,
            AutoSize = false,
            Location = new Point(30, 0),
            Size = new Size(186, 32),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9.25F, FontStyle.Bold),
            ForeColor = Ink,
            BackColor = Color.Transparent
        };
        panel.Controls.Add(label);
        return panel;
    }

    private void UpdateBadge(Panel panel, Label label, string text, Color accent)
    {
        if (panel is null) return;
        panel.Tag = accent;
        label.Text = text;
        panel.Invalidate();
    }

    private void BuildTabChung()
    {
        var scroll = WrapScrollable(_tabChung);
        var card = MakeCard();
        card.MinimumSize = new Size(700, 460);
        scroll.Controls.Add(card);

        var lbl = MakeSectionTitle("Thông tin chung");
        lbl.Location = new Point(24, 18);
        card.Controls.Add(lbl);

        var hint = new Label
        {
            Text = "Tên thuốc và đơn vị tính là bắt buộc. Nhóm thuốc bắt buộc để phân loại trên báo cáo.",
            AutoSize = true,
            Location = new Point(24, 46),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Muted
        };
        card.Controls.Add(hint);

        var grid = new TableLayoutPanel
        {
            Location = new Point(24, 80),
            Size = new Size(card.Width - 48, card.Height - 100),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            ColumnCount = 4,
            RowCount = 5,
            BackColor = Color.Transparent
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        for (var i = 0; i < 5; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));

        _txtTenThuoc = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _txtHoatChat = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _txtHamLuong = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _cboDonViTinh = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F)
        };
        _cboDonViTinh.Items.AddRange(["Viên", "Vỉ", "Hộp", "Gói", "Chai", "Lọ", "Ống", "Tuýp", "Viên sủi", "Kg", "Gam", "ml"]);
        _cboNhomThuoc = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F)
        };
        _txtHangSanXuat = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _txtNuocSanXuat = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _txtDongGoi = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };
        _txtSoDangKy = new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F) };

        _txtTenThuoc.TextChanged += (_, _) => CapNhatBadges();

        AddCellField(grid, 0, 0, columnSpan: 2, "Tên thuốc *", _txtTenThuoc);
        AddCellField(grid, 2, 0, columnSpan: 2, "Hoạt chất", _txtHoatChat);

        AddCellField(grid, 0, 1, columnSpan: 1, "Hàm lượng", _txtHamLuong);
        AddCellField(grid, 1, 1, columnSpan: 1, "Đơn vị tính *", _cboDonViTinh);
        AddCellField(grid, 2, 1, columnSpan: 2, "Nhóm thuốc *", _cboNhomThuoc);

        AddCellField(grid, 0, 2, columnSpan: 2, "Hãng sản xuất", _txtHangSanXuat);
        AddCellField(grid, 2, 2, columnSpan: 2, "Nước sản xuất", _txtNuocSanXuat);

        AddCellField(grid, 0, 3, columnSpan: 2, "Quy cách đóng gói", _txtDongGoi);
        AddCellField(grid, 2, 3, columnSpan: 2, "Số đăng ký", _txtSoDangKy);

        card.Controls.Add(grid);
    }

    private void BuildTabThuocTinh()
    {
        var scroll = WrapScrollable(_tabThuocTinh);
        var card = MakeCard();
        card.MinimumSize = new Size(700, 420);
        scroll.Controls.Add(card);

        var lbl = MakeSectionTitle("Giá & thuộc tính kho");
        lbl.Location = new Point(24, 18);
        card.Controls.Add(lbl);

        var hint = new Label
        {
            Text = "Giá bán nên lớn hơn hoặc bằng giá nhập. Tồn hiện tại sẽ được cộng lại qua nghiệp vụ Nhập kho.",
            AutoSize = true,
            Location = new Point(24, 46),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Muted
        };
        card.Controls.Add(hint);

        var grid = new TableLayoutPanel
        {
            Location = new Point(24, 80),
            Size = new Size(card.Width - 48, 300),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        for (var i = 0; i < 3; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));

        _numGiaNhap = NewNumericField(0, 9_999_999_999m, 0, decimals: 0);
        _numGiaBan = NewNumericField(0, 9_999_999_999m, 0, decimals: 0);
        _numTonToiThieu = NewNumericField(0, 1_000_000, 0, decimals: 0);
        _numSoLuongTon = NewNumericField(0, 1_000_000, 0, decimals: 0);

        AddCellField(grid, 0, 0, columnSpan: 1, "Giá nhập (VNĐ)", _numGiaNhap);
        AddCellField(grid, 1, 0, columnSpan: 1, "Giá bán (VNĐ)", _numGiaBan);
        AddCellField(grid, 0, 1, columnSpan: 1, "Tồn tối thiểu", _numTonToiThieu);
        AddCellField(grid, 1, 1, columnSpan: 1, "Tồn hiện tại (chỉ khi khởi tạo)", _numSoLuongTon);

        var leftBox = BuildHsdBox();
        var rightBox = BuildTrangThaiBox();

        grid.Controls.Add(leftBox, 0, 2);
        grid.Controls.Add(rightBox, 1, 2);

        card.Controls.Add(grid);

        _numGiaNhap.ValueChanged += (_, _) => CapNhatBadges();
        _numGiaBan.ValueChanged += (_, _) => CapNhatBadges();
    }

    private Panel BuildHsdBox()
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
            BackColor = BgSoft,
            Padding = new Padding(14, 10, 14, 10)
        };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1));
        };
        _chkHanSuDung = new CheckBox
        {
            Text = "Có hạn sử dụng (theo lô đầu tiên)",
            AutoSize = true,
            Location = new Point(8, 6),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Ink
        };
        _dtpHanSuDung = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Location = new Point(8, 34),
            Width = 240,
            MinDate = DateTime.Today,
            Value = DateTime.Today.AddYears(2),
            Enabled = false,
            Font = new Font("Segoe UI", 10F)
        };
        _chkHanSuDung.CheckedChanged += (_, _) =>
        {
            _dtpHanSuDung.Enabled = _chkHanSuDung.Checked;
            CapNhatBadges();
        };
        _dtpHanSuDung.ValueChanged += (_, _) => CapNhatBadges();
        p.Controls.Add(_chkHanSuDung);
        p.Controls.Add(_dtpHanSuDung);
        return p;
    }

    private Panel BuildTrangThaiBox()
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 4, 0, 4),
            BackColor = BgSoft,
            Padding = new Padding(14, 10, 14, 10)
        };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1));
        };
        _chkTrangThai = new CheckBox
        {
            Text = "Đang kinh doanh",
            AutoSize = true,
            Location = new Point(8, 6),
            Checked = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Ink
        };
        _lblLoiNhuanHint = new Label
        {
            Text = "Biên lợi nhuận: —",
            Location = new Point(8, 36),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Primary
        };
        p.Controls.Add(_chkTrangThai);
        p.Controls.Add(_lblLoiNhuanHint);
        return p;
    }

    private void BuildTabDonVi()
    {
        var card = MakeCard();
        card.Dock = DockStyle.Fill;
        _tabDonVi.Controls.Add(card);

        var lbl = MakeSectionTitle("Tra cứu Danh mục Dược Quốc Gia (DQG)");
        lbl.Location = new Point(24, 18);
        card.Controls.Add(lbl);

        var hint = new Label
        {
            Text = "Chọn 1 dòng để gắn DQG vào thuốc — hệ thống sẽ tự điền hoạt chất, hàm lượng, đóng gói, hãng SX.",
            AutoSize = true,
            Location = new Point(24, 46),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Muted
        };
        card.Controls.Add(hint);

        _txtTimDQG = new TextBox
        {
            Location = new Point(24, 80),
            Width = 460,
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Tìm theo tên / mã DQG / số đăng ký / hoạt chất"
        };
        _btnTimDQG = MakePrimaryButton("Tìm DQG", (_, _) => TaiDanhMucDQG(_txtTimDQG.Text));
        _btnTimDQG.Size = new Size(120, 32);
        _btnTimDQG.Location = new Point(492, 78);
        _btnXoaLienKetDQG = MakeOutlineButton("Bỏ liên kết", (_, _) =>
        {
            _maDQGDaChon = null;
            _lblDqgDaChon.Text = "Chưa gắn với DQG.";
            _lblDqgDaChon.ForeColor = Muted;
            CapNhatBadges();
            SetStatus("Đã bỏ liên kết với DQG.", Muted);
        });
        _btnXoaLienKetDQG.Size = new Size(120, 32);
        _btnXoaLienKetDQG.Location = new Point(620, 78);
        _txtTimDQG.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                TaiDanhMucDQG(_txtTimDQG.Text);
                e.SuppressKeyPress = true;
            }
        };

        var gridHost = new Panel
        {
            Location = new Point(24, 124),
            Size = new Size(card.Width - 56, card.Height - 200),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White,
            Padding = new Padding(0)
        };
        gridHost.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, gridHost.Width - 1, gridHost.Height - 1));
        };

        _gridDQG = new DataGridView
        {
            Dock = DockStyle.Fill,
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
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(250, 252, 250) },
            Font = new Font("Segoe UI", 9.5F),
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MintBg,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
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
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaDQGDonVi", HeaderText = "Mã DQG", FillWeight = 90 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenHangHoa", HeaderText = "Tên hàng hóa", FillWeight = 180 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoatChatChinh", HeaderText = "Hoạt chất", FillWeight = 150 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "HamLuong", HeaderText = "Hàm lượng", FillWeight = 80 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "DongGoi", HeaderText = "Đóng gói", FillWeight = 130 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "HangSanXuat", HeaderText = "Hãng SX", FillWeight = 130 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "NuocSanXuat", HeaderText = "Nước SX", FillWeight = 80 });
        _gridDQG.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThaiNhapKho", HeaderText = "Trạng thái", FillWeight = 100 });
        _gridDQG.CellDoubleClick += GridDQG_CellDoubleClick;
        gridHost.Controls.Add(_gridDQG);

        _lblDqgDaChon = new Label
        {
            Text = "Chưa gắn với DQG.",
            AutoSize = true,
            Location = new Point(24, card.Height - 58),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Muted
        };
        var btnApDung = MakePrimaryButton("Áp dụng dòng đang chọn  →", (_, _) => ApDungDongDQGDangChon());
        btnApDung.Size = new Size(240, 36);
        btnApDung.Location = new Point(card.Width - 268, card.Height - 64);
        btnApDung.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        card.Controls.Add(_txtTimDQG);
        card.Controls.Add(_btnTimDQG);
        card.Controls.Add(_btnXoaLienKetDQG);
        card.Controls.Add(gridHost);
        card.Controls.Add(_lblDqgDaChon);
        card.Controls.Add(btnApDung);
    }

    private static Panel WrapScrollable(TabPage tab)
    {
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = BgSoft
        };
        tab.Controls.Add(scroll);
        return scroll;
    }

    private static NumericUpDown NewNumericField(decimal min, decimal max, decimal value, int decimals = 0) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        DecimalPlaces = decimals,
        ThousandsSeparator = true,
        Font = new Font("Segoe UI", 10F),
        BorderStyle = BorderStyle.FixedSingle
    };

    private static void AddCellField(TableLayoutPanel host, int col, int row, int columnSpan, string caption, Control editor)
    {
        var holder = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0, 0, 12, 8) };
        var cap = new Label
        {
            Text = caption,
            AutoSize = true,
            Location = new Point(0, 4),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Muted
        };
        editor.Dock = DockStyle.Top;
        editor.Margin = new Padding(0, 4, 0, 0);

        var slot = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
        editor.Top = 4;
        editor.Left = 0;
        editor.Width = 100;
        editor.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        slot.SizeChanged += (_, _) => { editor.Width = slot.Width; };
        slot.Controls.Add(editor);

        holder.Controls.Add(slot);
        holder.Controls.Add(cap);
        slot.Top = 24;
        slot.Left = 0;
        slot.Width = 100;
        slot.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        holder.SizeChanged += (_, _) => { slot.Width = holder.Width - 12; };

        host.Controls.Add(holder, col, row);
        if (columnSpan > 1)
            host.SetColumnSpan(holder, columnSpan);
    }

    private Panel MakeCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(8)
        };
        card.Paint += (_, e) => DrawCardBorder(e.Graphics, card.ClientRectangle);
        return card;
    }

    private static Label MakeSectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI", 13F, FontStyle.Bold),
        ForeColor = Ink
    };

    private static Button MakePrimaryButton(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(150, 34)
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
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(140, 34)
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        b.FlatAppearance.MouseOverBackColor = MintBg;
        b.Click += onClick;
        return b;
    }

    private static void DrawCardBorder(Graphics g, Rectangle r)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var inner = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
        using var path = RoundRect(inner, 10);
        using var fill = new SolidBrush(Color.White);
        g.FillPath(fill, path);
        using var pen = new Pen(CardBorder, 1);
        g.DrawPath(pen, path);
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

    private void TaiDanhMucNhomThuoc()
    {
        try
        {
            _danhSachNhom = _medicineService.LayNhomThuoc();
            _cboNhomThuoc.Items.Clear();
            foreach (var n in _danhSachNhom)
                _cboNhomThuoc.Items.Add(new ComboItem(n.MaNhomThuoc, n.TenNhomThuoc));
            if (_cboNhomThuoc.Items.Count > 0)
                _cboNhomThuoc.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            SetStatus("Không tải được nhóm thuốc: " + ex.Message, DangerRed);
        }
    }

    private void TaiDanhMucDQG(string tuKhoa)
    {
        try
        {
            _ketQuaDQG = _medicineService.TraCuuDQG(tuKhoa);
            _gridDQG.Rows.Clear();
            foreach (var d in _ketQuaDQG)
            {
                _gridDQG.Rows.Add(
                    d.MaDQGDonVi ?? string.Empty,
                    d.TenHangHoa,
                    d.HoatChatChinh ?? string.Empty,
                    d.HamLuong ?? string.Empty,
                    d.DongGoi ?? string.Empty,
                    d.HangSanXuat ?? string.Empty,
                    d.NuocSanXuat ?? string.Empty,
                    d.TrangThaiNhapKho);
            }

            SetStatus($"Tìm thấy {_ketQuaDQG.Count} bản ghi DQG.", Muted);
        }
        catch (Exception ex)
        {
            SetStatus("Lỗi tra cứu DQG: " + ex.Message, DangerRed);
        }
    }

    private void GridDQG_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;
        ApDungDongDQGDangChon();
    }

    private void ApDungDongDQGDangChon()
    {
        if (_gridDQG.CurrentRow is null || _gridDQG.CurrentRow.Index < 0
            || _gridDQG.CurrentRow.Index >= _ketQuaDQG.Count)
        {
            SetStatus("Chưa chọn dòng DQG để áp dụng.", WarnOrange);
            return;
        }

        var dqg = _ketQuaDQG[_gridDQG.CurrentRow.Index];
        _maDQGDaChon = dqg.MaDQG;

        if (string.IsNullOrWhiteSpace(_txtTenThuoc.Text))
            _txtTenThuoc.Text = dqg.TenHangHoa;
        if (string.IsNullOrWhiteSpace(_txtHoatChat.Text))
            _txtHoatChat.Text = dqg.HoatChatChinh ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_txtHamLuong.Text))
            _txtHamLuong.Text = dqg.HamLuong ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_txtDongGoi.Text))
            _txtDongGoi.Text = dqg.DongGoi ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_txtHangSanXuat.Text))
            _txtHangSanXuat.Text = dqg.HangSanXuat ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_txtNuocSanXuat.Text))
            _txtNuocSanXuat.Text = dqg.NuocSanXuat ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_txtSoDangKy.Text))
            _txtSoDangKy.Text = dqg.SoDangKy ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(dqg.DonViTinh)
            && (_cboDonViTinh.SelectedItem is null || string.IsNullOrWhiteSpace(_cboDonViTinh.Text)))
            _cboDonViTinh.Text = dqg.DonViTinh;

        _lblDqgDaChon.Text = $"Đã gắn DQG: {dqg.MaDQGDonVi}  ·  {dqg.TenHangHoa}";
        _lblDqgDaChon.ForeColor = Primary;
        CapNhatBadges();
        SetStatus("Đã áp dụng dòng DQG vào form.", Primary);
    }

    private void CapNhatBadges()
    {
        if (_maDQGDaChon.HasValue)
            UpdateBadge(_badgeDQG, _lblBadgeDQG, "DQG: đã gắn", Primary);
        else
            UpdateBadge(_badgeDQG, _lblBadgeDQG, "DQG: chưa gắn", Muted);

        if (_chkHanSuDung is { Checked: true })
        {
            var dt = _dtpHanSuDung.Value.Date;
            var days = (int)(dt - DateTime.Today).TotalDays;
            var color = days switch
            {
                < 0 => DangerRed,
                <= 90 => WarnOrange,
                _ => Primary
            };
            UpdateBadge(_badgeHSD, _lblBadgeHSD, $"HSD: {dt:dd/MM/yyyy} ({days} ngày)", color);
        }
        else
        {
            UpdateBadge(_badgeHSD, _lblBadgeHSD, "HSD: chưa đặt", Muted);
        }

        CapNhatHintLoiNhuan();
    }

    private void CapNhatHintLoiNhuan()
    {
        if (_numGiaNhap is null || _numGiaBan is null) return;
        var nhap = _numGiaNhap.Value;
        var ban = _numGiaBan.Value;
        if (nhap <= 0)
        {
            _lblLoiNhuanHint.Text = "Biên lợi nhuận: —";
            _lblLoiNhuanHint.ForeColor = Muted;
            UpdateBadge(_badgeLoiNhuan, _lblBadgeLoiNhuan, "Biên LN: —", Muted);
            return;
        }
        var bien = (ban - nhap) / nhap * 100m;
        var diff = (ban - nhap).ToString("C0", Vi);
        _lblLoiNhuanHint.Text = $"Biên lợi nhuận: {bien:F1}% (chênh lệch {diff})";
        var color = bien switch
        {
            < 0 => DangerRed,
            < 5 => WarnOrange,
            _ => Primary
        };
        _lblLoiNhuanHint.ForeColor = color;
        UpdateBadge(_badgeLoiNhuan, _lblBadgeLoiNhuan, $"Biên LN: {bien:F1}%", color);
    }

    private void BtnReset_Click(object sender, EventArgs e) => ResetForm();

    private void ResetForm()
    {
        _txtTenThuoc.Text = string.Empty;
        _txtHoatChat.Text = string.Empty;
        _txtHamLuong.Text = string.Empty;
        _cboDonViTinh.SelectedIndex = -1;
        _cboDonViTinh.Text = string.Empty;
        if (_cboNhomThuoc.Items.Count > 0)
            _cboNhomThuoc.SelectedIndex = 0;
        _txtHangSanXuat.Text = string.Empty;
        _txtNuocSanXuat.Text = string.Empty;
        _txtDongGoi.Text = string.Empty;
        _txtSoDangKy.Text = string.Empty;

        _numGiaNhap.Value = 0;
        _numGiaBan.Value = 0;
        _numTonToiThieu.Value = 0;
        _numSoLuongTon.Value = 0;
        _chkHanSuDung.Checked = false;
        _dtpHanSuDung.Value = DateTime.Today.AddYears(2);
        _chkTrangThai.Checked = true;

        _maDQGDaChon = null;
        _lblDqgDaChon.Text = "Chưa gắn với DQG.";
        _lblDqgDaChon.ForeColor = Muted;

        CapNhatBadges();

        _tabs.SelectedIndex = 0;
        _txtTenThuoc.Focus();
        SetStatus("Đã làm mới biểu mẫu.", Muted);
    }

    private void BtnLuu_Click(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            SetStatus("Phiên đăng nhập đã hết hạn.", DangerRed);
            return;
        }

        if (!TryBuildDtoTuForm(out var dto, out var loi))
        {
            SetStatus(loi, DangerRed);
            return;
        }

        try
        {
            _btnLuu.Enabled = false;
            var ma = _medicineService.ThemThuoc(dto);
            SetStatus($"Đã thêm thuốc thành công (Mã thuốc #{ma}).", Primary);
            MessageBox.Show(this,
                $"Đã thêm thuốc \"{dto.TenThuoc}\" thành công với mã thuốc #{ma}.\n\n" +
                "Bạn có thể tiếp tục thêm thuốc khác hoặc đóng cửa sổ.",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetForm();
        }
        catch (UnauthorizedAccessException ex)
        {
            SetStatus(ex.Message, DangerRed);
        }
        catch (ArgumentException ex)
        {
            SetStatus(ex.Message, WarnOrange);
        }
        catch (Exception ex)
        {
            SetStatus("Lỗi lưu thuốc: " + ex.Message, DangerRed);
        }
        finally
        {
            _btnLuu.Enabled = true;
        }
    }

    private bool TryBuildDtoTuForm(out ThuocDTO dto, out string loi)
    {
        dto = new ThuocDTO();
        loi = string.Empty;

        var ten = (_txtTenThuoc.Text ?? string.Empty).Trim();
        if (ten.Length == 0)
        {
            _tabs.SelectedIndex = 0;
            _txtTenThuoc.Focus();
            loi = "Vui lòng nhập tên thuốc.";
            return false;
        }

        var dvt = (_cboDonViTinh.Text ?? string.Empty).Trim();
        if (dvt.Length == 0)
        {
            _tabs.SelectedIndex = 0;
            _cboDonViTinh.Focus();
            loi = "Vui lòng chọn đơn vị tính.";
            return false;
        }

        if (_cboNhomThuoc.SelectedItem is not ComboItem nhom)
        {
            _tabs.SelectedIndex = 0;
            _cboNhomThuoc.Focus();
            loi = "Vui lòng chọn nhóm thuốc.";
            return false;
        }

        if (_numGiaNhap.Value <= 0)
        {
            _tabs.SelectedIndex = 1;
            _numGiaNhap.Focus();
            loi = "Vui lòng nhập giá nhập (phải lớn hơn 0).";
            return false;
        }

        if (_numGiaBan.Value <= 0)
        {
            _tabs.SelectedIndex = 1;
            _numGiaBan.Focus();
            loi = "Vui lòng nhập giá bán (phải lớn hơn 0).";
            return false;
        }

        if (_numGiaBan.Value < _numGiaNhap.Value)
        {
            _tabs.SelectedIndex = 1;
            _numGiaBan.Focus();
            loi = "Giá bán không được nhỏ hơn giá nhập.";
            return false;
        }

        dto.TenThuoc = ten;
        dto.HoatChat = NullIfBlank(_txtHoatChat.Text);
        dto.HamLuong = NullIfBlank(_txtHamLuong.Text);
        dto.DonViTinh = dvt;
        dto.MaNhomThuoc = nhom.Value;
        dto.HangSanXuat = NullIfBlank(_txtHangSanXuat.Text);
        dto.NuocSanXuat = NullIfBlank(_txtNuocSanXuat.Text);
        dto.DongGoi = NullIfBlank(_txtDongGoi.Text);
        dto.SoDangKy = NullIfBlank(_txtSoDangKy.Text);
        dto.MaDQG = _maDQGDaChon;
        dto.GiaNhap = _numGiaNhap.Value;
        dto.GiaBan = _numGiaBan.Value;
        dto.TonToiThieu = (int)_numTonToiThieu.Value;
        dto.SoLuongTon = (int)_numSoLuongTon.Value;
        dto.HanSuDung = _chkHanSuDung.Checked ? _dtpHanSuDung.Value.Date : null;
        dto.TrangThai = _chkTrangThai.Checked;
        return true;
    }

    private static string NullIfBlank(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private void SetStatus(string text, Color color)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = color;
    }

    private void ToggleEditing(bool enabled)
    {
        _btnLuu.Enabled = enabled;
        _btnReset.Enabled = enabled;
        foreach (TabPage tp in _tabs.TabPages)
            tp.Enabled = enabled;
    }

    private sealed record ComboItem(int Value, string Display)
    {
        public override string ToString() => Display;
    }
}
