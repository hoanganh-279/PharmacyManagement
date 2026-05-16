#nullable disable

using System.Drawing.Drawing2D;
using Pharmacy.BLL;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Modal thêm hàng từ DQG vào phiếu nhập (Workflow §4.2).</summary>
public partial class FrmThemTuDqg : Form
{
    private readonly InventoryService _inventory = new(new DbContextDAL());
    private readonly bool _khongQuanLyLoHan;

    private TextBox _txtTim;
    private Panel _pnlDanhSach;
    private readonly List<Panel> _theCards = new();
    private Label _lblHoatChat;
    private Label _lblHamLuong;
    private Label _lblDongGoi;
    private Label _lblNuocSx;
    private CheckBox _chkLienThong;
    private ComboBox _cboNhom;
    private TextBox _txtViTri;
    private NumericUpDown _numSl;
    private ComboBox _cboDonVi;
    private TextBox _txtSoLo;
    private DateTimePicker _dtpHsd;
    private NumericUpDown _numVat;
    private NumericUpDown _numGiaNhap;
    private NumericUpDown _numGiaBan;
    private TextBox _txtGhiChu;
    private SplitContainer _split;

    private IReadOnlyList<TraCuuDanhMucDQGViewDTO> _ketQua = Array.Empty<TraCuuDanhMucDQGViewDTO>();
    private TraCuuDanhMucDQGViewDTO? _chon;
    private int _chiSoChon = -1;

    public FrmThemTuDqg(bool khongQuanLyLoHan)
    {
        _khongQuanLyLoHan = khongQuanLyLoHan;
        InitializeComponent();
        BuildLayout();
        Shown += FrmThemTuDqg_Shown;
        Resize += (_, _) => DieuChinhSplit();
    }

