#nullable disable
using Pharmacy.BLL;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Nhập lô/số lượng cho thuốc đã có trong danh mục (Workflow §4.3).</summary>
public partial class FrmThemThuocCoSan : Form
{
    private readonly InventoryService _inventory = new(new DbContextDAL());
    private readonly DanhSachThuocViewDTO _thuoc;
    private readonly bool _khongQuanLyLoHan;

    private Label _lblTen;
    private Label _lblHoatChat;
    private Label _lblDonVi;
    private Label _lblTon;
    private NumericUpDown _numSl;
    private TextBox _txtSoLo;
    private DateTimePicker _dtpHsd;
    private NumericUpDown _numVat;
    private NumericUpDown _numGiaNhap;
    private NumericUpDown _numGiaBan;
    private TextBox _txtViTri;
    private TextBox _txtGhiChu;

    public FrmThemThuocCoSan(DanhSachThuocViewDTO thuoc, bool khongQuanLyLoHan)
    {
        _thuoc = thuoc ?? throw new ArgumentNullException(nameof(thuoc));
        _khongQuanLyLoHan = khongQuanLyLoHan;
        InitializeComponent();
        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(TaoNoiDung());
        root.Controls.Add(scroll, 0, 0);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0)
        };
        var btnHuy = InventoryUiKit.TaoNut("Hủy", InventoryUiKit.Muted, outline: true);
        var btnThem = InventoryUiKit.TaoNut("Thêm vào phiếu", InventoryUiKit.Primary);
        btnHuy.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnThem.Click += (_, _) => ThemVaoPhieu();
        flow.Controls.Add(btnThem);
        flow.Controls.Add(btnHuy);
        root.Controls.Add(flow, 0, 1);
        Controls.Add(root);
    }

    private Control TaoNoiDung()
    {
        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1
        };

        stack.Controls.Add(TaoThongTinThuoc());
        stack.Controls.Add(TaoKhungNhapKho());
        return stack;
    }

    private Panel TaoThongTinThuoc()
    {
        var box = InventoryUiKit.TaoCard(string.Empty);
        box.Dock = DockStyle.Top;
        box.AutoSize = true;
        box.Margin = new Padding(0, 0, 0, 12);

        _lblTen = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Text = _thuoc.TenThuoc,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = InventoryUiKit.PrimaryDark,
            MaximumSize = new Size(560, 0)
        };
        _lblHoatChat = TaoDongPhu(
            $"Hoạt chất: {_thuoc.HoatChat ?? "—"} · Hàm lượng: {_thuoc.HamLuong ?? "—"} · Nhóm: {_thuoc.TenNhomThuoc}");
        _lblDonVi = TaoDongPhu($"Đơn vị: {_thuoc.DonViTinh}");
        _lblTon = TaoDongPhu($"Tồn hiện tại: {_thuoc.SoLuongTon:N0} · Trạng thái: {_thuoc.TrangThai}");

        box.Controls.Add(_lblTon);
        box.Controls.Add(_lblDonVi);
        box.Controls.Add(_lblHoatChat);
        box.Controls.Add(_lblTen);
        return box;
    }

    private Panel TaoKhungNhapKho()
    {
        var box = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Padding = new Padding(0, 0, 0, 8)
        };
        box.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, box.Width - 1, box.Height - 1);
        };

        var title = new Label
        {
            Text = "Thông tin nhập kho (lô mới)",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = InventoryUiKit.PrimaryDark,
            Padding = new Padding(16, 12, 16, 0)
        };

        var grid = InventoryUiKit.TaoLuoiForm4Cot(140);
        grid.Dock = DockStyle.Top;
        grid.AutoSize = true;
        grid.Padding = new Padding(16, 0, 16, 16);

        _numSl = new NumericUpDown { Minimum = 1, Maximum = 999999, Value = 1, Font = new Font("Segoe UI", 9.5F) };
        _txtSoLo = InventoryUiKit.TaoTextBox(placeholder: "LOT202305");
        _dtpHsd = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9.5F), Value = DateTime.Today.AddYears(2) };
        _numVat = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 0, DecimalPlaces = 1, Font = new Font("Segoe UI", 9.5F) };
        _numGiaNhap = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 999999999,
            Increment = 100,
            Value = Math.Max(0, _thuoc.GiaNhap),
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 9.5F)
        };
        _numGiaBan = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 999999999,
            Increment = 100,
            Value = Math.Max(0, _thuoc.GiaBan),
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = InventoryUiKit.Primary
        };
        _txtViTri = InventoryUiKit.TaoTextBox(placeholder: "Kệ A - Tầng 2");
        _txtGhiChu = InventoryUiKit.TaoTextBox(multiline: true, placeholder: "Ghi chú lô hàng...");

        if (_khongQuanLyLoHan)
        {
            _txtSoLo.Enabled = false;
            _dtpHsd.Enabled = false;
        }

        var r = 0;
        r = InventoryUiKit.ThemHangForm4(grid, r, "Số lượng nhập *", _numSl, true, "Vị trí kệ", _txtViTri);
        r = InventoryUiKit.ThemHangForm4(grid, r, "Số lô *", _txtSoLo, !_khongQuanLyLoHan, "Hạn sử dụng", _dtpHsd);
        r = InventoryUiKit.ThemHangForm4(grid, r, "Thuế VAT (%)", _numVat, false, "Giá nhập (VNĐ)", TaoGiaCoDong(_numGiaNhap));
        r = InventoryUiKit.ThemHangForm4(grid, r, "Giá bán lẻ (VNĐ)", TaoGiaCoDong(_numGiaBan, giaBan: true), false, "", new Panel { Visible = false, Size = Size.Empty });
        InventoryGiaBanUiHelper.GanTuDongGiaBanTheoBoYTe(_numGiaNhap, _numGiaBan, apDungNgayKhiKhoiTao: true);

        var rowGc = grid.RowCount;
        InventoryUiKit.DamBaoHang(grid, rowGc, 72);
        grid.SetColumnSpan(_txtGhiChu, 3);
        var lblGc = InventoryUiKit.TaoFieldLabel("Ghi chú nhập kho");
        lblGc.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        _txtGhiChu.Dock = DockStyle.Fill;
        _txtGhiChu.Height = 56;
        grid.Controls.Add(lblGc, 0, rowGc);
        grid.Controls.Add(_txtGhiChu, 1, rowGc);

        box.Controls.Add(grid);
        box.Controls.Add(title);
        return box;
    }

    private static Label TaoDongPhu(string text) => new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        Text = text,
        ForeColor = InventoryUiKit.Muted,
        Font = new Font("Segoe UI", 9.25F),
        MaximumSize = new Size(560, 0),
        Padding = new Padding(0, 0, 0, 4)
    };

    private static Control TaoGiaCoDong(NumericUpDown num, bool giaBan = false)
    {
        var wrap = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28f));
        num.Dock = DockStyle.Fill;
        wrap.Controls.Add(num, 0, 0);
        wrap.Controls.Add(new Label
        {
            Text = "đ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = giaBan ? InventoryUiKit.Primary : InventoryUiKit.Muted
        }, 1, 0);
        return wrap;
    }

    private void ThemVaoPhieu()
    {
        try
        {
            _inventory.ThemChiTietTuThuocCoSan(
                _thuoc.MaThuoc,
                (int)_numSl.Value,
                _txtSoLo.Text.Trim(),
                _dtpHsd.Value,
                _numVat.Value > 0 ? _numVat.Value : null,
                _numGiaNhap.Value,
                _numGiaBan.Value,
                string.IsNullOrWhiteSpace(_txtViTri.Text) ? null : _txtViTri.Text.Trim(),
                string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? null : _txtGhiChu.Text.Trim(),
                _khongQuanLyLoHan);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
