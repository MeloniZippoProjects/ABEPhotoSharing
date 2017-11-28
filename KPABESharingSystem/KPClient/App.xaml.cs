using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
#if !SKIP_LOGIN
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
#else
            if (String.IsNullOrEmpty(KPClient.Properties.Settings.Default.Universe))
            {
                //todo: should ask server
                KPClient.Properties.Settings.Default.Universe = @"'anime' 'mario' 'cose'";
                KPClient.Properties.Settings.Default.Save();
            }

            Universe = Universe.FromString(KPClient.Properties.Settings.Default.Universe, false);

            KPService.SuitePath = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");
            KpService = new KPService();
            KpService.Keys.PublicKey = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(),"pub_key"));
            KpService.Keys.PrivateKey = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "priv_key"));
#endif

#if DEBUG
            System.Windows.MessageBox.Show($"Universe is: {((App)Application.Current).Universe}");
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
