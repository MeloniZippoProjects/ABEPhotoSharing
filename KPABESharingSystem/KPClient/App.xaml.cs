using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using KPServices;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CheckAndSetDefaultPathSettings();

            var settings = KPClient.Properties.Settings.Default;
            KPService.SuitePath = settings.KPSuitePath;
            while (!KPService.ValidClientSuite)
            {
                MessageBox.Show("Invalid path for the kpabe suite!");
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        settings.KPSuitePath = fbd.SelectedPath;
                        settings.Save();
                        KPService.SuitePath = fbd.SelectedPath;
                    }
                    else
                    {
                        Shutdown();
                        return;
                    }
                }
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
#if !SKIP_LOGIN
                GetSettingsFromServer();
#else
                Universe = Universe.FromString(@"'anime' 'mario' 'cose'");
                KpService.Keys.PublicKey = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "pub_key"));
                KpService.Keys.PrivateKey = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "priv_key"));
#endif
            }

#if DEBUG
                System.Windows.MessageBox.Show($"Universe is: {((App)Application.Current).Universe}");
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowDialog();
            Shutdown();
        }

        private void GetSettingsFromServer()
        {
            var settings = KPClient.Properties.Settings.Default;

            KPRestClient = new KPRestClient(
                Host : settings.ServerAddress,
                Port : settings.ServerPort,
                UseHTTPS: true
            );

            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            if (!KPRestClient.IsLogged)
                Environment.Exit(0);

            Universe = KPRestClient.GetUniverse();
            byte[] publicKey = KPRestClient.GetPublicKey();
            byte[] privateKey = KPRestClient.GetPrivateKey();

            if (Universe == null || publicKey == null || privateKey == null)
            {
                MessageBox.Show("Incorrect user configuration!\nContact administrator for the system");
                Environment.Exit(0);
            }

            KpService.Keys.PublicKey = publicKey;
            KpService.Keys.PrivateKey = privateKey;

            settings.Universe = Universe.ToString();
            settings.Save();

            File.WriteAllBytes(
                path: settings.PublicKeyPath,
                bytes: publicKey);

            File.WriteAllBytes(
                path: settings.PrivateKeyPath,
                bytes: privateKey);
            
        }

        private void CheckAndSetDefaultPathSettings()
        {
            var settings = KPClient.Properties.Settings.Default;
            
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
