#nullable disable
using System.ComponentModel;
using System.Globalization;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Bước 2 — Danh sách hàng nhập kho + DQG (chưa cộng tồn cho đến khi Hoàn tất).</summary>
public partial class FrmDanhSachHangNhapKho : Form
{
    private readonly InventoryService _inventory = new(new DbContextDAL());
    private readonly MedicineService _medicine = new(new DbContextDAL());

    private TextBox _txtTimThuoc;
    private Button _btnTimThuoc;
    private Button _btnXoaTim;
    private CheckBox _chkKhongQuanLyLo;
    private FlowLayoutPanel _pnlKetQuaTim;
    private Label _lblKetQuaTim;
    private DataGridView _grid;
    private Button _btnSua;
    private Button _btnXoa;
    private Label _lblMatHang;
    private Label _lblTongTien;
    private Label _lblVat;
    private Label _lblThanhToan;
    private Label _lblMaPhieu;
    private decimal _vatPhieu;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Action<string> OnShellNavigate { get; set; }

    public FrmDanhSachHangNhapKho()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmDanhSachHangNhapKho_Load;
    }

    private void FrmDanhSachHangNhapKho_Load(object sender, EventArgs e)
    {
        KhoiTaoLuoi();
        NapPhieuVaChiTiet();
    }

    private void NapPhieuVaChiTiet()
    {
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
        {
            _lblMaPhieu.Text = "Chưa có phiếu — vui lòng lập thông tin phiếu nhập trước.";
            _grid.DataSource = null;
            CapNhatTong();
            return;
        }

        try
        {
            var phieu = _inventory.LayPhieu(PhieuNhapSession.MaPhieuNhap.Value);
            _lblMaPhieu.Text = $"Phiếu {PhieuNhapSession.MaPhieuHienThi}"
                               + (phieu is null ? "" : $" · {phieu.TrangThai}");
            _vatPhieu = phieu?.VAT ?? 0;
            var ds = _inventory.LayChiTietPhieuHienTai().ToList();
            _grid.DataSource = ds;
            ToMauHangSapHetHan();
            CapNhatTong(ds);
            KhoaNeuDaNhapKho(phieu?.TrangThai);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void KhoaNeuDaNhapKho(string? trangThai)
    {
        var khoa = string.Equals(trangThai, PhieuNhapTrangThai.DaNhapKho, StringComparison.Ordinal);
        _txtTimThuoc.Enabled = !khoa;
        _btnTimThuoc.Enabled = !khoa;
        _btnXoaTim.Enabled = !khoa;
        _chkKhongQuanLyLo.Enabled = !khoa;
        if (_btnSua != null)
            _btnSua.Enabled = !khoa && CoDongDuocChon();
        if (_btnXoa != null)
            _btnXoa.Enabled = !khoa && CoDongDuocChon();
    }

    private bool CoDongDuocChon() =>
        _grid.CurrentRow?.DataBoundItem is DanhSachHangNhapKhoViewDTO;

    private void MoFormSua()
    {
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
        {
            MessageBox.Show(this, "Hãy lưu thông tin phiếu nhập trước.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_grid.CurrentRow?.DataBoundItem is not DanhSachHangNhapKhoViewDTO dong)
        {
            MessageBox.Show(this, "Chọn một dòng hàng trên lưới để sửa.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new FrmSuaHangNhapKho(dong, _chkKhongQuanLyLo.Checked);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            NapPhieuVaChiTiet();
    }

    private void XoaDongDaChon()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DanhSachHangNhapKhoViewDTO dto)
            return;
        if (MessageBox.Show(this, "Xóa dòng hàng này?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        try
        {
            _inventory.XoaChiTiet(dto.MaCTPN);
            NapPhieuVaChiTiet();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void TimThuocTrongDanhMuc()
    {
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
        {
            MessageBox.Show(this, "Hãy lưu thông tin phiếu nhập trước.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            OnShellNavigate?.Invoke("kho_phieu");
            return;
        }

        var parsed = InventoryMedicineSearchKit.PhanTichTuKhoa(_txtTimThuoc.Text, MedicineSearchMode.ChonNhapKho);
        if (!parsed.CoTheTim)
        {
            AnKetQuaTim();
            HienThongBaoTim(parsed.ThongBaoGoiY, InventoryUiKit.Muted);
            return;
        }

        try
        {
            var ketQua = InventoryMedicineSearchKit.LocChoNhapKho(
                _medicine.TimKiemDanhSachThuoc(parsed.TuKhoaTim));
            VeKetQuaTim(ketQua, parsed.TuKhoaTim!);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void XoaTimKiemThuoc()
    {
        _txtTimThuoc.Clear();
        AnKetQuaTim();
        HienThongBaoTim(
            "Tìm thuốc đã có trong «Quản lý thuốc» — nếu không có, dùng «+ Thêm từ danh mục DQG».",
            InventoryUiKit.Muted);
        _txtTimThuoc.Focus();
    }

    private void HienThongBaoTim(string text, Color mau)
    {
        _lblKetQuaTim.Text = text;
        _lblKetQuaTim.ForeColor = mau;
        _lblKetQuaTim.Visible = true;
    }

    private void AnKetQuaTim()
    {
        _pnlKetQuaTim.Controls.Clear();
        _pnlKetQuaTim.Height = 0;
        _pnlKetQuaTim.Visible = false;
    }

    private void VeKetQuaTim(IReadOnlyList<DanhSachThuocViewDTO> ketQua, string tuKhoa)
    {
        _pnlKetQuaTim.SuspendLayout();
        _pnlKetQuaTim.Controls.Clear();

        if (ketQua.Count == 0)
        {
            HienThongBaoTim(
                InventoryMedicineSearchKit.ThongBaoKetQuaChonNhapKho(tuKhoa, 0),
                InventoryUiKit.Warn);
            _pnlKetQuaTim.Visible = false;
            _pnlKetQuaTim.Height = 0;
            _pnlKetQuaTim.ResumeLayout(true);
            return;
        }

        HienThongBaoTim(
            InventoryMedicineSearchKit.ThongBaoKetQuaChonNhapKho(tuKhoa, ketQua.Count),
            InventoryUiKit.PrimaryDark);

        foreach (var t in ketQua)
        {
            var thuoc = t;
            var card = TaoTheThuoc(thuoc);
            _pnlKetQuaTim.Controls.Add(card);
        }

        var h = 0;
        foreach (Control c in _pnlKetQuaTim.Controls)
            h += c.Height + c.Margin.Vertical;
        _pnlKetQuaTim.Height = Math.Min(280, Math.Max(72, h + 8));
        _pnlKetQuaTim.Visible = true;
        _pnlKetQuaTim.ResumeLayout(true);
    }

    private Panel TaoTheThuoc(DanhSachThuocViewDTO t)
    {
        var card = new Panel
        {
            Width = Math.Max(200, _pnlKetQuaTim.ClientSize.Width - 8),
            Height = 72,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = Color.White,
            Cursor = Cursors.Hand,
            Padding = new Padding(12, 8, 12, 8)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var lblTen = new Label
        {
            AutoSize = false,
            Location = new Point(12, 8),
            Size = new Size(card.Width - 160, 22),
            Text = t.TenThuoc,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = InventoryUiKit.Ink,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        var sub = new Label
        {
            AutoSize = false,
            Location = new Point(12, 32),
            Size = new Size(card.Width - 24, 32),
            Text = $"{t.HoatChat ?? "—"} · {t.DonViTinh} · Giá nhập {t.GiaNhap:N0} · Tồn {t.SoLuongTon:N0}",
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 8.75F),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        card.Controls.Add(sub);
        card.Controls.Add(lblTen);

        void Chon()
        {
            foreach (Control c in _pnlKetQuaTim.Controls)
                if (c is Panel p)
                    p.BackColor = Color.White;
            card.BackColor = InventoryUiKit.MintBg;
            MoThemThuocCoSan(t);
        }

        card.Click += (_, _) => Chon();
        foreach (Control c in card.Controls)
            c.Click += (_, _) => Chon();

        return card;
    }

    private void MoThemThuocCoSan(DanhSachThuocViewDTO thuoc)
    {
        using var dlg = new FrmThemThuocCoSan(thuoc, _chkKhongQuanLyLo.Checked);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            AnKetQuaTim();
            HienThongBaoTim(
                "Đã thêm dòng vào phiếu. Tiếp tục tìm thuốc khác hoặc chỉnh trên lưới.",
                InventoryUiKit.PrimaryDark);
            _txtTimThuoc.Clear();
            NapPhieuVaChiTiet();
            _txtTimThuoc.Focus();
        }
    }

    private void CapNhatTong(IReadOnlyList<DanhSachHangNhapKhoViewDTO>? ds = null)
    {
        ds ??= _grid.DataSource as IReadOnlyList<DanhSachHangNhapKhoViewDTO>
               ?? (_grid.DataSource as List<DanhSachHangNhapKhoViewDTO> ?? []);
        var tong = ds.Sum(x => x.ThanhTien);
        var vat = tong * (_vatPhieu / 100m);
        _lblMatHang.Text = ds.Count.ToString("00");
        _lblTongTien.Text = tong.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        _lblVat.Text = vat.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        _lblThanhToan.Text = (tong + vat).ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
    }

    private void ToMauHangSapHetHan()
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not DanhSachHangNhapKhoViewDTO dto)
                continue;
            if (dto.SoNgayConHan is int d && d <= 90)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                row.DefaultCellStyle.ForeColor = InventoryUiKit.Danger;
            }
        }
    }

    private void MoThemDqg()
    {
        if (!PhieuNhapSession.MaPhieuNhap.HasValue)
        {
            MessageBox.Show(this, "Hãy lưu thông tin phiếu nhập trước.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            OnShellNavigate?.Invoke("kho_phieu");
            return;
        }

        using var dlg = new FrmThemTuDqg(_chkKhongQuanLyLo.Checked);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            NapPhieuVaChiTiet();
    }

    private void KhoiTaoLuoi()
    {
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.MaThuoc), HeaderText = "Mã thuốc", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.TenThuoc), HeaderText = "Tên thuốc", FillWeight = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.DonViTinh), HeaderText = "Đơn vị", FillWeight = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.DonGiaNhap),
            HeaderText = "Giá nhập",
            FillWeight = 80,
            DefaultCellStyle = { Format = "N0" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.GiaBan),
            HeaderText = "Giá bán",
            FillWeight = 80,
            DefaultCellStyle = { Format = "N0" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.SoLuongNhap), HeaderText = "Số lượng", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.ThanhTien),
            HeaderText = "Thành tiền",
            FillWeight = 90,
            DefaultCellStyle = { Format = "N0" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.SoLo), HeaderText = "Số lô", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachHangNhapKhoViewDTO.HanSuDung),
            HeaderText = "Ngày hết hạn",
            FillWeight = 90,
            DefaultCellStyle = { Format = "dd/MM/yyyy", NullValue = "" }
        });
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = InventoryUiKit.PageBg,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(InventoryUiKit.TaoStepper(2), 0, 0);
        root.Controls.Add(TaoToolbar(), 0, 1);
        root.Controls.Add(TaoLuoiCard(), 0, 2);
        root.Controls.Add(TaoFooter(), 0, 3);
        Controls.Add(root);
    }

    private Control TaoToolbar()
    {
        var bar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = InventoryUiKit.PageBg,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        _lblMaPhieu = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            Text = "—",
            AutoEllipsis = true
        };

        _txtTimThuoc = new TextBox
        {
            PlaceholderText = InventoryMedicineSearchKit.PlaceholderTimThuoc,
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnTimThuoc = InventoryUiKit.TaoNut("Tìm kiếm", InventoryUiKit.Primary);
        _btnXoaTim = InventoryUiKit.TaoNut("Xóa", InventoryUiKit.Muted, outline: true);
        InventoryMedicineSearchKit.GanSuKienTimKiem(_txtTimThuoc, _btnTimThuoc, TimThuocTrongDanhMuc, XoaTimKiemThuoc);

        _chkKhongQuanLyLo = new CheckBox
        {
            Text = "Không quản lý lô/hạn dùng",
            AutoSize = true,
            ForeColor = InventoryUiKit.Muted
        };

        var rowTim = InventoryUiKit.TaoThanhTimKiem(_txtTimThuoc, _btnTimThuoc, _chkKhongQuanLyLo, _btnXoaTim);
        rowTim.Dock = DockStyle.Top;
        rowTim.Margin = new Padding(0, 4, 0, 4);

        _lblKetQuaTim = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            MaximumSize = new Size(2000, 0),
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            Text = "Tìm thuốc đã có trong «Quản lý thuốc» — nếu không có, dùng «+ Thêm từ danh mục DQG».",
            Padding = new Padding(0, 0, 0, 4),
            Visible = true
        };

        _pnlKetQuaTim = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = InventoryUiKit.PageBg,
            Visible = false,
            Height = 0,
            Padding = new Padding(0, 0, 0, 8)
        };

        var btnDqg = InventoryUiKit.TaoNut("+ Thêm từ danh mục DQG", InventoryUiKit.Primary);
        var btnExcelIn = InventoryUiKit.TaoNut("Upload Excel", InventoryUiKit.Primary, outline: true);
        var btnExcelOut = InventoryUiKit.TaoNut("Xuất Excel", InventoryUiKit.Primary, outline: true);
        btnDqg.Click += (_, _) => MoThemDqg();
        btnExcelIn.Click += (_, _) => MessageBox.Show(this, "Chức năng import Excel sẽ bổ sung sau.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnExcelOut.Click += (_, _) => MessageBox.Show(this, "Chức năng xuất Excel sẽ bổ sung sau.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 2, 0, 4)
        };
        flow.Controls.Add(btnDqg);
        flow.Controls.Add(btnExcelIn);
        flow.Controls.Add(btnExcelOut);

        bar.Controls.Add(flow);
        bar.Controls.Add(_pnlKetQuaTim);
        bar.Controls.Add(_lblKetQuaTim);
        bar.Controls.Add(rowTim);
        bar.Controls.Add(_lblMaPhieu);
        return bar;
    }

    private Control TaoLuoiCard()
    {
        var (body, card) = InventoryUiKit.TaoCardVoiThan(string.Empty);
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(0);
        body.Padding = new Padding(0);

        var footerLuoi = new Panel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 48),
            Padding = new Padding(0, 8, 0, 0)
        };
        _btnSua = InventoryUiKit.TaoNut("Sửa", InventoryUiKit.Primary);
        _btnXoa = InventoryUiKit.TaoNut("Xóa", InventoryUiKit.Danger);
        _btnSua.Enabled = false;
        _btnXoa.Enabled = false;
        _btnSua.Click += (_, _) => MoFormSua();
        _btnXoa.Click += (_, _) => XoaDongDaChon();
        var actions = InventoryUiKit.TaoActionBar(_btnSua, _btnXoa);
        actions.Dock = DockStyle.Fill;
        footerLuoi.Controls.Add(actions);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            MinimumSize = new Size(0, 160),
            RowTemplate = { Height = 36 }
        };
        _grid.ColumnHeadersDefaultCellStyle.BackColor = InventoryUiKit.Primary;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _grid.ColumnHeadersHeight = 40;
        _grid.DefaultCellStyle.SelectionBackColor = InventoryUiKit.MintBg;
        _grid.DefaultCellStyle.SelectionForeColor = InventoryUiKit.Ink;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 250);
        _grid.SelectionChanged += (_, _) =>
        {
            var coDong = CoDongDuocChon();
            var khoa = !_txtTimThuoc.Enabled;
            if (_btnSua != null)
                _btnSua.Enabled = coDong && !khoa;
            if (_btnXoa != null)
                _btnXoa.Enabled = coDong && !khoa;
        };
        _grid.CellDoubleClick += (_, _) => MoFormSua();

        body.Controls.Add(_grid);
        body.Controls.Add(footerLuoi);
        return card;
    }

    private Control TaoFooter()
    {
        var stats = InventoryUiKit.TaoSummaryStats(out _lblMatHang, out _lblTongTien, out _lblVat, out _lblThanhToan, $"VAT ({_vatPhieu:0.#}%)");

        var btnNhapKho = InventoryUiKit.TaoNut("Hoàn tất nhập kho", InventoryUiKit.Primary);
        var btnLuu = InventoryUiKit.TaoNut("Lưu", InventoryUiKit.Primary, outline: true);
        var btnIn = InventoryUiKit.TaoNut("In phiếu", InventoryUiKit.Primary, outline: true);
        var btnHuy = InventoryUiKit.TaoNut("Hủy", InventoryUiKit.Muted, outline: true);
        var btnQuayLai = InventoryUiKit.TaoNut("← Thông tin phiếu", InventoryUiKit.Primary, outline: true);

        btnNhapKho.Click += (_, _) =>
        {
            if (MessageBox.Show(this,
                    "Xác nhận nhập kho? Hệ thống sẽ cộng tồn theo từng lô (không hoàn tác).",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                _inventory.HoanTatNhapKho();
                MessageBox.Show(this, "Đã nhập kho thành công.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnShellNavigate?.Invoke("kho_phieu");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        btnLuu.Click += (_, _) =>
        {
            try
            {
                _inventory.LuuPhieuTam();
                MessageBox.Show(this, "Đã lưu phiếu (chưa cộng tồn).", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                NapPhieuVaChiTiet();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        btnIn.Click += (_, _) => MessageBox.Show(this, "In phiếu sẽ nối ReportViewer sau.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnHuy.Click += (_, _) =>
        {
            if (MessageBox.Show(this, "Hủy phiên làm việc phiếu này?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _inventory.HuyPhieuHienTai();
                NapPhieuVaChiTiet();
            }
        };
        btnQuayLai.Click += (_, _) => OnShellNavigate?.Invoke("kho_phieu");

        var actions = InventoryUiKit.TaoActionBar(btnNhapKho, btnLuu, btnIn, btnHuy, btnQuayLai);
        return InventoryUiKit.TaoFooterTongKet(stats, actions);
    }
}
