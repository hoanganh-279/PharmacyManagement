#nullable disable
using System.Collections;
using System.Globalization;
using System.Text;
using Pharmacy.BLL;
using Pharmacy.DAL;
using Pharmacy.DTO;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Admin;

/// <summary>Nhật ký hệ thống — Admin / Quản lý qua <see cref="AuditService"/>; nguồn <c>vw_AuditLogChiTiet</c>.</summary>
public partial class FrmAuditLog : Form
{
    private readonly AuditService _audit = new(new DbContextDAL());

    private DataGridView _grid;
    private DateTimePicker _dtpTu;
    private DateTimePicker _dtpDen;
    private ComboBox _cboNguoi;
    private ComboBox _cboHanhDong;
    private TextBox _txtTuKhoa;
    private Button _btnTim;
    private Button _btnReset;
    private Button _btnXuat;
    private Label _lblPhanTrang;
    private Button _btnTruoc;
    private Button _btnSau;
    private NumericUpDown _numTrang;
    private ComboBox _cboKichThuoc;
    private Label _lblThongKe1;
    private Label _lblThongKe2;
    private Label _lblThongKe3;

    private int _tongSo;
    private bool _dangDongBoTrang;

    private sealed class HanhDongLocItem
    {
        public string HienThi { get; init; } = "";
        public string? GiaTriDb { get; init; }
        public override string ToString() => HienThi;
    }

    public FrmAuditLog()
    {
        InitializeComponent();
        BuildUi();
        Load += FrmAuditLog_Load;
    }

