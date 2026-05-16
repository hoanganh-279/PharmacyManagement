namespace PharmacyManagement.Forms.Sales;

partial class FrmKeDonBanThuoc
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlKhach = new Panel();
        lblCccd = new Label();
        txtCccd = new TextBox();
        btnTraCuuKhach = new Button();
        btnKhachLe = new Button();
        btnTaoKhach = new Button();
        lblHoTen = new Label();
        txtHoTen = new TextBox();
        lblSdt = new Label();
        txtSoDienThoai = new TextBox();
        lblDiaChi = new Label();
        txtDiaChi = new TextBox();
        lblNgaySinh = new Label();
        dtpNgaySinh = new DateTimePicker();
        chkNgaySinh = new CheckBox();
        lblKhachTrangThai = new Label();
        pnlBody = new Panel();
        tblMain = new TableLayoutPanel();
        pnlThuoc = new Panel();
        pnlTimThuoc = new Panel();
        lblTimThuoc = new Label();
        txtTimThuoc = new TextBox();
        btnTimThuoc = new Button();
        dgvThuoc = new DataGridView();
        pnlGioHang = new Panel();
        lblGioHang = new Label();
        numSoLuong = new NumericUpDown();
        btnThemGio = new Button();
        btnXoaDong = new Button();
        btnInHoaDon = new Button();
        btnXuatHoaDon = new Button();
        dgvGioHang = new DataGridView();
        pnlThanhToan = new Panel();
        lblTongHang = new Label();
        lblTongHangVal = new Label();
        lblGiamGia = new Label();
        numGiamGia = new NumericUpDown();
        lblThanhTien = new Label();
        lblThanhTienVal = new Label();
        lblHinhThuc = new Label();
        cboHinhThucTT = new ComboBox();
        btnXacNhan = new Button();
        btnLamMoi = new Button();
        lblStatus = new Label();
        pnlKhach.SuspendLayout();
        pnlBody.SuspendLayout();
        tblMain.SuspendLayout();
        pnlThuoc.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvThuoc).BeginInit();
        pnlGioHang.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numSoLuong).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvGioHang).BeginInit();
        pnlThanhToan.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numGiamGia).BeginInit();
        SuspendLayout();

        var primary = Color.FromArgb(46, 125, 50);
        var mint = Color.FromArgb(232, 245, 233);
        var ink = Color.FromArgb(33, 37, 41);
        var muted = Color.FromArgb(97, 97, 97);

        Text = "Kê đơn bán thuốc";
        BackColor = Color.FromArgb(245, 247, 246);
        Font = new Font("Segoe UI", 9.75F);
        MinimumSize = new Size(1000, 640);

        // ── Khách hàng (TableLayout — tránh đè chữ khi thu hẹp) ──
        pnlKhach.Dock = DockStyle.Top;
        pnlKhach.Height = 132;
        pnlKhach.BackColor = Color.White;
        pnlKhach.Padding = new Padding(12, 8, 12, 8);

        var tblKhach = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 3,
            BackColor = Color.White
        };
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        tblKhach.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblKhach.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tblKhach.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tblKhach.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        Label Lbl(string text) => new()
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = muted,
            Margin = new Padding(0, 0, 6, 0)
        };

        static Control Inset(Control c) { c.Dock = DockStyle.Fill; c.Margin = new Padding(0, 4, 8, 4); return c; }

        lblCccd = Lbl("CCCD:");
        txtCccd = (TextBox)Inset(new TextBox { MaxLength = 14, PlaceholderText = "12 chữ số" });
        btnTraCuuKhach = (Button)Inset(new Button
        {
            Text = "Tra cứu",
            FlatStyle = FlatStyle.Flat,
            BackColor = primary,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        });
        btnTraCuuKhach.FlatAppearance.BorderSize = 0;
        btnKhachLe = (Button)Inset(new Button { Text = "Khách lẻ", FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand });
        btnTaoKhach = (Button)Inset(new Button
        {
            Text = "Lưu KH mới",
            FlatStyle = FlatStyle.Flat,
            BackColor = mint,
            ForeColor = primary,
            Cursor = Cursors.Hand
        });
        btnTaoKhach.FlatAppearance.BorderColor = primary;
        lblKhachTrangThai = new Label
        {
            Text = "Chưa tra cứu khách",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = primary,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoEllipsis = true,
            Margin = new Padding(4, 4, 0, 4)
        };

        tblKhach.Controls.Add(lblCccd, 0, 0);
        tblKhach.Controls.Add(txtCccd, 1, 0);
        tblKhach.Controls.Add(btnTraCuuKhach, 2, 0);
        tblKhach.Controls.Add(btnKhachLe, 3, 0);
        tblKhach.Controls.Add(btnTaoKhach, 4, 0);
        tblKhach.SetColumnSpan(lblKhachTrangThai, 3);
        tblKhach.Controls.Add(lblKhachTrangThai, 5, 0);

        lblHoTen = Lbl("Họ tên:");
        txtHoTen = (TextBox)Inset(new TextBox());
        lblSdt = Lbl("SĐT:");
        txtSoDienThoai = (TextBox)Inset(new TextBox());
        lblDiaChi = Lbl("Địa chỉ:");
        txtDiaChi = (TextBox)Inset(new TextBox());

        tblKhach.Controls.Add(lblHoTen, 0, 1);
        tblKhach.Controls.Add(txtHoTen, 1, 1);
        tblKhach.SetColumnSpan(txtHoTen, 2);
        tblKhach.Controls.Add(lblSdt, 3, 1);
        tblKhach.Controls.Add(txtSoDienThoai, 4, 1);
        tblKhach.Controls.Add(lblDiaChi, 5, 1);
        tblKhach.SetColumnSpan(txtDiaChi, 2);
        tblKhach.Controls.Add(txtDiaChi, 6, 1);

        lblNgaySinh = Lbl("Ngày sinh:");
        dtpNgaySinh = (DateTimePicker)Inset(new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Enabled = false
        });
        chkNgaySinh = new CheckBox
        {
            Text = "Có ngày sinh",
            AutoSize = true,
            Dock = DockStyle.Left,
            Margin = new Padding(0, 10, 0, 0)
        };

        tblKhach.Controls.Add(lblNgaySinh, 0, 2);
        tblKhach.Controls.Add(dtpNgaySinh, 1, 2);
        tblKhach.Controls.Add(chkNgaySinh, 2, 2);
        tblKhach.SetColumnSpan(chkNgaySinh, 2);

        pnlKhach.Controls.Add(tblKhach);

        // ── Nội dung dọc: tìm thuốc → kết quả → giỏ hàng ──
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.BackColor = BackColor;
        pnlBody.Padding = new Padding(8, 4, 8, 4);

        tblMain.Dock = DockStyle.Fill;
        tblMain.ColumnCount = 1;
        tblMain.RowCount = 3;
        tblMain.BackColor = BackColor;
        tblMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tblMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        tblMain.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        tblMain.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

        // Thuốc (chỉ lưới kết quả; thanh tìm nằm hàng riêng phía trên)
        pnlThuoc.Dock = DockStyle.Fill;
        pnlThuoc.BackColor = Color.White;

        pnlTimThuoc.Dock = DockStyle.Fill;
        pnlTimThuoc.BackColor = Color.White;
        pnlTimThuoc.Padding = new Padding(8, 4, 8, 4);

        lblTimThuoc.Text = "Tìm thuốc (tên / hoạt chất):";
        lblTimThuoc.AutoSize = true;
        lblTimThuoc.Location = new Point(8, 8);
        lblTimThuoc.ForeColor = muted;

        txtTimThuoc.Location = new Point(8, 28);
        txtTimThuoc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtTimThuoc.Width = 300;
        txtTimThuoc.PlaceholderText = "Nhập từ khóa...";

        btnTimThuoc.Text = "Tìm";
        btnTimThuoc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnTimThuoc.Size = new Size(72, 28);
        btnTimThuoc.Location = new Point(360, 28);
        btnTimThuoc.FlatStyle = FlatStyle.Flat;
        btnTimThuoc.BackColor = primary;
        btnTimThuoc.ForeColor = Color.White;
        btnTimThuoc.FlatAppearance.BorderSize = 0;

        pnlTimThuoc.Controls.AddRange([lblTimThuoc, txtTimThuoc, btnTimThuoc]);
        pnlTimThuoc.Resize += (_, _) =>
        {
            txtTimThuoc.Width = Math.Max(120, pnlTimThuoc.Width - 100);
            btnTimThuoc.Left = pnlTimThuoc.Width - btnTimThuoc.Width - 12;
        };

        dgvThuoc.Dock = DockStyle.Fill;
        dgvThuoc.ReadOnly = true;
        dgvThuoc.AllowUserToAddRows = false;
        dgvThuoc.AllowUserToDeleteRows = false;
        dgvThuoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvThuoc.MultiSelect = false;
        dgvThuoc.RowHeadersVisible = false;
        dgvThuoc.BackgroundColor = Color.White;
        dgvThuoc.BorderStyle = BorderStyle.None;
        dgvThuoc.Margin = new Padding(0, 4, 0, 0);

        pnlThuoc.Controls.Add(dgvThuoc);

        // Giỏ hàng
        pnlGioHang.Dock = DockStyle.Fill;
        pnlGioHang.BackColor = Color.White;

        lblGioHang.Text = "Giỏ hàng / Chi tiết hóa đơn";
        lblGioHang.Dock = DockStyle.Top;
        lblGioHang.Height = 32;
        lblGioHang.Padding = new Padding(8, 6, 0, 0);
        lblGioHang.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblGioHang.ForeColor = ink;

        var pnlGioToolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
        numSoLuong.Location = new Point(8, 6);
        numSoLuong.Width = 80;
        numSoLuong.Height = 30;
        numSoLuong.Minimum = 1;
        numSoLuong.Maximum = 9999;
        numSoLuong.Value = 1;

        btnThemGio.Text = "Thêm vào giỏ";
        btnThemGio.Location = new Point(96, 4);
        btnThemGio.Size = new Size(118, 30);
        btnThemGio.FlatStyle = FlatStyle.Flat;
        btnThemGio.BackColor = primary;
        btnThemGio.ForeColor = Color.White;
        btnThemGio.FlatAppearance.BorderSize = 0;

        btnXoaDong.Text = "Xóa dòng";
        btnXoaDong.Location = new Point(220, 4);
        btnXoaDong.Size = new Size(92, 30);
        btnXoaDong.FlatStyle = FlatStyle.Flat;

        btnInHoaDon.Text = "In hóa đơn";
        btnInHoaDon.Location = new Point(318, 4);
        btnInHoaDon.Size = new Size(108, 30);
        btnInHoaDon.FlatStyle = FlatStyle.Flat;
        btnInHoaDon.BackColor = mint;
        btnInHoaDon.ForeColor = primary;
        btnInHoaDon.FlatAppearance.BorderColor = primary;

        btnXuatHoaDon.Text = "Xuất hóa đơn";
        btnXuatHoaDon.Location = new Point(432, 4);
        btnXuatHoaDon.Size = new Size(118, 30);
        btnXuatHoaDon.FlatStyle = FlatStyle.Flat;
        btnXuatHoaDon.BackColor = mint;
        btnXuatHoaDon.ForeColor = primary;
        btnXuatHoaDon.FlatAppearance.BorderColor = primary;

        pnlGioToolbar.Controls.AddRange([numSoLuong, btnThemGio, btnXoaDong, btnInHoaDon, btnXuatHoaDon]);

        dgvGioHang.Dock = DockStyle.Fill;
        dgvGioHang.ReadOnly = true;
        dgvGioHang.AllowUserToAddRows = false;
        dgvGioHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvGioHang.RowHeadersVisible = false;
        dgvGioHang.BackgroundColor = Color.White;
        dgvGioHang.BorderStyle = BorderStyle.None;
        dgvGioHang.Margin = new Padding(0, 4, 0, 0);

        pnlThanhToan.Dock = DockStyle.Bottom;
        pnlThanhToan.Height = 132;
        pnlThanhToan.BackColor = mint;
        pnlThanhToan.Padding = new Padding(12, 10, 12, 10);

        var tblPay = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            BackColor = mint
        };
        tblPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        tblPay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        tblPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tblPay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        tblPay.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        tblPay.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        tblPay.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

        static Label PayLbl(string t) => new()
        {
            Text = t,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false,
            Margin = new Padding(0, 2, 8, 2)
        };

        lblTongHang = PayLbl("Tổng hàng:");
        lblTongHangVal = new Label
        {
            Text = "0 ₫",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoEllipsis = false
        };
        lblGiamGia = PayLbl("Giảm giá:");
        numGiamGia = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Maximum = 999_999_999,
            DecimalPlaces = 0,
            ThousandsSeparator = true,
            Margin = new Padding(0, 6, 8, 6)
        };
        lblThanhTien = PayLbl("Thanh toán:");
        lblThanhTienVal = new Label
        {
            Text = "0 ₫",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = primary,
            AutoEllipsis = false
        };
        lblHinhThuc = PayLbl("Hình thức:");
        cboHinhThucTT = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 6, 8, 6)
        };
        cboHinhThucTT.Items.AddRange(["Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử"]);
        cboHinhThucTT.SelectedIndex = 0;

        btnLamMoi = new Button
        {
            Text = "Làm mới",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 4, 8, 4)
        };
        btnXacNhan = new Button
        {
            Text = "Xác nhận bán",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 4)
        };
        btnXacNhan.FlatAppearance.BorderSize = 0;

        tblPay.Controls.Add(lblTongHang, 0, 0);
        tblPay.Controls.Add(lblTongHangVal, 1, 0);
        tblPay.Controls.Add(lblGiamGia, 0, 1);
        tblPay.Controls.Add(numGiamGia, 1, 1);
        tblPay.Controls.Add(lblHinhThuc, 2, 1);
        tblPay.Controls.Add(cboHinhThucTT, 3, 1);
        tblPay.Controls.Add(lblThanhTien, 0, 2);
        tblPay.Controls.Add(lblThanhTienVal, 1, 2);
        tblPay.Controls.Add(btnLamMoi, 2, 2);
        tblPay.Controls.Add(btnXacNhan, 3, 2);

        pnlThanhToan.Controls.Add(tblPay);

        pnlGioHang.Controls.Add(dgvGioHang);
        pnlGioHang.Controls.Add(pnlThanhToan);
        pnlGioHang.Controls.Add(pnlGioToolbar);
        pnlGioHang.Controls.Add(lblGioHang);

        tblMain.Controls.Add(pnlTimThuoc, 0, 0);
        tblMain.Controls.Add(pnlThuoc, 0, 1);
        tblMain.Controls.Add(pnlGioHang, 0, 2);
        pnlBody.Controls.Add(tblMain);

        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Height = 24;
        lblStatus.Padding = new Padding(12, 4, 0, 0);
        lblStatus.ForeColor = muted;
        lblStatus.Text = "Sẵn sàng";

        Controls.Add(pnlBody);
        Controls.Add(pnlKhach);
        Controls.Add(lblStatus);

        pnlKhach.ResumeLayout(false);
        pnlKhach.PerformLayout();
        pnlThuoc.ResumeLayout(false);
        pnlThuoc.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvThuoc).EndInit();
        pnlGioHang.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numSoLuong).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvGioHang).EndInit();
        pnlThanhToan.ResumeLayout(false);
        pnlThanhToan.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numGiamGia).EndInit();
        tblMain.ResumeLayout(false);
        pnlBody.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel pnlKhach;
    private Label lblCccd;
    private TextBox txtCccd;
    private Button btnTraCuuKhach;
    private Button btnKhachLe;
    private Button btnTaoKhach;
    private Label lblHoTen;
    private TextBox txtHoTen;
    private Label lblSdt;
    private TextBox txtSoDienThoai;
    private Label lblDiaChi;
    private TextBox txtDiaChi;
    private Label lblNgaySinh;
    private DateTimePicker dtpNgaySinh;
    private CheckBox chkNgaySinh;
    private Label lblKhachTrangThai;
    private Panel pnlBody;
    private TableLayoutPanel tblMain;
    private Panel pnlThuoc;
    private Panel pnlTimThuoc;
    private Label lblTimThuoc;
    private TextBox txtTimThuoc;
    private Button btnTimThuoc;
    private DataGridView dgvThuoc;
    private Panel pnlGioHang;
    private Label lblGioHang;
    private NumericUpDown numSoLuong;
    private Button btnThemGio;
    private Button btnXoaDong;
    private Button btnInHoaDon;
    private Button btnXuatHoaDon;
    private DataGridView dgvGioHang;
    private Panel pnlThanhToan;
    private Label lblTongHang;
    private Label lblTongHangVal;
    private Label lblGiamGia;
    private NumericUpDown numGiamGia;
    private Label lblThanhTien;
    private Label lblThanhTienVal;
    private Label lblHinhThuc;
    private ComboBox cboHinhThucTT;
    private Button btnXacNhan;
    private Button btnLamMoi;
    private Label lblStatus;
}
