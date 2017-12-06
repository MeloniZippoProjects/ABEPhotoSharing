using System.IO;
using System.Windows;
using System.Windows.Forms;
using KPClient.Properties;
using Application = System.Windows.Application;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            App app = ((App) Application.Current);
            UsernameLabel.Content = app.Username;
            UniverseLabel.Content = UniverseLabel.Content.ToString()
                .Replace("{}", app.Universe.ToString());
        }

        private void OpenUploadImagesWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            UploadImagesWindow uploadImagesWindow = new UploadImagesWindow();
            uploadImagesWindow.ShowDialog();
            SharedArea.LoadRootItems();
        }

        private void SetSharedSpaceLocationButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Settings.Default.SharedFolderPath = fbd.SelectedPath;
                    Settings.Default.Save();
                }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = Settings.Default;
            settings.Universe = null;
            File.Delete(settings.PublicKeyPath);
            settings.PublicKeyPath = null;
            File.Delete(settings.PrivateKeyPath);
            settings.PrivateKeyPath = null;
            settings.Save();

            Application.Current.Shutdown();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SharedArea.LoadRootItems();
        }

        private void ReloadSharedSpaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            SharedArea.ReloadView();
        }
    }
}