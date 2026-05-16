#nullable disable
using Pharmacy.BLL;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;
using PharmacyManagement.Helpers;

namespace PharmacyManagement.Forms.Inventory;

/// <summary>Tra cứu, sửa, ngừng kinh doanh thuốc đã nhập — không thêm mới (dùng Thêm hàng hóa).</summary>
public partial class FrmQuanLyThuoc : Form
{
    private const string GoiYMacDinh =
        "Chỉ tra cứu, sửa hoặc ngừng kinh doanh thuốc đã có. "
        + "Để thêm thuốc mới vào danh mục, dùng menu «Thêm hàng hóa».";

    private readonly MedicineService _medicine = new(new DbContextDAL());

    private TextBox _txtTimKiem;
    private Button _btnTim;
    private Button _btnXoaTim;
    private DataGridView _grid;
    private Button _btnSua;
    private Button _btnXoa;
    private Label _lblGoiY;
    private Label _lblTong;

    public FrmQuanLyThuoc()
    {
        InitializeComponent();
        BuildLayout();
        Load += FrmQuanLyThuoc_Load;
    }

    private void FrmQuanLyThuoc_Load(object sender, EventArgs e) => NapDanhSach();

    private void BuildLayout()
    {
        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = InventoryUiKit.PageBg };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20, 16, 20, 16)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(InventoryUiKit.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 32,
            Text = "Danh mục thuốc trong nhà thuốc",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = InventoryUiKit.PrimaryDark
        };

        _lblGoiY = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            MinimumSize = new Size(0, 36),
            ForeColor = InventoryUiKit.Muted,
            Font = new Font("Segoe UI", 9.25F, FontStyle.Italic),
            Text = GoiYMacDinh
        };

        _txtTimKiem = new TextBox { BorderStyle = BorderStyle.FixedSingle };
        _btnTim = InventoryUiKit.TaoNut("Tìm kiếm", InventoryUiKit.Primary);
        _btnXoaTim = InventoryUiKit.TaoNut("Xóa", InventoryUiKit.Muted, outline: true);
        InventoryMedicineSearchKit.GanSuKienTimKiem(_txtTimKiem, _btnTim, NapDanhSach, XoaTimKiem);

        var barTim = InventoryUiKit.TaoThanhTimKiem(_txtTimKiem, _btnTim, btnXoa: _btnXoaTim);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 48),
            Padding = new Padding(0, 8, 0, 0)
        };
        _lblTong = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Left,
            ForeColor = InventoryUiKit.Muted,
            Text = "0 thuốc",
            Padding = new Padding(0, 10, 0, 0)
        };
        _btnSua = InventoryUiKit.TaoNut("Sửa", InventoryUiKit.Primary);
        _btnXoa = InventoryUiKit.TaoNut("Xóa", InventoryUiKit.Danger);
        _btnSua.Enabled = false;
        _btnXoa.Enabled = false;
        _btnSua.Click += (_, _) => MoFormSua();
        _btnXoa.Click += (_, _) => XoaThuocDaChon();
        var actions = InventoryUiKit.TaoActionBar(_btnSua, _btnXoa);
        actions.Dock = DockStyle.Fill;
        footer.Controls.Add(_lblTong);
        footer.Controls.Add(actions);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            MinimumSize = new Size(0, 120),
            RowTemplate = { Height = 34 }
        };
        _grid.ColumnHeadersDefaultCellStyle.BackColor = InventoryUiKit.MintBg;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = InventoryUiKit.PrimaryDark;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _grid.ColumnHeadersHeight = 38;
        _grid.DefaultCellStyle.SelectionBackColor = InventoryUiKit.MintBg;
        _grid.DefaultCellStyle.SelectionForeColor = InventoryUiKit.Ink;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 250);
        _grid.CellDoubleClick += (_, _) => MoFormSua();
        _grid.SelectionChanged += (_, _) => CapNhatNutHanhDong();
        KhoiTaoCotLuoi();

        card.Controls.Add(_grid);
        card.Controls.Add(footer);
        card.Controls.Add(barTim);
        card.Controls.Add(_lblGoiY);
        card.Controls.Add(title);
        root.Controls.Add(card);
        Controls.Add(root);
    }

    private void KhoiTaoCotLuoi()
    {
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.TenThuoc),
            HeaderText = "Tên thuốc",
            FillWeight = 160
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.HoatChat),
            HeaderText = "Hoạt chất",
            FillWeight = 110
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.HamLuong),
            HeaderText = "Hàm lượng",
            FillWeight = 80
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.TenNhomThuoc),
            HeaderText = "Nhóm",
            FillWeight = 90
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.DonViTinh),
            HeaderText = "Đơn vị",
            FillWeight = 55
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.GiaNhap),
            HeaderText = "Giá nhập",
            FillWeight = 75,
            DefaultCellStyle = { Format = "N0" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.GiaBan),
            HeaderText = "Giá bán",
            FillWeight = 75,
            DefaultCellStyle = { Format = "N0" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.SoLuongTon),
            HeaderText = "Tồn",
            FillWeight = 50
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.HanSuDung),
            HeaderText = "HSD",
            FillWeight = 75,
            DefaultCellStyle = { Format = "dd/MM/yyyy", NullValue = "" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DanhSachThuocViewDTO.TrangThai),
            HeaderText = "Trạng thái",
            FillWeight = 90
        });
    }

    private void NapDanhSach()
    {
        var parsed = InventoryMedicineSearchKit.PhanTichTuKhoa(_txtTimKiem.Text, MedicineSearchMode.DanhMuc);
        if (!parsed.CoTheTim)
        {
            CapNhatGoiY(parsed.ThongBaoGoiY, InventoryUiKit.Warn);
            return;
        }

        try
        {
            var list = _medicine.TimKiemDanhSachThuoc(parsed.TuKhoaTim).ToList();
            _grid.DataSource = list;
            _lblTong.Text = list.Count + " thuốc";
            CapNhatGoiY(
                InventoryMedicineSearchKit.ThongBaoKetQuaDanhMuc(parsed.TuKhoaTim, list.Count),
                list.Count == 0 && !string.IsNullOrEmpty(parsed.TuKhoaTim)
                    ? InventoryUiKit.Warn
                    : InventoryUiKit.Muted);
            CapNhatNutHanhDong();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void XoaTimKiem()
    {
        _txtTimKiem.Clear();
        CapNhatGoiY(GoiYMacDinh, InventoryUiKit.Muted);
        NapDanhSach();
        _txtTimKiem.Focus();
    }

    private void CapNhatGoiY(string text, Color mau)
    {
        _lblGoiY.Text = text;
        _lblGoiY.ForeColor = mau;
    }

    private void CapNhatNutHanhDong()
    {
        var row = ThuocDangChon();
        var coDong = row is not null;
        _btnSua.Enabled = coDong;
        _btnXoa.Enabled = coDong
            && !string.Equals(row.TrangThai, "Ngừng bán", StringComparison.OrdinalIgnoreCase);
    }

    private DanhSachThuocViewDTO ThuocDangChon()
    {
        if (_grid.CurrentRow?.DataBoundItem is DanhSachThuocViewDTO t)
            return t;
        return null;
    }

    private void MoFormSua()
    {
        var row = ThuocDangChon();
        if (row is null)
        {
            MessageBox.Show(this, "Chọn một dòng thuốc trong lưới.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new FrmSuaThuoc(row.MaThuoc);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            NapDanhSach();
    }

    private void XoaThuocDaChon()
    {
        var row = ThuocDangChon();
        if (row is null)
        {
            MessageBox.Show(this, "Chọn một dòng thuốc trong lưới.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.Equals(row.TrangThai, "Ngừng bán", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "Thuốc này đã ngừng kinh doanh.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var ok = MessageBox.Show(this,
            "Xóa thuốc «" + row.TenThuoc + "» khỏi danh mục bán?\n\n"
            + "Hệ thống sẽ chuyển sang trạng thái «Ngừng bán» (giữ lịch sử nhập/bán).",
            Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ok != DialogResult.Yes)
            return;

        try
        {
            _medicine.XoaThuoc(row.MaThuoc);
            NapDanhSach();
            MessageBox.Show(this, "Đã ngừng kinh doanh thuốc.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
