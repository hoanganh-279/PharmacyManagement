// File: PharmacyManagement/Forms/Finance/FrmQuanLyDoanhThu.Designer.cs
// Auto-generated layout – đặt cùng thư mục với FrmQuanLyDoanhThu.cs

namespace PharmacyManagement.Forms.Finance
{
    partial class FrmQuanLyDoanhThu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            // ── top panel (filter bar) ──────────────────────────────
            pnlFilter = new Panel();
            lblTuNgay = new Label();
            dtpTuNgay = new DateTimePicker();
            lblDenNgay = new Label();
            dtpDenNgay = new DateTimePicker();
            lblTrangThai = new Label();
            cboTrangThai = new ComboBox();
            lblTimKiem = new Label();
            txtTimKiem = new TextBox();
            btnLocDuLieu = new Button();
            btnXuatExcel = new Button();

            // ── metric cards (FlowLayoutPanel) ─────────────────────
            pnlMetrics = new Panel();
            pnlDoanhThu = new Panel();
            lblDoanhThuTitle = new Label();
            lblDoanhThuVal = new Label();
            pnlLoiNhuan = new Panel();
            lblLoiNhuanTitle = new Label();
            lblLoiNhuanVal = new Label();
            pnlDonHang = new Panel();
            lblDonHangTitle = new Label();
            lblDonHangVal = new Label();
            pnlTyLe = new Panel();
            lblTyLeTitle = new Label();
            lblTyLeVal = new Label();

            // ── grid ───────────────────────────────────────────────
            dgvDoanhThu = new DataGridView();
            pnlStatus = new Panel();
            lblStatus = new Label();

            // ────────────────────────────────────────────────────────
            this.SuspendLayout();

            // ── Form ───────────────────────────────────────────────
            this.Text = "Quản lý doanh thu";
            this.Size = new System.Drawing.Size(1200, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.MinimumSize = new System.Drawing.Size(900, 600);

            // ── pnlFilter ──────────────────────────────────────────
            pnlFilter.Dock = DockStyle.Top;
            pnlFilter.Height = 56;
            pnlFilter.BackColor = System.Drawing.Color.White;
            pnlFilter.Padding = new Padding(12, 10, 12, 0);

            int x = 12;
            void PlaceLabel(Label l, string text, int left)
            {
                l.Text = text; l.AutoSize = true;
                l.Location = new System.Drawing.Point(left, 18);
                l.ForeColor = System.Drawing.Color.FromArgb(100, 100, 120);
            }
            void PlaceCtrl(Control c, int left, int width)
            {
                c.Location = new System.Drawing.Point(left, 12);
                c.Width = width; c.Height = 28;
            }

            PlaceLabel(lblTuNgay, "Từ ngày:", x);
            x += 60;
            dtpTuNgay.Format = DateTimePickerFormat.Short;
            dtpTuNgay.Value = DateTime.Today.AddMonths(-1);
            PlaceCtrl(dtpTuNgay, x, 110); x += 120;

            PlaceLabel(lblDenNgay, "Đến ngày:", x);
            x += 70;
            dtpDenNgay.Format = DateTimePickerFormat.Short;
            dtpDenNgay.Value = DateTime.Today;
            PlaceCtrl(dtpDenNgay, x, 110); x += 120;

            PlaceLabel(lblTrangThai, "Trạng thái:", x);
            x += 80;
            cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Đã thanh toán", "Chờ thanh toán", "Đã hủy" });
            cboTrangThai.SelectedIndex = 0;
            PlaceCtrl(cboTrangThai, x, 130); x += 140;

            PlaceLabel(lblTimKiem, "Tìm kiếm:", x);
            x += 70;
            txtTimKiem.PlaceholderText = "Mã HĐ / khách hàng / nhân viên...";
            PlaceCtrl(txtTimKiem, x, 200); x += 210;

            btnLocDuLieu.Text = "🔍  Lọc";
            btnLocDuLieu.Size = new System.Drawing.Size(80, 28);
            btnLocDuLieu.Location = new System.Drawing.Point(x, 12);
            btnLocDuLieu.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            btnLocDuLieu.ForeColor = System.Drawing.Color.White;
            btnLocDuLieu.FlatStyle = FlatStyle.Flat;
            btnLocDuLieu.FlatAppearance.BorderSize = 0;
            btnLocDuLieu.Cursor = Cursors.Hand;
            x += 90;

            btnXuatExcel.Text = "📥  Xuất Excel";
            btnXuatExcel.Size = new System.Drawing.Size(110, 28);
            btnXuatExcel.Location = new System.Drawing.Point(x, 12);
            btnXuatExcel.BackColor = System.Drawing.Color.FromArgb(76, 175, 80);
            btnXuatExcel.ForeColor = System.Drawing.Color.White;
            btnXuatExcel.FlatStyle = FlatStyle.Flat;
            btnXuatExcel.FlatAppearance.BorderSize = 0;
            btnXuatExcel.Cursor = Cursors.Hand;

            pnlFilter.Controls.AddRange(new Control[] {
                lblTuNgay, dtpTuNgay, lblDenNgay, dtpDenNgay,
                lblTrangThai, cboTrangThai, lblTimKiem, txtTimKiem,
                btnLocDuLieu, btnXuatExcel
            });

            // ── pnlMetrics ─────────────────────────────────────────
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 96;
            pnlMetrics.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            pnlMetrics.Padding = new Padding(12, 8, 12, 8);

            void MakeCard(Panel card, Label title, Label val,
                          string titleText, System.Drawing.Color accent, int cardX)
            {
                card.Size = new System.Drawing.Size(240, 76);
                card.Location = new System.Drawing.Point(cardX, 8);
                card.BackColor = System.Drawing.Color.White;
                card.Padding = new Padding(14, 10, 14, 10);

                // left accent bar
                var bar = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = accent
                };

                title.Text = titleText;
                title.Dock = DockStyle.Top;
                title.Font = new System.Drawing.Font("Segoe UI", 8f);
                title.ForeColor = System.Drawing.Color.FromArgb(130, 130, 150);
                title.Height = 18;

                val.Text = "0 ₫";
                val.Dock = DockStyle.Fill;
                val.Font = new System.Drawing.Font("Segoe UI Semibold", 14f, System.Drawing.FontStyle.Bold);
                val.ForeColor = accent;
                val.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

                card.Controls.Add(val);
                card.Controls.Add(title);
                card.Controls.Add(bar);
            }

