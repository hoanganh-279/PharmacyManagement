using System.Drawing.Drawing2D;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Pharmacy.BLL;
using Pharmacy.DAL;
using PharmacyManagement;

namespace PharmacyManagement.Forms.Auth;

public partial class FrmLogin : Form
{
    private const string AppFolderName = "PharmacyManagementALN";
    private const string RememberFile = "remembered_username.txt";

    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryHover = Color.FromArgb(36, 105, 40);
    private static readonly Color FieldBg = Color.FromArgb(250, 251, 250);
    private static readonly Color FieldBgHover = Color.FromArgb(240, 244, 240);

    private readonly AuthService _auth = new(new DbContextDAL());
    private bool _passwordVisible;

    public FrmLogin()
    {
        InitializeComponent();
        Text = $"Đăng nhập — {BrandTitle}";
        AcceptButton = btnDangNhap;
        WireChrome();
    }

    private static string BrandTitle => "Pharmacy Management ALN";

    private static string RememberPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName, RememberFile);

    private void WireChrome()
    {
        foreach (Control c in new Control[] { panelBackdrop, panelCard, panelFormLeft, panelPromoRight, panelLogoBadge, panelFieldUser, panelFieldPass })
            SetDoubleBuffered(c);

        panelLogoBadge.Paint += PanelLogoBadge_OnPaint;
        panelFieldUser.Paint += InputPanel_OnPaint;
        panelFieldPass.Paint += InputPanel_OnPaint;
        panelPromoRight.Paint += PanelPromoRight_OnPaint;

        btnDangNhap.MouseEnter += (_, _) => btnDangNhap.BackColor = PrimaryHover;
        btnDangNhap.MouseLeave += (_, _) => btnDangNhap.BackColor = Primary;

        btnToggleMatKhau.MouseEnter += (_, _) => btnToggleMatKhau.BackColor = FieldBgHover;
        btnToggleMatKhau.MouseLeave += (_, _) => btnToggleMatKhau.BackColor = FieldBg;

        txtTenDangNhap.Enter += (_, _) => HighlightField(panelFieldUser, true);
        txtTenDangNhap.Leave += (_, _) => ScheduleFieldHighlight(panelFieldUser);

        txtMatKhau.Enter += (_, _) => HighlightField(panelFieldPass, true);
        txtMatKhau.Leave += (_, _) => ScheduleFieldHighlight(panelFieldPass);
        btnToggleMatKhau.Enter += (_, _) => HighlightField(panelFieldPass, true);
        btnToggleMatKhau.Leave += (_, _) => ScheduleFieldHighlight(panelFieldPass);
    }

    private void ScheduleFieldHighlight(Panel field)
    {
        BeginInvoke(() =>
        {
            if (!field.ContainsFocus)
                HighlightField(field, false);
        });
    }

    private static void SetDoubleBuffered(Control c)
    {
        typeof(Control).InvokeMember("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
            null, c, [true]);
    }

    private void HighlightField(Panel field, bool on)
    {
        field.Tag = on ? 1 : null;
        field.Invalidate();
    }

    private static GraphicsPath CreateRoundRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var d = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void PanelLogoBadge_OnPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, panelLogoBadge.Width - 1, panelLogoBadge.Height - 1);
        using var path = CreateRoundRect(rect, 12);
        using var fill = new SolidBrush(Primary);
        g.FillPath(fill, path);
        using var edge = new Pen(Color.FromArgb(70, 150, 74), 1f);
        g.DrawPath(edge, path);
        using var f = new Font("Segoe UI", 16F, FontStyle.Bold);
        using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("\u2695", f, Brushes.White, rect, fmt);
    }

    private void InputPanel_OnPaint(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel p)
            return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
        using var path = CreateRoundRect(rect, 12);
        var focus = p.Tag is int;
        var penColor = focus ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 212, 200);
        using var pen = new Pen(penColor, focus ? 1.6f : 1.1f);
        g.DrawPath(pen, path);
    }

    private void PanelPromoRight_OnPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = panelPromoRight.ClientRectangle;
        using (var brush = new LinearGradientBrush(r,
                   Color.FromArgb(242, 252, 245),
                   Color.FromArgb(216, 236, 222),
                   LinearGradientMode.ForwardDiagonal))
        {
            g.FillRectangle(brush, r);
        }

        using (var haze = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
        {
            g.FillEllipse(haze, r.Width - 200, -80, 320, 320);
            g.FillEllipse(haze, -100, r.Height - 220, 340, 340);
        }

        using var accent = new SolidBrush(Color.FromArgb(14, 46, 125, 50));
        g.FillEllipse(accent, r.Width - 140, r.Height - 160, 220, 220);
    }

    private void FrmLogin_Load(object? sender, EventArgs e)
    {
        CenterCard();
        PanelFormLeft_Resize(panelFormLeft, EventArgs.Empty);
        TryLoadRememberedUsername();
        var hoTro = "Liên hệ quản trị viên";
        var idx = lnkHoTro.Text.IndexOf(hoTro, StringComparison.Ordinal);
        if (idx >= 0)
            lnkHoTro.LinkArea = new LinkArea(idx, hoTro.Length);
    }

    private void FrmLogin_Resize(object? sender, EventArgs e) => CenterCard();

    private void CenterCard()
    {
        panelCard.Left = Math.Max(0, (panelBackdrop.ClientSize.Width - panelCard.Width) / 2);
        panelCard.Top = Math.Max(0, (panelBackdrop.ClientSize.Height - panelCard.Height) / 2);
    }

    private void TryLoadRememberedUsername()
    {
        try
        {
            var path = RememberPath;
            if (!File.Exists(path))
                return;
            var user = File.ReadAllText(path).Trim();
            if (user.Length > 0)
            {
                txtTenDangNhap.Text = user;
                chkGhiNho.Checked = true;
            }
        }
        catch
        {
            // bỏ qua nếu không đọc được file prefs
        }
    }

    private void SaveOrClearRememberedUsername()
    {
        try
        {
            var dir = Path.GetDirectoryName(RememberPath);
            if (dir is not null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (chkGhiNho.Checked && !string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
                File.WriteAllText(RememberPath, txtTenDangNhap.Text.Trim());
            else if (File.Exists(RememberPath))
                File.Delete(RememberPath);
        }
        catch
        {
            // không chặn đăng nhập nếu ghi file thất bại
        }
    }

    private void BtnToggleMatKhau_Click(object? sender, EventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        txtMatKhau.UseSystemPasswordChar = !_passwordVisible;
        btnToggleMatKhau.Text = _passwordVisible ? "Ẩn" : "Hiện";
    }

    private void BtnDangNhap_Click(object? sender, EventArgs e)
    {
        var user = txtTenDangNhap.Text;
        var pass = txtMatKhau.Text;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show(this, "Vui lòng nhập tên đăng nhập và mật khẩu.", BrandTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_auth.DangNhap(user, pass))
            {
                SaveOrClearRememberedUsername();
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show(this, "Sai tên đăng nhập hoặc mật khẩu.", BrandTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this, ConnectionSettings.FormatSqlExceptionForUser(ex), BrandTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LnkQuenMatKhau_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) =>
        MessageBox.Show(this, "Vui lòng liên hệ quản trị viên để được cấp lại mật khẩu.", BrandTitle,
            MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void LnkHoTro_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e) =>
        MessageBox.Show(this, "Liên hệ quản trị viên hệ thống để được hỗ trợ kỹ thuật.", BrandTitle,
            MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void Txt_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnDangNhap_Click(btnDangNhap, EventArgs.Empty);
        }
    }
}
