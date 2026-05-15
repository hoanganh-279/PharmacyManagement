#nullable disable
namespace PharmacyManagement.Forms.Auth;

public partial class FrmLogin
{
    private System.ComponentModel.IContainer components;
    private Panel panelBackdrop;
    private Panel panelCard;
    private Panel panelFormLeft;
    private Panel panelPromoRight;
    private Panel panelLogoBadge;
    private Label lblTenDuAn;
    private Label lblHeThong;
    private Label lblChaoMung;
    private Label lblHuongDan;
    private Label lblTenDangNhap;
    private Panel panelFieldUser;
    private TextBox txtTenDangNhap;
    private Label lblMatKhau;
    private Panel panelFieldPass;
    private TableLayoutPanel tablePassRow;
    private TextBox txtMatKhau;
    private Button btnToggleMatKhau;
    private CheckBox chkGhiNho;
    private LinkLabel lnkQuenMatKhau;
    private Button btnDangNhap;
    private Panel panelDivider;
    private LinkLabel lnkHoTro;
    private Label lblFooter;
    private Label lblNenTang;
    private Label lblMoTa;
    private FlowLayoutPanel flowFeatures;
    private Panel panelFeat1;
    private Panel panelFeat2;
    private Panel panelFeat3;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var primary = Color.FromArgb(46, 125, 50);
        var textMuted = Color.FromArgb(97, 97, 97);
        var ink = Color.FromArgb(33, 37, 41);
        var fieldBg = Color.FromArgb(250, 251, 250);
        var backdrop = Color.FromArgb(241, 245, 242);

        panelBackdrop = new Panel();
        panelCard = new Panel();
        panelFormLeft = new Panel();
        panelPromoRight = new Panel();
        panelLogoBadge = new Panel();
        lblTenDuAn = new Label();
        lblHeThong = new Label();
        lblChaoMung = new Label();
        lblHuongDan = new Label();
        lblTenDangNhap = new Label();
        panelFieldUser = new Panel();
        txtTenDangNhap = new TextBox();
        lblMatKhau = new Label();
        panelFieldPass = new Panel();
        tablePassRow = new TableLayoutPanel();
        txtMatKhau = new TextBox();
        btnToggleMatKhau = new Button();
        chkGhiNho = new CheckBox();
        lnkQuenMatKhau = new LinkLabel();
        btnDangNhap = new Button();
        panelDivider = new Panel();
        lnkHoTro = new LinkLabel();
        lblFooter = new Label();
        lblNenTang = new Label();
        lblMoTa = new Label();
        flowFeatures = new FlowLayoutPanel();
        panelFeat1 = new Panel();
        panelFeat2 = new Panel();
        panelFeat3 = new Panel();

