#nullable disable
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Main;

public partial class FrmDashboard : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    private static readonly Color SidebarBg = Color.FromArgb(250, 252, 250);
    private static readonly Color Muted = Color.FromArgb(97, 97, 97);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);

    /// <summary>
    /// Đệm phía trên vùng client để logo/menu trái và header+nội dung phải không sát mép trên (title bar / DPI).
    /// </summary>
    private const int ShellTopInsetPx = 40;

    private readonly AuthService _auth = new(new DbContextDAL());
    private readonly ReportService _reportService = new(new DbContextDAL());
    private readonly ToolTip _dashTooltip = new() { ShowAlways = true };

    private DashboardHienThiDTO _dashData;
    private int _hoverBarIndex = -1;
    private int _hoverPieIndex = -1;
    private RectangleF[] _barHitRects = Array.Empty<RectangleF>();
    private GraphicsPath[] _pieHitPaths;

    private TableLayoutPanel _root;
    private Panel _sidebar;
    private Panel _sidebarHeader;
    private Label _lblBrand;
    private Label _lblRoleTop;
    private FlowLayoutPanel _navFlow;
    private Panel _sidebarFooter;
    private Panel _panelAvatar;
    private Label _lblAvatarName;
    private Label _lblAvatarLogin;
    private Panel _workspace;
    private Panel _header;
    private TextBox _txtSearch;
    private Button _btnBell;
    private Panel _scrollHost;
    private FlowLayoutPanel _dashboardFlow;
    private Label _lblWelcomeTitle;
    private Label _lblWelcomeSub;
    private TableLayoutPanel _quickActions;
    private TableLayoutPanel _kpiRow;
    private Panel _panelBarChart;
    private Panel _panelPieChart;
    private Panel _alertsPanel;
    private DataGridView _gridInvoices;
    private Panel _gridInvoicesHost;
    private TableLayoutPanel _titleBlock;
    private TableLayoutPanel _midRowPanel;
    private string _tooltipBarText = "";
    private string _tooltipPieText = "";
    private readonly Font _tooltipBodyFont = new("Segoe UI", 9.75f);
    private Label _lblKpiDtVal;
    private Label _lblKpiDtBadge;
    private Label _lblKpiHdVal;
    private Label _lblKpiHdBadge;
    private Label _lblKpiHetVal;
    private Label _lblKpiHetBadge;
    private Label _lblKpiSapVal;
    private Label _lblKpiSapBadge;

    private string _avatarInitials = "";
    private string _roleBadge = "";
    private Color _avatarColor = Primary;

    private readonly List<NavItem> _navDefs =
    [
        new("dash", "Dashboard", []),
        new("pos", "Bán hàng (POS)", [VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi]),
        new("thuoc", "Thuốc", [VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.DuocSi, VaiTroTen.NhanVienKho]),
        new("nhap", "Nhập kho", [VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho]),
        new("ton", "Tồn kho / Kiểm kê", []),
        new("canh", "Cảnh báo hết hạn", []),
        new("bc", "Báo cáo doanh thu", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
        new("nv", "Nhân viên", [VaiTroTen.Admin]),
        new("audit", "Audit log", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
    ];

    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.Ordinal);
    private string _activeNav = "dash";

    public bool ReLoginRequested { get; private set; }

    public FrmDashboard()
    {
        InitializeComponent();
        Text = "Pharmacy Management ALN — Dashboard";
        BuildLayout();
        WireEvents();
        SetDoubleBufferedRecursive(this);
    }

    private void WireEvents()
    {
        Load += FrmDashboard_Load;
        _dashTooltip.OwnerDraw = true;
        _dashTooltip.Draw += DashTooltip_OnDraw;
        _dashTooltip.Popup += DashTooltip_OnPopup;
        Shown += (_, _) => ApplySession();
        FormClosed += (_, _) =>
        {
            foreach (var p in _pieHitPaths ?? Array.Empty<GraphicsPath>())
                p.Dispose();
            _pieHitPaths = null;
            _dashTooltip.Dispose();
            _tooltipBodyFont.Dispose();
        };
    }

    private void FrmDashboard_Load(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            MessageBox.Show(this, "Phiên đăng nhập không hợp lệ.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ReLoginRequested = true;
            Close();
        }
    }

    private void ApplySession()
    {
        if (!UserSession.IsAuthenticated)
            return;

        var hoTen = UserSession.HoTen ?? "";
        var login = UserSession.TenDangNhap ?? "";
        var role = UserSession.TenVaiTro;

        _avatarInitials = UserDisplayHelper.GetAvatarInitials(hoTen, login);
        _roleBadge = UserDisplayHelper.GetRoleBadgeLetters(role);
        _avatarColor = UserDisplayHelper.GetAvatarBackColor(role);

        _lblRoleTop.Text = UserDisplayHelper.GetVaiTroDisplayName(role);
        _lblAvatarName.Text = hoTen;
        _lblAvatarLogin.Text = "@" + login;
        _lblWelcomeSub.Text = $"Chào mừng trở lại, {hoTen}. Đang tải dữ liệu tổng quan…";

        _panelAvatar.Invalidate();
        ApplyNavVisibility();
        HighlightNav("dash");

        TaiLaiDashboardTuCoSoDuLieu();
    }

    private void TaiLaiDashboardTuCoSoDuLieu()
    {
        try
        {
            _dashData = _reportService.LayDashboardHienThi();
        }
        catch (Exception ex)
        {
            _dashData = null;
            _lblWelcomeSub.Text =
                $"Không tải được dữ liệu dashboard: {ex.Message}";
            MessageBox.Show(this,
                "Không tải được dữ liệu dashboard. Kiểm tra kết nối SQL và quyền tài khoản.\n\n" + ex.Message,
                Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        CapNhatGiaoDienDashboard();
    }

    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private void CapNhatGiaoDienDashboard()
    {
        var d = _dashData;
        var hoTen = UserSession.HoTen ?? "";

        if (d is null)
        {
            _lblKpiDtVal.Text = _lblKpiHdVal.Text = "—";
            _lblKpiDtBadge.Text = _lblKpiHdBadge.Text = "—";
            _lblKpiHetVal.Text = _lblKpiSapVal.Text = "—";
            _lblKpiHetBadge.Text = _lblKpiSapBadge.Text = "—";
            RebuildAlertLabels([]);
            _gridInvoices.Rows.Clear();
            _panelBarChart?.Invalidate();
            _panelPieChart?.Invalidate();
            return;
        }

        _lblWelcomeSub.Text = d.CoTaiChinh
            ? $"Chào mừng trở lại, {hoTen}. Doanh thu và hóa đơn theo tuần bắt đầu {d.TuanBatDau:dd/MM/yyyy} (dữ liệu CSDL)."
            : $"Chào mừng trở lại, {hoTen}. Chỉ số tồn kho theo CSDL; doanh thu / hóa đơn ẩn với vai trò kho.";

        if (d.CoTaiChinh)
        {
            _lblKpiDtVal.Text = FormatVnd(d.DoanhThuTuanNay);
            _lblKpiDtBadge.Text = d.ChenhLechDoanhThuTuanTruoc ?? "—";
            _lblKpiDtBadge.ForeColor = Primary;
            _lblKpiHdVal.Text = d.SoHoaDonHoanThanhTuan.ToString("N0", Vi);
            _lblKpiHdBadge.Text = "Hoàn thành · tuần này";
            _lblKpiHdBadge.ForeColor = Muted;
        }
        else
        {
            _lblKpiDtVal.Text = "—";
            _lblKpiDtBadge.Text = "Không hiển thị";
            _lblKpiDtBadge.ForeColor = Muted;
            _lblKpiHdVal.Text = "—";
            _lblKpiHdBadge.Text = "Không hiển thị";
            _lblKpiHdBadge.ForeColor = Muted;
        }

        _lblKpiHetVal.Text = d.SoThuocHetHang.ToString("N0", Vi);
        _lblKpiHetBadge.Text = d.SoThuocHetHang > 0 ? "Cảnh báo" : "Ổn định";
        _lblKpiHetBadge.ForeColor = d.SoThuocHetHang > 0 ? Color.FromArgb(251, 140, 0) : Muted;

        _lblKpiSapVal.Text = d.SoThuocSapHetHan.ToString("N0", Vi);
        _lblKpiSapBadge.Text = d.SoThuocSapHetHan > 0 ? "Khẩn cấp" : "Ổn định";
        _lblKpiSapBadge.ForeColor = d.SoThuocSapHetHan > 0 ? Color.FromArgb(211, 47, 47) : Muted;

        RebuildAlertLabels(d.CanhBao);

        _gridInvoices.Rows.Clear();
        foreach (var hd in d.HoaDonGanDay)
        {
            _gridInvoices.Rows.Add(
                "#HD-" + hd.MaHoaDon.ToString("D5", Vi),
                hd.TenKhachHang,
                hd.NgayLap.ToString("dd/MM/yyyy HH:mm", Vi),
                FormatVnd(hd.ThanhTien),
                hd.TrangThai);
        }

        StyleInvoiceStatusCells();
        _hoverBarIndex = _hoverPieIndex = -1;
        _dashTooltip.SetToolTip(_panelBarChart, null);
        _dashTooltip.SetToolTip(_panelPieChart, null);
        _panelBarChart?.Invalidate();
        _panelPieChart?.Invalidate();

        if (_scrollHost is { IsHandleCreated: true } && _dashboardFlow is not null)
        {
            var cw = Math.Max(600, _scrollHost.ClientSize.Width - _dashboardFlow.Padding.Horizontal);
            ApplyWelcomeWrapWidths(cw);
        }
    }

    private static string FormatVnd(decimal amount) =>
        amount.ToString("C0", Vi);

    private void RebuildAlertLabels(IReadOnlyList<DashboardCanhBaoItemDTO> items)
    {
        _alertsPanel.Controls.Clear();

        var y = 52;
        foreach (var it in items)
        {
            var dotColor = it.MoTa.Contains("hạn", StringComparison.OrdinalIgnoreCase)
                ? Color.FromArgb(211, 47, 47)
                : Color.FromArgb(251, 140, 0);
            var dotP = new Panel { Location = new Point(20, y + 6), Size = new Size(8, 8), BackColor = dotColor };
            var lblN = new Label
            {
                Text = it.TenThuoc,
                Location = new Point(36, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            var lblS = new Label
            {
                Text = it.MoTa,
                Location = new Point(36, y + 22),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5F)
            };
            _alertsPanel.Controls.Add(dotP);
            _alertsPanel.Controls.Add(lblN);
            _alertsPanel.Controls.Add(lblS);
            y += 56;
        }

        if (items.Count == 0)
        {
            var lbl = new Label
            {
                Text = "Không có cảnh báo tồn kho / hạn.",
                Location = new Point(20, 52),
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5F)
            };
            _alertsPanel.Controls.Add(lbl);
        }
    }

    private void ApplyNavVisibility()
    {
        var role = UserSession.TenVaiTro;
        foreach (var def in _navDefs)
        {
            if (!_navButtons.TryGetValue(def.Key, out var btn))
                continue;
            btn.Visible = def.AllowedRoles.Length == 0
                || (role is not null && def.AllowedRoles.Contains(role, StringComparer.Ordinal));
        }
    }

    private void HighlightNav(string key)
    {
        _activeNav = key;
        foreach (var kv in _navButtons)
        {
            var on = kv.Key == key;
            kv.Value.BackColor = on ? MintBg : Color.Transparent;
            kv.Value.ForeColor = on ? PrimaryDark : Ink;
        }
    }

    private void BuildLayout()
    {
        Padding = new Padding(0, ShellTopInsetPx, 0, 0);

        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor
        };
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 264));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(_root);

        _sidebar = new Panel { Dock = DockStyle.Fill, BackColor = SidebarBg };
        _sidebar.Paint += Sidebar_OnPaint;
        _root.Controls.Add(_sidebar, 0, 0);

        _sidebarHeader = new Panel { Height = 96, Dock = DockStyle.Top, BackColor = SidebarBg };
        _lblBrand = new Label
        {
            AutoSize = false,
            Bounds = new Rectangle(20, 18, 220, 28),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = PrimaryDark,
            Text = "Pharmacy ALN",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblRoleTop = new Label
        {
            AutoSize = false,
            Bounds = new Rectangle(20, 48, 220, 22),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Muted,
            Text = "—",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _sidebarHeader.Controls.Add(_lblBrand);
        _sidebarHeader.Controls.Add(_lblRoleTop);

        _sidebarFooter = new Panel { Height = 108, Dock = DockStyle.Bottom, BackColor = SidebarBg };
        _panelAvatar = new Panel
        {
            Bounds = new Rectangle(16, 12, 52, 52),
            BackColor = Color.Transparent
        };
        _panelAvatar.Paint += PanelAvatar_OnPaint;
        _lblAvatarName = new Label
        {
            AutoSize = false,
            Bounds = new Rectangle(78, 14, 170, 22),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Ink,
            Text = "—",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblAvatarLogin = new Label
        {
            AutoSize = false,
            Bounds = new Rectangle(78, 38, 170, 20),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Muted,
            Text = "—",
            TextAlign = ContentAlignment.MiddleLeft
        };
        var btnLogout = new Button
        {
            Text = "Đăng xuất",
            Bounds = new Rectangle(16, 72, 110, 30),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9F),
            ForeColor = PrimaryDark,
            BackColor = Color.White
        };
        btnLogout.FlatAppearance.BorderColor = Color.FromArgb(200, 220, 202);
        btnLogout.Click += BtnLogout_Click;
        _sidebarFooter.Controls.Add(_panelAvatar);
        _sidebarFooter.Controls.Add(_lblAvatarName);
        _sidebarFooter.Controls.Add(_lblAvatarLogin);
        _sidebarFooter.Controls.Add(btnLogout);

        _navFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8, 12, 8, 8),
            BackColor = SidebarBg
        };
        _navFlow.Controls.Add(new Panel { Height = 4, Width = 240, Margin = new Padding(0) }); // spacer
        foreach (var def in _navDefs)
        {
            var b = CreateNavButton(def.Text, def.Key);
            _navFlow.Controls.Add(b);
            _navButtons[def.Key] = b;
        }

        // Thứ tự Dock WinForms: thêm Fill trước, rồi Bottom/Top — tránh vùng menu bị tính chồng lên header logo.
        _sidebar.Controls.Add(_navFlow);
        _sidebar.Controls.Add(_sidebarFooter);
        _sidebar.Controls.Add(_sidebarHeader);

        _workspace = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        _root.Controls.Add(_workspace, 1, 0);

        _header = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.White };
        _header.Paint += Header_OnPaint;
        var lblSys = new Label
        {
            Text = "Hệ thống nhà thuốc",
            AutoSize = true,
            Location = new Point(20, 18),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Ink
        };
        _txtSearch = new TextBox
        {
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Text = ""
        };
        _txtSearch.PlaceholderText = "Tìm kiếm thuốc, hóa đơn...";
        _txtSearch.Location = new Point(320, 16);
        _txtSearch.Width = 420;
        _txtSearch.Height = 32;
        _btnBell = new Button
        {
            Text = "🔔",
            Width = 40,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Location = new Point(_workspace.Width - 72, 14),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Font = new Font("Segoe UI", 12F)
        };
        _btnBell.FlatAppearance.BorderSize = 0;
        _btnBell.Click += (_, _) => MessageBox.Show(this, "Chưa có thông báo mới.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        _header.Controls.Add(lblSys);
        _header.Controls.Add(_txtSearch);
        _header.Controls.Add(_btnBell);
        _header.Resize += (_, _) => { _btnBell.Left = _header.Width - 56; };

        _scrollHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(245, 247, 246), Padding = new Padding(0) };

        _dashboardFlow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = false,
            Padding = new Padding(24, 28, 24, 32),
            BackColor = Color.FromArgb(245, 247, 246),
            Location = Point.Empty,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _scrollHost.Controls.Add(_dashboardFlow);
        // Fill trước, Top sau — vùng cuộn không chiếm phía dưới thanh tìm kiếm (tránh nút/cards bị che).
        _workspace.Controls.Add(_scrollHost);
        _workspace.Controls.Add(_header);

        _scrollHost.Resize += (_, _) => SyncDashboardFlowLayout();
        _workspace.Resize += (_, _) => SyncDashboardFlowLayout();
        Shown += (_, _) => SyncDashboardFlowLayout();

        BuildDashboardContent();
        SyncDashboardFlowLayout();
    }

    /// <summary>
    /// Giữ vùng dashboard theo chiều ngang của vùng cuộn (full màn hình / maximize);
    /// tránh FlowLayoutPanel + AutoSize co nội dung về ~840px và hàng tiêu đề co sát nút bên trái.
    /// </summary>
    private void SyncDashboardFlowLayout()
    {
        if (_scrollHost is null || _dashboardFlow is null || !_scrollHost.IsHandleCreated)
            return;

        var inner = Math.Max(600, _scrollHost.ClientSize.Width);
        var contentW = inner - _dashboardFlow.Padding.Horizontal;

        _dashboardFlow.SuspendLayout();
        try
        {
            _dashboardFlow.Width = inner;
            foreach (Control c in _dashboardFlow.Controls)
                c.Width = contentW;
        }
        finally
        {
            _dashboardFlow.ResumeLayout(performLayout: true);
        }

        var bottom = _dashboardFlow.Padding.Top;
        foreach (Control c in _dashboardFlow.Controls)
            bottom = Math.Max(bottom, c.Bottom + c.Margin.Bottom);
        _dashboardFlow.Height = Math.Max(1, bottom + _dashboardFlow.Padding.Bottom);

        ApplyWelcomeWrapWidths(contentW);
        _midRowPanel?.PerformLayout();
        _kpiRow?.PerformLayout();
    }

    private void ApplyWelcomeWrapWidths(int contentWidth)
    {
        if (_lblWelcomeTitle is null || _lblWelcomeSub is null)
            return;
        var maxW = Math.Max(280, contentWidth - 40);
        _lblWelcomeTitle.MaximumSize = new Size(maxW, 0);
        _lblWelcomeSub.MaximumSize = new Size(maxW, 0);
    }

    private void AddDashboardRow(Control c) => AddDashboardRow(c, new Padding(0, 0, 0, 12));

    private void AddDashboardRow(Control c, Padding margin)
    {
        c.Margin = margin;
        c.Width = Math.Max(560, _scrollHost.ClientSize.Width > 0
            ? _scrollHost.ClientSize.Width - _dashboardFlow.Padding.Horizontal - 8
            : 840);
        _dashboardFlow.Controls.Add(c);
    }

    private void BuildDashboardContent()
    {
        _titleBlock = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Color.Transparent
        };
        _titleBlock.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _titleBlock.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblWelcomeTitle = new Label
        {
            Text = "Tổng quan nhà thuốc",
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Ink,
            Margin = new Padding(0, 0, 0, 6),
            UseMnemonic = false
        };
        _lblWelcomeSub = new Label
        {
            Text = "—",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Muted,
            UseMnemonic = false
        };
        var titleStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(0)
        };
        titleStack.Controls.Add(_lblWelcomeTitle);
        titleStack.Controls.Add(_lblWelcomeSub);
        _titleBlock.Controls.Add(titleStack, 0, 0);

        _quickActions = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 4, 0, 0)
        };
        for (var i = 0; i < 3; i++)
            _quickActions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _quickActions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _quickActions.Controls.Add(MkPrimaryBtn("+ Tạo hóa đơn mới", BtnQuickInvoice), 0, 0);
        _quickActions.Controls.Add(MkOutlineBtn("+ Thêm thuốc mới", BtnQuickMedicine), 1, 0);
        _quickActions.Controls.Add(MkOutlineBtn("Báo cáo nhanh", BtnQuickReport), 2, 0);

        var quickBar = new Panel { Height = 48, Dock = DockStyle.Fill, BackColor = Color.Transparent };
        quickBar.Controls.Add(_quickActions);
        quickBar.Resize += (_, _) =>
        {
            _quickActions.Left = Math.Max(0, quickBar.ClientSize.Width - _quickActions.Width);
            _quickActions.Top = (quickBar.ClientSize.Height - _quickActions.Height) / 2;
        };
        _titleBlock.Controls.Add(quickBar, 0, 1);

        AddDashboardRow(_titleBlock, new Padding(0, 0, 0, 16));

        _kpiRow = new TableLayoutPanel
        {
            ColumnCount = 4,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 128),
            Margin = new Padding(0, 0, 0, 20)
        };
        for (var i = 0; i < 4; i++)
            _kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _kpiRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var k0 = MkKpiCard("Doanh thu", "—", "—", Primary);
        _lblKpiDtVal = k0.lblValue;
        _lblKpiDtBadge = k0.lblBadge;
        _kpiRow.Controls.Add(k0.panel, 0, 0);
        var k1 = MkKpiCard("Hóa đơn", "—", "—", Muted, tag: "ok");
        _lblKpiHdVal = k1.lblValue;
        _lblKpiHdBadge = k1.lblBadge;
        _kpiRow.Controls.Add(k1.panel, 1, 0);
        var k2 = MkKpiCard("Hết hàng", "—", "—", Color.FromArgb(251, 140, 0));
        _lblKpiHetVal = k2.lblValue;
        _lblKpiHetBadge = k2.lblBadge;
        _kpiRow.Controls.Add(k2.panel, 2, 0);
        var k3 = MkKpiCard("Sắp hết hạn", "—", "—", Color.FromArgb(211, 47, 47));
        _lblKpiSapVal = k3.lblValue;
        _lblKpiSapBadge = k3.lblBadge;
        _kpiRow.Controls.Add(k3.panel, 3, 0);
        AddDashboardRow(_kpiRow, new Padding(0, 0, 0, 20));

        _midRowPanel = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = false,
            Height = 360,
            Margin = new Padding(0, 0, 0, 20)
        };
        _midRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));
        _midRowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));

        var leftCharts = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            BackColor = Color.FromArgb(245, 247, 246)
        };
        leftCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        leftCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));

        _panelBarChart = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 6),
            BackColor = Color.White
        };
        _panelBarChart.Paint += BarChart_OnPaint;
        _panelBarChart.MouseMove += BarChart_OnMouseMove;
        _panelBarChart.MouseLeave += (_, _) => ResetBarHover();

        _panelPieChart = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 0),
            BackColor = Color.White
        };
        _panelPieChart.Paint += PieChart_OnPaint;
        _panelPieChart.MouseMove += PieChart_OnMouseMove;
        _panelPieChart.MouseLeave += (_, _) => ResetPieHover();

        leftCharts.Controls.Add(_panelBarChart, 0, 0);
        leftCharts.Controls.Add(_panelPieChart, 0, 1);

        _alertsPanel = new Panel
        {
            Height = 280,
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 0, 0, 0),
            BackColor = Color.White
        };
        _alertsPanel.Paint += AlertsPanel_OnPaint;

        _midRowPanel.Controls.Add(leftCharts, 0, 0);
        _midRowPanel.Controls.Add(_alertsPanel, 1, 0);
        AddDashboardRow(_midRowPanel, new Padding(0, 0, 0, 20));

        var lblInv = new Label
        {
            Text = "Hóa đơn gần đây",
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Ink,
            Margin = new Padding(0, 0, 0, 8)
        };
        AddDashboardRow(lblInv, new Padding(0, 0, 0, 4));

        var invToolbar = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        invToolbar.Controls.Add(MkOutlineSmall("Lọc", (_, _) => MessageBox.Show(this, "Bộ lọc sẽ kết nối dữ liệu sau.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)));
        invToolbar.Controls.Add(MkOutlineSmall("Xuất file", (_, _) => MessageBox.Show(this, "Xuất Excel / PDF theo module báo cáo.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)));
        AddDashboardRow(invToolbar, new Padding(0, 0, 0, 8));

        _gridInvoicesHost = new Panel
        {
            BackColor = Color.FromArgb(218, 228, 220),
            Padding = new Padding(10, 10, 18, 10),
            Margin = new Padding(0, 0, 0, 4)
        };

        _gridInvoices = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(228, 234, 229),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 36 },
            Font = new Font("Segoe UI", 10F),
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MintBg,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(10, 8, 10, 8),
                WrapMode = DataGridViewTriState.False
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(200, 230, 201),
                SelectionForeColor = Ink,
                Padding = new Padding(10, 6, 10, 6),
                WrapMode = DataGridViewTriState.True
            }
        };
        _gridInvoices.CellFormatting += GridInvoices_CellFormatting;
        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn { Name = "cMa", HeaderText = "Mã HĐ", FillWeight = 88, MinimumWidth = 88 });
        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn { Name = "cKh", HeaderText = "Khách hàng", FillWeight = 160, MinimumWidth = 120 });
        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn { Name = "cTg", HeaderText = "Thời gian", FillWeight = 118, MinimumWidth = 108 });
        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn { Name = "cTt", HeaderText = "Tổng tiền", FillWeight = 92, MinimumWidth = 86 });
        _gridInvoices.Columns.Add(new DataGridViewTextBoxColumn { Name = "cTtai", HeaderText = "Trạng thái", FillWeight = 100, MinimumWidth = 96 });
        _gridInvoicesHost.Controls.Add(_gridInvoices);
        AddDashboardRow(_gridInvoicesHost, new Padding(0, 0, 0, 4));

        var lnkMore = new LinkLabel
        {
            Text = "Xem thêm hóa đơn",
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0),
            LinkColor = Primary
        };
        lnkMore.LinkClicked += (_, _) => MessageBox.Show(this, "Mở module hóa đơn khi đã triển khai.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        AddDashboardRow(lnkMore, new Padding(0, 4, 0, 0));

        _gridInvoicesHost.MinimumSize = new Size(0, 228);
        SetDoubleBufferedOne(_panelBarChart);
        SetDoubleBufferedOne(_panelPieChart);
        SetDoubleBufferedOne(_alertsPanel);
        SetDoubleBufferedOne(_gridInvoicesHost);
        SetDoubleBufferedOne(_gridInvoices);
    }

    private void StyleInvoiceStatusCells()
    {
        foreach (DataGridViewRow row in _gridInvoices.Rows)
        {
            if (row.Cells.Count < 5)
                continue;
            var st = row.Cells[4].Value?.ToString() ?? "";
            var isCancel = st.Contains("Hủy", StringComparison.OrdinalIgnoreCase)
                           || st.Contains("HUY", StringComparison.OrdinalIgnoreCase);
            row.Cells[4].Style.ForeColor = isCancel
                ? Color.FromArgb(211, 47, 47)
                : Primary;
            row.Cells[4].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    private static Button MkPrimaryBtn(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(16, 8, 16, 8),
            Margin = new Padding(0, 0, 8, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            ForeColor = Color.White,
            BackColor = Primary,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += onClick;
        return b;
    }

    private static Button MkOutlineBtn(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(14, 8, 14, 8),
            Margin = new Padding(0, 0, 8, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            ForeColor = PrimaryDark,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9.5F)
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        b.Click += onClick;
        return b;
    }

    private static Button MkOutlineSmall(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(0, 0, 8, 0),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            ForeColor = PrimaryDark,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        b.Click += onClick;
        return b;
    }

    private void BtnQuickInvoice(object s, EventArgs e) =>
        TryNavigateOrMessage("pos", "Mở màn bán hàng / POS khi module sẵn sàng.");

    private void BtnQuickMedicine(object s, EventArgs e) =>
        TryNavigateOrMessage("thuoc", "Mở màn quản lý thuốc khi module sẵn sàng.");

    private void BtnQuickReport(object s, EventArgs e) =>
        TryNavigateOrMessage("bc", "Mở báo cáo doanh thu khi module sẵn sàng.");

    private void TryNavigateOrMessage(string navKey, string message)
    {
        if (_navButtons.TryGetValue(navKey, out var btn) && btn.Visible)
        {
            HighlightNav(navKey);
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
            MessageBox.Show(this, "Tài khoản của bạn không có quyền truy cập chức năng này.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static (Panel panel, Label lblValue, Label lblBadge) MkKpiCard(
        string title, string value, string badge, Color badgeColor, string tag = "")
    {
        var p = new Panel
        {
            Margin = new Padding(6, 0, 6, 0),
            MinimumSize = new Size(100, 126),
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14, 12, 14, 12)
        };
        p.Paint += (_, e) => DrawCardBorder(e.Graphics, p.ClientRectangle);

        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = Color.Transparent
        };
        tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        // Không dùng Percent cho dòng số: khi badge xuống nhiều dòng, Percent ăn hết chiều cao và cắt chữ số.
        tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblT = new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9F),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 4)
        };
        var lblV = new Label
        {
            Text = value,
            AutoSize = true,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Ink,
            AutoEllipsis = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        var lblB = new Label
        {
            Text = badge,
            AutoSize = true,
            ForeColor = badgeColor,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(tag == "ok" ? 238 : 245, tag == "ok" ? 248 : 248, tag == "ok" ? 239 : 248),
            Padding = new Padding(10, 8, 10, 8),
            Margin = new Padding(0, 0, 0, 0),
            UseMnemonic = false
        };

        tlp.Controls.Add(lblT, 0, 0);
        tlp.Controls.Add(lblV, 0, 1);
        tlp.Controls.Add(lblB, 0, 2);
        p.Controls.Add(tlp);

        void WrapValueAndBadge()
        {
            if (p.ClientSize.Width < 48)
                return;
            var w = p.ClientSize.Width - p.Padding.Horizontal - 4;
            lblV.MaximumSize = new Size(Math.Max(72, w), 0);
            lblB.MaximumSize = new Size(Math.Max(72, w), 0);
        }

        p.Resize += (_, _) => WrapValueAndBadge();
        WrapValueAndBadge();

        return (p, lblV, lblB);
    }

    private Button CreateNavButton(string text, string key)
    {
        var b = new Button
        {
            Text = "  " + text,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 42,
            Width = 232,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(4, 2, 4, 2),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Ink,
            BackColor = Color.Transparent
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += (_, _) => OnNavClick(key);
        return b;
    }

    private void OnNavClick(string key)
    {
        HighlightNav(key);
        if (key == "dash")
            return;
        MessageBox.Show(this,
            "Chức năng đang được nối với các form nghiệp vụ (Bán hàng, Thuốc, Kho, Báo cáo...).",
            Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLogout_Click(object sender, EventArgs e)
    {
        _auth.DangXuat();
        ReLoginRequested = true;
        Close();
    }

    private void Sidebar_OnPaint(object sender, PaintEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(230, 236, 231), 1);
        e.Graphics.DrawLine(pen, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);
    }

    private void Header_OnPaint(object sender, PaintEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(232, 236, 233), 1);
        e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
    }

    private void PanelAvatar_OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, _panelAvatar.Width - 1, _panelAvatar.Height - 1);
        using var path = RoundRect(rect, 14);
        using var br = new SolidBrush(_avatarColor);
        g.FillPath(br, path);
        using var edge = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
        g.DrawPath(edge, path);

        var initialsRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - 14);
        using var fBig = new Font("Segoe UI", 13F, FontStyle.Bold);
        using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(_avatarInitials, fBig, Brushes.White, initialsRect, fmt);

        var badge = _roleBadge;
        if (badge.Length == 0)
            return;
        using var fSm = new Font("Segoe UI", 6.5F, FontStyle.Bold);
        var badgeSize = g.MeasureString(badge, fSm);
        var bx = (rect.Width - badgeSize.Width - 10) / 2f;
        var by = rect.Bottom - 15f;
        var badgeRect = new RectangleF(bx, by, badgeSize.Width + 8, 13);
        using var bbg = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
        using var bp = new Pen(Color.FromArgb(120, 255, 255, 255), 1);
        g.FillRoundedRectangle(bbg, badgeRect, 3);
        g.DrawRoundedRectangle(bp, badgeRect, 3);
        using var fmt2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var badgeInk = new SolidBrush(PrimaryDark);
        g.DrawString(badge, fSm, badgeInk, badgeRect, fmt2);
    }

    private void BarChart_OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = _panelBarChart.ClientRectangle;
        DrawCardBorder(g, r);
        using var fTitle = new Font("Segoe UI", 11F, FontStyle.Bold);
        g.DrawString("Biểu đồ doanh thu tuần này (Hoàn thành)", fTitle, Brushes.Black, 16, 10);

        var series = _dashData?.DoanhThuTheoNgay;
        if (series is null || series.Count == 0 || _dashData is not { CoTaiChinh: true })
        {
            using var f = new Font("Segoe UI", 9.5F);
            g.DrawString("Không có dữ liệu hiển thị (hoặc vai trò không xem doanh thu).", f, Brushes.DimGray, 16, 80);
            _barHitRects = Array.Empty<RectangleF>();
            return;
        }

        var max = (double)series.Max(x => x.DoanhThu);
        if (max < 1)
            max = 1;

        var plot = new Rectangle(36, 42, Math.Max(40, r.Width - 48), Math.Max(40, r.Height - 78));
        var baseY = plot.Bottom;
        var n = series.Count;
        const float gap = 6f;
        var barW = (plot.Width - gap * Math.Max(0, n - 1)) / Math.Max(1, n);
        _barHitRects = new RectangleF[n];

        using var fAxis = new Font("Segoe UI", 9F);
        using var fScale = new Font("Segoe UI", 8.25F);
        g.DrawString("Max: " + FormatVnd((decimal)max), fScale, Brushes.DimGray, plot.Right - 120, plot.Top - 2);
        for (var i = 0; i < n; i++)
        {
            var pt = series[i];
            var h = (float)((double)pt.DoanhThu / max) * plot.Height;
            var x = plot.Left + i * (barW + gap);
            var rect = new RectangleF(x, baseY - h, barW, h);
            _barHitRects[i] = rect;

            var isHover = i == _hoverBarIndex;
            var isLast = i == n - 1;
            var fill = isHover
                ? Color.FromArgb(255, Math.Min(255, MintBg.R + 40), Math.Min(255, MintBg.G + 30), MintBg.B)
                : (isLast ? PrimaryDark : Color.FromArgb(187, 222, 191));
            using (var br = new SolidBrush(fill))
                g.FillRectangle(br, rect);
            if (isHover)
            {
                using var hi = new Pen(PrimaryDark, 2f);
                g.DrawRectangle(hi, rect.X, rect.Y, rect.Width, rect.Height);
            }

            g.DrawString(pt.NhanThu, fAxis, Brushes.DimGray, rect.X - 2, baseY + 4);
        }
    }

    private void BarChart_OnMouseMove(object sender, MouseEventArgs e)
    {
        var series = _dashData?.DoanhThuTheoNgay;
        if (_dashData is null || series is null || !_dashData.CoTaiChinh)
            return;

        var idx = -1;
        for (var i = 0; i < _barHitRects.Length; i++)
        {
            if (_barHitRects[i].Contains(e.Location))
            {
                idx = i;
                break;
            }
        }

        if (idx == _hoverBarIndex)
            return;

        _hoverBarIndex = idx;
        if (idx >= 0 && idx < series.Count)
        {
            var s = series[idx];
            _tooltipBarText =
                $"{s.NhanThu} · {s.Ngay:dd/MM/yyyy}\nDoanh thu: {FormatVnd(s.DoanhThu)}\nHĐ hoàn thành: {s.SoHoaDonHoanThanh}";
            _dashTooltip.SetToolTip(_panelBarChart, _tooltipBarText);
        }
        else
            _dashTooltip.SetToolTip(_panelBarChart, null);

        _panelBarChart.Invalidate();
    }

    private void ResetBarHover()
    {
        if (_hoverBarIndex < 0)
            return;
        _hoverBarIndex = -1;
        _dashTooltip.SetToolTip(_panelBarChart, null);
        _panelBarChart.Invalidate();
    }

    private void PieChart_OnPaint(object sender, PaintEventArgs e)
    {
        foreach (var p in _pieHitPaths ?? Array.Empty<GraphicsPath>())
            p.Dispose();
        _pieHitPaths = null;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = _panelPieChart.ClientRectangle;
        DrawCardBorder(g, r);
        using var fTitle = new Font("Segoe UI", 11F, FontStyle.Bold);
        g.DrawString("Hóa đơn tuần này (theo trạng thái)", fTitle, Brushes.Black, 16, 8);

        var slices = _dashData?.PhanBoTrangThaiTuan;
        if (slices is null || slices.Count == 0 || _dashData is not { CoTaiChinh: true })
        {
            using var f = new Font("Segoe UI", 9.5F);
            g.DrawString("Không có hóa đơn trong tuần.", f, Brushes.DimGray, 16, 72);
            return;
        }

        var weights = slices
            .Select(s => (double)Math.Max(0.0001m, s.TongThanhTien > 0 ? s.TongThanhTien : (decimal)s.SoLuong))
            .ToList();
        var tw = weights.Sum();

        var pieRect = new Rectangle(24, 36, Math.Min(160, r.Width / 2 - 20), Math.Min(160, r.Height - 52));
        var paths = new GraphicsPath[slices.Count];
        float start = -90f;
        for (var i = 0; i < slices.Count; i++)
        {
            var sl = slices[i];
            var sweep = (float)(weights[i] / tw * 360);
            var path = new GraphicsPath();
            path.AddPie(pieRect, start, sweep);
            paths[i] = path;

            var col = MauTrangThaiHoaDon(sl.TrangThai);
            var light = i == _hoverPieIndex;
            using (var br = new SolidBrush(light ? ControlPaint.Light(col, 0.25f) : col))
                g.FillPath(br, path);
            if (light)
            {
                using var pen = new Pen(PrimaryDark, 2f);
                g.DrawPath(pen, path);
            }

            start += sweep;
        }

        _pieHitPaths = paths;

        var lx = pieRect.Right + 16;
        var ly = 44f;
        using var fLeg = new Font("Segoe UI", 9.25F);
        for (var i = 0; i < slices.Count; i++)
        {
            var sl = slices[i];
            using var b = new SolidBrush(MauTrangThaiHoaDon(sl.TrangThai));
            g.FillRectangle(b, lx, ly, 10, 10);
            var txt = $"{sl.TrangThai}  ·  {sl.SoLuong} HĐ  ·  {FormatVnd(sl.TongThanhTien)}";
            g.DrawString(txt, fLeg, Brushes.Black, lx + 16, ly - 1);
            ly += 26;
        }
    }

    private static Color MauTrangThaiHoaDon(string trangThai)
    {
        if (trangThai.Contains("Hoàn", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb(46, 125, 50);
        if (trangThai.Contains("Hủy", StringComparison.OrdinalIgnoreCase) || trangThai.Contains("Huy", StringComparison.OrdinalIgnoreCase))
            return Color.FromArgb(211, 47, 47);
        return Color.FromArgb(120, 144, 156);
    }

    private void PieChart_OnMouseMove(object sender, MouseEventArgs e)
    {
        var slices = _dashData?.PhanBoTrangThaiTuan;
        if (_dashData is null || slices is null || !_dashData.CoTaiChinh || _pieHitPaths is null || _pieHitPaths.Length == 0)
            return;

        var idx = -1;
        for (var i = 0; i < _pieHitPaths.Length; i++)
        {
            using var region = new Region(_pieHitPaths[i]);
            if (region.IsVisible(e.Location))
            {
                idx = i;
                break;
            }
        }

        if (idx == _hoverPieIndex)
            return;

        _hoverPieIndex = idx;
        if (idx >= 0 && idx < slices.Count)
        {
            var sl = slices[idx];
            _tooltipPieText = $"{sl.TrangThai}\nSố lượng: {sl.SoLuong}\nTổng tiền: {FormatVnd(sl.TongThanhTien)}";
            _dashTooltip.SetToolTip(_panelPieChart, _tooltipPieText);
        }
        else
            _dashTooltip.SetToolTip(_panelPieChart, null);

        _panelPieChart.Invalidate();
    }

    private void ResetPieHover()
    {
        if (_hoverPieIndex < 0)
            return;
        _hoverPieIndex = -1;
        _dashTooltip.SetToolTip(_panelPieChart, null);
        _panelPieChart.Invalidate();
    }

    private void AlertsPanel_OnPaint(object sender, PaintEventArgs e)
    {
        DrawCardBorder(e.Graphics, _alertsPanel.ClientRectangle);
        using var title = new Font("Segoe UI", 11F, FontStyle.Bold);
        e.Graphics.DrawString("Danh sách cảnh báo", title, Brushes.Black, 16, 12);
    }

    private static void DrawCardBorder(Graphics g, Rectangle r)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var inner = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
        using var path = RoundRect(inner, 12);
        using var fill = new SolidBrush(Color.White);
        g.FillPath(fill, path);
        using var pen = new Pen(Color.FromArgb(228, 234, 229), 1);
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

    private static void SetDoubleBufferedRecursive(Control c)
    {
        SetDoubleBufferedOne(c);
        foreach (Control ch in c.Controls)
            SetDoubleBufferedRecursive(ch);
    }

    private static void SetDoubleBufferedOne(Control c)
    {
        typeof(Control).InvokeMember("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
            null, c, [true]);
    }

    private void DashTooltip_OnPopup(object sender, PopupEventArgs e)
    {
        if (e.AssociatedControl != _panelBarChart && e.AssociatedControl != _panelPieChart)
            return;
        var text = e.AssociatedControl == _panelBarChart ? _tooltipBarText : _tooltipPieText;
        if (string.IsNullOrWhiteSpace(text))
            return;
        const int maxW = 360;
        var sz = TextRenderer.MeasureText(text, _tooltipBodyFont, new Size(maxW, 800),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        e.ToolTipSize = new Size(Math.Min(maxW + 24, sz.Width + 24), Math.Min(150, sz.Height + 18));
    }

    private void DashTooltip_OnDraw(object sender, DrawToolTipEventArgs e)
    {
        using var bg = new SolidBrush(Color.FromArgb(252, 253, 252));
        e.Graphics.FillRectangle(bg, e.Bounds);
        using var pen = new Pen(Color.FromArgb(190, 205, 192), 1);
        e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
        var inner = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 8, e.Bounds.Width - 20, e.Bounds.Height - 16);
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, _tooltipBodyFont, inner, Ink,
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
    }

    private void GridInvoices_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.Value is not string s || string.IsNullOrEmpty(s))
            return;
        if (e.ColumnIndex is not (1 or 4))
            return;
        e.Value = UnicodeTextHelper.TryRepairMojibakeForDisplay(s);
    }

    private sealed record NavItem(string Key, string Text, string[] AllowedRoles);
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
    {
        using var path = CreateRoundRect(rect, radius);
        g.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
    {
        using var path = CreateRoundRect(rect, radius);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundRect(RectangleF bounds, float radius)
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
}
