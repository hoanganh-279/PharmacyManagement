#nullable disable
using System.ComponentModel;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Bước 1 — Lập phiếu nhập kho (chưa cộng tồn). Nội dung theo mockup; shell giữ sidebar/header.</summary>
public partial class FrmThongTinPhieuNhapKho : Form
{
    private readonly InventoryService _inventory = new(new DbContextDAL());

    private TextBox _txtMaPhieu;
    private TextBox _txtSoHoaDon;
    private ComboBox _cboLoaiPhieu;
    private ComboBox _cboThuKho;
    private ComboBox _cboKho;
    private ComboBox _cboNcc;
    private RadioButton _rbCongNo;
    private RadioButton _rbThanhToanNgay;
    private DateTimePicker _dtpNgayNhap;
    private DateTimePicker _dtpNgayHoaDon;
    private TextBox _txtGhiChu;
    private TextBox _txtPhuongTien;
    private TextBox _txtDonViVc;
    private TextBox _txtNguoiGiao;
    private NumericUpDown _numVat;

    /// <summary>Điều hướng shell: "kho_ds", "kho_phieu", …</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Action<string> OnShellNavigate { get; set; }

    public FrmThongTinPhieuNhapKho()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmThongTinPhieuNhapKho_Load;
    }

    private void FrmThongTinPhieuNhapKho_Load(object sender, EventArgs e)
    {
        try
        {
            NapCombo();
            NapPhieuHienTai();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void NapCombo()
    {
        _cboLoaiPhieu.Items.Clear();
        _cboLoaiPhieu.Items.AddRange([
            "Nhập kho từ nhà cung cấp",
            "Nhập điều chỉnh tồn",
            "Nhập chuyển kho"
        ]);
        _cboLoaiPhieu.SelectedIndex = 0;

        _cboThuKho.DisplayMember = nameof(IdTenDTO.Ten);
        _cboThuKho.ValueMember = nameof(IdTenDTO.Id);
        _cboThuKho.DataSource = _inventory.LayThuKho().ToList();

        _cboKho.DisplayMember = nameof(IdTenDTO.Ten);
        _cboKho.ValueMember = nameof(IdTenDTO.Id);
        _cboKho.DataSource = _inventory.LayKho().ToList();

        _cboNcc.DisplayMember = nameof(IdTenDTO.Ten);
        _cboNcc.ValueMember = nameof(IdTenDTO.Id);
        _cboNcc.DataSource = _inventory.LayNhaCungCap().ToList();

        if (UserSession.MaNhanVien.HasValue)
            _cboThuKho.SelectedValue = UserSession.MaNhanVien.Value;

        InventoryUiKit.MoRongCombo(_cboLoaiPhieu);
        InventoryUiKit.MoRongCombo(_cboThuKho);
        InventoryUiKit.MoRongCombo(_cboKho);
        InventoryUiKit.MoRongCombo(_cboNcc);
    }

    private void NapPhieuHienTai()
    {
        var phieu = PhieuNhapSession.MaPhieuNhap.HasValue
            ? _inventory.LayPhieu(PhieuNhapSession.MaPhieuNhap.Value)
            : null;

        _txtMaPhieu.Text = phieu is null ? PhieuNhapSession.MaPhieuHienThi : $"PN{phieu.MaPhieuNhap:D5}";

        if (phieu is null)
        {
            _dtpNgayNhap.Value = DateTime.Today;
            _dtpNgayHoaDon.Value = DateTime.Today;
            _numVat.Value = 8;
            _rbCongNo.Checked = true;
            return;
        }

        _txtSoHoaDon.Text = phieu.SoHoaDon ?? "";
        ChonComboText(_cboLoaiPhieu, phieu.LoaiPhieuNhap);
        if (phieu.MaNhanVien > 0)
            _cboThuKho.SelectedValue = phieu.MaNhanVien;
        if (phieu.MaKho.HasValue)
            _cboKho.SelectedValue = phieu.MaKho.Value;
        if (phieu.MaNhaCungCap.HasValue)
            _cboNcc.SelectedValue = phieu.MaNhaCungCap.Value;
        _dtpNgayNhap.Value = phieu.NgayNhap.Date;
        _dtpNgayHoaDon.Value = phieu.NgayHoaDon ?? phieu.NgayNhap.Date;
        _txtGhiChu.Text = phieu.GhiChu ?? "";
        _txtPhuongTien.Text = phieu.PhuongTienVanChuyen ?? "";
        _txtDonViVc.Text = phieu.DonViVanChuyen ?? "";
        _txtNguoiGiao.Text = phieu.NguoiGiaoHang ?? "";
        _numVat.Value = Math.Clamp(phieu.VAT, 0, 100);
        if (phieu.CongNo > 0)
            _rbCongNo.Checked = true;
        else
            _rbThanhToanNgay.Checked = true;

        if (string.Equals(phieu.TrangThai, PhieuNhapTrangThai.DaNhapKho, StringComparison.Ordinal))
            KhoaForm(true);
    }

    private static int? LayIdTuCombo(ComboBox cbo)
    {
        if (cbo.SelectedValue is int id)
            return id;
        if (cbo.SelectedValue is not null && int.TryParse(cbo.SelectedValue.ToString(), out var parsed))
            return parsed;
        return null;
    }

    private static void ChonComboText(ComboBox cbo, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var idx = cbo.Items.IndexOf(text);
        if (idx >= 0)
            cbo.SelectedIndex = idx;
    }

    private PhieuNhapDTO DocPhieuTuForm()
    {
        var dto = new PhieuNhapDTO
        {
            MaPhieuNhap = PhieuNhapSession.MaPhieuNhap ?? 0,
            NgayNhap = _dtpNgayNhap.Value.Date.Add(DateTime.Now.TimeOfDay),
            MaNhanVien = (int)(_cboThuKho.SelectedValue ?? UserSession.MaNhanVien ?? 0),
            SoHoaDon = _txtSoHoaDon.Text.Trim(),
            NgayHoaDon = _dtpNgayHoaDon.Value.Date,
            LoaiPhieuNhap = _cboLoaiPhieu.SelectedItem?.ToString(),
            MaKho = LayIdTuCombo(_cboKho),
            MaNhaCungCap = LayIdTuCombo(_cboNcc),
            PhuongTienVanChuyen = _txtPhuongTien.Text.Trim(),
            DonViVanChuyen = _txtDonViVc.Text.Trim(),
            NguoiGiaoHang = _txtNguoiGiao.Text.Trim(),
            VAT = _numVat.Value,
            GhiChu = _txtGhiChu.Text.Trim()
        };
        return dto;
    }

    private void LuuPhieu(bool chuyenBuoc2)
    {
        try
        {
            var dto = DocPhieuTuForm();
            _inventory.LuuThongTinPhieu(dto, _rbCongNo.Checked, chuyenBuoc2);
            _txtMaPhieu.Text = PhieuNhapSession.MaPhieuHienThi;
            MessageBox.Show(this,
                chuyenBuoc2 ? "Đã lưu phiếu. Chuyển sang danh sách hàng nhập." : "Đã lưu thông tin phiếu.",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (chuyenBuoc2)
                OnShellNavigate?.Invoke("kho_ds");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void KhoaForm(bool khoa)
    {
        foreach (Control c in Controls)
            KhoaControl(c, khoa);
    }

    private static void KhoaControl(Control c, bool khoa)
    {
        c.Enabled = !khoa;
        foreach (Control ch in c.Controls)
            KhoaControl(ch, khoa);
    }

    private void BuildLayout()
    {
        var btnTiep = InventoryUiKit.TaoNut("Tiếp tục nhập hàng →", InventoryUiKit.Primary);
        var btnLuu = InventoryUiKit.TaoNut("Lưu thông tin phiếu", InventoryUiKit.Primary, outline: true);
        var btnHuy = InventoryUiKit.TaoNut("Hủy", InventoryUiKit.Danger, outline: true);
        btnTiep.Click += (_, _) => LuuPhieu(chuyenBuoc2: true);
        btnLuu.Click += (_, _) => LuuPhieu(chuyenBuoc2: false);
        btnHuy.Click += (_, _) =>
        {
            if (MessageBox.Show(this, "Hủy phiếu đang lập?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _inventory.HuyPhieuHienTai();
                NapPhieuHienTai();
            }
        };

        var actions = InventoryUiKit.TaoActionBar(btnTiep, btnLuu, btnHuy);
        var thanhTac = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Padding = new Padding(16, 4, 16, 8)
        };
        thanhTac.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, thanhTac.Width, 0);
        };
        actions.Dock = DockStyle.Fill;
        thanhTac.Controls.Add(actions);

        Controls.Add(InventoryUiKit.TaoKhungTrang(1, TaoNoiDung(), thanhTac));
    }

    private Control TaoNoiDung()
    {
        var scroll = InventoryUiKit.TaoVungCuonVoiCard("Chi tiết thông tin phiếu nhập", out var body);

        var grid = InventoryUiKit.TaoLuoiForm4Cot();

        _txtMaPhieu = InventoryUiKit.TaoTextBox();
        _txtMaPhieu.ReadOnly = true;
        _txtMaPhieu.BackColor = Color.FromArgb(248, 250, 248);
        _txtSoHoaDon = InventoryUiKit.TaoTextBox(placeholder: "Nhập số hóa đơn...");
        _cboLoaiPhieu = InventoryUiKit.TaoCombo();
        _cboThuKho = InventoryUiKit.TaoCombo();
        _cboKho = InventoryUiKit.TaoCombo();
        _cboNcc = InventoryUiKit.TaoCombo();
        _rbCongNo = new RadioButton { Text = "Có quản lý nợ", AutoSize = true, Checked = true, Margin = new Padding(0, 8, 16, 0) };
        _rbThanhToanNgay = new RadioButton { Text = "Thanh toán ngay", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        var pnlCongNo = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 4)
        };
        pnlCongNo.Controls.Add(_rbCongNo);
        pnlCongNo.Controls.Add(_rbThanhToanNgay);

        _dtpNgayNhap = new DateTimePicker { Format = DateTimePickerFormat.Short, MinDate = new DateTime(2000, 1, 1), Font = new Font("Segoe UI", 9.5F) };
        _dtpNgayHoaDon = new DateTimePicker { Format = DateTimePickerFormat.Short, MinDate = new DateTime(2000, 1, 1), Font = new Font("Segoe UI", 9.5F) };
        _txtGhiChu = InventoryUiKit.TaoTextBox(multiline: true, placeholder: "Ghi chú thêm về lô hàng...");
        _txtPhuongTien = InventoryUiKit.TaoTextBox(placeholder: "Ví dụ: Xe tải, Xe máy...");
        _txtDonViVc = InventoryUiKit.TaoTextBox(placeholder: "Tên đơn vị...");
        _txtNguoiGiao = InventoryUiKit.TaoTextBox(placeholder: "Họ tên người giao...");
        _numVat = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 1, Width = 100, Value = 8, Font = new Font("Segoe UI", 9.5F) };

        var row = 0;
        row = InventoryUiKit.ThemHangForm4(grid, row, "Mã phiếu", _txtMaPhieu, false, "Ngày nhập", _dtpNgayNhap);
        row = InventoryUiKit.ThemHangForm4(grid, row, "Số hóa đơn", _txtSoHoaDon, true, "Ngày hóa đơn", _dtpNgayHoaDon);
        row = InventoryUiKit.ThemHangForm4(grid, row, "Loại phiếu nhập", _cboLoaiPhieu, true, "Ghi chú", _txtGhiChu, rowHeight: 84);
        row = InventoryUiKit.ThemHangForm4(grid, row, "Thủ kho nhập", _cboThuKho, true, "Phương tiện vận chuyển", _txtPhuongTien);
        row = InventoryUiKit.ThemHangForm4(grid, row, "Kho nhập", _cboKho, true, "Đơn vị vận chuyển", _txtDonViVc);
        row = InventoryUiKit.ThemHangForm4(grid, row, "Nhà cung cấp", _cboNcc, true, "Người giao hàng", _txtNguoiGiao);
        InventoryUiKit.ThemHangForm4(grid, row, "Công nợ", pnlCongNo, false, "Thuế VAT (%)", _numVat);

        InventoryUiKit.DatChieuCaoLuoiCoDinh(grid);
        body.Controls.Add(grid);
        return scroll;
    }
}
