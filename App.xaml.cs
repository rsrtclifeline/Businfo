using Businfo.Helper;
using Businfo.Login;
using System.Windows;

namespace Businfo
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool isLoggedIn = false;
            try
            {
                isLoggedIn = await ApiClient.VerifySavedSessionAsync();
            }
            catch
            {
                isLoggedIn = false;
            }

            if (isLoggedIn)
            {
                var main = new MainWindow();
                this.MainWindow = main;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                main.Show();
            }
            else
            {
                var login = new LoginWindow();
                this.MainWindow = login;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                login.Show();
            }
        }
    }
}