using System.Drawing;
using Pharmacy.BLL;
using Pharmacy.DAL;
using Pharmacy.DTO.Views;

namespace PharmacyManagement.Forms.Report;

public partial class FrmCanhBaoThuoc : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color Mint = Color.FromArgb(232, 245, 233);
    private static readonly Color Warn = Color.FromArgb(251, 140, 0);
    private static readonly Color WarnBg = Color.FromArgb(255, 243, 224);
    private static readonly Color Danger = Color.FromArgb(211, 47, 47);
    private static readonly Color Ink = Color.FromArgb(33, 37, 41);

    private const int NguyHiemHanNgay = 30;

    private readonly ReportService _report = new(new DbContextDAL());
    private string _loaiHienTai = "HET_HANG";

    public FrmCanhBaoThuoc()
    {
        InitializeComponent();

        Load += FrmCanhBaoThuoc_Load;
        btnHetHan.Click += BtnHetHan_Click;
        btnHetHang.Click += BtnHetHang_Click;
        btnLamMoi.Click += BtnLamMoi_Click;
        btnThoat.Click += (_, _) => Close();
    }

    private void FrmCanhBaoThuoc_Load(object? sender, EventArgs e)
    {
        if (!TopLevel)
            btnThoat.Visible = false;

        SetActiveButton(btnHetHang);
        LoadDuLieu("HET_HANG");
    }

    private void BtnHetHan_Click(object? sender, EventArgs e)
    {
        SetActiveButton(btnHetHan);
        LoadDuLieu("HET_HAN");
    }

    private void BtnHetHang_Click(object? sender, EventArgs e)
    {
        SetActiveButton(btnHetHang);
        LoadDuLieu("HET_HANG");
    }

    private void BtnLamMoi_Click(object? sender, EventArgs e) => LoadDuLieu(_loaiHienTai);

    private void SetActiveButton(Button active)
    {
        foreach (var b in new[] { btnHetHan, btnHetHang })
        {
            var on = ReferenceEquals(b, active);
            b.BackColor = on ? Primary : Color.White;
            b.ForeColor = on ? Color.White : PrimaryDark;
            b.Font = new Font("Segoe UI", 9.75F, on ? FontStyle.Bold : FontStyle.Regular);
            b.FlatAppearance.BorderSize = on ? 0 : 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(200, 220, 202);
        }
    }

    private void LoadDuLieu(string loai)
    {
        _loaiHienTai = loai;

        lblTrangThai.Text = loai == "HET_HAN"
            ? "Đang hiển thị: Thuốc sắp hết hạn hoặc đã hết hạn (theo lô / mức thuốc)"
            : "Đang hiển thị: Thuốc tồn dưới mức tối thiểu hoặc hết hàng";

        try
        {
            Cursor = Cursors.WaitCursor;
            dgvCanhBao.DataSource = null;

            if (loai == "HET_HAN")
            {
                var data = _report.LayThuocSapHetHan();
                dgvCanhBao.DataSource = data;
                ConfigureColumnsHetHan();
                ToMauDongHetHan();
                lblSoLuong.Text = $"Tổng: {data.Count:N0} dòng";
            }
            else
            {
                var data = _report.LayThuocTonThap();
                dgvCanhBao.DataSource = data;
                ConfigureColumnsHetHang();
                ToMauDongHetHang();
                lblSoLuong.Text = $"Tổng: {data.Count:N0} dòng";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Lỗi tải dữ liệu:\n{ex.Message}", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblSoLuong.Text = "Tổng: —";
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void ConfigureColumnsHetHan()
    {
        if (dgvCanhBao.Columns.Count == 0) return;
        SetHeader(nameof(ThuocSapHetHanViewDTO.MaThuoc), "Mã");
        SetHeader(nameof(ThuocSapHetHanViewDTO.TenThuoc), "Tên thuốc");
        SetHeader(nameof(ThuocSapHetHanViewDTO.DonViTinh), "ĐVT");
        SetHeader(nameof(ThuocSapHetHanViewDTO.SoLuongTon), "SL tồn");
        SetHeader(nameof(ThuocSapHetHanViewDTO.HanSuDung), "Hạn dùng");
        SetHeader(nameof(ThuocSapHetHanViewDTO.SoNgayConLai), "Còn (ngày)");
        SetHeader(nameof(ThuocSapHetHanViewDTO.TrangThaiHanDung), "Trạng thái HSD");
        if (dgvCanhBao.Columns[nameof(ThuocSapHetHanViewDTO.HanSuDung)] is DataGridViewColumn hsd)
            hsd.DefaultCellStyle.Format = "dd/MM/yyyy";
    }

    private void ConfigureColumnsHetHang()
    {
        if (dgvCanhBao.Columns.Count == 0) return;
        SetHeader(nameof(ThuocTonThapViewDTO.MaThuoc), "Mã");
        SetHeader(nameof(ThuocTonThapViewDTO.TenThuoc), "Tên thuốc");
        SetHeader(nameof(ThuocTonThapViewDTO.DonViTinh), "ĐVT");
        SetHeader(nameof(ThuocTonThapViewDTO.SoLuongTon), "Tồn kho");
        SetHeader(nameof(ThuocTonThapViewDTO.TonToiThieu), "Tồn tối thiểu");
        SetHeader(nameof(ThuocTonThapViewDTO.HanSuDung), "Hạn dùng");
        if (dgvCanhBao.Columns[nameof(ThuocTonThapViewDTO.HanSuDung)] is DataGridViewColumn hsd)
            hsd.DefaultCellStyle.Format = "dd/MM/yyyy";
    }

    private void SetHeader(string colName, string header)
    {
        if (dgvCanhBao.Columns.Contains(colName))
            dgvCanhBao.Columns[colName]!.HeaderText = header;
    }

    private void ToMauDongHetHan()
    {
        ResetRowStyles();

        foreach (DataGridViewRow row in dgvCanhBao.Rows)
        {
            if (row.IsNewRow || row.DataBoundItem is not ThuocSapHetHanViewDTO item)
                continue;

            if (item.SoNgayConLai < 0 || item.TrangThaiHanDung.Contains("hết hạn", StringComparison.OrdinalIgnoreCase))
                ApplyRowStyle(row, Color.White, Danger, Danger);
            else if (item.SoNgayConLai <= NguyHiemHanNgay)
                ApplyRowStyle(row, WarnBg, Color.FromArgb(130, 60, 0), Warn);
            else
                ApplyRowStyle(row, Mint, PrimaryDark, Primary);
        }
    }

    private void ToMauDongHetHang()
    {
        ResetRowStyles();

        foreach (DataGridViewRow row in dgvCanhBao.Rows)
        {
            if (row.IsNewRow || row.DataBoundItem is not ThuocTonThapViewDTO item)
                continue;

            if (item.SoLuongTon <= 0)
                ApplyRowStyle(row, Color.White, Danger, Danger);
            else
                ApplyRowStyle(row, WarnBg, Color.FromArgb(130, 60, 0), Warn);
        }
    }

    private void ResetRowStyles()
    {
        dgvCanhBao.RowsDefaultCellStyle.BackColor = Color.White;
        dgvCanhBao.RowsDefaultCellStyle.ForeColor = Ink;
        dgvCanhBao.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 250);
        dgvCanhBao.AlternatingRowsDefaultCellStyle.ForeColor = Ink;
    }

    private static void ApplyRowStyle(DataGridViewRow row, Color back, Color fore, Color selectionBack)
    {
        row.DefaultCellStyle.BackColor = back;
        row.DefaultCellStyle.ForeColor = fore;
        row.DefaultCellStyle.SelectionBackColor = selectionBack;
        row.DefaultCellStyle.SelectionForeColor = Color.White;
    }
}
