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
        public KPService KpService = new KPService();
        public string Username;
        public string Password;
        public RestClient RestClient;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CheckAndPopulateDefaultSettings();
            var settings = KPClient.Properties.Settings.Default;
            KPService.SuitePath = settings.KPSuite;
            if (!KPService.ValidClientSuite)
            {
                //todo: prompt error and ask to choose correct path
                Shutdown();
            }

            try
            {
                Universe = Universe.FromString(settings.Universe);
                KpService.Keys.PublicKey = File.ReadAllBytes(settings.PublicKey);
                KpService.Keys.PrivateKey = File.ReadAllBytes(settings.PrivateKey);
            }
            catch (Exception)
            {
                //todo: this should mean we have to contact the server
                GetKeys();
            }

#if !SKIP_LOGIN

            GetKeys();
            RestClient = new RestClient
            {
                Host = KPClient.Properties.Settings.Default.ServerAddress,
                Port = KPClient.Properties.Settings.Default.ServerPort
            };

            if (Username == null || Password == null)
            {
                LoginForm loginForm = new LoginForm();
                loginForm.ShowDialog();
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
            mainWindow.ShowDialog();
            Shutdown();
        }

        private void GetKeys()
        {
            //
        }

        private void CheckAndPopulateDefaultSettings()
        {
            var settings = KPClient.Properties.Settings.Default;

            if (String.IsNullOrEmpty(settings.ServerAddress))
                settings.ServerAddress = @"localhost";
            
            if (settings.ServerPort == 0)
                settings.ServerPort = 1234;
            
            if(String.IsNullOrEmpty(settings.KPSuite))
                settings.KPSuite = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");

            if(String.IsNullOrEmpty(settings.PublicKey))
                settings.PublicKey = Path.Combine(Directory.GetCurrentDirectory(), "pub_key");

            if (String.IsNullOrEmpty(settings.PrivateKey))
                settings.PrivateKey = Path.Combine(Directory.GetCurrentDirectory(), "priv_key");
            
            settings.Save();
        }
    }
}
