#nullable disable
namespace PharmacyManagement.Forms.Main;

partial class FrmDashboard
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
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(1024, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Pharmacy Management ALN";
        BackColor = Color.FromArgb(245, 247, 246);
        Font = new Font("Segoe UI", 10F);
        ResumeLayout(false);
    }
}