        SuspendLayout();

        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1020, 640);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(880, 560);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        BackColor = backdrop;

        lblFooter.Dock = DockStyle.Bottom;
        lblFooter.Height = 40;
        lblFooter.TextAlign = ContentAlignment.MiddleCenter;
        lblFooter.ForeColor = Color.FromArgb(120, 130, 125);
        lblFooter.Font = new Font("Segoe UI", 8.5F);
        lblFooter.Text = $"© {DateTime.Now.Year} Pharmacy Management ALN · Hệ thống quản lý nhà thuốc v1.0";

        panelBackdrop.Dock = DockStyle.Fill;
        panelBackdrop.BackColor = backdrop;
        panelBackdrop.Controls.Add(panelCard);

        panelCard.Size = new Size(920, 540);
        panelCard.BackColor = Color.White;
        panelCard.Controls.Add(panelFormLeft);
        panelCard.Controls.Add(panelPromoRight);

        panelFormLeft.Location = new Point(0, 0);
        panelFormLeft.Size = new Size(440, 540);
        panelFormLeft.BackColor = Color.White;
        panelFormLeft.Padding = new Padding(40, 32, 32, 28);

        panelLogoBadge.Size = new Size(44, 44);
        panelLogoBadge.Location = new Point(40, 32);
        panelLogoBadge.BackColor = Color.Transparent;
        panelLogoBadge.Cursor = Cursors.Default;

        lblTenDuAn.AutoSize = false;
        lblTenDuAn.Location = new Point(96, 32);
        lblTenDuAn.Size = new Size(300, 24);
        lblTenDuAn.Text = "Pharmacy Management ALN";
        lblTenDuAn.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
        lblTenDuAn.ForeColor = primary;

        lblHeThong.AutoSize = false;
        lblHeThong.Location = new Point(96, 56);
        lblHeThong.Size = new Size(300, 22);
        lblHeThong.Text = "Hệ thống quản lý nhà thuốc";
        lblHeThong.ForeColor = textMuted;
        lblHeThong.Font = new Font("Segoe UI", 9F);

        lblChaoMung.AutoSize = false;
        lblChaoMung.Location = new Point(40, 104);
        lblChaoMung.Size = new Size(360, 40);
        lblChaoMung.Text = "Chào mừng trở lại";
        lblChaoMung.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblChaoMung.ForeColor = ink;

        lblHuongDan.AutoSize = false;
        lblHuongDan.Location = new Point(40, 148);
        lblHuongDan.Size = new Size(360, 36);
        lblHuongDan.Text = "Đăng nhập để tiếp tục làm việc trên hệ thống.";
        lblHuongDan.ForeColor = textMuted;
        lblHuongDan.Font = new Font("Segoe UI", 10F);

        lblTenDangNhap.AutoSize = false;
        lblTenDangNhap.Location = new Point(40, 200);
        lblTenDangNhap.Size = new Size(360, 22);
        lblTenDangNhap.Text = "Tên đăng nhập";
        lblTenDangNhap.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblTenDangNhap.ForeColor = Color.FromArgb(55, 60, 56);

        panelFieldUser.Location = new Point(40, 224);
        panelFieldUser.Size = new Size(360, 44);
        panelFieldUser.BackColor = fieldBg;
        panelFieldUser.Padding = new Padding(14, 10, 14, 10);

        txtTenDangNhap.BorderStyle = BorderStyle.None;
        txtTenDangNhap.Dock = DockStyle.Fill;
        txtTenDangNhap.BackColor = fieldBg;
        txtTenDangNhap.Font = new Font("Segoe UI", 10.5F);
        txtTenDangNhap.PlaceholderText = "Nhập tên đăng nhập";

        panelFieldUser.Controls.Add(txtTenDangNhap);

        lblMatKhau.AutoSize = false;
        lblMatKhau.Location = new Point(40, 282);
        lblMatKhau.Size = new Size(360, 22);
        lblMatKhau.Text = "Mật khẩu";
        lblMatKhau.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblMatKhau.ForeColor = Color.FromArgb(55, 60, 56);

        panelFieldPass.Location = new Point(40, 306);
        panelFieldPass.Size = new Size(360, 44);
        panelFieldPass.BackColor = fieldBg;
        panelFieldPass.Padding = new Padding(2);

        tablePassRow.ColumnCount = 2;
        tablePassRow.RowCount = 1;
        tablePassRow.Dock = DockStyle.Fill;
        tablePassRow.BackColor = fieldBg;
        tablePassRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tablePassRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        tablePassRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tablePassRow.Margin = Padding.Empty;
        tablePassRow.Padding = new Padding(12, 6, 8, 6);

        txtMatKhau.BorderStyle = BorderStyle.None;
        txtMatKhau.Dock = DockStyle.Fill;
        txtMatKhau.BackColor = fieldBg;
        txtMatKhau.Font = new Font("Segoe UI", 10.5F);
        txtMatKhau.UseSystemPasswordChar = true;

        btnToggleMatKhau.Dock = DockStyle.Fill;
        btnToggleMatKhau.Margin = new Padding(6, 2, 2, 2);
        btnToggleMatKhau.Text = "Hiện";
        btnToggleMatKhau.FlatStyle = FlatStyle.Flat;
        btnToggleMatKhau.FlatAppearance.BorderSize = 0;
        btnToggleMatKhau.BackColor = fieldBg;
        btnToggleMatKhau.ForeColor = Color.FromArgb(80, 95, 85);
        btnToggleMatKhau.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnToggleMatKhau.Cursor = Cursors.Hand;
        btnToggleMatKhau.TabStop = true;
        btnToggleMatKhau.Click += BtnToggleMatKhau_Click;

        tablePassRow.Controls.Add(txtMatKhau, 0, 0);
        tablePassRow.Controls.Add(btnToggleMatKhau, 1, 0);
        panelFieldPass.Controls.Add(tablePassRow);

        chkGhiNho.AutoSize = true;
        chkGhiNho.Location = new Point(40, 364);
        chkGhiNho.Text = "Ghi nhớ đăng nhập";
        chkGhiNho.Font = new Font("Segoe UI", 9F);
        chkGhiNho.ForeColor = ink;

        lnkQuenMatKhau.AutoSize = true;
        lnkQuenMatKhau.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lnkQuenMatKhau.Top = 360;
        lnkQuenMatKhau.Text = "Quên mật khẩu?";
        lnkQuenMatKhau.LinkColor = primary;
        lnkQuenMatKhau.ActiveLinkColor = Color.FromArgb(36, 105, 40);
        lnkQuenMatKhau.VisitedLinkColor = primary;
        lnkQuenMatKhau.TabStop = true;
        lnkQuenMatKhau.Font = new Font("Segoe UI", 9F);
        lnkQuenMatKhau.LinkClicked += LnkQuenMatKhau_LinkClicked;

        btnDangNhap.Location = new Point(40, 408);
        btnDangNhap.Size = new Size(360, 48);
        btnDangNhap.Text = "Đăng nhập";
        btnDangNhap.BackColor = primary;
        btnDangNhap.ForeColor = Color.White;
        btnDangNhap.FlatStyle = FlatStyle.Flat;
        btnDangNhap.FlatAppearance.BorderSize = 0;
        btnDangNhap.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnDangNhap.Cursor = Cursors.Hand;
        btnDangNhap.Click += BtnDangNhap_Click;

        panelDivider.Location = new Point(40, 476);
        panelDivider.Size = new Size(360, 1);
        panelDivider.BackColor = Color.FromArgb(232, 236, 233);

        lnkHoTro.AutoSize = false;
        lnkHoTro.Location = new Point(40, 486);
        lnkHoTro.Size = new Size(360, 28);
        lnkHoTro.Text = "Cần hỗ trợ kỹ thuật? Liên hệ quản trị viên";
        lnkHoTro.LinkColor = primary;
        lnkHoTro.ForeColor = textMuted;
        lnkHoTro.Font = new Font("Segoe UI", 9F);
        lnkHoTro.UseCompatibleTextRendering = false;
        lnkHoTro.LinkClicked += LnkHoTro_LinkClicked;

        panelFormLeft.Controls.Add(panelLogoBadge);
        panelFormLeft.Controls.Add(lblTenDuAn);
        panelFormLeft.Controls.Add(lblHeThong);
        panelFormLeft.Controls.Add(lblChaoMung);
        panelFormLeft.Controls.Add(lblHuongDan);
        panelFormLeft.Controls.Add(lblTenDangNhap);
        panelFormLeft.Controls.Add(panelFieldUser);
        panelFormLeft.Controls.Add(lblMatKhau);
        panelFormLeft.Controls.Add(panelFieldPass);
        panelFormLeft.Controls.Add(chkGhiNho);
        panelFormLeft.Controls.Add(lnkQuenMatKhau);
        panelFormLeft.Controls.Add(btnDangNhap);
        panelFormLeft.Controls.Add(panelDivider);
        panelFormLeft.Controls.Add(lnkHoTro);

        panelPromoRight.Location = new Point(440, 0);
        panelPromoRight.Size = new Size(480, 540);
        panelPromoRight.BackColor = Color.FromArgb(232, 245, 233);

        lblNenTang.AutoSize = false;
        lblNenTang.Location = new Point(36, 44);
        lblNenTang.Size = new Size(408, 72);
        lblNenTang.Text = "Nền tảng quản lý" + Environment.NewLine + "dược phẩm hiện đại";
        lblNenTang.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblNenTang.ForeColor = Color.FromArgb(28, 58, 32);
        lblNenTang.BackColor = Color.Transparent;

        lblMoTa.AutoSize = false;
        lblMoTa.Location = new Point(36, 124);
        lblMoTa.Size = new Size(408, 96);
        lblMoTa.Text = "Tối ưu quy trình bán hàng, quản lý kho và theo dõi thông tin một cách nhanh chóng, chính xác và an toàn.";
        lblMoTa.Font = new Font("Segoe UI", 10F);
        lblMoTa.ForeColor = Color.FromArgb(88, 102, 92);
        lblMoTa.BackColor = Color.Transparent;

        flowFeatures.Location = new Point(36, 236);
        flowFeatures.Size = new Size(408, 100);
        flowFeatures.WrapContents = false;
        flowFeatures.FlowDirection = FlowDirection.LeftToRight;
        flowFeatures.BackColor = Color.Transparent;
        flowFeatures.Padding = new Padding(0, 4, 0, 0);

        void StyleFeaturePanel(Panel p, string icon, string caption)
        {
            p.Size = new Size(128, 96);
            p.BackColor = Color.Transparent;
            p.Margin = new Padding(0, 0, 12, 0);
            var iconLbl = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 44,
                Text = icon,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F),
                ForeColor = primary,
                BackColor = Color.Transparent
            };
            var cap = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 40,
                Text = caption,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 62, 52),
                BackColor = Color.Transparent
            };
            p.Controls.Add(cap);
            p.Controls.Add(iconLbl);
        }

        StyleFeaturePanel(panelFeat1, "\u26A1", "Nhanh chóng");
        StyleFeaturePanel(panelFeat2, "\u2713", "Bảo mật");
        StyleFeaturePanel(panelFeat3, "\u25A4", "Chính xác");

        flowFeatures.Controls.Add(panelFeat1);
        flowFeatures.Controls.Add(panelFeat2);
        flowFeatures.Controls.Add(panelFeat3);

        panelPromoRight.Controls.Add(lblNenTang);
        panelPromoRight.Controls.Add(lblMoTa);
        panelPromoRight.Controls.Add(flowFeatures);

        Controls.Add(panelBackdrop);
        Controls.Add(lblFooter);

        txtTenDangNhap.KeyDown += Txt_KeyDown;
        txtMatKhau.KeyDown += Txt_KeyDown;

        panelFormLeft.Resize += PanelFormLeft_Resize;

        Load += FrmLogin_Load;
        Resize += FrmLogin_Resize;

        ResumeLayout(false);
    }

    private void PanelFormLeft_Resize(object sender, EventArgs e)
    {
        lnkQuenMatKhau.Left = panelFormLeft.ClientSize.Width - lnkQuenMatKhau.Width - 40;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }
}