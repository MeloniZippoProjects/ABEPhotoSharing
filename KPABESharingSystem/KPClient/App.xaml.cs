using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using KPClient.Properties;
using KPServices;
using MessageBox = System.Windows.MessageBox;

namespace KPClient
{
    /// <inheritdoc />
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App
    {
        public Universe Universe;
        public KpService KpService = new KpService();
        public KpRestClient KpRestClient;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CheckAndSetDefaultPathSettings();

            Settings settings = Settings.Default;
            KpService.SuitePath = settings.KPSuitePath;
            while (!KpService.ValidClientSuite)
            {
                MessageBox.Show("Invalid path for the kpabe suite!");
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        settings.KPSuitePath = fbd.SelectedPath;
                        settings.Save();
                        KpService.SuitePath = fbd.SelectedPath;
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

                using (TemporaryBytes publicKey = File.ReadAllBytes(settings.PublicKeyPath),
                    privateKey = File.ReadAllBytes(settings.PrivateKeyPath))
                {
                    KpService.Keys.PublicKey = publicKey;
                    KpService.Keys.PrivateKey = privateKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if !SKIP_LOGIN
                GetSettingsFromServer();
#else
                Universe = Universe.FromString(@"'anime' 'mario' 'cose'");
                KpService.Keys.PublicKey = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "pub_key"));
                KpService.Keys.PrivateKey =
File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "priv_key"));
#endif
            }

#if DEBUG
            MessageBox.Show($"Universe is: {((App)Current).Universe}");
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowDialog();
            Shutdown();
        }

        private void GetSettingsFromServer()
        {
            Settings settings = Settings.Default;

            KpRestClient = new KpRestClient(
                host: settings.ServerAddress,
                port: settings.ServerPort,
                useHttps: true
            );

            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            if (!KpRestClient.IsLogged)
                Environment.Exit(0);

            Universe = KpRestClient.GetUniverse();
            using (TemporaryBytes publicKey = KpRestClient.GetPublicKey(),
                privateKey = KpRestClient.GetPrivateKey())
            {
                if (Universe == null || publicKey.Bytes == null || privateKey.Bytes == null)
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
        }

        private void CheckAndSetDefaultPathSettings()
        {
            Settings settings = Settings.Default;

            if (String.IsNullOrEmpty(settings.KPSuitePath))
                settings.KPSuitePath = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");

            if (String.IsNullOrEmpty(settings.PublicKeyPath))
                settings.PublicKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "pub_key");

            if (String.IsNullOrEmpty(settings.PrivateKeyPath))
                settings.PrivateKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "priv_key");

            settings.Save();
        }
    }
}