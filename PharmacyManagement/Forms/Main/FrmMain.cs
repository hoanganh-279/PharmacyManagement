#nullable disable
using System.Drawing.Drawing2D;
using System.Reflection;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Main;

/// <summary>
/// Shell ứng dụng: sidebar trái + header + vùng nội dung (theo project_Context §3.1).
/// </summary>
public partial class FrmMain : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    private static readonly Color SidebarBg = Color.FromArgb(250, 252, 250);
    private static readonly Color Muted = Color.FromArgb(97, 97, 97);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);

    private const int ShellTopInsetPx = 40;

    private readonly AuthService _auth = new(new DbContextDAL());

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
    private Label _lblPageTitle;
    private TextBox _txtSearch;
    private Button _btnBell;
    private Panel _contentHost;

    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _pageTitles = new(StringComparer.Ordinal);
    private string _activeNav = "";

    private string _avatarInitials = "";
    private string _roleBadge = "";
    private Color _avatarColor = Primary;

    private FrmDashboard _dashboardForm;

    public bool ReLoginRequested { get; private set; }

    private static readonly NavDef[] NavDefs =
    [
        new("dash", "Dashboard", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
        new("kho", "Quản lý kho", [VaiTroTen.Admin, VaiTroTen.QuanLy, VaiTroTen.NhanVienKho]),
        new("hang", "Thêm hàng hóa", [VaiTroTen.Admin, VaiTroTen.NhanVienKho]),
        new("kedon", "Kê đơn bán thuốc", [VaiTroTen.DuocSi]),
        new("doanhthu", "Quản lý doanh thu", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
        new("nv", "Quản lý nhân viên", [VaiTroTen.Admin]),
        new("bc_canh", "Cảnh báo tồn / hạn", []),
        new("bc_thuoc", "Báo cáo thuốc", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
        new("audit", "Audit log", [VaiTroTen.Admin, VaiTroTen.QuanLy]),
    ];

    public FrmMain()
    {
        InitializeComponent();
        foreach (var d in NavDefs)
            _pageTitles[d.Key] = d.Text;
        BuildLayout();
        WireChrome();
        SetDoubleBufferedRecursive(this);
    }

    private void WireChrome()
    {
        Load += FrmMain_Load;
        Shown += (_, _) => ApplySession();
    }

    private void FrmMain_Load(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            MessageBox.Show(this, "Phiên đăng nhập không hợp lệ.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ReLoginRequested = true;
            Close();
        }
    }

    public void RequestRelogin()
    {
        ReLoginRequested = true;
        Close();
    }

    public bool IsNavKeyVisible(string key)
    {
        var role = UserSession.TenVaiTro;
        foreach (var d in NavDefs)
        {
            if (!string.Equals(d.Key, key, StringComparison.Ordinal))
                continue;
            return d.AllowedRoles.Length == 0
                   || (role is not null && d.AllowedRoles.Contains(role, StringComparer.Ordinal));
        }
        return false;
    }

    public void HighlightNavWithoutHostChange(string key)
    {
        HighlightNav(key);
        if (_pageTitles.TryGetValue(key, out var t))
            _lblPageTitle.Text = t;
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
        _panelAvatar.Invalidate();

        ApplyNavVisibility();
        var first = FirstVisibleNavKey();
        if (first is not null)
            SelectNav(first);
    }

    private string FirstVisibleNavKey()
    {
        foreach (var d in NavDefs)
        {
            if (!_navButtons.TryGetValue(d.Key, out var b) || !b.Visible)
                continue;
            return d.Key;
        }
        return null;
    }

    private void ApplyNavVisibility()
    {
        var role = UserSession.TenVaiTro;
        foreach (var def in NavDefs)
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
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 268));
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
        _navFlow.Controls.Add(new Panel { Height = 4, Width = 240, Margin = new Padding(0) });
        foreach (var def in NavDefs)
        {
            var b = CreateNavButton(def.Text, def.Key);
            _navFlow.Controls.Add(b);
            _navButtons[def.Key] = b;
        }

        _sidebar.Controls.Add(_navFlow);
        _sidebar.Controls.Add(_sidebarFooter);
        _sidebar.Controls.Add(_sidebarHeader);

        _workspace = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        _root.Controls.Add(_workspace, 1, 0);

        _header = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.White };
        _header.Paint += Header_OnPaint;
        _lblPageTitle = new Label
        {
            AutoSize = true,
            Location = new Point(20, 18),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Ink,
            Text = "—"
        };
        _txtSearch = new TextBox
        {
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Text = "",
            PlaceholderText = "Tìm kiếm thuốc, hóa đơn...",
            Location = new Point(320, 16),
            Width = 420,
            Height = 32
        };
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
        _header.Controls.Add(_lblPageTitle);
        _header.Controls.Add(_txtSearch);
        _header.Controls.Add(_btnBell);
        _header.Resize += (_, _) => { _btnBell.Left = _header.Width - 56; };

        _contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 246),
            Padding = Padding.Empty
        };

        _workspace.Controls.Add(_contentHost);
        _workspace.Controls.Add(_header);
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
        b.Click += (_, _) => SelectNav(key);
        return b;
    }

    private void SelectNav(string key)
    {
        if (!_navButtons.TryGetValue(key, out var btn) || !btn.Visible)
        {
            MessageBox.Show(this, "Bạn không có quyền truy cập mục này.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        HighlightNav(key);
        _lblPageTitle.Text = _pageTitles.GetValueOrDefault(key, "—");

        ClearContentHost();

        if (key == "dash")
        {
            _dashboardForm = new FrmDashboard
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            _dashboardForm.OnShellQuickNavigate = ShellQuickNavigateFromDashboard;
            _contentHost.Controls.Add(_dashboardForm);
            _dashboardForm.Show();
            return;
        }

        var ph = new Panel { Dock = DockStyle.Fill, BackColor = _contentHost.BackColor };
        var msg = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Muted,
            Padding = new Padding(32),
            Text = "Chức năng «" + _pageTitles.GetValueOrDefault(key, key) + "» đang được nối với form nghiệp vụ.\n\n"
                   + "Tham chiếu ánh xạ menu trong project_Context.md (mục 2.2)."
        };
        ph.Controls.Add(msg);
        _contentHost.Controls.Add(ph);
    }

    private void ShellQuickNavigateFromDashboard(string navKey, string message)
    {
        if (!IsNavKeyVisible(navKey))
        {
            MessageBox.Show(this, "Tài khoản của bạn không có quyền truy cập chức năng này.", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        HighlightNavWithoutHostChange(navKey);
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearContentHost()
    {
        foreach (Control c in _contentHost.Controls)
            c.Dispose();
        _contentHost.Controls.Clear();
        _dashboardForm = null;
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

    private sealed record NavDef(string Key, string Text, string[] AllowedRoles);
}
