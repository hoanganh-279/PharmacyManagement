#nullable disable
namespace PharmacyManagement.Forms.Report;

partial class FrmBaoCao
{
    private System.ComponentModel.IContainer components;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1080, 720);
        MinimumSize = new Size(960, 600);
        Text = "Báo cáo";
        BackColor = Color.FromArgb(245, 247, 246);
        Font = new Font("Segoe UI", 10F);
        ResumeLayout(false);
    }
}
