#nullable disable
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;

namespace PharmacyManagement.Forms.Report;

/// <summary>
/// Host menu "Báo cáo" (mục 7) — landing với KPI nhanh + hai thẻ điều hướng tới
/// 7a) Cảnh báo tồn / hạn (FrmCanhBaoThuoc) và 7b) Báo cáo thuốc (FrmBaoCaoThuoc).
/// </summary>
public partial class FrmBaoCao : Form
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
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly ReportService _reportService = new(new DbContextDAL());

    private Label _lblKpiTonThap;
    private Label _lblKpiSapHetHan;
    private Label _lblKpiHoaDon;
    private Label _lblKpiDoanhThu;
    private Label _lblUpdated;

    /// <summary>Bộ điều hướng nhanh sang sidebar của FrmMain. Có thể null khi mở rời.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Action<string> OnNavigateToChild { get; set; }

    public FrmBaoCao()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmBaoCao_Load;
    }

    private void FrmBaoCao_Load(object sender, EventArgs e)
    {
        TaiKpi();
    }

    private void BuildLayout()
    {
        var header = BuildHeader();
        var kpiRow = BuildKpiRow();
        var body = BuildBody();

        Controls.Add(body);
        Controls.Add(kpiRow);
        Controls.Add(header);
    }

    private Panel BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.White };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };
        header.Controls.Add(new Label
        {
            Text = "Trung tâm báo cáo",
            AutoSize = true,
            Location = new Point(28, 18),
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Ink
        });
        header.Controls.Add(new Label
        {
            Text = "Tổng quan dữ liệu nghiệp vụ — chọn nhóm báo cáo để xem chi tiết theo thời gian thực.",
            AutoSize = true,
            Location = new Point(30, 54),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted
        });

        var btnRefresh = new Button
        {
            Text = "↻ Tải lại KPI",
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Color.White,
            ForeColor = PrimaryDark,
            Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
            Size = new Size(140, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(180, 210, 182);
        btnRefresh.FlatAppearance.MouseOverBackColor = MintBg;
        btnRefresh.Click += (_, _) => TaiKpi();
        header.Controls.Add(btnRefresh);
        header.Resize += (_, _) =>
        {
            btnRefresh.Location = new Point(header.Width - btnRefresh.Width - 28, 22);
        };
        return header;
    }

    private Panel BuildKpiRow()
    {
        var wrapper = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = BgSoft, Padding = new Padding(28, 16, 28, 8) };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        for (var i = 0; i < 4; i++)
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _lblKpiTonThap = new Label();
        _lblKpiSapHetHan = new Label();
        _lblKpiHoaDon = new Label();
        _lblKpiDoanhThu = new Label();
        table.Controls.Add(BuildKpiCard("Tồn thấp", "thuốc cần nhập thêm", _lblKpiTonThap, WarnOrange), 0, 0);
        table.Controls.Add(BuildKpiCard("Sắp hết hạn", "≤ 90 ngày tới", _lblKpiSapHetHan, DangerRed), 1, 0);
        table.Controls.Add(BuildKpiCard("Hóa đơn hôm nay", "số phiếu bán đã lập", _lblKpiHoaDon, Primary), 2, 0);
        table.Controls.Add(BuildKpiCard("Doanh thu hôm nay", "tính theo ca trong ngày", _lblKpiDoanhThu, PrimaryDark), 3, 0);

        wrapper.Controls.Add(table);
        return wrapper;
    }

    private Panel BuildBody()
    {
        var body = new Panel { Dock = DockStyle.Fill, BackColor = BgSoft, Padding = new Padding(28, 4, 28, 16) };

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        split.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

        var card1 = BuildCard(
            "Cảnh báo tồn / hạn",
            "Theo dõi thuốc tồn thấp, sắp hết hạn và đã hết hạn theo dữ liệu CSDL.",
            new[]
            {
                ("Tồn thấp", "Cảnh báo khi SoLuongTon < TonToiThieu.", WarnOrange),
                ("Sắp hết hạn", "Còn ≤ 90 ngày tính đến hôm nay.", DangerRed),
                ("Hết hàng", "Số lượng tồn bằng 0.", Muted)
            },
            "Mở cảnh báo",
            navKey: "bc_canh");
        var card2 = BuildCard(
            "Báo cáo thuốc",
            "Danh mục, tồn kho, bán chạy, lịch sử nhập / bán — chỉ dành cho Admin / Quản lý.",
            new[]
            {
                ("Danh mục thuốc", "Toàn bộ thuốc, nhóm, giá, tồn, trạng thái.", Primary),
                ("Bán chạy", "Top thuốc theo số lượng và doanh thu.", PrimaryDark),
                ("Lịch sử", "Lịch sử nhập kho / bán hàng theo thời gian.", Color.FromArgb(120, 144, 156))
            },
            "Mở báo cáo",
            navKey: "bc_thuoc");

        card1.Margin = new Padding(0, 0, 12, 0);
        card2.Margin = new Padding(12, 0, 0, 0);
        split.Controls.Add(card1, 0, 0);
        split.Controls.Add(card2, 1, 0);

        _lblUpdated = new Label
        {
            Text = "Cập nhật KPI…",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Muted,
            Padding = new Padding(2, 8, 0, 0)
        };
        split.Controls.Add(_lblUpdated, 0, 1);
        split.SetColumnSpan(_lblUpdated, 2);

        body.Controls.Add(split);
        return body;
    }

    private Panel BuildKpiCard(string title, string sub, Label valueLabel, Color accent)
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6, 0, 6, 0),
            Padding = new Padding(18, 14, 18, 14),
            Cursor = Cursors.Default
        };
        p.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = RoundRect(rect, 10);
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
            Location = new Point(18, 6),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9.25F, FontStyle.Bold)
        };
        valueLabel.Text = "—";
        valueLabel.AutoSize = true;
        valueLabel.Location = new Point(18, 28);
        valueLabel.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
        valueLabel.ForeColor = Ink;

        var lblS = new Label
        {
            Text = sub,
            AutoSize = true,
            Location = new Point(18, 70),
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9F)
        };

        p.Controls.Add(lblT);
        p.Controls.Add(valueLabel);
        p.Controls.Add(lblS);
        return p;
    }

    private Panel BuildCard(string title, string subtitle, (string Heading, string Body, Color Accent)[] bullets,
        string ctaText, string navKey)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(24, 22, 24, 22)
        };
        card.Paint += (_, e) => DrawCardBorder(e.Graphics, card.ClientRectangle);

        var lblTitle = new Label
        {
            Text = title,
            AutoSize = true,
            Location = new Point(24, 22),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Ink
        };
        var lblSub = new Label
        {
            Text = subtitle,
            AutoSize = false,
            Location = new Point(24, 54),
            Size = new Size(card.Width - 64, 44),
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Muted,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        card.Controls.Add(lblTitle);
        card.Controls.Add(lblSub);

        var y = 112;
        foreach (var b in bullets)
        {
            var chip = MakeChip(b.Heading, b.Accent);
            chip.Location = new Point(28, y);
            var body = new Label
            {
                Text = b.Body,
                Location = new Point(28 + chip.Width + 12, y + 4),
                AutoSize = false,
                Size = new Size(card.Width - chip.Width - 80, 24),
                Font = new Font("Segoe UI", 9.25F),
                ForeColor = Muted,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(chip);
            card.Controls.Add(body);
            y += 40;
        }

        var btn = new Button
        {
            Text = ctaText + "  →",
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = Primary,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Size = new Size(180, 40),
            Location = new Point(24, card.Height - 72),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = PrimaryDark;
        btn.Click += (_, _) => OpenChild(navKey);
        card.Controls.Add(btn);

        return card;
    }

    private static Panel MakeChip(string text, Color accent)
    {
        var size = TextRenderer.MeasureText(text, new Font("Segoe UI", 9F, FontStyle.Bold));
        var p = new Panel
        {
            Size = new Size(size.Width + 28, 26),
            BackColor = MintBg
        };
        p.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = RoundRect(rect, 13);
            using var fill = new SolidBrush(Color.FromArgb(40, accent));
            e.Graphics.FillPath(fill, path);
            using var pen = new Pen(Color.FromArgb(80, accent), 1);
            e.Graphics.DrawPath(pen, path);
            using var dot = new SolidBrush(accent);
            e.Graphics.FillEllipse(dot, 8, p.Height / 2 - 4, 8, 8);
        };
        var lbl = new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(22, 0, 8, 0),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Ink,
            BackColor = Color.Transparent
        };
        p.Controls.Add(lbl);
        return p;
    }

    private void TaiKpi()
    {
        try
        {
            var data = _reportService.LayDashboardTongQuan();
            if (data is not null)
            {
                _lblKpiTonThap.Text = data.SoThuocTonThap.ToString("N0", Vi);
                _lblKpiTonThap.ForeColor = data.SoThuocTonThap > 0 ? WarnOrange : Primary;

                _lblKpiSapHetHan.Text = data.SoThuocSapHetHan.ToString("N0", Vi);
                _lblKpiSapHetHan.ForeColor = data.SoThuocSapHetHan > 0 ? DangerRed : Primary;

                _lblKpiHoaDon.Text = data.SoHoaDonHomNay.ToString("N0", Vi);
                _lblKpiHoaDon.ForeColor = Ink;

                _lblKpiDoanhThu.Text = data.DoanhThuHomNay.ToString("C0", Vi);
                _lblKpiDoanhThu.ForeColor = Ink;

                _lblUpdated.Text = $"Cập nhật lúc {DateTime.Now:HH:mm:ss dd/MM/yyyy} · nguồn ReportService.LayDashboardTongQuan().";
            }
            else
            {
                ResetKpi();
                _lblUpdated.Text = "Chưa có dữ liệu KPI để hiển thị.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ResetKpi();
            _lblUpdated.Text = "Tài khoản không có quyền xem KPI tổng quan.";
            _lblUpdated.ForeColor = WarnOrange;
        }
        catch (Exception ex)
        {
            ResetKpi();
            _lblUpdated.Text = "Không tải được KPI: " + ex.Message;
            _lblUpdated.ForeColor = DangerRed;
        }
    }

    private void ResetKpi()
    {
        _lblKpiTonThap.Text = "—";
        _lblKpiTonThap.ForeColor = Ink;
        _lblKpiSapHetHan.Text = "—";
        _lblKpiSapHetHan.ForeColor = Ink;
        _lblKpiHoaDon.Text = "—";
        _lblKpiHoaDon.ForeColor = Ink;
        _lblKpiDoanhThu.Text = "—";
        _lblKpiDoanhThu.ForeColor = Ink;
    }

    private void OpenChild(string navKey)
    {
        if (OnNavigateToChild is not null)
        {
            OnNavigateToChild(navKey);
            return;
        }

        MessageBox.Show(this,
            "Hãy mở mục này từ sidebar (Báo cáo → " + (navKey == "bc_canh" ? "Cảnh báo tồn / hạn" : "Báo cáo thuốc") + ").",
            Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void DrawCardBorder(Graphics g, Rectangle r)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var inner = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
        using var path = RoundRect(inner, 12);
        using var fill = new SolidBrush(Color.White);
        g.FillPath(fill, path);
        using var pen = new Pen(CardBorder, 1);
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
}
