using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using System.Drawing;
using System.Windows.Forms;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Sales;

public partial class FrmKeDonBanThuoc : Form
{
    private const string TenHoaDonCoSo = "NHÀ THUỐC — Pharmacy Management ALN";
    private const string DiaChiHoaDonCoSo = "(Cập nhật địa chỉ nhà thuốc)";
    private const string DienThoaiCoSo = "(Cập nhật điện thoại liên hệ)";

    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly SalesService _sales = new(new DbContextDAL());
    private readonly KhachHangService _khachHang = new(new DbContextDAL());

    private readonly List<ThuocKeDonViewDTO> _thuocCache = new();
    private readonly List<DonHangGioHangDTO> _gioHang = new();

    private KhachHangDTO? _khachHienTai;
    private bool _khachLe;

    public FrmKeDonBanThuoc()
    {
        InitializeComponent();
        WireEvents();
        SetupGrids();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        TaiDanhSachThuoc();
        CapNhatTongTien();
        DatTrangThaiKhach("Tra cứu CCCD hoặc chọn Khách lẻ");
        CapNhatNutLuuKhach();
    }

    private void WireEvents()
    {
        btnTraCuuKhach.Click += (_, _) => TraCuuKhach();
        btnKhachLe.Click += (_, _) => ChonKhachLe();
        btnTaoKhach.Click += (_, _) => LuuKhachMoi();
        btnTimThuoc.Click += (_, _) => TaiDanhSachThuoc();

        txtTimThuoc.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TaiDanhSachThuoc();
            }
        };

        dgvThuoc.SelectionChanged += (_, _) => CapNhatGoiYSoLuong();
        dgvThuoc.CellFormatting += DgvThuoc_CellFormatting;
        dgvGioHang.DataBindingComplete += (_, _) => dgvGioHang.ClearSelection();

        btnThemGio.Click += (_, _) => ThemVaoGio();
        btnXoaDong.Click += (_, _) => XoaDongGio();
        numGiamGia.ValueChanged += (_, _) => CapNhatTongTien();
        btnXacNhan.Click += (_, _) => XacNhanBan();
        btnLamMoi.Click += (_, _) => LamMoiDon();
        chkNgaySinh.CheckedChanged += chkNgaySinh_CheckedChanged;
        btnInHoaDon.Click += (_, _) => InHoaDonGioHang();
        btnXuatHoaDon.Click += (_, _) => XuatHoaDonGioHang();
    }

    private void SetupGrids()
    {
        dgvThuoc.AutoGenerateColumns = false;
        dgvThuoc.Columns.Clear();

        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.TenThuoc), "Tên thuốc", 160, 40));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.HoatChat), "Hoạt chất", 120, 22));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.HamLuong), "Hàm lượng", 90, 14));
        dgvThuoc.Columns.Add(CotChu(nameof(ThuocKeDonViewDTO.DonViTinh), "ĐVT", 52, 8));
        dgvThuoc.Columns.Add(CotSo(nameof(ThuocKeDonViewDTO.GiaBan), "Giá bán", 88, 12, "N0"));
        dgvThuoc.Columns.Add(CotSo(nameof(ThuocKeDonViewDTO.TonLoConHan), "Tồn lô", 58, 10, "N0"));

        dgvGioHang.AutoGenerateColumns = false;
        dgvGioHang.Columns.Clear();

        dgvGioHang.Columns.Add(CotChu(nameof(DonHangGioHangDTO.TenThuoc), "Thuốc", 140, 45));
        dgvGioHang.Columns.Add(CotChu(nameof(DonHangGioHangDTO.DonViTinh), "ĐVT", 48, 10));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.SoLuong), "SL", 44, 10, "N0"));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.DonGia), "Đơn giá", 88, 18, "N0"));
        dgvGioHang.Columns.Add(CotSo(nameof(DonHangGioHangDTO.ThanhTien), "Thành tiền", 96, 22, "N0"));

        ApDungStyleLuoi(dgvThuoc);
        ApDungStyleLuoi(dgvGioHang);
    }

    private static DataGridViewTextBoxColumn CotChu(string prop, string header, int minWidth, int fillWeight) =>
        new()
        {
            DataPropertyName = prop,
            HeaderText = header,
            MinimumWidth = minWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                WrapMode = DataGridViewTriState.True,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            }
        };

    private static DataGridViewTextBoxColumn CotSo(string prop, string header, int minWidth, int fillWeight, string format) =>
        new()
        {
            DataPropertyName = prop,
            HeaderText = header,
            MinimumWidth = minWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = format,
                WrapMode = DataGridViewTriState.False,
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };

    private static void ApDungStyleLuoi(DataGridView g)
    {
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        g.RowTemplate.MinimumHeight = 34;
        g.ColumnHeadersHeight = 40;
        g.EnableHeadersVisualStyles = false;

        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 125, 50);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
    }

    private void TraCuuKhach()
    {
        _khachLe = false;
        if (!Validator.TryNormalizeCccd(txtCccd.Text, out var cccd))
        {
            MessageBox.Show("CCCD phải 12 số");
            return;
        }

        txtCccd.Text = cccd;

        var kh = _khachHang.TraCuuTheoCccd(cccd);

        if (kh is null)
        {
            _khachHienTai = null;
            GanThongTinKhachForm(new KhachHangDTO { CCCD = cccd });
            DatTrangThaiKhach("Chưa có trong hệ thống — nhập đủ thông tin và bấm «Lưu KH mới»");
            CapNhatNutLuuKhach();
            return;
        }

        _khachHienTai = kh;
        GanThongTinKhachForm(kh);
        DatTrangThaiKhach($"Đã tìm: {kh.HoTen}");
        CapNhatNutLuuKhach();
    }

    private void ChonKhachLe()
    {
        _khachLe = true;
        _khachHienTai = null;
        txtCccd.Clear();
        txtHoTen.Clear();
        txtSoDienThoai.Clear();
        txtDiaChi.Clear();
        chkNgaySinh.Checked = false;
        DatTrangThaiKhach("Khách lẻ");
        CapNhatNutLuuKhach();
    }

    private void LuuKhachMoi()
    {
        if (_khachLe)
        {
            MessageBox.Show("Đang chọn «Khách lẻ» — không lưu CCCD.", "Không thể lưu",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!Validator.TryNormalizeCccd(txtCccd.Text, out var cccd))
        {
            MessageBox.Show("CCCD phải gồm đúng 12 chữ số.", "Thiếu / sai CCCD",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        txtCccd.Text = cccd;

        if (Validator.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên khách hàng.", "Thiếu họ tên",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Validator.IsPhoneOptional(txtSoDienThoai.Text))
        {
            MessageBox.Show("Số điện thoại không hợp lệ (để trống nếu không có).", "SĐT không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var dto = new KhachHangDTO
            {
                CCCD = cccd,
                HoTen = txtHoTen.Text.Trim(),
                SoDienThoai = Validator.IsNullOrWhiteSpace(txtSoDienThoai.Text)
                    ? null
                    : txtSoDienThoai.Text.Trim(),
                DiaChi = Validator.IsNullOrWhiteSpace(txtDiaChi.Text)
                    ? null
                    : txtDiaChi.Text.Trim(),
                NgaySinh = chkNgaySinh.Checked ? dtpNgaySinh.Value.Date : null
            };

            _khachHang.ThemKhachHang(dto);
            _khachHienTai = _khachHang.TraCuuTheoCccd(cccd);
            _khachLe = false;

            GanThongTinKhachForm(_khachHienTai!);
            DatTrangThaiKhach($"Đã lưu khách mới: {_khachHienTai!.HoTen}");
            CapNhatNutLuuKhach();
            MessageBox.Show("Đã lưu thông tin khách hàng vào hệ thống.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể lưu khách",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GanThongTinKhachForm(KhachHangDTO kh)
    {
        txtCccd.Text = kh.CCCD;
        txtHoTen.Text = kh.HoTen ?? string.Empty;
        txtSoDienThoai.Text = kh.SoDienThoai ?? string.Empty;
        txtDiaChi.Text = kh.DiaChi ?? string.Empty;

        if (kh.NgaySinh.HasValue)
        {
            chkNgaySinh.Checked = true;
            dtpNgaySinh.Value = kh.NgaySinh.Value;
        }
        else
        {
            chkNgaySinh.Checked = false;
            dtpNgaySinh.Enabled = false;
        }
    }

    private void DatTrangThaiKhach(string text)
        => lblKhachTrangThai.Text = text;

    private void CapNhatNutLuuKhach()
        => btnTaoKhach.Enabled = !_khachLe && _khachHienTai is null;

    private void TaiDanhSachThuoc()
    {
        _thuocCache.Clear();
        _thuocCache.AddRange(_sales.TimKiemThuocBan(txtTimThuoc.Text.Trim()));

        dgvThuoc.DataSource = null;
        dgvThuoc.DataSource = _thuocCache;
    }

    private ThuocKeDonViewDTO? ThuocDangChon()
        => dgvThuoc.CurrentRow?.DataBoundItem as ThuocKeDonViewDTO;

    private void CapNhatGoiYSoLuong()
    {
        var t = ThuocDangChon();
        if (t is null) return;

        numSoLuong.Maximum = Math.Max(1, t.TonLoConHan);
    }

    private void GanNguonGioHang()
    {
        dgvGioHang.DataSource = null;
        dgvGioHang.DataSource = _gioHang;
    }

    private void ThemVaoGio()
    {
        var t = ThuocDangChon();
        if (t is null)
        {
            MessageBox.Show("Chọn một dòng thuốc trong danh sách bên trái.", "Thêm vào giỏ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (t.TonLoConHan <= 0)
        {
            MessageBox.Show("Thuốc này không còn tồn khả dụng.", "Không thể thêm",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var slThem = (int)numSoLuong.Value;
        if (slThem < 1) return;

        var dong = _gioHang.FirstOrDefault(x => x.MaThuoc == t.MaThuoc);
        var slMoi = dong is null ? slThem : dong.SoLuong + slThem;
        if (slMoi > t.TonLoConHan)
        {
            MessageBox.Show(
                $"Số lượng vượt tồn kho ({t.TonLoConHan:N0}).",
                "Không thể thêm",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (dong is null)
        {
            _gioHang.Add(new DonHangGioHangDTO
            {
                MaThuoc = t.MaThuoc,
                TenThuoc = t.TenThuoc,
                DonViTinh = t.DonViTinh,
                SoLuong = slThem,
                DonGia = t.GiaBan,
                TonToiDa = t.TonLoConHan
            });
        }
        else
        {
            dong.SoLuong = slMoi;
            dong.TonToiDa = t.TonLoConHan;
            dong.DonGia = t.GiaBan;
        }

        GanNguonGioHang();
        CapNhatTongTien();
        lblStatus.Text = $"Đã thêm «{t.TenThuoc}» × {slThem}.";
    }

    private void XoaDongGio()
    {
        if (dgvGioHang.CurrentRow?.DataBoundItem is not DonHangGioHangDTO dong)
        {
            MessageBox.Show("Chọn một dòng trong giỏ hàng để xóa.", "Xóa dòng",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _gioHang.Remove(dong);
        GanNguonGioHang();
        CapNhatTongTien();
        lblStatus.Text = "Đã xóa dòng khỏi giỏ.";
    }

    private void CapNhatTongTien()
    {
        var tongHang = _gioHang.Sum(x => x.ThanhTien);
        var giam = numGiamGia.Value;
        if (giam > tongHang)
        {
            var clamped = Math.Min(numGiamGia.Maximum, Math.Max(numGiamGia.Minimum, tongHang));
            if (numGiamGia.Value != clamped)
                numGiamGia.Value = clamped;
            giam = numGiamGia.Value;
        }

        var thanh = tongHang - giam;
        lblTongHangVal.Text = string.Format(Vi, "{0:#,##0} ₫", tongHang);
        lblThanhTienVal.Text = string.Format(Vi, "{0:#,##0} ₫", thanh);
    }

    private void XacNhanBan()
    {
        if (_gioHang.Count == 0)
        {
            MessageBox.Show("Giỏ hàng đang trống.", "Chưa thể bán",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string? cccd = null;
        if (!_khachLe)
        {
            if (_khachHienTai is not null)
                cccd = _khachHienTai.CCCD;
            else if (Validator.TryNormalizeCccd(txtCccd.Text, out var norm))
                cccd = norm;
            else
            {
                MessageBox.Show(
                    "Chọn «Khách lẻ», tra cứu khách theo CCCD, hoặc nhập đúng CCCD 12 số.",
                    "Thiếu thông tin khách",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        try
        {
            var snapshotGio = SaoChepGioHang();

            var maHd = _sales.XacNhanDonHang(
                cccd,
                numGiamGia.Value,
                cboHinhThucTT.SelectedItem?.ToString(),
                _gioHang);

            var noiDungHd = TaoNoiDungHoaDon(maHd, DateTime.Now, snapshotGio);

            _gioHang.Clear();
            GanNguonGioHang();
            numGiamGia.Value = 0;
            CapNhatTongTien();
            TaiDanhSachThuoc();
            lblStatus.Text = $"Hoàn tất hóa đơn #{maHd}.";

            HienBangNoiDungHoaDon(this, $"Hóa đơn bán hàng #{maHd}", noiDungHd);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xác nhận bán",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LamMoiDon()
    {
        _gioHang.Clear();
        GanNguonGioHang();
        numGiamGia.Value = 0;
        cboHinhThucTT.SelectedIndex = 0;
        CapNhatTongTien();
        lblStatus.Text = "Đã làm mới giỏ hàng.";
    }

    private void DgvThuoc_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
    }

    private void chkNgaySinh_CheckedChanged(object? sender, EventArgs e)
    {
        dtpNgaySinh.Enabled = chkNgaySinh.Checked;
    }

    private List<DonHangGioHangDTO> SaoChepGioHang() =>
        _gioHang.Select(g => new DonHangGioHangDTO
        {
            MaThuoc = g.MaThuoc,
            TenThuoc = g.TenThuoc,
            DonViTinh = g.DonViTinh,
            SoLuong = g.SoLuong,
            DonGia = g.DonGia,
            TonToiDa = g.TonToiDa
        }).ToList();

    private static void HienBangNoiDungHoaDon(IWin32Window owner, string title, string body)
    {
        using var f = new Form
        {
            Text = title,
            Size = new Size(680, 720),
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            ShowInTaskbar = false,
            Font = new Font("Segoe UI", 9.75f)
        };

        var tb = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9.25f),
            WordWrap = false,
            Text = body,
            BorderStyle = BorderStyle.FixedSingle
        };

        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8) };
        var btn = new Button
        {
            Text = "Đóng",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Width = 108,
            Height = 34,
            FlatStyle = FlatStyle.Flat
        };
        btn.Location = new Point(pnl.Width - btn.Width - 8, 8);
        pnl.Resize += (_, _) => btn.Left = pnl.Width - btn.Width - 8;

        pnl.Controls.Add(btn);
        f.Controls.Add(tb);
        f.Controls.Add(pnl);
        f.AcceptButton = btn;
        f.CancelButton = btn;
        f.ShowDialog(owner);
    }

    private string TaoNoiDungHoaDon(
        int? maHoaDon,
        DateTime? ngayLap,
        IReadOnlyList<DonHangGioHangDTO>? dongHang = null)
    {
        var ds = dongHang ?? _gioHang;
        var sb = new StringBuilder();
        var now = ngayLap ?? DateTime.Now;
        var laBanNhap = maHoaDon is null;

        const int wSep = 74;

        sb.AppendLine(new string('=', wSep));
        sb.AppendLine(TenHoaDonCoSo);
        sb.AppendLine($"Địa chỉ: {DiaChiHoaDonCoSo}");
        sb.AppendLine($"Điện thoại: {DienThoaiCoSo}");
        sb.AppendLine(new string('-', wSep));
        sb.AppendLine(laBanNhap
            ? "       PHIẾU THANH TOÁN — HÓA ĐƠN NHÁP (CHƯA GHI SỔ KHO / CHƯA PHÁT HÀNH)"
            : "                    HÓA ĐƠN BÁN LẺ THUỐC");
        sb.AppendLine(new string('=', wSep));

        sb.AppendLine($"Số hóa đơn / chứng từ: {(laBanNhap ? "— CHƯA PHÁT HÀNH —" : maHoaDon!.Value.ToString(Vi))}");
        sb.AppendLine($"Ngày lập: {now:dd/MM/yyyy}          Giờ: {now:HH:mm}");
        var nv = UserSession.HoTen;
        sb.AppendLine($"Nhân viên / Dược sĩ: {(string.IsNullOrWhiteSpace(nv) ? "—" : nv.Trim())}");

        sb.AppendLine();
        sb.AppendLine("THÔNG TIN NGƯỜI MUA");
        sb.AppendLine(new string('-', wSep));
        if (_khachLe)
        {
            sb.AppendLine("Loại khách: Khách lẻ (không gắn CCCD trong hệ thống)");
            sb.AppendLine("Ghi chú: Giao dịch không lưu danh tính chi tiết.");
        }
        else
        {
            var cccdTxt = Validator.TryNormalizeCccd(txtCccd.Text, out var ccNorm)
                ? ccNorm
                : txtCccd.Text.Trim();
            sb.AppendLine($"CCCD: {(string.IsNullOrWhiteSpace(cccdTxt) ? "—" : cccdTxt)}");
            sb.AppendLine($"Họ tên: {(string.IsNullOrWhiteSpace(txtHoTen.Text) ? "—" : txtHoTen.Text.Trim())}");
            sb.AppendLine($"SĐT: {(string.IsNullOrWhiteSpace(txtSoDienThoai.Text) ? "—" : txtSoDienThoai.Text.Trim())}");
            sb.AppendLine($"Địa chỉ: {(string.IsNullOrWhiteSpace(txtDiaChi.Text) ? "—" : txtDiaChi.Text.Trim())}");
            if (chkNgaySinh.Checked)
                sb.AppendLine($"Ngày sinh: {dtpNgaySinh.Value:dd/MM/yyyy}");
        }

        sb.AppendLine();
        sb.AppendLine("CHI TIẾT THUỐC / HÀNG HÓA");
        sb.AppendLine(new string('-', wSep));
        sb.AppendLine(
            $"{PadRt("STT", 4)}{PadRt("Tên thuốc", 30)}{PadRt("ĐVT", 6)}{PadRt("SL", 6)}{PadLf("Đơn giá", 14)}{PadLf("Thành tiền", 14)}");
        sb.AppendLine(new string('-', wSep));

        var i = 1;
        decimal tongHang = 0;
        foreach (var g in ds)
        {
            tongHang += g.ThanhTien;
            sb.AppendLine(
                $"{PadRt(i.ToString(Vi), 4)}{PadRt(CatChuoi(g.TenThuoc, 28), 30)}{PadRt(CatChuoi(g.DonViTinh, 5), 6)}{PadRt(g.SoLuong.ToString(Vi), 6)}{PadLf(string.Format(Vi, "{0:#,##0}", g.DonGia), 14)}{PadLf(string.Format(Vi, "{0:#,##0}", g.ThanhTien), 14)}");
            i++;
        }

        sb.AppendLine(new string('-', wSep));
        var giam = numGiamGia.Value;
        var thanhToan = tongHang - giam;
        sb.AppendLine(
            $"{PadRt("", 46)}{PadLf(string.Format(Vi, "Tổng tiền hàng: {0:#,##0} ₫", tongHang), 28)}");
        sb.AppendLine(
            $"{PadRt("", 46)}{PadLf(string.Format(Vi, "Giảm giá: {0:#,##0} ₫", giam), 28)}");
        sb.AppendLine(
            $"{PadRt("", 46)}{PadLf(string.Format(Vi, "THÀNH TIỀN: {0:#,##0} ₫", thanhToan), 28)}");
        sb.AppendLine($"Hình thức thanh toán: {cboHinhThucTT.SelectedItem}");

        sb.AppendLine();
        sb.AppendLine(new string('=', wSep));
        sb.AppendLine("Lưu ý nghiệp vụ:");
        sb.AppendLine("- Thuốc chỉ dùng theo chỉ định của cơ sở y tế / dược sĩ tư vấn.");
        sb.AppendLine("- Quý khách kiểm tra tên thuốc, hạn dùng và hướng dẫn trước khi rời quầy.");
        sb.AppendLine("- Giữ hóa đơn để đối chiếu khi đổi trả (theo quy định nhà thuốc).");
        sb.AppendLine(new string('=', wSep));
        sb.AppendLine(laBanNhap
            ? "(Bản nháp — chưa khấu trừ tồn kho; dùng «Xác nhận bán» để phát hành hóa đơn chính thức.)"
            : "(Đã ghi nhận vào hệ thống — Xin cảm ơn quý khách đã tin tưởng nhà thuốc!)");

        return sb.ToString();
    }

    private static string PadRt(string s, int w)
    {
        s ??= string.Empty;
        return s.Length >= w ? s[..w] : s.PadRight(w);
    }

    private static string PadLf(string s, int w)
    {
        s ??= string.Empty;
        return s.Length >= w ? s[^w..] : s.PadLeft(w);
    }

    private static string CatChuoi(string? s, int maxChars)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= maxChars ? s : s[..Math.Max(1, maxChars - 1)] + "...";
    }

    private void InHoaDonGioHang()
    {
        if (_gioHang.Count == 0)
        {
            MessageBox.Show("Giỏ hàng đang trống — không có nội dung để in.", "In hóa đơn",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        InVanBanRaMayIn(TaoNoiDungHoaDon(null, DateTime.Now));
    }

    private void XuatHoaDonGioHang()
    {
        if (_gioHang.Count == 0)
        {
            MessageBox.Show("Giỏ hàng đang trống — không có nội dung để xuất.", "Xuất hóa đơn",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Title = "Xuất hóa đơn (Excel)",
            Filter = "Excel (*.xlsx)|*.xlsx",
            DefaultExt = "xlsx",
            FileName = $"HoaDonBan_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            GhiHoaDonNhapRaExcel(dlg.FileName);
            lblStatus.Text = $"Đã xuất hóa đơn: {dlg.FileName}";
            MessageBox.Show("Đã xuất file Excel thành công.", "Xuất hóa đơn",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Không thể xuất file",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GhiHoaDonNhapRaExcel(string filePath)
    {
        var now = DateTime.Now;
        var headerGreen = XLColor.FromArgb(46, 125, 50);
        var mint = XLColor.FromArgb(232, 245, 233);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Hoa don nhap");

        var r = 1;
        ws.Range(r, 1, r, 6).Merge();
        ws.Cell(r, 1).Value = TenHoaDonCoSo;
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Cell(r, 1).Style.Font.FontSize = 14;
        ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        r++;

        ws.Cell(r, 1).Value = "Địa chỉ:";
        ws.Cell(r, 2).Value = DiaChiHoaDonCoSo;
        ws.Range(r, 2, r, 6).Merge();
        r++;
        ws.Cell(r, 1).Value = "Điện thoại:";
        ws.Cell(r, 2).Value = DienThoaiCoSo;
        ws.Range(r, 2, r, 6).Merge();
        r += 2;

        ws.Range(r, 1, r, 6).Merge();
        ws.Cell(r, 1).Value = "PHIẾU THANH TOÁN — HÓA ĐƠN NHÁP (CHƯA PHÁT HÀNH)";
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Cell(r, 1).Style.Fill.BackgroundColor = mint;
        ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        r += 2;

        ws.Cell(r, 1).Value = "Ngày lập:";
        ws.Cell(r, 2).Value = now.ToString("dd/MM/yyyy HH:mm", Vi);
        ws.Range(r, 2, r, 6).Merge();
        r++;
        ws.Cell(r, 1).Value = "Nhân viên / Dược sĩ:";
        ws.Cell(r, 2).Value = string.IsNullOrWhiteSpace(UserSession.HoTen) ? "—" : UserSession.HoTen.Trim();
        ws.Range(r, 2, r, 6).Merge();
        r += 2;

        ws.Range(r, 1, r, 6).Merge();
        ws.Cell(r, 1).Value = "THÔNG TIN NGƯỜI MUA";
        ws.Cell(r, 1).Style.Font.Bold = true;
        r++;

        if (_khachLe)
        {
            ws.Cell(r, 1).Value = "Loại khách:";
            ws.Cell(r, 2).Value = "Khách lẻ";
            ws.Range(r, 2, r, 6).Merge();
            r++;
        }
        else
        {
            var cccdTxt = Validator.TryNormalizeCccd(txtCccd.Text, out var ccNorm)
                ? ccNorm
                : txtCccd.Text.Trim();
            ws.Cell(r, 1).Value = "CCCD:";
            ws.Cell(r, 2).Value = string.IsNullOrWhiteSpace(cccdTxt) ? "—" : cccdTxt;
            ws.Range(r, 2, r, 6).Merge();
            r++;
            ws.Cell(r, 1).Value = "Họ tên:";
            ws.Cell(r, 2).Value = string.IsNullOrWhiteSpace(txtHoTen.Text) ? "—" : txtHoTen.Text.Trim();
            ws.Range(r, 2, r, 6).Merge();
            r++;
            ws.Cell(r, 1).Value = "SĐT:";
            ws.Cell(r, 2).Value = string.IsNullOrWhiteSpace(txtSoDienThoai.Text) ? "—" : txtSoDienThoai.Text.Trim();
            ws.Range(r, 2, r, 6).Merge();
            r++;
            ws.Cell(r, 1).Value = "Địa chỉ:";
            ws.Cell(r, 2).Value = string.IsNullOrWhiteSpace(txtDiaChi.Text) ? "—" : txtDiaChi.Text.Trim();
            ws.Range(r, 2, r, 6).Merge();
            r++;
            if (chkNgaySinh.Checked)
            {
                ws.Cell(r, 1).Value = "Ngày sinh:";
                ws.Cell(r, 2).Value = dtpNgaySinh.Value.ToString("dd/MM/yyyy", Vi);
                ws.Range(r, 2, r, 6).Merge();
                r++;
            }
        }

        r++;

        string[] headers = ["STT", "Tên thuốc", "ĐVT", "SL", "Đơn giá (₫)", "Thành tiền (₫)"];
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(r, c + 1);
            cell.Value = headers[c];
            cell.Style.Fill.BackgroundColor = headerGreen;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = c >= 3
                ? XLAlignmentHorizontalValues.Right
                : XLAlignmentHorizontalValues.Left;
        }

        r++;

        var tongHang = 0m;
        var idx = 1;
        foreach (var g in _gioHang)
        {
            tongHang += g.ThanhTien;
            ws.Cell(r, 1).Value = idx;
            ws.Cell(r, 2).Value = g.TenThuoc;
            ws.Cell(r, 3).Value = g.DonViTinh;
            ws.Cell(r, 4).Value = g.SoLuong;
            ws.Cell(r, 5).Value = g.DonGia;
            ws.Cell(r, 6).Value = g.ThanhTien;
            ws.Cell(r, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
            r++;
            idx++;
        }

        r++;
        var giam = numGiamGia.Value;
        var thanhToan = tongHang - giam;

        ws.Cell(r, 5).Value = "Tổng tiền hàng:";
        ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 5).Style.Font.Bold = true;
        ws.Cell(r, 6).Value = tongHang;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 6).Style.Font.Bold = true;
        r++;

        ws.Cell(r, 5).Value = "Giảm giá:";
        ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 6).Value = giam;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        r++;

        ws.Cell(r, 5).Value = "THÀNH TIỀN:";
        ws.Cell(r, 5).Style.Font.Bold = true;
        ws.Cell(r, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 6).Value = thanhToan;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 6).Style.Font.Bold = true;
        ws.Cell(r, 6).Style.Font.FontColor = headerGreen;
        r++;

        ws.Cell(r, 1).Value = "Hình thức thanh toán:";
        ws.Cell(r, 2).Value = cboHinhThucTT.SelectedItem?.ToString() ?? "—";
        ws.Range(r, 2, r, 6).Merge();
        r += 2;

        ws.Cell(r, 1).Value =
            "Lưu ý: Thuốc chỉ dùng theo chỉ định; quý khách kiểm tra tên thuốc và HSD; giữ phiếu để đối chiếu.";
        ws.Range(r, 1, r, 6).Merge();
        ws.Cell(r, 1).Style.Font.Italic = true;
        ws.Cell(r, 1).Style.Font.FontColor = XLColor.Gray;

        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    private void InVanBanRaMayIn(string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var idx = 0;
        using var doc = new PrintDocument();
        doc.BeginPrint += (_, _) => idx = 0;
        doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics;
            if (g is null)
                return;

            using var font = new Font("Consolas", 8.5f);
            var lh = font.GetHeight(g);
            float y = e.MarginBounds.Top;
            while (idx < lines.Length && y + lh <= e.MarginBounds.Bottom)
            {
                var line = lines[idx++];
                g.DrawString(line, font, Brushes.Black,
                    new RectangleF(e.MarginBounds.Left, y, e.MarginBounds.Width, lh));
                y += lh;
            }

            e.HasMorePages = idx < lines.Length;
        };

        using var dlg = new PrintDialog
        {
            Document = doc,
            AllowSomePages = false,
            UseEXDialog = true
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            doc.Print();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi in",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}