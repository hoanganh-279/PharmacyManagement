using System.Globalization;
using PharmacyManagement.Forms.Auth;
using PharmacyManagement.Forms.Main;

namespace PharmacyManagement;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var vi = CultureInfo.GetCultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture = vi;
        CultureInfo.DefaultThreadCurrentUICulture = vi;
        ConnectionSettings.ApplyFromJsonFile();
        while (true)
        {
            using var login = new FrmLogin();
            if (login.ShowDialog() != DialogResult.OK)
                return;

            var dash = new FrmDashboard();
            Application.Run(dash);
            if (!dash.ReLoginRequested)
                break;
        }
    }
}
