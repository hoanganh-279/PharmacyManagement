#nullable disable
using System.Drawing.Drawing2D;
using Pharmacy.BLL;
using Pharmacy.Common;
using Pharmacy.DAL;
using Pharmacy.DTO;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Admin;

public partial class FrmQuanLyNhanVien : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color Surface = Color.White;
    private static readonly Color BorderColor = Color.FromArgb(230, 236, 231);
    private static readonly Color TextMain = Color.FromArgb(33, 37, 41);
    private static readonly Color TextMuted = Color.FromArgb(108, 117, 125);

    private readonly NhanVienAdminService _service;
    private IReadOnlyList<NhanVienDTO> _dsNhanVien;
    private IReadOnlyList<VaiTroDTO> _dsVaiTro;

    private Panel _pnlLeft;
    private Panel _pnlRight;
    private DataGridView _grid;
    private TextBox _txtSearch;

    private TextBox _txtHoTen;
    private TextBox _txtTenDangNhap;
    private TextBox _txtMatKhau;
    private TextBox _txtSoDienThoai;
    private TextBox _txtEmail;
    private ComboBox _cboVaiTro;
    private ComboBox _cboTrangThai;

    private Button _btnLamMoi;
    private Button _btnLuu;
    private Label _lblStatus;

    private int _maNhanVienDangChon = 0;

    public FrmQuanLyNhanVien()
    {
        InitializeComponent();
        _service = new NhanVienAdminService(new DbContextDAL());
        BuildLayout();
        WireEvents();
    }

    private void BuildLayout()
    {
        _pnlLeft = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            BackColor = Color.Transparent
        };

        _pnlRight = new Panel
        {
            Dock = DockStyle.Right,
            Width = 360,
            Padding = new Padding(24),
            BackColor = Surface
        };
        _pnlRight.Paint += (s, e) =>
        {
            using var p = new Pen(BorderColor);
            e.Graphics.DrawLine(p, 0, 0, 0, _pnlRight.Height);
        };

        Controls.Add(_pnlLeft);
        Controls.Add(_pnlRight);

        // WinForms tính toán Docking từ dưới lên trên theo Z-order.
        // Cần SendToBack _pnlRight để nó được ghim vào lề phải trước tiên.
        // Sau đó _pnlLeft BringToFront sẽ lấp đầy không gian còn lại một cách chính xác.
        _pnlRight.SendToBack();
        _pnlLeft.BringToFront();

        BuildLeftPanel();
        BuildRightPanel();
    }

    private void BuildLeftPanel()
    {
        var lblTitle = new Label
        {
            Text = "Danh sách nhân viên",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Primary,
            AutoSize = true,
            Location = new Point(24, 24)
        };

        _txtSearch = new TextBox
        {
            Location = new Point(24, 64),
            Width = 300,
            PlaceholderText = "Tìm theo tên, tài khoản, SĐT..."
        };

        _grid = new DataGridView
        {
            Location = new Point(24, 104),
            BackgroundColor = Surface,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            RowTemplate = { Height = 40 },
            AllowUserToResizeRows = false
        };

        // Ép kích thước Grid luôn bám sát theo _pnlLeft mỗi khi Form thay đổi kích thước
        _pnlLeft.Resize += (_, _) =>
        {
            _grid.Width = _pnlLeft.Width - 48; // Cách lề phải 24px
            _grid.Height = _pnlLeft.Height - 128; // Cách lề dưới 24px
        };

        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 246);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = _grid.ColumnHeadersDefaultCellStyle.BackColor;
        _grid.ColumnHeadersHeight = 44;

        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 245, 233);
        _grid.DefaultCellStyle.SelectionForeColor = TextMain;
        _grid.DefaultCellStyle.Padding = new Padding(4);

        _pnlLeft.Controls.Add(lblTitle);
        _pnlLeft.Controls.Add(_txtSearch);
        _pnlLeft.Controls.Add(_grid);
    }

    private void BuildRightPanel()
    {
        var lblTitle = new Label
        {
            Text = "Thông tin nhân viên",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = TextMain,
            AutoSize = true,
            Location = new Point(24, 24)
        };

        int y = 64;
        int gap = 12;
        int txtHeight = 28;
        int width = 312;

        _txtHoTen = CreateFormInput(ref y, "Họ tên *", width, gap, txtHeight);
        _txtTenDangNhap = CreateFormInput(ref y, "Tên đăng nhập *", width, gap, txtHeight);
        _txtMatKhau = CreateFormInput(ref y, "Mật khẩu (để trống nếu không đổi)", width, gap, txtHeight);
        _txtMatKhau.UseSystemPasswordChar = true;
        _txtSoDienThoai = CreateFormInput(ref y, "Số điện thoại", width, gap, txtHeight);
        _txtEmail = CreateFormInput(ref y, "Email", width, gap, txtHeight);

        var lblRole = new Label { Text = "Vai trò *", Location = new Point(24, y), AutoSize = true, ForeColor = TextMuted };
        y += 24;
        _cboVaiTro = new ComboBox { Location = new Point(24, y), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
        y += txtHeight + gap;

        var lblStatus = new Label { Text = "Trạng thái", Location = new Point(24, y), AutoSize = true, ForeColor = TextMuted };
        y += 24;
        _cboTrangThai = new ComboBox { Location = new Point(24, y), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboTrangThai.Items.AddRange(["Đang làm việc", "Đã nghỉ việc"]);
        _cboTrangThai.SelectedIndex = 0;
        y += txtHeight + 24;

        _btnLuu = new Button
        {
            Text = "Lưu nhân viên",
            Location = new Point(24, y),
            Width = 150,
            Height = 36,
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        _btnLuu.FlatAppearance.BorderSize = 0;

        _btnLamMoi = new Button
        {
            Text = "Thêm mới",
            Location = new Point(186, y),
            Width = 150,
            Height = 36,
            BackColor = Color.White,
            ForeColor = TextMain,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnLamMoi.FlatAppearance.BorderColor = BorderColor;

        y += 48;
        _lblStatus = new Label
        {
            Location = new Point(24, y),
            Width = width,
            Height = 40,
            ForeColor = Color.Red,
            AutoSize = false,
            AutoEllipsis = true
        };

        _pnlRight.Controls.AddRange([
            lblTitle, _txtHoTen, _txtTenDangNhap, _txtMatKhau, _txtSoDienThoai, _txtEmail,
            lblRole, _cboVaiTro, lblStatus, _cboTrangThai, _btnLuu, _btnLamMoi
        ]);
        
        foreach (Control c in _pnlRight.Controls)
        {
            if (c is Label l && (l.Text.EndsWith("*") || l.Text == "Vai trò *"))
                l.ForeColor = TextMuted;
        }
    }

    private TextBox CreateFormInput(ref int y, string label, int width, int gap, int height)
    {
        var lbl = new Label { Text = label, Location = new Point(24, y), AutoSize = true, ForeColor = TextMuted };
        _pnlRight.Controls.Add(lbl);
        y += 24;
        var txt = new TextBox { Location = new Point(24, y), Width = width, Height = height };
        _pnlRight.Controls.Add(txt);
        y += height + gap;
        return txt;
    }

    private void WireEvents()
    {
        Load += async (_, _) =>
        {
            try
            {
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };

        _txtSearch.TextChanged += (_, _) => HienThiDanhSach();
        _btnLamMoi.Click += (_, _) => ResetForm();
        _btnLuu.Click += BtnLuu_Click;

        _grid.SelectionChanged += (_, _) =>
        {
            if (_grid.CurrentRow?.DataBoundItem is NhanVienViewItem item)
            {
                ToForm(item.MaNhanVien);
            }
        };
    }

    private async Task LoadData()
    {
        _dsVaiTro = _service.LayVaiTro();
        _cboVaiTro.Items.Clear();
        foreach (var vt in _dsVaiTro)
        {
            _cboVaiTro.Items.Add(new ComboItem(vt.MaVaiTro, vt.TenVaiTro));
        }
        if (_cboVaiTro.Items.Count > 0) _cboVaiTro.SelectedIndex = 0;

        await ReloadGrid();
    }

    private async Task ReloadGrid()
    {
        _dsNhanVien = _service.LayTatCa();
        HienThiDanhSach();
    }

    private void HienThiDanhSach()
    {
        if (_dsNhanVien == null) return;
        var kw = _txtSearch.Text.Trim().ToLower();

        var viewList = _dsNhanVien
            .Where(x => string.IsNullOrEmpty(kw) ||
                        x.HoTen.ToLower().Contains(kw) ||
                        x.TenDangNhap.ToLower().Contains(kw) ||
                        (x.SoDienThoai?.Contains(kw) == true))
            .Select(x => new NhanVienViewItem
            {
                MaNhanVien = x.MaNhanVien,
                HoTen = x.HoTen,
                TenDangNhap = x.TenDangNhap,
                SoDienThoai = x.SoDienThoai ?? "",
                Email = x.Email ?? "",
                VaiTro = UserDisplayHelper.GetVaiTroDisplayName(x.TenVaiTro),
                TrangThai = x.TrangThai ? "Đang làm việc" : "Đã nghỉ",
                NgayTao = x.NgayTao.ToString("dd/MM/yyyy")
            })
            .ToList();

        var prevId = _maNhanVienDangChon;
        _grid.DataSource = viewList;
        CustomizeGrid();

        if (prevId > 0)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (((NhanVienViewItem)row.DataBoundItem).MaNhanVien == prevId)
                {
                    row.Selected = true;
                    break;
                }
            }
        }
    }

    private void CustomizeGrid()
    {
        if (_grid.Columns.Count == 0) return;
        _grid.Columns["MaNhanVien"].HeaderText = "Mã";
        _grid.Columns["MaNhanVien"].Width = 40;
        _grid.Columns["HoTen"].HeaderText = "Họ tên";
        _grid.Columns["HoTen"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _grid.Columns["TenDangNhap"].HeaderText = "Tài khoản";
        _grid.Columns["TenDangNhap"].Width = 90;
        _grid.Columns["SoDienThoai"].HeaderText = "SĐT";
        _grid.Columns["SoDienThoai"].Width = 90;
        _grid.Columns["Email"].HeaderText = "Email";
        _grid.Columns["Email"].Width = 120;
        _grid.Columns["VaiTro"].HeaderText = "Vai trò";
        _grid.Columns["VaiTro"].Width = 100;
        _grid.Columns["TrangThai"].HeaderText = "Trạng thái";
        _grid.Columns["TrangThai"].Width = 100;
        _grid.Columns["NgayTao"].HeaderText = "Ngày tạo";
        _grid.Columns["NgayTao"].Width = 80;
    }

    private void ResetForm()
    {
        _maNhanVienDangChon = 0;
        _txtHoTen.Text = "";
        _txtTenDangNhap.Text = "";
        _txtMatKhau.Text = "";
        _txtSoDienThoai.Text = "";
        _txtEmail.Text = "";
        if (_cboVaiTro.Items.Count > 0) _cboVaiTro.SelectedIndex = 0;
        _cboTrangThai.SelectedIndex = 0;
        _txtTenDangNhap.Enabled = true; // Cho phép sửa khi tạo mới
        _lblStatus.Text = "";
        _grid.ClearSelection();
    }

    private void ToForm(int id)
    {
        var nv = _dsNhanVien.FirstOrDefault(x => x.MaNhanVien == id);
        if (nv == null) return;

        _maNhanVienDangChon = nv.MaNhanVien;
        _txtHoTen.Text = nv.HoTen;
        _txtTenDangNhap.Text = nv.TenDangNhap;
        _txtTenDangNhap.Enabled = false; // Không cho đổi username
        _txtMatKhau.Text = "";
        _txtSoDienThoai.Text = nv.SoDienThoai;
        _txtEmail.Text = nv.Email;

        for (int i = 0; i < _cboVaiTro.Items.Count; i++)
        {
            if (((ComboItem)_cboVaiTro.Items[i]).Value == nv.MaVaiTro)
            {
                _cboVaiTro.SelectedIndex = i;
                break;
            }
        }

        _cboTrangThai.SelectedIndex = nv.TrangThai ? 0 : 1;
        _lblStatus.Text = "";
    }

    private async void BtnLuu_Click(object sender, EventArgs e)
    {
        _lblStatus.Text = "";
        _lblStatus.ForeColor = Color.Red;

        try
        {
            if (string.IsNullOrWhiteSpace(_txtHoTen.Text)) throw new ArgumentException("Vui lòng nhập họ tên.");
            if (string.IsNullOrWhiteSpace(_txtTenDangNhap.Text)) throw new ArgumentException("Vui lòng nhập tên đăng nhập.");
            if (_cboVaiTro.SelectedItem == null) throw new ArgumentException("Vui lòng chọn vai trò.");

            var dto = new NhanVienDTO
            {
                MaNhanVien = _maNhanVienDangChon,
                HoTen = _txtHoTen.Text.Trim(),
                TenDangNhap = _txtTenDangNhap.Text.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(_txtSoDienThoai.Text) ? null : _txtSoDienThoai.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
                MaVaiTro = ((ComboItem)_cboVaiTro.SelectedItem).Value,
                TrangThai = _cboTrangThai.SelectedIndex == 0
            };

            var mk = _txtMatKhau.Text;

            if (_maNhanVienDangChon == 0)
            {
                if (string.IsNullOrEmpty(mk)) throw new ArgumentException("Mật khẩu là bắt buộc khi tạo mới.");
                _service.Them(dto, mk);
                _lblStatus.ForeColor = Primary;
                _lblStatus.Text = "Thêm nhân viên thành công.";
                ResetForm();
            }
            else
            {
                _service.CapNhat(dto, string.IsNullOrEmpty(mk) ? null : mk);
                _lblStatus.ForeColor = Primary;
                _lblStatus.Text = "Cập nhật nhân viên thành công.";
            }

            await ReloadGrid();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = ex.Message;
        }
    }

    private class NhanVienViewItem
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
        public string NgayTao { get; set; }
    }

    private class ComboItem
    {
        public int Value { get; }
        public string Text { get; }
        public ComboItem(int v, string t) { Value = v; Text = t; }
        public override string ToString() => Text;
    }
}
