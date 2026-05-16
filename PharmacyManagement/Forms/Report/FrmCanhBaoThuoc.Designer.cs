namespace PharmacyManagement.Forms.Report;

partial class FrmCanhBaoThuoc
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
        pnlToolbar = new FlowLayoutPanel();
        lblTitle = new Label();
        btnHetHang = new Button();
        btnHetHan = new Button();
        btnLamMoi = new Button();
        btnThoat = new Button();
        pnlLegend = new Panel();
        pnlGridHost = new Panel();
        dgvCanhBao = new DataGridView();
        pnlStatus = new Panel();
        lblTrangThai = new Label();
        lblSoLuong = new Label();

        var primary = Color.FromArgb(46, 125, 50);
        var primaryDark = Color.FromArgb(27, 94, 32);
        var mint = Color.FromArgb(232, 245, 233);
        var pageBg = Color.FromArgb(245, 247, 246);
        var ink1 = Color.FromArgb(33, 37, 41);
        var muted = Color.FromArgb(97, 97, 97);

        pnlToolbar.SuspendLayout();
        pnlLegend.SuspendLayout();
        pnlGridHost.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvCanhBao).BeginInit();
        pnlStatus.SuspendLayout();
        SuspendLayout();

        // ── Toolbar (Giữ nguyên cấu trúc - Nới rộng bề ngang các nút bấm) ──
        pnlToolbar.Dock = DockStyle.Top;
        pnlToolbar.Height = 60;
        pnlToolbar.BackColor = Color.White;
        pnlToolbar.Padding = new Padding(16, 12, 16, 8);
        ((FlowLayoutPanel)pnlToolbar).FlowDirection = FlowDirection.LeftToRight;
        ((FlowLayoutPanel)pnlToolbar).WrapContents = false;

        lblTitle.AutoSize = true;
        lblTitle.Text = "Cảnh báo tồn / hạn dùng";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = primaryDark;
        lblTitle.Margin = new Padding(0, 4, 35, 0); // Tăng margin phải để chống đè chữ tuyệt đối

        btnHetHang.Text = "Tồn thấp / hết hàng";
        btnHetHang.Size = new Size(180, 34); // Nới rộng từ 168 lên 180px để chữ hiển thị thoải mái
        btnHetHang.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnHetHang.FlatStyle = FlatStyle.Flat;
        btnHetHang.Cursor = Cursors.Hand;
        btnHetHang.BackColor = primary;
        btnHetHang.ForeColor = Color.White;
        btnHetHang.FlatAppearance.BorderSize = 0;
        btnHetHang.Margin = new Padding(0, 0, 10, 0);

        btnHetHan.Text = "Sắp / đã hết hạn";
        btnHetHan.Size = new Size(165, 34); // Nới rộng từ 158 lên 165px tránh co chữ "hạn"
        btnHetHan.Font = new Font("Segoe UI", 9.75F);
        btnHetHan.FlatStyle = FlatStyle.Flat;
        btnHetHan.Cursor = Cursors.Hand;
        btnHetHan.BackColor = Color.White;
        btnHetHan.ForeColor = primaryDark;
        btnHetHan.FlatAppearance.BorderColor = Color.FromArgb(200, 220, 202);
        btnHetHan.FlatAppearance.BorderSize = 1;
        btnHetHan.Margin = new Padding(0, 0, 10, 0);

        btnLamMoi.Text = "Làm mới";
        btnLamMoi.Size = new Size(95, 34); // Nới rộng từ 88 lên 95px giúp text thoáng hơn
        btnLamMoi.Font = new Font("Segoe UI", 9.75F);
        btnLamMoi.FlatStyle = FlatStyle.Flat;
        btnLamMoi.Cursor = Cursors.Hand;
        btnLamMoi.BackColor = mint;
        btnLamMoi.ForeColor = primaryDark;
        btnLamMoi.FlatAppearance.BorderColor = primary;
        btnLamMoi.FlatAppearance.BorderSize = 1;
        btnLamMoi.Margin = new Padding(150, 0, 10, 0);

        btnThoat.Text = "Đóng";
        btnThoat.Size = new Size(76, 34); // Nới rộng nhẹ từ 72 lên 76px
        btnThoat.Font = new Font("Segoe UI", 9.75F);
        btnThoat.FlatStyle = FlatStyle.Flat;
        btnThoat.Cursor = Cursors.Hand;
        btnThoat.BackColor = Color.White;
        btnThoat.ForeColor = muted;
        btnThoat.FlatAppearance.BorderColor = Color.FromArgb(220, 224, 220);
        btnThoat.FlatAppearance.BorderSize = 1;
        btnThoat.Margin = new Padding(0, 0, 0, 0);

        pnlToolbar.Controls.AddRange([lblTitle, btnHetHang, btnHetHan, btnLamMoi, btnThoat]);

        pnlToolbar.Resize += (s, _) =>
        {
            if (s is Control ctrl)
            {
                // Thay hằng số trừ từ 90 lên 110 để bù trừ cho khoảng nới rộng của các nút ở trên
                int dongKhongGian = ctrl.Width - lblTitle.Width - btnHetHang.Width - btnHetHan.Width - btnLamMoi.Width - btnThoat.Width - 110;
                btnLamMoi.Margin = new Padding(Math.Max(10, dongKhongGian), 0, 10, 0);
            }
        };

        // ── Chú thích màu (Giữ nguyên cấu trúc - Tăng nhẹ chiều cao) ──
        pnlLegend.Dock = DockStyle.Top;
        pnlLegend.Height = 40; // Nâng lên 40px để các dấu tiếng Việt không bị sát mép panel
        pnlLegend.BackColor = mint;
        pnlLegend.Padding = new Padding(12, 6, 12, 6);

        void AddLegendItem(FlowLayoutPanel flow, string text, Color dotColor)
        {
            var wrap = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 32, 0),
                BackColor = Color.Transparent
            };
            wrap.Controls.Add(new Panel { Size = new Size(12, 12), BackColor = dotColor, Margin = new Padding(0, 5, 8, 0) });
            wrap.Controls.Add(new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9.5F), // Tăng từ 9F lên 9.5F để nhìn rõ ràng hơn
                ForeColor = ink1,
                Margin = new Padding(0, 2, 0, 0)
            });
            flow.Controls.Add(wrap);
        }

        var flowLegend = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = mint,
            Padding = new Padding(8, 2, 8, 0)
        };
        AddLegendItem(flowLegend, "Hết hàng / hết hạn", Color.FromArgb(211, 47, 47));
        AddLegendItem(flowLegend, "Sắp hết hạn (≤ 30 ngày)", Color.FromArgb(251, 140, 0));
        AddLegendItem(flowLegend, "Tồn thấp / còn hạn dài", Color.FromArgb(46, 125, 50));
        pnlLegend.Controls.Add(flowLegend);

        // ── Lưới dữ liệu (Nới rộng chiều cao tiêu đề cột chống che dấu tiếng Việt) ──
        pnlGridHost.Dock = DockStyle.Fill;
        pnlGridHost.BackColor = pageBg;
        pnlGridHost.Padding = new Padding(12, 8, 12, 8);

        dgvCanhBao.Dock = DockStyle.Fill;
        dgvCanhBao.AllowUserToAddRows = false;
        dgvCanhBao.AllowUserToDeleteRows = false;
        dgvCanhBao.ReadOnly = true;
        dgvCanhBao.RowHeadersVisible = false;
        dgvCanhBao.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCanhBao.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCanhBao.BorderStyle = BorderStyle.None;
        dgvCanhBao.BackgroundColor = Color.White;
        dgvCanhBao.GridColor = Color.FromArgb(230, 236, 231);
        dgvCanhBao.ColumnHeadersHeight = 42; // Tăng lên 42px để các từ có dấu nặng/hỏi ở Header (như Trạng thái) thoải mái tuyệt đối
        dgvCanhBao.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvCanhBao.RowTemplate.Height = 34; // Tăng từ 32 lên 34px cho dữ liệu thênh thang
        dgvCanhBao.Font = new Font("Segoe UI", 10F);
        dgvCanhBao.EnableHeadersVisualStyles = false;

        dgvCanhBao.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        dgvCanhBao.ColumnHeadersDefaultCellStyle.BackColor = primary;
        dgvCanhBao.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvCanhBao.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvCanhBao.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

        dgvCanhBao.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0); // Tăng padding lọt lòng của ô dữ liệu giúp cột không bị ép chữ
        dgvCanhBao.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 201);
        dgvCanhBao.DefaultCellStyle.SelectionForeColor = primaryDark;
        dgvCanhBao.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 250);

        pnlGridHost.Controls.Add(dgvCanhBao);

        // ── Status Bar (Sửa triệt để lỗi biến thành "0..." khi số lượng dòng lớn) ──
        pnlStatus.Dock = DockStyle.Bottom;
        pnlStatus.Height = 36; // Tăng từ 34 lên 36px để text vùng đáy không chạm biên dưới
        pnlStatus.BackColor = Color.White;
        pnlStatus.Padding = new Padding(12, 0, 16, 0);

        lblTrangThai.AutoSize = true;
        lblTrangThai.Font = new Font("Segoe UI", 9.5F);
        lblTrangThai.ForeColor = muted;
        lblTrangThai.Location = new Point(12, 9);
        lblTrangThai.Text = "Đang hiển thị: ...";
        lblTrangThai.Anchor = AnchorStyles.Left | AnchorStyles.Top;

        lblSoLuong.AutoSize = false;
        lblSoLuong.Size = new Size(260, 22); // Nới rộng từ 220 lên 260px để chứa thoải mái các chuỗi số cực dài
        lblSoLuong.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblSoLuong.ForeColor = primaryDark;
        lblSoLuong.Location = new Point(824, 9); // Dịch tọa độ X sang trái tương ứng (1100 - 260 - 16px padding = 824) để nhường không gian cho Size mới
        lblSoLuong.Text = "Tổng: 0 dòng";
        lblSoLuong.TextAlign = ContentAlignment.TopRight;
        lblSoLuong.Anchor = AnchorStyles.Right | AnchorStyles.Top;

        pnlStatus.Controls.Add(lblTrangThai);
        pnlStatus.Controls.Add(lblSoLuong);

        // ── Form Main Layout ──
        AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.None;
        this.DoubleBuffered = true;
        BackColor = pageBg;
        ClientSize = new Size(1100, 650);
        Font = new Font("Segoe UI", 9.75F);
        MinimumSize = new Size(950, 500);
        Name = "FrmCanhBaoThuoc";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Cảnh báo thuốc";

        Controls.Add(pnlGridHost);
        Controls.Add(pnlLegend);
        Controls.Add(pnlToolbar);
        Controls.Add(pnlStatus);

        pnlToolbar.ResumeLayout(false);
        pnlToolbar.PerformLayout();
        pnlLegend.ResumeLayout(false);
        pnlGridHost.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvCanhBao).EndInit();
        pnlStatus.ResumeLayout(false);
        pnlStatus.PerformLayout();
        ResumeLayout(false);
    }

    private Control pnlToolbar;
    private Label lblTitle;
    private Button btnHetHan;
    private Button btnHetHang;
    private Button btnLamMoi;
    private Button btnThoat;
    private Panel pnlLegend;
    private Panel pnlGridHost;
    private DataGridView dgvCanhBao;
    private Panel pnlStatus;
    private Label lblTrangThai;
    private Label lblSoLuong;
}