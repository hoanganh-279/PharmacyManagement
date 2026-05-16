#nullable disable
namespace PharmacyManagement.Forms.Inventory;

partial class FrmSuaThuoc
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
        ClientSize = new Size(520, 520);
        MinimumSize = new Size(440, 400);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Sửa thông tin thuốc";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        ResumeLayout(false);
    }
}
