using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Grapevine.Client;
using KPServices;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Universe Universe;
        public KPService KpService;
        public string Username;
        public string Password;
        public RestClient RestClient;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {

            if (String.IsNullOrEmpty(KPClient.Properties.Settings.Default.ServerAddress))
            {
                KPClient.Properties.Settings.Default.ServerAddress = @"localhost";
                KPClient.Properties.Settings.Default.Save();
            }

            if (KPClient.Properties.Settings.Default.ServerPort == 0)
            {
                KPClient.Properties.Settings.Default.ServerPort = 1234;
                KPClient.Properties.Settings.Default.Save();
            }
            RestClient = new RestClient
            {
                Host = KPClient.Properties.Settings.Default.ServerAddress,
                Port = KPClient.Properties.Settings.Default.ServerPort
            };

            if (Username == null || Password == null)
            {
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
            }

            if (String.IsNullOrEmpty(KPClient.Properties.Settings.Default.Universe))
            {
                //todo: should ask server
                KPClient.Properties.Settings.Default.Universe = @"'anime' 'mario' 'cose'";
                KPClient.Properties.Settings.Default.Save();
            }

            ((App)Application.Current).Universe = Universe.FromString(KPClient.Properties.Settings.Default.Universe, false);
#if DEBUG
            System.Windows.MessageBox.Show($"Universe is: {((App)Application.Current).Universe}");
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
