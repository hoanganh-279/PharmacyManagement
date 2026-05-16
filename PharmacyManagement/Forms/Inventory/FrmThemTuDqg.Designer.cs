#nullable disable
namespace PharmacyManagement.Forms.Inventory;

partial class FrmThemTuDqg
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
        ClientSize = new Size(1120, 720);
        MinimumSize = new Size(800, 520);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm từ danh mục Dược Quốc Gia (DQG)";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        ResumeLayout(false);
    }
}
