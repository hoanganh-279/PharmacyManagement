#nullable disable
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Dialog sửa thuốc đã có trong danh mục nhà thuốc (không thêm mới).</summary>
public partial class FrmSuaThuoc : Form
{
    private readonly MedicineService _medicine = new(new DbContextDAL());
    private readonly int _maThuoc;

    private TextBox _txtTenThuoc;
    private TextBox _txtHoatChat;
    private TextBox _txtHamLuong;
    private TextBox _txtDonViTinh;
    private ComboBox _cboNhom;
    private NumericUpDown _numGiaNhap;
    private NumericUpDown _numGiaBan;
    private NumericUpDown _numTonToiThieu;
    private DateTimePicker _dtpHanSuDung;
    private CheckBox _chkTrangThai;
    private Label _lblTonHienTai;

    public FrmSuaThuoc(int maThuoc)
    {
        _maThuoc = maThuoc;
        InitializeComponent();
        BuildLayout();
        Load += FrmSuaThuoc_Load;
    }

    private void FrmSuaThuoc_Load(object sender, EventArgs e)
    {
        try
        {
            var nhom = _medicine.LayNhomThuoc();
            _cboNhom.DisplayMember = nameof(NhomThuocDTO.TenNhomThuoc);
            _cboNhom.ValueMember = nameof(NhomThuocDTO.MaNhomThuoc);
            _cboNhom.DataSource = nhom.ToList();

            var t = _medicine.LayChiTietThuoc(_maThuoc)
                    ?? throw new InvalidOperationException("Không tìm thấy thuốc.");

            _txtTenThuoc.Text = t.TenThuoc;
            _txtHoatChat.Text = t.HoatChat ?? "";
            _txtHamLuong.Text = t.HamLuong ?? "";
            _txtDonViTinh.Text = t.DonViTinh;
            _cboNhom.SelectedValue = t.MaNhomThuoc;
            _numGiaNhap.Value = ClampDecimal(t.GiaNhap, _numGiaNhap);
            _numGiaBan.Value = ClampDecimal(t.GiaBan, _numGiaBan);
            _numTonToiThieu.Value = Math.Max(0, Math.Min(t.TonToiThieu, _numTonToiThieu.Maximum));
            _lblTonHienTai.Text = t.SoLuongTon.ToString();
            _chkTrangThai.Checked = t.TrangThai;
            if (t.HanSuDung.HasValue)
            {
                _dtpHanSuDung.Checked = true;
                _dtpHanSuDung.Value = t.HanSuDung.Value;
            }
            else
                _dtpHanSuDung.Checked = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private void BuildLayout()
    {
        var btnHuy = InventoryUiKit.TaoNut("Hủy", InventoryUiKit.Muted, outline: true);
        btnHuy.DialogResult = DialogResult.Cancel;
        var btnLuu = InventoryUiKit.TaoNut("Lưu", InventoryUiKit.Primary);
        btnLuu.Click += BtnLuu_Click;

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Padding = new Padding(16, 8, 16, 12)
        };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };
        var actions = InventoryUiKit.TaoActionBar(btnLuu, btnHuy);
        actions.Dock = DockStyle.Fill;
        footer.Controls.Add(actions);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20, 16, 20, 8) };

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 11,
            Width = scroll.ClientSize.Width - scroll.Padding.Horizontal
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        _txtTenThuoc = TaoInput();
        _txtHoatChat = TaoInput();
        _txtHamLuong = TaoInput();
        _txtDonViTinh = TaoInput();
        _cboNhom = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _numGiaNhap = TaoSo(0, 999_999_999, 0);
        _numGiaBan = TaoSo(0, 999_999_999, 0);
        _numTonToiThieu = TaoSo(0, 999_999, 0);
        _lblTonHienTai = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(97, 97, 97),
            Text = "—"
        };
        _dtpHanSuDung = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Short,
            ShowCheckBox = true,
            Checked = false
        };
        _chkTrangThai = new CheckBox { Text = "Đang kinh doanh", AutoSize = true, Checked = true };

        ThemHang(tbl, 0, "Tên thuốc *", _txtTenThuoc);
        ThemHang(tbl, 1, "Hoạt chất", _txtHoatChat);
        ThemHang(tbl, 2, "Hàm lượng", _txtHamLuong);
        ThemHang(tbl, 3, "Đơn vị tính *", _txtDonViTinh);
        ThemHang(tbl, 4, "Nhóm thuốc *", _cboNhom);
        ThemHang(tbl, 5, "Giá nhập", _numGiaNhap);
        ThemHang(tbl, 6, "Giá bán", _numGiaBan);
        ThemHang(tbl, 7, "Tồn tối thiểu", _numTonToiThieu);
        ThemHang(tbl, 8, "Tồn hiện tại", _lblTonHienTai);
        ThemHang(tbl, 9, "Hạn sử dụng", _dtpHanSuDung);
        ThemHang(tbl, 10, "Trạng thái", _chkTrangThai);

        scroll.Controls.Add(tbl);
        scroll.Resize += (_, _) =>
        {
            var w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            if (w > 0)
                tbl.Width = w;
        };

        Controls.Add(scroll);
        Controls.Add(footer);
        AcceptButton = btnLuu;
        CancelButton = btnHuy;
    }

    private void BtnLuu_Click(object sender, EventArgs e)
    {
        try
        {
            if (_cboNhom.SelectedValue is not int maNhom)
                throw new ArgumentException("Chọn nhóm thuốc.");

            var dto = new ThuocDTO
            {
                MaThuoc = _maThuoc,
                TenThuoc = _txtTenThuoc.Text.Trim(),
                HoatChat = string.IsNullOrWhiteSpace(_txtHoatChat.Text) ? null : _txtHoatChat.Text.Trim(),
                HamLuong = string.IsNullOrWhiteSpace(_txtHamLuong.Text) ? null : _txtHamLuong.Text.Trim(),
                DonViTinh = _txtDonViTinh.Text.Trim(),
                MaNhomThuoc = maNhom,
                GiaNhap = _numGiaNhap.Value,
                GiaBan = _numGiaBan.Value,
                TonToiThieu = (int)_numTonToiThieu.Value,
                HanSuDung = _dtpHanSuDung.Checked ? _dtpHanSuDung.Value.Date : null,
                TrangThai = _chkTrangThai.Checked
            };

            _medicine.CapNhatThuoc(dto);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static decimal ClampDecimal(decimal value, NumericUpDown num) =>
        Math.Max(num.Minimum, Math.Min(value, num.Maximum));

    private static void ThemHang(TableLayoutPanel tbl, int row, string label, Control value)
    {
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 4),
            ForeColor = Color.FromArgb(33, 37, 41)
        }, 0, row);
        value.Margin = new Padding(0, 4, 0, 4);
        tbl.Controls.Add(value, 1, row);
    }

    private static TextBox TaoInput() =>
        new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };

    private static NumericUpDown TaoSo(decimal min, decimal max, int decimals) =>
        new()
        {
            Dock = DockStyle.Fill,
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            ThousandsSeparator = true
        };

}
