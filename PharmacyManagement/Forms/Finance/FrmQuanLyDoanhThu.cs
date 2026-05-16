// File: PharmacyManagement/Forms/Finance/FrmQuanLyDoanhThu.cs

using Pharmacy.BLL;
using Pharmacy.DTO.Views;
using System.Drawing;

namespace PharmacyManagement.Forms.Finance
{
    public partial class FrmQuanLyDoanhThu : Form
    {
        // ── BLL ──────────────────────────────────────────────────
        private readonly DoanhThuBLL _bll = new();

        // ── cache ─────────────────────────────────────────────────
        private List<DoanhThuDTO> _cachedData = new();

        // ═════════════════════════════════════════════════════════
        public FrmQuanLyDoanhThu()
        {
            InitializeComponent();
            WireEvents();
            SetupGrid();
            dtpTuNgay.Format = DateTimePickerFormat.Custom;
            dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            dtpDenNgay.Format = DateTimePickerFormat.Custom;
            dtpDenNgay.CustomFormat = "dd/MM/yyyy";
        }

        // ── khởi chạy sau khi Form load ──────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TaiDuLieu();
        }

        // ─────────────────────────────────────────────────────────
        #region Setup
        private void WireEvents()
        {
            btnLocDuLieu.Click += (_, _) => TaiDuLieu();
            btnXuatExcel.Click += BtnXuatExcel_Click;
            txtTimKiem.TextChanged += (_, _) => LocDuLieuLocal();
            dgvDoanhThu.CellFormatting += DgvDoanhThu_CellFormatting;
        }