    private void FrmThemTuDqg_Shown(object sender, EventArgs e)
    {
        DieuChinhSplit();
        try
        {
            _cboNhom.DisplayMember = nameof(NhomThuocDTO.TenNhomThuoc);
            _cboNhom.ValueMember = nameof(NhomThuocDTO.MaNhomThuoc);
            _cboNhom.DataSource = _inventory.LayNhomThuoc().ToList();
            InventoryUiKit.MoRongCombo(_cboNhom);
            InventoryUiKit.MoRongCombo(_cboDonVi);
            TimDqg();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DieuChinhSplit()
    {
        if (_split is null || !_split.IsHandleCreated)
            return;

        var total = _split.Width - _split.SplitterWidth;
        if (total < 120)
            return;

        const int preferMin1 = 280;
        const int preferMin2 = 420;
        var min1 = total >= preferMin1 + preferMin2 + 40 ? preferMin1 : Math.Max(100, total / 3);
        var min2 = total >= preferMin1 + preferMin2 + 40 ? preferMin2 : Math.Max(120, total - min1 - 40);

        try
        {
            _split.Panel1MinSize = min1;
            _split.Panel2MinSize = min2;
            var maxDist = total - _split.Panel2MinSize;
            var dist = Math.Clamp((int)(total * 0.34), _split.Panel1MinSize, Math.Max(_split.Panel1MinSize, maxDist));
            if (_split.SplitterDistance != dist)
                _split.SplitterDistance = dist;
        }
        catch (InvalidOperationException)
        {
            // Bỏ qua khi form chưa layout xong
        }
    }

    private void TimDqg()
    {
        _ketQua = _inventory.TraCuuDqg(_txtTim.Text);
        VeDanhSachThe();

        if (_ketQua.Count > 0)
            ChonThe(0);
        else
            ChonDqg(null);
    }

    private void VeDanhSachThe()
    {
        _pnlDanhSach.SuspendLayout();
        _pnlDanhSach.Controls.Clear();
        _theCards.Clear();
        _chiSoChon = -1;

        for (var i = 0; i < _ketQua.Count; i++)
        {
            var idx = i;
            var card = TaoTheDqg(_ketQua[i], idx);
            _theCards.Add(card);
            _pnlDanhSach.Controls.Add(card);
        }

        CapNhatChieuCaoDanhSach();
        _pnlDanhSach.ResumeLayout(true);
    }

    private void CapNhatChieuCaoDanhSach()
    {
        var h = 0;
        foreach (Control c in _pnlDanhSach.Controls)
            h += c.Height + c.Margin.Vertical;
        _pnlDanhSach.Height = Math.Max(80, h + 8);
    }

    private Panel TaoTheDqg(TraCuuDanhMucDQGViewDTO d, int index)
    {
        var badge = (d.SoDangKy ?? d.MaDQGDonVi ?? "").Trim();
        var card = new Panel
        {
            Width = Math.Max(200, _pnlDanhSach.ClientSize.Width - 24),
            Height = 96,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = Color.White,
            Cursor = Cursors.Hand,
            Tag = index,
            Padding = new Padding(12, 10, 12, 10)
        };

        var lblTen = new Label
        {
            AutoSize = false,
            Location = new Point(12, 10),
            Size = new Size(card.Width - 140, 22),
            Text = d.TenHangHoa,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = InventoryUiKit.Ink,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        Label? badgeLbl = null;
        if (!string.IsNullOrEmpty(badge))
        {
            badgeLbl = new Label
            {
                AutoSize = true,
                Text = badge,
                ForeColor = Color.White,
                BackColor = InventoryUiKit.PrimaryDark,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Padding = new Padding(8, 3, 8, 3),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
        }

        var sub = TaoDongPhu($"Hoạt chất: {d.HoatChatChinh ?? "—"}");
        sub.Top = 36;
        var sub2 = TaoDongPhu($"Hãng SX: {d.HangSanXuat ?? "—"}");
        sub2.Top = 54;
        var sub3 = TaoDongPhu($"Quy cách: {d.DongGoi ?? "—"}");
        sub3.Top = 72;

        card.Controls.AddRange([sub3, sub2, sub, lblTen]);
        if (badgeLbl is not null)
            card.Controls.Add(badgeLbl);

        void LayoutCard()
        {
            lblTen.Width = Math.Max(80, card.Width - (badgeLbl?.Width ?? 0) - 36);
            if (badgeLbl is not null)
                badgeLbl.Location = new Point(card.Width - badgeLbl.Width - 12, 10);
            sub.Width = sub2.Width = sub3.Width = card.Width - 24;
        }

        card.Resize += (_, _) => LayoutCard();
        card.HandleCreated += (_, _) => LayoutCard();

        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            var selected = (int)card.Tag == _chiSoChon;
            using var path = TaoBoGoc(rect, 8);
            using var br = new SolidBrush(selected ? InventoryUiKit.MintBg : Color.White);
            g.FillPath(br, path);
            using var pen = new Pen(selected ? InventoryUiKit.Primary : InventoryUiKit.CardBorder, selected ? 2f : 1f);
            g.DrawPath(pen, path);
        };

        card.Click += (_, _) => ChonThe(index);
        foreach (Control c in card.Controls)
        {
            c.Click += (_, _) => ChonThe(index);
            c.Cursor = Cursors.Hand;
        }

        return card;
    }

    private static Label TaoDongPhu(string text) => new()
    {
        AutoSize = false,
        Height = 16,
        Left = 12,
        Text = text,
        ForeColor = InventoryUiKit.Muted,
        Font = new Font("Segoe UI", 8.25F)
    };

    private void ChonThe(int index)
    {
        if (index < 0 || index >= _ketQua.Count)
            return;

        _chiSoChon = index;
        for (var i = 0; i < _theCards.Count; i++)
        {
            var card = _theCards[i];
            var selected = i == index;
            card.BackColor = selected ? InventoryUiKit.MintBg : Color.White;
            foreach (Control c in card.Controls)
            {
                if (c is Label lbl && lbl.Font.Bold)
                    lbl.ForeColor = selected ? InventoryUiKit.Primary : InventoryUiKit.Ink;
            }
            card.Invalidate();
        }

        ChonDqg(_ketQua[index]);
    }

    private void ChonDqg(TraCuuDanhMucDQGViewDTO? d)
    {
        _chon = d;
        if (d is null)
        {
            _lblHoatChat.Text = _lblHamLuong.Text = _lblDongGoi.Text = _lblNuocSx.Text = "—";
            return;
        }

        _lblHoatChat.Text = d.HoatChatChinh ?? "—";
        _lblHamLuong.Text = d.HamLuong ?? "—";
        _lblDongGoi.Text = d.DongGoi ?? "—";
        _lblNuocSx.Text = d.NuocSanXuat ?? "—";
        if (!string.IsNullOrEmpty(d.DonViTinh))
            _cboDonVi.Text = d.DonViTinh;
    }

    private bool ThemVaoPhieu(bool themTiep)
    {
        if (_chon is null)
        {
            MessageBox.Show(this, "Vui lòng chọn thuốc từ danh mục DQG.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (_cboNhom.SelectedValue is not int maNhom)
        {
            MessageBox.Show(this, "Vui lòng chọn nhóm hàng hóa.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        try
        {
            _inventory.ThemChiTietTuDqg(
                _chon.MaDQG,
                maNhom,
                _chkLienThong.Checked,
                (int)_numSl.Value,
                _cboDonVi.Text,
                _txtSoLo.Text.Trim(),
                _dtpHsd.Value,
                _numVat.Value,
                _numGiaNhap.Value,
                _numGiaBan.Value,
                _txtViTri.Text.Trim(),
                _txtGhiChu.Text.Trim(),
                _khongQuanLyLoHan);

            if (!themTiep)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _numSl.Value = 1;
                _txtSoLo.Clear();
                MessageBox.Show(this, "Đã thêm dòng. Tiếp tục nhập hàng khác.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    private void BuildLayout()
    {
        var header = TaoHeader();
        var footer = TaoFooter();
        // TaoFooterTongKet dùng Dock Fill (đúng khi footer nằm trong TableLayoutPanel).
        // Trên Form cùng SplitContainer.Fill, hai Fill khiến thanh nút bị mất/che — gắn Bottom.
        footer.Dock = DockStyle.Bottom;

        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = Color.White,
            SplitterWidth = 8,
            Panel1 = { BackColor = Color.White },
            Panel2 = { BackColor = Color.White }
        };
        _split.Panel1.Controls.Add(TaoPanelTraCuu());
        _split.Panel2.Controls.Add(TaoPanelChiTiet());

        Controls.Add(_split);
        Controls.Add(footer);
        Controls.Add(header);

        Load += (_, _) => BeginInvoke(DieuChinhSplit);
    }

    private Panel TaoHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.White,
            Padding = new Padding(16, 12, 12, 12)
        };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        var icon = new Panel
        {
            Size = new Size(32, 32),
            BackColor = InventoryUiKit.Primary,
            Location = new Point(16, 12)
        };
        icon.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = TaoBoGoc(new Rectangle(0, 0, icon.Width - 1, icon.Height - 1), 6);
            using var br = new SolidBrush(InventoryUiKit.Primary);
            e.Graphics.FillPath(br, path);
            using var pen = new Pen(Color.White, 2.5f);
            var cx = icon.Width / 2;
            var cy = icon.Height / 2;
            e.Graphics.DrawLine(pen, cx - 7, cy, cx + 7, cy);
            e.Graphics.DrawLine(pen, cx, cy - 7, cx, cy + 7);
        };

        var lbl = new Label
        {
            Text = "Thêm từ danh mục Dược Quốc Gia (DQG)",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = InventoryUiKit.Ink,
            AutoSize = true,
            Location = new Point(56, 16)
        };

        var btnDong = new Button
        {
            Text = "✕",
            Size = new Size(36, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 11F),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnDong.FlatAppearance.BorderSize = 0;
        btnDong.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        header.Resize += (_, _) => btnDong.Location = new Point(header.Width - btnDong.Width - 12, 10);

        header.Controls.AddRange([btnDong, lbl, icon]);
        return header;
    }

    private Panel TaoFooter()
    {
        var infoIcon = new Panel { Size = new Size(20, 20), BackColor = Color.Transparent, Margin = new Padding(0, 2, 8, 0) };
        infoIcon.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new SolidBrush(InventoryUiKit.Primary);
            e.Graphics.FillEllipse(br, 0, 0, 19, 19);
            using var f = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("i", f, Brushes.White, new RectangleF(0, 0, 19, 19), fmt);
        };

        var lblInfo = new Label
        {
            Text = "Vui lòng kiểm tra kỹ số lô và hạn dùng trước khi lưu.",
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 9F),
            AutoSize = true,
            MaximumSize = new Size(480, 0),
            Margin = new Padding(0, 4, 0, 0)
        };

        var infoRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 4, 0, 0)
        };
        infoRow.Controls.Add(infoIcon);
        infoRow.Controls.Add(lblInfo);

        var btnThem = InventoryUiKit.TaoNut("✓  Thêm vào phiếu nhập", InventoryUiKit.Primary);
        var btnTiep = InventoryUiKit.TaoNut("Lưu & thêm tiếp", InventoryUiKit.Primary, outline: true);
        var btnHuy = InventoryUiKit.TaoNut("Hủy", InventoryUiKit.Muted, outline: true);
        btnThem.Click += (_, _) => ThemVaoPhieu(false);
        btnTiep.Click += (_, _) => ThemVaoPhieu(true);
        btnHuy.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var actions = InventoryUiKit.TaoActionBar(btnThem, btnTiep, btnHuy);
        return InventoryUiKit.TaoFooterTongKet(infoRow, actions);
    }

    private Control TaoPanelTraCuu()
    {
        var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 8, 12), BackColor = Color.White };

        var timWrap = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(250, 252, 250),
            Padding = new Padding(12, 0, 12, 0)
        };
        timWrap.Paint += (_, e) =>
        {
            using var path = TaoBoGoc(new Rectangle(0, 0, timWrap.Width - 1, timWrap.Height - 1), 8);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new SolidBrush(Color.FromArgb(250, 252, 250));
            e.Graphics.FillPath(br, path);
            using var pen = new Pen(InventoryUiKit.CardBorder);
            e.Graphics.DrawPath(pen, path);
        };

        var lblIcon = new Label
        {
            Text = "🔍",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = InventoryUiKit.Muted,
            Location = new Point(4, 9),
            BackColor = Color.Transparent
        };

        _txtTim = new TextBox
        {
            BorderStyle = BorderStyle.None,
            PlaceholderText = "Tìm tên thuốc, số đăng ký hoặc hoạt chất...",
            Font = new Font("Segoe UI", 9.5F),
            BackColor = Color.FromArgb(250, 252, 250),
            Location = new Point(32, 10),
            Width = 200,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _txtTim.TextChanged += (_, _) => TimDqg();
        timWrap.Resize += (_, _) => _txtTim.Width = Math.Max(80, timWrap.Width - 44);
        timWrap.Controls.AddRange([_txtTim, lblIcon]);

        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 12, 4, 0),
            BackColor = Color.White
        };
        _pnlDanhSach = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Location = new Point(0, 0)
        };
        scrollHost.Controls.Add(_pnlDanhSach);
        scrollHost.Resize += (_, _) =>
        {
            var w = Math.Max(200, scrollHost.ClientSize.Width - 20);
            _pnlDanhSach.Width = w;
            foreach (var card in _theCards)
                card.Width = w;
        };

        p.Controls.Add(scrollHost);
        p.Controls.Add(timWrap);
        return p;
    }

    private Control TaoPanelChiTiet()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 12, 16, 12), BackColor = Color.White };

        var boxGoc = TaoKhungDuLieuGoc();
        var boxNhap = TaoKhungNhapKho();

        scroll.Controls.Add(boxNhap);
        scroll.Controls.Add(boxGoc);

        void SyncCardWidth()
        {
            var w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            if (w > 0)
            {
                boxGoc.Width = w;
                boxNhap.Width = w;
            }
        }

        scroll.Resize += (_, _) => SyncCardWidth();
        SyncCardWidth();
        return scroll;
    }

    private Panel TaoKhungDuLieuGoc()
    {
        var box = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = Color.FromArgb(248, 249, 248),
            Padding = new Padding(16, 14, 16, 14),
            Margin = new Padding(0, 0, 0, 16)
        };
        box.Paint += (_, e) =>
        {
            using var path = TaoBoGoc(new Rectangle(0, 0, box.Width - 1, box.Height - 1), 8);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new SolidBrush(Color.FromArgb(248, 249, 248));
            e.Graphics.FillPath(br, path);
            using var pen = new Pen(InventoryUiKit.CardBorder);
            e.Graphics.DrawPath(pen, path);
        };

        var title = new Label
        {
            Text = "DỮ LIỆU GỐC TỪ DQG",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = InventoryUiKit.Muted,
            AutoSize = true,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 10)
        };

        var grid = new TableLayoutPanel
        {
            ColumnCount = 4,
            RowCount = 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 4, 0, 0)
        };
        for (var i = 0; i < 4; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblHoatChat = TaoGiaTriGoc();
        _lblHamLuong = TaoGiaTriGoc();
        _lblDongGoi = TaoGiaTriGoc();
        _lblNuocSx = TaoGiaTriGoc();

        ThemOGoc(grid, 0, "HOẠT CHẤT", _lblHoatChat);
        ThemOGoc(grid, 1, "HÀM LƯỢNG", _lblHamLuong);
        ThemOGoc(grid, 2, "ĐÓNG GÓI", _lblDongGoi);
        ThemOGoc(grid, 3, "NƯỚC SX", _lblNuocSx);

        void CapNhatDoRongGoc()
        {
            if (grid.Width < 80)
                return;
            var colW = Math.Max(72, (grid.Width - 36) / 4);
            foreach (Label lbl in new[] { _lblHoatChat, _lblHamLuong, _lblDongGoi, _lblNuocSx })
                lbl.MaximumSize = new Size(colW, 0);
        }

        grid.Resize += (_, _) => CapNhatDoRongGoc();
        box.Resize += (_, _) => CapNhatDoRongGoc();

        box.Controls.Add(grid);
        box.Controls.Add(title);
        return box;
    }

    private static Label TaoGiaTriGoc() => new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = InventoryUiKit.Ink,
        MaximumSize = new Size(280, 0),
        Margin = new Padding(0, 4, 0, 0)
    };

    private static void ThemOGoc(TableLayoutPanel grid, int col, string title, Label val)
    {
        var wrap = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, col < 3 ? 12 : 0, 0)
        };
        wrap.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        wrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        wrap.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Height = 18,
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(0)
        }, 0, 0);

        val.Dock = DockStyle.Top;
        val.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        wrap.Controls.Add(val, 0, 1);
        grid.Controls.Add(wrap, col, 0);
    }

    private Panel TaoKhungNhapKho()
    {
        var box = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(0, 0, 0, 8)
        };
        box.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, box.Width - 1, box.Height - 1);
        };

        var titleRow = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Top,
            Height = 36,
            Margin = new Padding(16, 16, 16, 8)
        };
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        titleRow.Controls.Add(new Label
        {
            Text = "Thông tin nhập kho",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = InventoryUiKit.PrimaryDark,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 0, 0)
        }, 0, 0);

        _chkLienThong = new CheckBox
        {
            Text = "Cho phép liên thông DQG",
            Checked = true,
            AutoSize = true,
            ForeColor = InventoryUiKit.PrimaryDark,
            Font = new Font("Segoe UI", 9F),
            Anchor = AnchorStyles.Right
        };
        titleRow.Controls.Add(_chkLienThong, 1, 0);

        var grid = InventoryUiKit.TaoLuoiForm4Cot(140);
        grid.Dock = DockStyle.Top;
        grid.AutoSize = true;
        grid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grid.Padding = new Padding(16, 0, 16, 16);
        grid.Margin = new Padding(0);

        _cboNhom = InventoryUiKit.TaoCombo();
        _txtViTri = InventoryUiKit.TaoTextBox(placeholder: "Kệ A - Tầng 2");
        _numSl = new NumericUpDown { Minimum = 1, Maximum = 999999, Value = 100, Font = new Font("Segoe UI", 9.5F) };
        _cboDonVi = InventoryUiKit.TaoCombo(dropDownList: false);
        _cboDonVi.Items.AddRange(["Viên", "Hộp", "Tuýp", "Gói", "Chai"]);
        _cboDonVi.SelectedIndex = 0;
        _txtSoLo = InventoryUiKit.TaoTextBox(placeholder: "LOT202305");
        _dtpHsd = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9.5F) };
        _numVat = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 5, DecimalPlaces = 1, Font = new Font("Segoe UI", 9.5F) };
        _numGiaNhap = new NumericUpDown { Minimum = 0, Maximum = 999999999, Increment = 1000, Value = 45000, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F) };
        _numGiaBan = new NumericUpDown { Minimum = 0, Maximum = 999999999, Increment = 1000, Value = 55000, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F), ForeColor = InventoryUiKit.Primary };
        _txtGhiChu = InventoryUiKit.TaoTextBox(multiline: true, placeholder: "Nhập ghi chú chi tiết cho lô hàng này...");

        var r = 0;
        r = InventoryUiKit.ThemHangForm4(grid, r, "Nhóm hàng hóa", _cboNhom, true, "Vị trí kệ", _txtViTri);
        r = InventoryUiKit.ThemHangForm4(grid, r, "Số lượng nhập", _numSl, true, "Đơn vị nhập", _cboDonVi);
        r = InventoryUiKit.ThemHangForm4(grid, r, "Số lô", _txtSoLo, true, "Hạn sử dụng", _dtpHsd);
        r = InventoryUiKit.ThemHangForm4(grid, r, "Thuế VAT (%)", _numVat, false, "Giá nhập (VNĐ)", TaoGiaCoDong(_numGiaNhap));
        r = InventoryUiKit.ThemHangForm4(grid, r, "Giá bán lẻ (VNĐ)", TaoGiaCoDong(_numGiaBan, giaBan: true), false, "", new Panel { Visible = false, Size = Size.Empty });
        InventoryGiaBanUiHelper.GanTuDongGiaBanTheoBoYTe(_numGiaNhap, _numGiaBan, apDungNgayKhiKhoiTao: true);

        // Ghi chú full width
        var rowGc = grid.RowCount;
        InventoryUiKit.DamBaoHang(grid, rowGc, 80);
        grid.SetColumnSpan(_txtGhiChu, 3);
        var lblGc = InventoryUiKit.TaoFieldLabel("Ghi chú nhập kho");
        lblGc.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        lblGc.Margin = new Padding(0, 10, 8, 0);
        _txtGhiChu.Dock = DockStyle.Fill;
        _txtGhiChu.Margin = new Padding(0, 6, 0, 6);
        _txtGhiChu.Height = 64;
        grid.Controls.Add(lblGc, 0, rowGc);
        grid.Controls.Add(_txtGhiChu, 1, rowGc);
        grid.RowCount = rowGc + 1;

        box.Controls.Add(grid);
        box.Controls.Add(titleRow);
        return box;
    }

    private static Control TaoGiaCoDong(NumericUpDown num, bool giaBan = false)
    {
        var wrap = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 6)
        };
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f));
        num.Dock = DockStyle.Fill;
        num.Margin = new Padding(0, 0, 4, 0);
        if (giaBan)
            num.ForeColor = InventoryUiKit.Primary;
        wrap.Controls.Add(num, 0, 0);
        wrap.Controls.Add(new Label
        {
            Text = "đ",
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            ForeColor = giaBan ? InventoryUiKit.Primary : InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 9.5F, giaBan ? FontStyle.Bold : FontStyle.Regular)
        }, 1, 0);
        return wrap;
    }

    private static GraphicsPath TaoBoGoc(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
