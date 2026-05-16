using System.Drawing;
using System.Windows.Forms;

namespace PharmacyManagement.Forms.Report;

/// <summary>Menu mẹ báo cáo — điều hướng tới các màn con (theme Pharmacy ALN).</summary>
public class FrmBaoCao : Form
{
    private static readonly Color Primary = Color.FromArgb(46, 125, 50);
    private static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
    private static readonly Color Mint = Color.FromArgb(232, 245, 233);
    private static readonly Color PageBg = Color.FromArgb(245, 247, 246);

    private Panel _pnlHeader = null!;
    private Label _lblTitle = null!;
    private Label _lblSubtitle = null!;
    private FlowLayoutPanel _flowCards = null!;

    public FrmBaoCao()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        _pnlHeader = new Panel();
        _lblTitle = new Label();
        _lblSubtitle = new Label();
        _flowCards = new FlowLayoutPanel();

        SuspendLayout();

        BackColor = PageBg;
        Font = new Font("Segoe UI", 9.75F);
        Text = "Báo cáo";
        MinimumSize = new Size(520, 360);

        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Height = 72;
        _pnlHeader.BackColor = Color.White;
        _pnlHeader.Padding = new Padding(20, 14, 20, 10);

        _lblTitle.AutoSize = true;
        _lblTitle.Text = "Báo cáo";
        _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        _lblTitle.ForeColor = PrimaryDark;
        _lblTitle.Location = new Point(20, 14);

        _lblSubtitle.AutoSize = true;
        _lblSubtitle.Text = "Chọn loại báo cáo cần xem";
        _lblSubtitle.Font = new Font("Segoe UI", 9.5F);
        _lblSubtitle.ForeColor = Color.FromArgb(97, 97, 97);
        _lblSubtitle.Location = new Point(22, 42);

        _pnlHeader.Controls.Add(_lblSubtitle);
        _pnlHeader.Controls.Add(_lblTitle);

        _flowCards.Dock = DockStyle.Fill;
        _flowCards.AutoScroll = true;
        _flowCards.Padding = new Padding(24, 20, 24, 24);
        _flowCards.WrapContents = true;
        _flowCards.BackColor = PageBg;

        _flowCards.Controls.Add(CreateReportCard(
            "Cảnh báo tồn / hạn",
            "Thuốc tồn thấp, sắp hết hạn và đã hết hạn theo lô.",
            Primary,
            () =>
            {
                using var frm = new FrmCanhBaoThuoc();
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }));

        Controls.Add(_flowCards);
        Controls.Add(_pnlHeader);

        ResumeLayout(false);
    }

    private static Panel CreateReportCard(string title, string desc, Color accent, Action onClick)
    {
        var card = new Panel
        {
            Size = new Size(280, 120),
            BackColor = Color.White,
            Margin = new Padding(0, 0, 16, 16),
            Cursor = Cursors.Hand
        };

        var stripe = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = accent
        };

        var lblT = new Label
        {
            Text = title,
            AutoSize = false,
            Bounds = new Rectangle(20, 16, 240, 28),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = PrimaryDark
        };

        var lblD = new Label
        {
            Text = desc,
            AutoSize = false,
            Bounds = new Rectangle(20, 48, 240, 56),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(97, 97, 97)
        };

        void Hover(bool on)
        {
            card.BackColor = on ? Mint : Color.White;
        }

        card.MouseEnter += (_, _) => Hover(true);
        card.MouseLeave += (_, _) => Hover(false);
        card.Click += (_, _) => onClick();
        foreach (Control c in new Control[] { lblT, lblD, stripe })
        {
            c.Click += (_, _) => onClick();
            c.MouseEnter += (_, _) => Hover(true);
            c.MouseLeave += (_, _) => Hover(false);
        }

        card.Controls.Add(lblD);
        card.Controls.Add(lblT);
        card.Controls.Add(stripe);
        return card;
    }
}