            MakeCard(pnlDoanhThu, lblDoanhThuTitle, lblDoanhThuVal,
                     "DOANH THU",
                     System.Drawing.Color.FromArgb(33, 150, 243), 12);
            MakeCard(pnlLoiNhuan, lblLoiNhuanTitle, lblLoiNhuanVal,
                     "LỢI NHUẬN",
                     System.Drawing.Color.FromArgb(76, 175, 80), 264);
            MakeCard(pnlDonHang, lblDonHangTitle, lblDonHangVal,
                     "ĐƠN HÀNG",
                     System.Drawing.Color.FromArgb(255, 152, 0), 516);
            MakeCard(pnlTyLe, lblTyLeTitle, lblTyLeVal,
                     "TỶ LỆ LỢI NHUẬN",
                     System.Drawing.Color.FromArgb(156, 39, 176), 768);

            pnlMetrics.Controls.AddRange(new Control[] {
                pnlDoanhThu, pnlLoiNhuan, pnlDonHang, pnlTyLe
            });

            // ── dgvDoanhThu ────────────────────────────────────────
            dgvDoanhThu.Dock = DockStyle.Fill;
            dgvDoanhThu.ReadOnly = true;
            dgvDoanhThu.AllowUserToAddRows = false;
            dgvDoanhThu.AllowUserToDeleteRows = false;
            dgvDoanhThu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDoanhThu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDoanhThu.BackgroundColor = System.Drawing.Color.White;
            dgvDoanhThu.BorderStyle = BorderStyle.None;
            dgvDoanhThu.RowHeadersVisible = false;
            dgvDoanhThu.GridColor = System.Drawing.Color.FromArgb(230, 230, 240);
            // header style
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold);
            dgvDoanhThu.ColumnHeadersHeight = 36;
            dgvDoanhThu.EnableHeadersVisualStyles = false;
            // alternating row
            dgvDoanhThu.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 255);
            dgvDoanhThu.RowTemplate.Height = 30;

            // ── status bar ─────────────────────────────────────────
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Height = 28;
            pnlStatus.BackColor = System.Drawing.Color.FromArgb(33, 150, 243);

            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Text = "Sẵn sàng";
            lblStatus.ForeColor = System.Drawing.Color.White;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(10, 0, 0, 0);
            pnlStatus.Controls.Add(lblStatus);

            // ── wire up ────────────────────────────────────────────
            this.Controls.Add(dgvDoanhThu);
            this.Controls.Add(pnlMetrics);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlStatus);

            this.ResumeLayout(false);
        }
        #endregion

        // ── control declarations ───────────────────────────────────
        private Panel pnlFilter, pnlMetrics, pnlStatus;
        private Panel pnlDoanhThu, pnlLoiNhuan, pnlDonHang, pnlTyLe;
        private Label lblTuNgay, lblDenNgay, lblTrangThai, lblTimKiem;
        private Label lblDoanhThuTitle, lblDoanhThuVal;
        private Label lblLoiNhuanTitle, lblLoiNhuanVal;
        private Label lblDonHangTitle, lblDonHangVal;
        private Label lblTyLeTitle, lblTyLeVal;
        private Label lblStatus;
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private ComboBox cboTrangThai;
        private TextBox txtTimKiem;
        private Button btnLocDuLieu, btnXuatExcel;
        private DataGridView dgvDoanhThu;
    }
}