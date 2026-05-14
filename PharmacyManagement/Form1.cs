using Pharmacy.Common;

namespace PharmacyManagement;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = UserSession.IsAuthenticated
            ? $"Pharmacy Management ALN — {UserSession.HoTen}"
            : "Pharmacy Management ALN";
    }
}
