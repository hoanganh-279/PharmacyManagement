#nullable disable
using System.Drawing.Drawing2D;
using System.Globalization;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Report;

/// <summary>
/// Báo cáo 7a — Cảnh báo tồn thấp / sắp hết hạn / đã hết hạn.
/// Đọc qua <see cref="ReportService"/> (view vw_ThuocTonThap, vw_ThuocSapHetHan).
/// Quyền: Admin, Quản lý, Kho, Dược sĩ.
/// </summary>
public partial class FrmCanhBaoThuoc : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    private static readonly Color Muted = Color.FromArgb(97, 97, 97);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);
    private static readonly Color WarnOrange = Color.FromArgb(251, 140, 0);
    private static readonly Color DangerRed = Color.FromArgb(211, 47, 47);
    private static readonly Color CardBorder = Color.FromArgb(228, 234, 229);
    private static readonly Color BgSoft = Color.FromArgb(245, 247, 246);
    private static readonly Color RowAlt = Color.FromArgb(250, 252, 250);
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly ReportService _reportService = new(new DbContextDAL());

    private Panel _header;
    private Label _lblTongTonThap;
    private Label _lblTongSapHet;
    private Label _lblTongHetHan;
    private Label _lblHintTonThap;
    private Label _lblHintSapHet;
    private Label _lblHintHetHan;

    private TabControl _tabs;
    private DataGridView _gridTonThap;
    private DataGridView _gridSapHetHan;
    private DataGridView _gridHetHan;
    private TextBox _txtSearch;
    private Label _lblStatus;
    private Button _btnRefresh;
    private Button _btnXuat;

    private IReadOnlyList<ThuocTonThapViewDTO> _dsTonThap = Array.Empty<ThuocTonThapViewDTO>();
    private List<ThuocSapHetHanViewDTO> _dsSapHet = new();
    private List<ThuocSapHetHanViewDTO> _dsHetHan = new();

    public FrmCanhBaoThuoc()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmCanhBaoThuoc_Load;
    }

    private void FrmCanhBaoThuoc_Load(object sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            SetStatus("Phiên đăng nhập không hợp lệ.", DangerRed);
            return;
        }
        TaiDuLieu();
    }

    private void BuildLayout()
    {
        // Add Controls. Lệnh BringToFront ở cuối giúp _tabs (Dock = Fill) 
        // không đè lên Header và Footer.
        BuildHeader();
        BuildFooter();
        BuildTabs();
        
        _tabs.BringToFront();
    }

    private void BuildHeader()
    {
        // Tổng chiều cao: 16 + 28(title) + 6 + 20(sub) + 8 + 34(search/btn) + 8 + KPI(~118) + 8(bottom) ≈ 246 — KPI đủ chỗ cho số 22pt + dòng hint (tránh cắt chữ dọc)
        _header = new Panel { Dock = DockStyle.Top, Height = 246, BackColor = Color.White };
        _header.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        };

        // Row 1: Title (y=16)
        var lblTitle = new Label
        {
            Text = "Cảnh báo tồn / hạn",
            AutoSize = true,
            Location = new Point(28, 16),
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Ink
        };

        // Row 2: Subtitle (y=50)
        var lblSub = new Label
        {
            Text = "Số liệu được lấy trực tiếp từ CSDL — cập nhật tại thời điểm bấm Tải lại.",
            AutoSize = true,
            Location = new Point(30, 50),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted
        };

        // Row 3: Search box + Buttons (y=78)
        _txtSearch = new TextBox
        {
            PlaceholderText = "Lọc nhanh theo tên / mã thuốc / ĐVT...",
            Width = 360,
            Height = 28,
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Location = new Point(28, 78)
        };
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        _btnRefresh = MakeOutlineButton("↻ Tải lại", (_, _) => TaiDuLieu());
        _btnRefresh.Size = new Size(120, 34);
        _btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _btnXuat = MakePrimaryButton("Xuất CSV", BtnXuat_Click);
        _btnXuat.Size = new Size(140, 34);
        _btnXuat.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Đặt nút căn phải theo chiều ngang, cùng hàng y=72 với search
        void RepositionButtons()
        {
            _btnXuat.Location = new Point(_header.Width - _btnXuat.Width - 28, 72);
            _btnRefresh.Location = new Point(_btnXuat.Left - _btnRefresh.Width - 10, 72);
        }
        _header.Resize += (_, _) => RepositionButtons();

        // Row 4: KPI cards — height đủ cho value 22pt + hint 9pt (92px trước đây làm cắt dòng hint)
        var kpiRow = new TableLayoutPanel
        {
            Location = new Point(0, 120),
            Height = 118,
            BackColor = Color.White,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(28, 4, 28, 8),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

        // Kéo giãn kpiRow khi resize form
        _header.Resize += (_, _) => kpiRow.Width = _header.Width;

        _lblTongTonThap = new Label();
        _lblTongSapHet = new Label();
        _lblTongHetHan = new Label();
        _lblHintTonThap = new Label();
        _lblHintSapHet = new Label();
        _lblHintHetHan = new Label();

        kpiRow.Controls.Add(BuildKpiCard("Tồn thấp", "SL còn < tối thiểu", _lblTongTonThap, _lblHintTonThap, WarnOrange), 0, 0);
        kpiRow.Controls.Add(BuildKpiCard("Sắp hết hạn", "còn ≤ 90 ngày", _lblTongSapHet, _lblHintSapHet, DangerRed), 1, 0);
        kpiRow.Controls.Add(BuildKpiCard("Đã hết hạn", "không được phép bán", _lblTongHetHan, _lblHintHetHan, DangerRed), 2, 0);

        _header.Controls.Add(lblTitle);
        _header.Controls.Add(lblSub);
        _header.Controls.Add(_txtSearch);
        _header.Controls.Add(_btnRefresh);
        _header.Controls.Add(_btnXuat);
        _header.Controls.Add(kpiRow);

        Controls.Add(_header);

        // Gọi một lần để đặt vị trí ban đầu cho nút
        RepositionButtons();
        kpiRow.Width = ClientSize.Width;
    }

    private void BuildTabs()
    {
        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ItemSize = new Size(210, 38),
            SizeMode = TabSizeMode.Fixed,
            Padding = new Point(16, 8)
        };

        var tab1 = new TabPage("  Tồn thấp") { BackColor = BgSoft, Padding = new Padding(16) };
        var tab2 = new TabPage("  Sắp hết hạn") { BackColor = BgSoft, Padding = new Padding(16) };
        var tab3 = new TabPage("  Đã hết hạn") { BackColor = BgSoft, Padding = new Padding(16) };
        _gridTonThap = BuildGridTonThap();
        _gridSapHetHan = BuildGridSapHetHan(showSoNgay: true);
        _gridHetHan = BuildGridSapHetHan(showSoNgay: false);
        tab1.Controls.Add(WrapGridInCard(_gridTonThap));
        tab2.Controls.Add(WrapGridInCard(_gridSapHetHan));
        tab3.Controls.Add(WrapGridInCard(_gridHetHan));

        _tabs.TabPages.Add(tab1);
        _tabs.TabPages.Add(tab2);
        _tabs.TabPages.Add(tab3);

        Controls.Add(_tabs);
    }

    private void BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };
        _lblStatus = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 0, 28, 0),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9.5F),
            Text = "Sẵn sàng."
        };
        footer.Controls.Add(_lblStatus);
        Controls.Add(footer);
    }

    private static Panel BuildKpiCard(string title, string hint, Label valueLabel, Label hintLabel, Color accent)
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6, 0, 6, 0),
            Padding = new Padding(16, 10, 16, 12)
        };
        p.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = RoundRect(rect, 8);
            using var fill = new SolidBrush(Color.White);
            e.Graphics.FillPath(fill, path);
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawPath(pen, path);
            using var bar = new SolidBrush(accent);
            e.Graphics.FillRectangle(bar, new Rectangle(0, 0, 4, p.Height));
        };

        var lblT = new Label
        {
            Text = title,
            AutoSize = true,
            Location = new Point(22, 6),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9.25F, FontStyle.Bold)
        };
        valueLabel.Text = "—";
        valueLabel.AutoSize = true;
        valueLabel.Location = new Point(22, 26);
        valueLabel.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        valueLabel.ForeColor = Ink;

        hintLabel.Text = hint;
        hintLabel.AutoSize = true;
        hintLabel.Location = new Point(22, 70);
        hintLabel.Font = new Font("Segoe UI", 9F);
        hintLabel.ForeColor = Muted;

        p.Controls.Add(lblT);
        p.Controls.Add(valueLabel);
        p.Controls.Add(hintLabel);
        return p;
    }

    private static Panel WrapGridInCard(DataGridView grid)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, card.Width - 1, card.Height - 1));
        };
        grid.Dock = DockStyle.Fill;
        card.Controls.Add(grid);
        return card;
    }

    private DataGridView BuildGridTonThap()
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaThuoc", HeaderText = "Mã", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", FillWeight = 220 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonViTinh", HeaderText = "ĐVT", FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongTon", HeaderText = "Tồn", FillWeight = 80 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TonToiThieu", HeaderText = "Tối thiểu", FillWeight = 80 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Thieu", HeaderText = "Đang thiếu", FillWeight = 80 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HanSuDung", HeaderText = "HSD", FillWeight = 100 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "Trạng thái", FillWeight = 110 });
        g.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["TonToiThieu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        g.Columns["Thieu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView BuildGridSapHetHan(bool showSoNgay)
    {
        var g = NewGrid();
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaThuoc", HeaderText = "Mã", FillWeight = 50 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenThuoc", HeaderText = "Tên thuốc", FillWeight = 220 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "DonViTinh", HeaderText = "ĐVT", FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuongTon", HeaderText = "Tồn", FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HanSuDung", HeaderText = "Hạn sử dụng", FillWeight = 110 });
        if (showSoNgay)
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoNgayConLai", HeaderText = "Còn (ngày)", FillWeight = 90 });
        else
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "QuaHan", HeaderText = "Quá hạn (ngày)", FillWeight = 110 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThaiHanDung", HeaderText = "Trạng thái", FillWeight = 130 });
        g.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        return g;
    }

    private DataGridView NewGrid()
    {
        return new DataGridView
        {
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
            RowTemplate = { Height = 42 },
            Font = new Font("Segoe UI", 9.75F),
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = RowAlt
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MintBg,
                ForeColor = Ink,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(10, 8, 10, 8),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = MintBg,
                SelectionForeColor = Ink,
                Padding = new Padding(8, 3, 8, 3),
                WrapMode = DataGridViewTriState.False
            }
        };
    }

    private void TaiDuLieu()
    {
        try
        {
            _dsTonThap = _reportService.LayThuocTonThap();
            var all = _reportService.LayThuocSapHetHan();
            _dsSapHet = all.Where(t => t.SoNgayConLai >= 0).ToList();
            _dsHetHan = all.Where(t => t.SoNgayConLai < 0).ToList();

            ApplyFilter();

            _lblTongTonThap.Text = _dsTonThap.Count.ToString("N0", Vi);
            _lblTongTonThap.ForeColor = _dsTonThap.Count > 0 ? WarnOrange : Primary;
            _lblTongSapHet.Text = _dsSapHet.Count.ToString("N0", Vi);
            _lblTongSapHet.ForeColor = _dsSapHet.Count > 0 ? DangerRed : Primary;
            _lblTongHetHan.Text = _dsHetHan.Count.ToString("N0", Vi);
            _lblTongHetHan.ForeColor = _dsHetHan.Count > 0 ? DangerRed : Primary;

            SetStatus($"Tải lúc {DateTime.Now:HH:mm:ss}: {_dsTonThap.Count} tồn thấp · {_dsSapHet.Count} sắp hết hạn · {_dsHetHan.Count} đã hết hạn.", Muted);
        }
        catch (UnauthorizedAccessException ex)
        {
            SetStatus(ex.Message, DangerRed);
        }
        catch (Exception ex)
        {
            SetStatus("Không tải được cảnh báo: " + ex.Message, DangerRed);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyFilter()
    {
        var kw = (_txtSearch.Text ?? string.Empty).Trim();

        _gridTonThap.Rows.Clear();
        foreach (var t in _dsTonThap.Where(t => MatchTonThap(t, kw)))
        {
            var thieu = Math.Max(0, t.TonToiThieu - t.SoLuongTon);
            var status = t.SoLuongTon == 0 ? "Hết hàng" : "Tồn thấp";
            var idx = _gridTonThap.Rows.Add(
                t.MaThuoc,
                t.TenThuoc,
                t.DonViTinh,
                t.SoLuongTon.ToString("N0", Vi),
                t.TonToiThieu.ToString("N0", Vi),
                thieu.ToString("N0", Vi),
                t.HanSuDung?.ToString("dd/MM/yyyy", Vi) ?? "—",
                status);
            ApplyStatusBadge(_gridTonThap.Rows[idx].Cells["TrangThai"], status);
            var tonCell = _gridTonThap.Rows[idx].Cells["SoLuongTon"];
            tonCell.Style.ForeColor = t.SoLuongTon == 0 ? DangerRed : WarnOrange;
            tonCell.Style.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            _gridTonThap.Rows[idx].Cells["Thieu"].Style.ForeColor = thieu > 0 ? DangerRed : Primary;
            _gridTonThap.Rows[idx].Cells["Thieu"].Style.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        }

        _gridSapHetHan.Rows.Clear();
        foreach (var t in _dsSapHet.Where(t => MatchSapHet(t, kw)))
        {
            var idx = _gridSapHetHan.Rows.Add(
                t.MaThuoc,
                t.TenThuoc,
                t.DonViTinh,
                t.SoLuongTon.ToString("N0", Vi),
                t.HanSuDung.ToString("dd/MM/yyyy", Vi),
                t.SoNgayConLai.ToString("N0", Vi),
                t.TrangThaiHanDung);
            ApplyStatusBadge(_gridSapHetHan.Rows[idx].Cells["TrangThaiHanDung"], t.TrangThaiHanDung);
            var color = t.SoNgayConLai <= 30 ? DangerRed : (t.SoNgayConLai <= 60 ? WarnOrange : Ink);
            var cell = _gridSapHetHan.Rows[idx].Cells["SoNgayConLai"];
            cell.Style.ForeColor = color;
            cell.Style.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        }

        _gridHetHan.Rows.Clear();
        foreach (var t in _dsHetHan.Where(t => MatchSapHet(t, kw)))
        {
            var qua = Math.Abs(t.SoNgayConLai);
            var idx = _gridHetHan.Rows.Add(
                t.MaThuoc,
                t.TenThuoc,
                t.DonViTinh,
                t.SoLuongTon.ToString("N0", Vi),
                t.HanSuDung.ToString("dd/MM/yyyy", Vi),
                qua.ToString("N0", Vi),
                t.TrangThaiHanDung);
            ApplyStatusBadge(_gridHetHan.Rows[idx].Cells["TrangThaiHanDung"], t.TrangThaiHanDung);
            var row = _gridHetHan.Rows[idx];
            row.DefaultCellStyle.ForeColor = DangerRed;
            row.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        }
    }

    private static bool MatchTonThap(ThuocTonThapViewDTO t, string kw)
    {
        if (string.IsNullOrEmpty(kw)) return true;
        return Contains(t.TenThuoc, kw)
               || Contains(t.DonViTinh, kw)
               || t.MaThuoc.ToString(CultureInfo.InvariantCulture).Contains(kw);
    }

    private static bool MatchSapHet(ThuocSapHetHanViewDTO t, string kw)
    {
        if (string.IsNullOrEmpty(kw)) return true;
        return Contains(t.TenThuoc, kw)
               || Contains(t.DonViTinh, kw)
               || t.MaThuoc.ToString(CultureInfo.InvariantCulture).Contains(kw);
    }

    private static bool Contains(string s, string kw)
        => !string.IsNullOrEmpty(s) && s.Contains(kw, StringComparison.OrdinalIgnoreCase);

    private static void ApplyStatusBadge(DataGridViewCell cell, string status)
    {
        if (string.IsNullOrEmpty(status)) return;
        Color bg, fg;
        if (status.Contains("Hết", StringComparison.OrdinalIgnoreCase))
        {
            bg = Color.FromArgb(252, 230, 230);
            fg = DangerRed;
        }
        else if (status.Contains("Sắp", StringComparison.OrdinalIgnoreCase))
        {
            bg = Color.FromArgb(255, 240, 220);
            fg = WarnOrange;
        }
        else if (status.Contains("Tồn thấp", StringComparison.OrdinalIgnoreCase))
        {
            bg = Color.FromArgb(255, 240, 220);
            fg = WarnOrange;
        }
        else
        {
            bg = MintBg;
            fg = Primary;
        }
        cell.Style.BackColor = bg;
        cell.Style.SelectionBackColor = bg;
        cell.Style.ForeColor = fg;
        cell.Style.SelectionForeColor = fg;
        cell.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private void BtnXuat_Click(object sender, EventArgs e)
    {
        var grid = _tabs.SelectedIndex switch
        {
            0 => _gridTonThap,
            1 => _gridSapHetHan,
            2 => _gridHetHan,
            _ => null
        };
        if (grid is null || grid.Rows.Count == 0)
        {
            SetStatus("Không có dữ liệu để xuất.", WarnOrange);
            return;
        }

        var name = _tabs.SelectedIndex switch
        {
            0 => "TonThap",
            1 => "SapHetHan",
            _ => "DaHetHan"
        };

        using var dlg = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"CanhBao_{name}_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            Title = "Xuất cảnh báo (CSV)"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            using var sw = new System.IO.StreamWriter(dlg.FileName, false, new System.Text.UTF8Encoding(true));
            var headers = grid.Columns.Cast<DataGridViewColumn>().Select(c => Escape(c.HeaderText));
            sw.WriteLine(string.Join(",", headers));
            foreach (DataGridViewRow row in grid.Rows)
            {
                var cells = row.Cells.Cast<DataGridViewCell>().Select(c => Escape(c.Value?.ToString() ?? ""));
                sw.WriteLine(string.Join(",", cells));
            }
            SetStatus("Đã xuất " + dlg.FileName, Primary);
        }
        catch (Exception ex)
        {
            SetStatus("Lỗi xuất file: " + ex.Message, DangerRed);
        }
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;
        var need = s.Contains(',') || s.Contains('"') || s.Contains('\n');
        var escaped = s.Replace("\"", "\"\"");
        return need ? $"\"{escaped}\"" : escaped;
    }

    private static Button MakePrimaryButton(string text, EventHandler onClick)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
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
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        b.FlatAppearance.MouseOverBackColor = MintBg;
        b.Click += onClick;
        return b;
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

    private void SetStatus(string text, Color color)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = color;
    }
}