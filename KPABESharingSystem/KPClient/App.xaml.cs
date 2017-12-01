using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Grapevine.Client;
using Grapevine.Shared;
using KPServices;
using Newtonsoft.Json;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Universe Universe;
        public KPService KpService = new KPService();
        public KPRestClient KPRestClient;
        public string Username;
        public string Password;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CheckAndPopulateDefaultSettings();
            var settings = KPClient.Properties.Settings.Default;
            KPService.SuitePath = settings.KPSuitePath;
            if (!KPService.ValidClientSuite)
            {
                //todo: prompt error and ask to choose correct path
                Shutdown();
            }

            try
            {
                Universe = Universe.FromString(settings.Universe);
                KpService.Keys.PublicKey = File.ReadAllBytes(settings.PublicKeyPath);
                KpService.Keys.PrivateKey = File.ReadAllBytes(settings.PrivateKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                GetSettingsFromServer();
            }

#if !SKIP_LOGIN

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
            KpService.Keys.PublicKeyPath = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(),"pub_key"));
            KpService.Keys.PrivateKeyPath = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "priv_key"));
#endif

#if DEBUG
            System.Windows.MessageBox.Show($"Universe is: {((App)Application.Current).Universe}");
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowDialog();
            Shutdown();
        }

        private void GetSettingsFromServer()
        {
            //todo: setup ssl/tls

            var settings = KPClient.Properties.Settings.Default;

            KPRestClient = new KPRestClient(
                Host : settings.ServerAddress,
                Port : settings.ServerPort
            );

            if (Username == null || Password == null || !KPRestClient.Login(Username, Password))
            {
                LoginForm loginForm = new LoginForm();
                loginForm.ShowDialog();
                if (!KPRestClient.IsLogged)
                    Shutdown();
            }

            byte[] publicKey = KPRestClient.GetPublicKey();
            byte[] privateKey = KPRestClient.GetPrivateKey();

            if (publicKey == null || privateKey == null)
            {
                MessageBox.Show("Incorrect user configuration!\nContact administrator for the system");
                Shutdown();
                return;
            }

            KpService.Keys.PublicKey = publicKey;
            KpService.Keys.PrivateKey = privateKey;

            File.WriteAllBytes(
                path: settings.PublicKeyPath,
                bytes: publicKey);
            File.WriteAllBytes(
                path: settings.PrivateKeyPath,
                bytes: privateKey);

        }

        private void CheckAndPopulateDefaultSettings()
        {
            var settings = KPClient.Properties.Settings.Default;

            if (String.IsNullOrEmpty(settings.ServerAddress))
                settings.ServerAddress = @"localhost";
            
            if (settings.ServerPort == 0)
                settings.ServerPort = 1234;
            
            if(String.IsNullOrEmpty(settings.KPSuitePath))
                settings.KPSuitePath = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");

            if(String.IsNullOrEmpty(settings.PublicKeyPath))
                settings.PublicKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "pub_key");

            if (String.IsNullOrEmpty(settings.PrivateKeyPath))
                settings.PrivateKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "priv_key");
            
            settings.Save();
        }
    }
}
