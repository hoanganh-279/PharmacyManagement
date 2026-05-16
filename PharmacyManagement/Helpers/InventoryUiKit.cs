#nullable disable
using System.Drawing.Drawing2D;
using Pharmacy.DTO;

namespace PharmacyManagement.Helpers;

/// <summary>Màu và control dùng chung cho màn nhập kho (khớp project_Context §3.2).</summary>
internal static class InventoryUiKit
{
    public static readonly Color Primary = Color.FromArgb(46, 125, 50);
    public static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    public static readonly Color MintBg = Color.FromArgb(232, 245, 233);
    public static readonly Color Muted = Color.FromArgb(97, 97, 97);
    public static readonly Color Ink = Color.FromArgb(33, 37, 41);
    public static readonly Color CardBorder = Color.FromArgb(220, 230, 222);
    public static readonly Color Warn = Color.FromArgb(251, 140, 0);
    public static readonly Color Danger = Color.FromArgb(211, 47, 47);
    public static readonly Color PageBg = Color.FromArgb(245, 247, 246);

    /// <summary>Thẻ trắng viền xám (tự co chiều cao khi không chỉ định height).</summary>
    public static Panel TaoCard(string title, int? height = null)
    {
        var autoHeight = !height.HasValue;
        var card = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(20, 16, 20, 16),
            AutoSize = autoHeight,
            AutoSizeMode = autoHeight ? AutoSizeMode.GrowAndShrink : AutoSizeMode.GrowOnly
        };
        if (height.HasValue)
            card.Height = height.Value;
        VeVienCard(card);
        if (!string.IsNullOrWhiteSpace(title))
        {
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryDark
            });
        }
        return card;
    }

    /// <summary>Thẻ có vùng <paramref name="body"/> Dock Fill — thêm control vào body, không add thẳng lên card.</summary>
    public static (Panel Body, Panel Card) TaoCardVoiThan(string title, int? height = null)
    {
        var card = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(20, 16, 20, 16),
            AutoSize = false
        };
        if (height.HasValue)
            card.Height = height.Value;

        VeVienCard(card);

        var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, MinimumSize = new Size(0, 200) };
        card.Controls.Add(body);

        if (!string.IsNullOrWhiteSpace(title))
        {
            var lbl = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryDark,
                Margin = new Padding(0, 0, 0, 4)
            };
            card.Controls.Add(lbl);
        }

        return (body, card);
    }

    private static void VeVienCard(Panel card)
    {
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
    }

    /// <summary>Lưới form 2 cặp nhãn–ô (4 cột), Dock Fill — không dùng AutoSize.</summary>
    public static TableLayoutPanel TaoLuoiForm4Cot(int labelWidth = 152)
    {
        var g = new TableLayoutPanel
        {
            ColumnCount = 4,
            RowCount = 0,
            Dock = DockStyle.Fill,
            AutoSize = false,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Padding = new Padding(0, 4, 0, 0)
        };
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        return g;
    }

    /// <summary>Lưới form 2 cột (nhãn + ô).</summary>
    public static TableLayoutPanel TaoLuoiForm2Cot(int labelWidth = 152)
    {
        var g = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 0,
            Dock = DockStyle.Fill,
            AutoSize = false,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Padding = new Padding(0, 4, 0, 0)
        };
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        return g;
    }

    /// <summary>Thêm một hàng vào lưới 4 cột; trả về chỉ số hàng kế tiếp.</summary>
    public static int ThemHangForm4(
        TableLayoutPanel grid, int row,
        string label1, Control field1, bool req1,
        string label2, Control field2,
        int rowHeight = 40)
    {
        DamBaoHang(grid, row, rowHeight);
        var lbl1 = TaoFieldLabel(label1, req1);
        lbl1.Anchor = AnchorStyles.Left;
        lbl1.AutoSize = true;
        lbl1.MaximumSize = new Size(148, 36);
        lbl1.Margin = new Padding(0, 10, 8, 0);
        field1.Dock = DockStyle.Fill;
        field1.Margin = new Padding(0, 6, 14, 6);
        field1.MinimumSize = new Size(80, 28);
        grid.Controls.Add(lbl1, 0, row);
        grid.Controls.Add(field1, 1, row);

        var lbl2 = TaoFieldLabel(label2, false);
        lbl2.Anchor = AnchorStyles.Left;
        lbl2.AutoSize = true;
        lbl2.MaximumSize = new Size(148, 36);
        lbl2.Margin = new Padding(0, 10, 8, 0);
        field2.Dock = DockStyle.Fill;
        field2.Margin = new Padding(0, 6, 0, 6);
        field2.MinimumSize = new Size(80, 28);
        grid.Controls.Add(lbl2, 2, row);
        grid.Controls.Add(field2, 3, row);
        return row + 1;
    }

    /// <summary>Thêm một hàng vào lưới 2 cột.</summary>
    public static int ThemHangForm2(TableLayoutPanel grid, int row, string label, Control field, bool required = false, int rowHeight = 40)
    {
        DamBaoHang(grid, row, rowHeight);
        var lbl = TaoFieldLabel(label, required);
        lbl.Anchor = AnchorStyles.Left;
        lbl.AutoSize = true;
        lbl.MaximumSize = new Size(148, 36);
        lbl.Margin = new Padding(0, 10, 8, 0);
        field.Dock = DockStyle.Fill;
        field.Margin = new Padding(0, 6, 0, 6);
        field.MinimumSize = new Size(80, 28);
        grid.Controls.Add(lbl, 0, row);
        grid.Controls.Add(field, 1, row);
        return row + 1;
    }

    public static void DamBaoHang(TableLayoutPanel grid, int row, int rowHeight)
    {
        while (grid.RowStyles.Count <= row)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        if (row < grid.RowStyles.Count)
            grid.RowStyles[row] = new RowStyle(SizeType.Absolute, rowHeight);
        if (grid.RowCount <= row)
            grid.RowCount = row + 1;
    }

    /// <summary>Khung trang nhập kho: stepper + nội dung (Fill) + thanh tác (AutoSize).</summary>
    public static TableLayoutPanel TaoKhungTrang(int buocStepper, Control noiDung, Control thanhTac = null, bool coFooterTrangThai = false)
    {
        var rowCount = 2 + (thanhTac != null ? 1 : 0) + (coFooterTrangThai ? 1 : 0);
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = PageBg,
            ColumnCount = 1,
            RowCount = rowCount,
            Padding = new Padding(12)
        };

        var ri = 0;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.Controls.Add(TaoStepper(buocStepper), 0, ri++);

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        noiDung.Dock = DockStyle.Fill;
        root.Controls.Add(noiDung, 0, ri++);

        if (thanhTac is not null)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            thanhTac.Dock = DockStyle.Fill;
            root.Controls.Add(thanhTac, 0, ri++);
        }

        if (coFooterTrangThai)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(TaoStatusFooter(), 0, ri);
        }

        return root;
    }

    /// <summary>Panel cuộn + card — card full ngang, cao tối thiểu bằng vùng nhìn thấy.</summary>
    public static Panel TaoVungCuonVoiCard(string title, out Panel body)
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = PageBg };
        (body, var card) = TaoCardVoiThan(title);
        GanCardVaoScroll(scroll, card);
        return scroll;
    }

    /// <summary>Đặt chiều cao lưới form theo tổng các hàng Absolute (dùng trong vùng cuộn).</summary>
    public static void DatChieuCaoLuoiCoDinh(TableLayoutPanel grid)
    {
        grid.Dock = DockStyle.Top;
        var h = grid.Padding.Vertical;
        for (var i = 0; i < grid.RowStyles.Count; i++)
        {
            var rs = grid.RowStyles[i];
            h += rs.SizeType == SizeType.Absolute ? (int)Math.Ceiling(rs.Height) : 40;
        }
        grid.Height = Math.Max(h + 4, 80);
    }

    /// <summary>Gắn card vào panel cuộn — full ngang; cao = max(viewport, nội dung thực).</summary>
    public static void GanCardVaoScroll(Panel scroll, Panel card, int padding = 12)
    {
        scroll.Padding = new Padding(padding);
        card.Dock = DockStyle.None;
        card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        card.Location = new Point(padding, padding);
        scroll.Controls.Add(card);

        void Sync()
        {
            var w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            if (w > 0)
                card.Width = w;

            var minH = scroll.ClientSize.Height - scroll.Padding.Vertical;
            var contentH = TinhChieuCaoCard(card);
            card.Height = Math.Max(minH > 0 ? minH : contentH, contentH);
        }

        scroll.Resize += (_, _) => Sync();
        card.ControlAdded += (_, _) => Sync();
        foreach (Control c in card.Controls)
            GanLangNgheThayDoi(c, Sync);
        Sync();
    }

    private static void GanLangNgheThayDoi(Control control, Action sync)
    {
        control.SizeChanged += (_, _) => sync();
        if (control is not Panel panel)
            return;
        foreach (Control ch in panel.Controls)
            GanLangNgheThayDoi(ch, sync);
        panel.ControlAdded += (_, e) => GanLangNgheThayDoi(e.Control, sync);
    }

    private static int TinhChieuCaoCard(Panel card)
    {
        var h = card.Padding.Vertical;
        Panel body = null;
        foreach (Control c in card.Controls)
        {
            if (c.Dock == DockStyle.Top)
                h += c.Height + c.Margin.Vertical;
            else if (c is Panel p && p.Dock == DockStyle.Fill)
                body = p;
        }

        if (body is null)
            return h + 8;

        var inner = body.Padding.Vertical;
        foreach (Control ch in body.Controls)
            inner += ch.Height + ch.Margin.Vertical + ch.Padding.Vertical;
        return h + inner + 8;
    }

    public static Label TaoLabel(string text, bool required = false)
    {
        var t = required ? text + " *" : text;
        return new Label
        {
            AutoSize = true,
            Text = t,
            ForeColor = required ? Danger : Muted,
            Font = new Font("Segoe UI", 9F),
            Margin = new Padding(0, 6, 8, 4)
        };
    }

    /// <summary>Nhãn trường form — AutoSize, một dòng; dấu * cho trường bắt buộc.</summary>
    public static Label TaoFieldLabel(string text, bool required = false)
    {
        var t = required ? text + " *" : text;
        return new Label
        {
            AutoSize = true,
            Text = t,
            ForeColor = required ? Danger : Muted,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static TextBox TaoTextBox(bool multiline = false, string placeholder = null)
    {
        var tb = new TextBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5F)
        };
        if (!string.IsNullOrEmpty(placeholder))
            tb.PlaceholderText = placeholder;
        if (multiline)
        {
            tb.Multiline = true;
            tb.Height = 72;
            tb.ScrollBars = ScrollBars.Vertical;
        }
        return tb;
    }

    public static ComboBox TaoCombo(bool dropDownList = true)
    {
        return new ComboBox
        {
            DropDownStyle = dropDownList ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown,
            FlatStyle = FlatStyle.Standard,
            IntegralHeight = false,
            Font = new Font("Segoe UI", 9.5F)
        };
    }

    public static Button TaoNut(string text, Color back, Color? fore = null, bool outline = false)
    {
        const int h = 40;
        var font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Height = h,
            Margin = new Padding(6, 4, 6, 4),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = font,
            Padding = new Padding(18, 8, 18, 8),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = outline ? Color.White : back,
            ForeColor = fore ?? (outline ? back : Color.White),
            UseCompatibleTextRendering = true
        };
        b.FlatAppearance.BorderColor = back;
        b.FlatAppearance.BorderSize = outline ? 1 : 0;
        // Khóa chiều cao, giữ chiều ngang do AutoSize (tránh cắt chữ khi DPI/scale).
        var w = Math.Max(100, b.PreferredSize.Width + 12);
        b.AutoSize = false;
        b.Size = new Size(w, h);
        b.MinimumSize = new Size(w, h);
        return b;
    }

    /// <summary>Căn chỉnh ComboBox để không cắt chữ trong ô và danh sách xổ.</summary>
    public static void MoRongCombo(ComboBox cbo)
    {
        if (cbo.IsDisposed)
            return;
        var max = cbo.Width;
        using var g = cbo.CreateGraphics();
        foreach (var item in cbo.Items)
        {
            var text = item switch
            {
                null => "",
                IdTenDTO dto => dto.Ten,
                _ => item.ToString() ?? ""
            };
            if (string.IsNullOrEmpty(text))
                continue;
            var w = (int)Math.Ceiling(g.MeasureString(text, cbo.Font).Width) + 28;
            if (w > max)
                max = w;
        }
        cbo.DropDownWidth = Math.Max(cbo.Width, max);
    }

    /// <summary>Hàng nút căn phải; tự xuống dòng khi hẹp — chiều cao host co theo số dòng nút.</summary>
    public static Panel TaoActionBar(params Button[] buttons)
    {
        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(8, 6, 8, 6),
            Margin = new Padding(0),
            BackColor = Color.Transparent
        };
        foreach (var btn in buttons)
            flow.Controls.Add(btn);

        var host = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            MinimumSize = new Size(0, 48),
            Padding = new Padding(0, 4, 0, 4)
        };
        host.Controls.Add(flow);

        void Align()
        {
            flow.Left = Math.Max(0, host.ClientSize.Width - flow.Width);
            flow.Top = Math.Max(0, (host.ClientSize.Height - flow.Height) / 2);
            var needH = flow.Height + host.Padding.Vertical;
            if (host.Height != needH && needH >= host.MinimumSize.Height)
                host.Height = needH;
        }

        host.Resize += (_, _) => Align();
        flow.SizeChanged += (_, _) => Align();
        host.HandleCreated += (_, _) => Align();
        return host;
    }

    /// <summary>Footer tổng kết: thống kê + nút — xếp dọc khi hẹp (&lt;720px).</summary>
    public static Panel TaoFooterTongKet(Control stats, Control actions, Color? nen = null)
    {
        var bar = new Panel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = nen ?? Color.White,
            Padding = new Padding(12, 10, 12, 10),
            MinimumSize = new Size(0, 72)
        };
        bar.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
        };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
        stats.Dock = DockStyle.Fill;
        stats.Margin = new Padding(0, 0, 8, 0);
        actions.Dock = DockStyle.Fill;
        tbl.Controls.Add(stats, 0, 0);
        tbl.Controls.Add(actions, 1, 0);
        bar.Controls.Add(tbl);

        const int stackBreakpoint = 720;
        void LayoutFooter()
        {
            var stack = bar.Width > 0 && bar.Width < stackBreakpoint;
            tbl.SuspendLayout();
            tbl.Controls.Clear();
            tbl.ColumnStyles.Clear();
            tbl.RowStyles.Clear();

            if (stack)
            {
                tbl.ColumnCount = 1;
                tbl.RowCount = 2;
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                stats.Margin = new Padding(0, 0, 0, 8);
                tbl.Controls.Add(stats, 0, 0);
                tbl.Controls.Add(actions, 0, 1);
            }
            else
            {
                tbl.ColumnCount = 2;
                tbl.RowCount = 1;
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                stats.Margin = new Padding(0, 0, 8, 0);
                tbl.Controls.Add(stats, 0, 0);
                tbl.Controls.Add(actions, 1, 0);
            }

            tbl.ResumeLayout(true);
        }

        bar.Resize += (_, _) => LayoutFooter();
        bar.HandleCreated += (_, _) => LayoutFooter();
        return bar;
    }

    /// <summary>Thanh tìm kiếm + nút — ô tìm Fill; tùy chọn nút Xóa và control bên phải.</summary>
    public static Panel TaoThanhTimKiem(TextBox txt, Button btnTim, Control optionalRight = null, Button btnXoa = null)
    {
        var colCount = 2 + (btnXoa is not null ? 1 : 0) + (optionalRight is not null ? 1 : 0);
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = colCount,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 8)
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        if (btnXoa is not null)
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        if (optionalRight is not null)
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        txt.Dock = DockStyle.Fill;
        txt.Margin = new Padding(0, 4, 12, 4);
        txt.MinimumSize = new Size(120, 28);
        btnTim.Margin = new Padding(0, 2, 8, 2);
        bar.Controls.Add(txt, 0, 0);
        bar.Controls.Add(btnTim, 1, 0);

        var col = 2;
        if (btnXoa is not null)
        {
            btnXoa.Margin = new Padding(0, 2, optionalRight is null ? 0 : 8, 2);
            bar.Controls.Add(btnXoa, col++, 0);
        }

        if (optionalRight is not null)
        {
            optionalRight.Anchor = AnchorStyles.Left;
            optionalRight.Margin = new Padding(4, 6, 0, 4);
            bar.Controls.Add(optionalRight, col, 0);
        }

        return bar;
    }

    /// <summary>4 ô tổng kết (Mặt hàng, Tổng tiền, VAT, Thanh toán) co giãn theo chiều ngang.</summary>
    public static Panel TaoSummaryStats(out Label matHang, out Label tongTien, out Label vat, out Label thanhToan, string vatTitle = "VAT")
    {
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            AutoSize = false,
            Margin = new Padding(0)
        };
        for (var i = 0; i < 4; i++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        matHang = TaoStatValue("0", false);
        tongTien = TaoStatValue("0", false);
        vat = TaoStatValue("0", false);
        thanhToan = TaoStatValue("0", true);

        ThemStatCell(tbl, 0, "MẶT HÀNG", matHang);
        ThemStatCell(tbl, 1, "TỔNG TIỀN", tongTien);
        ThemStatCell(tbl, 2, vatTitle, vat);
        ThemStatCell(tbl, 3, "THANH TOÁN", thanhToan);
        return tbl;
    }

    private static void ThemStatCell(TableLayoutPanel tbl, int col, string title, Label value)
    {
        tbl.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = title,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(0, 0, 8, 0)
        }, col, 0);
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.Margin = new Padding(0, 0, 8, 0);
        tbl.Controls.Add(value, col, 1);
    }

    private static Label TaoStatValue(string text, bool highlight) => new()
    {
        Text = text,
        ForeColor = highlight ? Primary : Ink,
        Font = new Font("Segoe UI", highlight ? 14F : 11F, FontStyle.Bold),
        AutoEllipsis = true
    };

    public static Panel TaoStepper(int activeStep)
    {
        var wrap = new Panel { Dock = DockStyle.Top, Height = 72, MinimumSize = new Size(0, 64), BackColor = PageBg, Padding = new Padding(4, 8, 4, 4) };
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

        var steps = new[]
        {
            (1, "Thông tin phiếu nhập"),
            (2, "Danh sách hàng nhập"),
            (3, "Hoàn tất nhập kho")
        };

        for (var i = 0; i < steps.Length; i++)
        {
            var (num, title) = steps[i];
            var on = num == activeStep;
            var done = num < activeStep;
            var chip = new Panel { Dock = DockStyle.Fill, Margin = new Padding(i > 0 ? 8 : 0, 0, i < 2 ? 8 : 0, 0), BackColor = Color.Transparent, MinimumSize = new Size(160, 52) };
            chip.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var circleColor = on || done ? Primary : Color.FromArgb(200, 210, 202);
                using var br = new SolidBrush(circleColor);
                g.FillEllipse(br, 4, 8, 32, 32);
                using var f = new Font("Segoe UI", 10F, FontStyle.Bold);
                using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(num.ToString(), f, Brushes.White, new RectangleF(4, 8, 32, 32), fmt);
                using var ft = new Font("Segoe UI", 9.25F, on ? FontStyle.Bold : FontStyle.Regular);
                using var ink = new SolidBrush(on ? PrimaryDark : Muted);
                var titleRect = new RectangleF(44, 6, Math.Max(40, chip.Width - 48), chip.Height - 8);
                g.DrawString(title, ft, ink, titleRect);
            };
            tbl.Controls.Add(chip, i, 0);
        }

        wrap.Controls.Add(tbl);
        return wrap;
    }

    public static Panel TaoStatusFooter()
    {
        var bar = new Panel { Dock = DockStyle.Fill, MinimumSize = new Size(0, 64), BackColor = PageBg, Padding = new Padding(0, 8, 0, 0) };
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

        tbl.Controls.Add(TaoStatusCard("Trạng thái hệ thống", "Kết nối ổn định"), 0, 0);
        tbl.Controls.Add(TaoStatusCard("Lần cuối đồng bộ", DateTime.Now.ToString("HH:mm:ss") + " Hôm nay"), 1, 0);
        tbl.Controls.Add(TaoStatusCard("Hàng sắp về", "—"), 2, 0);
        bar.Controls.Add(tbl);
        return bar;
    }

    private static Panel TaoStatusCard(string title, string value)
    {
        var p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), BackColor = Color.White, Padding = new Padding(12, 10, 12, 10) };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };
        p.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 20,
            Text = title,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 8.5F)
        });
        p.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = value,
            ForeColor = PrimaryDark,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        });
        return p;
    }
}