    private void FrmAuditLog_Load(object sender, EventArgs e)
    {
        try
        {
            NapComboHanhDong();
            NapComboNguoi();
            DatMacDinhThoiGian();
            TaiTrang();
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Bạn không có quyền xem nhật ký.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Không tải được nhật ký:\n" + TomTatLoiSql(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Hiển thị lỗi ADO/view (bao gồm InnerException) thay vì nuốt silently.</summary>
    private static string TomTatLoiSql(Exception ex)
    {
        var s = ex.Message;
        var inner = ex.InnerException;
        while (inner != null)
        {
            s += "\n→ " + inner.Message;
            inner = inner.InnerException;
        }

        if (s.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase)
            && (s.Contains("MaNhanVien", StringComparison.OrdinalIgnoreCase)
                || s.Contains("DiaChiMay", StringComparison.OrdinalIgnoreCase)))
        {
            s += "\n\nGợi ý: chạy script SQL/Migration_AuditLog_MaNhanVien_DiaChiMay.sql trên CSDL, sau đó chạy khối CREATE OR ALTER VIEW dbo.vw_AuditLogChiTiet trong SQL/View_PharmacyManagement.sql.";
        }

        return s;
    }

    private void BuildUi()
    {
        /* TableLayout tránh lỗi WinForms khi chồng Dock Top + Fill + Bottom (Fill thêm trước Bottom
           có thể khiến vùng lưới còn 0 chiều cao — chỉ thấy nền trắng của card). */
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16),
            BackColor = InventoryUiKit.PageBg
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F)); // banner + Margin dưới
        /* Khung lọc: 2 hàng (~38 + ~52) + padding card 20 → cần > 98px (nút TaoNut cao 40px + margin). */
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 114F));

        var head = new Panel { Dock = DockStyle.Fill, AutoSize = true, MinimumSize = new Size(0, 52), Padding = new Padding(0, 0, 0, 4) };
        var lblTitle = new Label
        {
            AutoSize = true,
            Location = new Point(0, 0),
            Font = new Font("Segoe UI", 14.25F, FontStyle.Bold),
            ForeColor = InventoryUiKit.PrimaryDark,
            Text = "Nhật ký hệ thống (Audit log)"
        };
        var lblSub = new Label
        {
            AutoSize = true,
            Location = new Point(0, 28),
            Font = new Font("Segoe UI", 9.25F),
            ForeColor = InventoryUiKit.Muted,
            Text = "Pharmacy Management ALN — tra cứu từ SQL Server, phân trang server-side."
        };
        _btnXuat = InventoryUiKit.TaoNut("Xuất CSV (mở bằng Excel)", InventoryUiKit.Primary);
        _btnXuat.AutoSize = true;
        _btnXuat.Click += BtnXuat_Click;
        head.Controls.Add(lblTitle);
        head.Controls.Add(lblSub);
        head.Controls.Add(_btnXuat);
        head.Layout += (_, _) =>
        {
            _btnXuat.Left = Math.Max(120, head.Width - _btnXuat.Width);
            _btnXuat.Top = 6;
        };

        var banner = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = InventoryUiKit.MintBg,
            Padding = new Padding(12, 10, 12, 8)
        };
        banner.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.25F, FontStyle.Italic),
            ForeColor = Color.FromArgb(46, 125, 50),
            Text = "Chỉ Admin / Quản lý. Dữ liệu lấy từ view vw_AuditLogChiTiet; trigger ghi nhận nhập kho, bán, đổi giá, ngừng KD, tạo hóa đơn…"
        });

        var filter = TaoKhungLoc();
        filter.Dock = DockStyle.Fill;
        filter.Margin = new Padding(0, 0, 0, 10);

        var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 10) };
        gridCard.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
        };

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            ColumnHeadersVisible = true,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Color.White,
            EnableHeadersVisualStyles = false,
            MinimumSize = new Size(0, 120),
            RowTemplate = { Height = 34 },
            DefaultCellStyle = { Font = new Font("Segoe UI", 9.25F), SelectionBackColor = Color.FromArgb(200, 230, 201), WrapMode = DataGridViewTriState.True },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 248),
                ForeColor = InventoryUiKit.Ink,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(8, 6, 8, 6)
            },
            ColumnHeadersHeight = 40,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ThoiGian", HeaderText = "Thời gian", FillWeight = 12, MinimumWidth = 145 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NhanVien", HeaderText = "Người thực hiện", FillWeight = 14, MinimumWidth = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "HanhDong", HeaderText = "Thao tác", FillWeight = 12, MinimumWidth = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhanHe", HeaderText = "Phân hệ", FillWeight = 11, MinimumWidth = 118 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NoiDung", HeaderText = "Chi tiết nội dung", FillWeight = 36, MinimumWidth = 200 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiaChiMay", HeaderText = "Địa chỉ IP / máy", FillWeight = 10, MinimumWidth = 95 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaLog", HeaderText = "Mã log", FillWeight = 5, MinimumWidth = 50, Visible = false });
        _grid.CellFormatting += Grid_CellFormatting;

        gridCard.Controls.Add(_grid);

        var foot = TaoChanTrang();
        foot.Dock = DockStyle.Fill;
        foot.Margin = new Padding(0, 6, 0, 0);

        root.Controls.Add(head, 0, 0);
        root.Controls.Add(banner, 0, 1);
        root.Controls.Add(filter, 0, 2);
        root.Controls.Add(gridCard, 0, 3);
        root.Controls.Add(foot, 0, 4);

        Controls.Add(root);
    }

    private Panel TaoKhungLoc()
    {
        var p = new Panel { BackColor = Color.White, Padding = new Padding(12, 10, 12, 12) };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 12,
            RowCount = 2,
            AutoSize = true
        };
        for (var i = 0; i < 12; i++)
            layout.ColumnStyles.Add(new ColumnStyle(i is 1 or 3 or 5 or 7 or 9 ? SizeType.Absolute : SizeType.Percent, i is 1 or 3 or 5 or 7 or 9 ? 92F : 20F));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        /* Hàng nút Lọc / Đặt lại — cao hơn hàng ô nhập vì nút InventoryUiKit.TaoNut = 40px. */
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

        _dtpTu = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Width = 110,
            Anchor = AnchorStyles.Left
        };
        _dtpDen = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Width = 110,
            Anchor = AnchorStyles.Left
        };
        _cboNguoi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        _cboHanhDong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        _txtTuKhoa = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };
        _btnTim = InventoryUiKit.TaoNut("Lọc", InventoryUiKit.Primary);
        _btnTim.AutoSize = true;
        _btnTim.Click += (_, _) => { GanTrang1(); TaiTrang(); };
        _btnReset = InventoryUiKit.TaoNut("Đặt lại", InventoryUiKit.Muted, outline: true);
        _btnReset.AutoSize = true;
        _btnReset.Click += BtnReset_Click;

        layout.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 8, 0) }, 0, 0);
        layout.Controls.Add(_dtpTu, 1, 0);
        layout.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(8, 8, 8, 0) }, 2, 0);
        layout.Controls.Add(_dtpDen, 3, 0);
        layout.Controls.Add(new Label { Text = "Người TH", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(8, 8, 8, 0) }, 4, 0);
        layout.SetColumnSpan(_cboNguoi, 3);
        layout.Controls.Add(_cboNguoi, 5, 0);
        layout.Controls.Add(new Label { Text = "Loại TH", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(8, 8, 8, 0) }, 8, 0);
        layout.SetColumnSpan(_cboHanhDong, 3);
        layout.Controls.Add(_cboHanhDong, 9, 0);

        layout.Controls.Add(new Label { Text = "Từ khóa", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 8, 0) }, 0, 1);
        layout.SetColumnSpan(_txtTuKhoa, 7);
        layout.Controls.Add(_txtTuKhoa, 1, 1);
        var btnFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Anchor = AnchorStyles.Right };
        btnFlow.Controls.Add(_btnTim);
        btnFlow.Controls.Add(_btnReset);
        layout.SetColumnSpan(btnFlow, 4);
        layout.Controls.Add(btnFlow, 8, 1);

        p.Controls.Add(layout);
        return p;
    }

    private Panel TaoChanTrang()
    {
        var p = new Panel { BackColor = Color.Transparent };
        var top = new Panel { Dock = DockStyle.Top, Height = 52 };
        var flowPg = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0, 6, 0, 10)
        };
        _lblPhanTrang = new Label { AutoSize = true, Padding = new Padding(0, 10, 16, 0), ForeColor = InventoryUiKit.Muted, Font = new Font("Segoe UI", 9.25F) };
        _btnTruoc = InventoryUiKit.TaoNut("←", InventoryUiKit.Muted, outline: true);
        _btnTruoc.Width = 44;
        _btnTruoc.Margin = new Padding(0, 6, 0, 4);
        _btnTruoc.Click += (_, _) => DoiTrang(-1);
        _numTrang = new NumericUpDown { Minimum = 1, Maximum = 1_000_000, Width = 72, Height = 36, Margin = new Padding(4, 6, 4, 0) };
        _numTrang.ValueChanged += (_, _) =>
        {
            if (_dangDongBoTrang)
                return;
            TaiTrang();
        };
        _btnSau = InventoryUiKit.TaoNut("→", InventoryUiKit.Muted, outline: true);
        _btnSau.Width = 44;
        _btnSau.Margin = new Padding(0, 6, 0, 4);
        _btnSau.Click += (_, _) => DoiTrang(1);
        _cboKichThuoc = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 110,
            Height = 36,
            Margin = new Padding(12, 6, 0, 0)
        };
        _cboKichThuoc.Items.AddRange(new object[] { "25 dòng", "50 dòng", "100 dòng" });
        _cboKichThuoc.SelectedIndex = 0;
        _cboKichThuoc.SelectedIndexChanged += (_, _) => { GanTrang1(); TaiTrang(); };

        flowPg.Controls.Add(_lblPhanTrang);
        flowPg.Controls.Add(_btnTruoc);
        flowPg.Controls.Add(_numTrang);
        flowPg.Controls.Add(_btnSau);
        flowPg.Controls.Add(new Label { Text = "    ", AutoSize = true });
        flowPg.Controls.Add(_cboKichThuoc);

        top.Controls.Add(flowPg);

        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        for (var i = 0; i < 3; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        _lblThongKe1 = TaoOThongKe("Dung lượng bảng AuditLog");
        _lblThongKe2 = TaoOThongKe("Cảnh báo nhạy cảm (24h)");
        _lblThongKe3 = TaoOThongKe("Tỷ lệ thao tác nhạy cảm (theo lọc)");
        cards.Controls.Add(WrapCard(_lblThongKe1), 0, 0);
        cards.Controls.Add(WrapCard(_lblThongKe2), 1, 0);
        cards.Controls.Add(WrapCard(_lblThongKe3), 2, 0);

        p.Controls.Add(top);
        p.Controls.Add(cards);
        return p;
    }

    private static Label TaoOThongKe(string tieuDe) => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = false,
        Font = new Font("Segoe UI", 9.25F),
        ForeColor = InventoryUiKit.Ink,
        Text = tieuDe + "\r\n—",
        Padding = new Padding(10, 8, 10, 8)
    };

    private static Panel WrapCard(Label inner)
    {
        var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0), BackColor = Color.Transparent };
        inner.Dock = DockStyle.Fill;
        pad.Controls.Add(inner);
        pad.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, pad.Width - 9), Math.Max(0, pad.Height - 1));
        };
        return pad;
    }

    private void GanTrang1()
    {
        _dangDongBoTrang = true;
        _numTrang.Value = 1;
        _dangDongBoTrang = false;
    }

    private void DoiTrang(int buoc)
    {
        var next = (int)_numTrang.Value + buoc;
        if (next < (int)_numTrang.Minimum)
            return;
        if (next > (int)_numTrang.Maximum)
            return;
        _dangDongBoTrang = true;
        _numTrang.Value = next;
        _dangDongBoTrang = false;
        TaiTrang();
    }

    private int LayKichThuocTrang() => _cboKichThuoc.SelectedIndex switch
    {
        1 => 50,
        2 => 100,
        _ => 25
    };

    private AuditLogTimKiemThamSo BuildThamSo()
    {
        int? maNv = null;
        if (_cboNguoi.SelectedItem is AuditLogNguoiTomTatDTO n && n.MaNhanVien > 0)
            maNv = n.MaNhanVien;

        string? hanh = null;
        if (_cboHanhDong.SelectedItem is HanhDongLocItem hi && !string.IsNullOrEmpty(hi.GiaTriDb))
            hanh = hi.GiaTriDb;

        var kw = _txtTuKhoa.Text?.Trim();
        if (string.IsNullOrEmpty(kw))
            kw = null;

        return new AuditLogTimKiemThamSo
        {
            TuNgay = _dtpTu.Value.Date,
            DenNgay = _dtpDen.Value.Date,
            MaNhanVien = maNv,
            HanhDong = hanh,
            TuKhoa = kw,
            Trang = (int)_numTrang.Value,
            KichThuocTrang = LayKichThuocTrang()
        };
    }

    private void TaiTrang()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            var th = BuildThamSo();
            var trang = _audit.TimPhanTrang(th);
            _tongSo = (int)Math.Min(trang.TongSoBanGhi, int.MaxValue);

            if (trang.Items.Count == 0 && _tongSo > 0 && th.Trang > 1)
            {
                GanTrang1();
                TaiTrang();
                return;
            }

            _grid.Rows.Clear();
            foreach (var x in trang.Items)
            {
                _grid.Rows.Add(
                    x.ThoiGian.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    string.IsNullOrWhiteSpace(x.NhanVien) ? "—" : x.NhanVien,
                    x.HanhDong,
                    AuditLogUiHelper.PhanHeTuBang(x.TenBang ?? "", x.HanhDong),
                    x.NoiDung ?? "",
                    string.IsNullOrWhiteSpace(x.DiaChiMay) ? "—" : x.DiaChiMay,
                    x.MaLog);
            }

            var size = LayKichThuocTrang();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(_tongSo / (double)size));

            _dangDongBoTrang = true;
            _numTrang.Maximum = tongTrang;
            if (_numTrang.Value > tongTrang)
                _numTrang.Value = tongTrang;
            _dangDongBoTrang = false;

            var hienTai = (int)_numTrang.Value;
            var tu = _tongSo == 0 ? 0 : (hienTai - 1) * size + 1;
            var den = Math.Min(hienTai * size, _tongSo);
            _lblPhanTrang.Text = _tongSo == 0
                ? "Không có bản ghi phù hợp."
                : $"Hiển thị {tu:N0}–{den:N0} trong tổng {_tongSo:N0} bản ghi.";

            _btnTruoc.Enabled = hienTai > 1;
            _btnSau.Enabled = hienTai < tongTrang;

            var tk = _audit.LayThongKeManHinh(BuildThamSo());
            _lblThongKe1.Text =
                $"Dung lượng bảng AuditLog\r\n{tk.DungLuongBangMb:N2} MB · {tk.TongBanGhiToanCuc:N0} dòng (toàn CSDL)";
            _lblThongKe2.Text =
                $"Cảnh báo nhạy cảm (24h) — đổi giá, ngừng KD, nhân sự / quyền\r\n{tk.CanhBaoNhayCam24h:N0} sự kiện";
            _lblThongKe3.Text =
                $"Tỷ lệ thao tác nhạy cảm (trong bộ lọc hiện tại)\r\n{tk.TyLeThaoTacNhayCamTrongBoLoc:N1} %";
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Bạn không có quyền xem nhật ký.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Tải trang nhật ký thất bại:\n" + TomTatLoiSql(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        if (_grid.Columns[e.ColumnIndex].Name != "HanhDong")
            return;
        var hd = e.Value?.ToString() ?? "";
        e.CellStyle.ForeColor = AuditLogUiHelper.MauChuHanhDong(hd);
        e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
    }

    private void NapComboHanhDong()
    {
        _cboHanhDong.Items.Clear();
        _cboHanhDong.Items.Add(new HanhDongLocItem { HienThi = "(Tất cả loại thao tác)", GiaTriDb = null });
        foreach (var s in _audit.LayDanhSachHanhDong())
            _cboHanhDong.Items.Add(new HanhDongLocItem { HienThi = s, GiaTriDb = s });
        _cboHanhDong.SelectedIndex = 0;
    }

    private void NapComboNguoi()
    {
        var list = new List<AuditLogNguoiTomTatDTO>
        {
            new() { MaNhanVien = 0, HoTen = "(Tất cả nhân viên trong log)" }
        };
        list.AddRange(_audit.LayNguoiTrongNhatKy());
        _cboNguoi.DisplayMember = nameof(AuditLogNguoiTomTatDTO.HoTen);
        _cboNguoi.ValueMember = nameof(AuditLogNguoiTomTatDTO.MaNhanVien);
        _cboNguoi.DataSource = list;
    }

    private void DatMacDinhThoiGian()
    {
        _dtpDen.Value = DateTime.Today;
        _dtpTu.Value = DateTime.Today.AddMonths(-1);
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        DatMacDinhThoiGian();
        _txtTuKhoa.Clear();
        if (_cboNguoi.DataSource is IList l && l.Count > 0)
            _cboNguoi.SelectedIndex = 0;
        if (_cboHanhDong.Items.Count > 0)
            _cboHanhDong.SelectedIndex = 0;
        _cboKichThuoc.SelectedIndex = 0;
        GanTrang1();
        TaiTrang();
    }

    private void BtnXuat_Click(object sender, EventArgs e)
    {
        try
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv|Tất cả (*.*)|*.*",
                FileName = $"audit_log_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var th = BuildThamSo();
            var rows = _audit.LayXuatDuLieu(th, 20_000);
            var utf8Bom = new UTF8Encoding(true);
            using var sw = new StreamWriter(dlg.FileName, false, utf8Bom);
            sw.WriteLine("ThoiGian;NguoiThucHien;ThaoTac;PhanHe;TenBang;MaBanGhi;NoiDung;DiaChiMay;MaLog");
            foreach (var x in rows)
            {
                static string Q(string? s)
                {
                    if (string.IsNullOrEmpty(s))
                        return "\"\"";
                    var t = s.Replace("\"", "\"\"", StringComparison.Ordinal);
                    return $"\"{t}\"";
                }

                sw.WriteLine(string.Join(';',
                    Q(x.ThoiGian.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
                    Q(x.NhanVien),
                    Q(x.HanhDong),
                    Q(AuditLogUiHelper.PhanHeTuBang(x.TenBang ?? "", x.HanhDong)),
                    Q(x.TenBang),
                    Q(x.MaBanGhi),
                    Q(x.NoiDung),
                    Q(x.DiaChiMay),
                    x.MaLog));
            }

            MessageBox.Show(this, $"Đã xuất {rows.Count:N0} dòng.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Không xuất được CSV:\n" + TomTatLoiSql(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
