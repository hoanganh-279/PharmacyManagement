#nullable disable
namespace PharmacyManagement.Forms.Inventory;

partial class FrmSuaHangNhapKho
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
        ClientSize = new Size(640, 520);
        MinimumSize = new Size(520, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Sửa dòng hàng nhập kho";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        ResumeLayout(false);
    }
}