        private void SetupGrid()
        {
            dgvDoanhThu.EnableHeadersVisualStyles = false;

            // ================== MÀU SẮC ==================
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 168, 107);
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75f, FontStyle.Bold);
            dgvDoanhThu.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvDoanhThu.BackgroundColor = Color.FromArgb(245, 250, 245);
            dgvDoanhThu.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 245, 235);
            dgvDoanhThu.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 200, 130);
            dgvDoanhThu.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDoanhThu.GridColor = Color.FromArgb(200, 220, 200);

            // ================== CĂN CHỈNH BẢNG ==================
            dgvDoanhThu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvDoanhThu.Columns.Count > 0)
            {
                dgvDoanhThu.Columns["Khách hàng"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvDoanhThu.Columns["Nhân viên"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                dgvDoanhThu.Columns["Mã HD"].Width = 90;
                dgvDoanhThu.Columns["Ngày bán"].Width = 135;
                dgvDoanhThu.Columns["Số điện thoại"].Width = 120;
                dgvDoanhThu.Columns["Tổng tiền"].Width = 125;
                dgvDoanhThu.Columns["Giảm giá"].Width = 105;
                dgvDoanhThu.Columns["Thanh toán"].Width = 125;
                dgvDoanhThu.Columns["Lợi nhuận"].Width = 115;
                dgvDoanhThu.Columns["Trạng thái"].Width = 110;
            }

            // ================== FIX HIỂN THỊ NGÀY ==================
            dtpTuNgay.Format = DateTimePickerFormat.Custom;
            dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            dtpTuNgay.Width = 120;                    // Tăng chiều rộng

            dtpDenNgay.Format = DateTimePickerFormat.Custom;
            dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            dtpDenNgay.Width = 120;
        }
        #endregion

        // ─────────────────────────────────────────────────────────
        #region Tải & lọc dữ liệu
        private void TaiDuLieu()
        {
            try
            {
                SetStatus("Đang tải dữ liệu...");
                this.Cursor = Cursors.WaitCursor;

                string? trangThai = cboTrangThai.SelectedItem?.ToString();

                _cachedData = _bll.LayDoanhThu(
                    dtpTuNgay.Value,
                    dtpDenNgay.Value,
                    trangThai);

                BindGrid(_cachedData);
                CapNhatMetrics(_cachedData);
                SetStatus($"Hiển thị {_cachedData.Count:N0} hóa đơn | Từ {dtpTuNgay.Value:dd/MM/yyyy} đến {dtpDenNgay.Value:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Lỗi – không thể tải dữ liệu.");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void LocDuLieuLocal()
        {
            string keyword = txtTimKiem.Text.Trim();
            var ds = string.IsNullOrWhiteSpace(keyword)
                ? _cachedData
                : _bll.TimKiem(dtpTuNgay.Value, dtpDenNgay.Value, keyword);

            BindGrid(ds);
            CapNhatMetrics(ds);
            SetStatus($"Kết quả tìm: {ds.Count:N0} hóa đơn");
        }
        #endregion

        // ─────────────────────────────────────────────────────────
        #region Bind & Format grid
        private void BindGrid(List<DoanhThuDTO> ds)
        {
            dgvDoanhThu.DataSource = _bll.ToDataTable(ds);

            // Căn phải cột tiền
            foreach (DataGridViewColumn col in dgvDoanhThu.Columns)
            {
                if (col.Name is "Tổng tiền" or "Giảm giá" or "Thanh toán" or "Lợi nhuận")
                {
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.DefaultCellStyle.Format = "N0";
                }
            }

            // Tự động điều chỉnh lại sau khi bind
            dgvDoanhThu.AutoResizeColumns();
        }

        private void DgvDoanhThu_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvDoanhThu.Columns[e.ColumnIndex].Name != "Trạng thái") return;
            if (e.Value == null) return;

            string val = e.Value.ToString()!;
            e.CellStyle.ForeColor = val switch
            {
                "Đã thanh toán" => Color.FromArgb(46, 125, 50),
                "Chờ thanh toán" => Color.FromArgb(230, 119, 0),
                "Đã hủy" => Color.FromArgb(198, 40, 40),
                _ => dgvDoanhThu.DefaultCellStyle.ForeColor,
            };
            e.CellStyle.Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
        }
        #endregion

        // ─────────────────────────────────────────────────────────
        #region Metric cards
        private void CapNhatMetrics(List<DoanhThuDTO> ds)
        {
            decimal tongDT = ds.Sum(x => x.ThanhToan);
            decimal tongLN = ds.Sum(x => x.LoiNhuan);
            int tongDon = ds.Count;
            decimal tyLe = tongDT == 0 ? 0 : Math.Round(tongLN / tongDT * 100, 1);

            lblDoanhThuVal.Text = tongDT.ToString("N0") + " ₫";
            lblLoiNhuanVal.Text = tongLN.ToString("N0") + " ₫";
            lblDonHangVal.Text = tongDon.ToString("N0") + " đơn";
            lblTyLeVal.Text = tyLe.ToString("N1") + " %";
        }
        #endregion

        // ─────────────────────────────────────────────────────────
        #region Xuất Excel
        private void BtnXuatExcel_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"DoanhThu_{DateTime.Today:yyyyMMdd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                SetStatus("Đang xuất Excel...");
                var dt = _bll.ToDataTable(_cachedData);

                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.Worksheets.Add("Doanh thu");

                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    var cell = ws.Cell(1, c + 1);
                    cell.Value = dt.Columns[c].ColumnName;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(0, 168, 107);
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    cell.Style.Font.Bold = true;
                }

                for (int r = 0; r < dt.Rows.Count; r++)
                    for (int c = 0; c < dt.Columns.Count; c++)
                        ws.Cell(r + 2, c + 1).Value = ClosedXML.Excel.XLCellValue.FromObject(dt.Rows[r][c]);

                ws.Columns().AdjustToContents();
                wb.SaveAs(sfd.FileName);

                SetStatus($"Xuất Excel thành công: {sfd.FileName}");
                if (MessageBox.Show("Mở file vừa xuất?", "Thành công",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất Excel:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // ─────────────────────────────────────────────────────────
        #region Helpers
        private void SetStatus(string msg)
        {
            lblStatus.Text = $"  {msg}";
            lblStatus.Refresh();
        }
        #endregion
    }
}