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

            var settings = Settings.Default;
            FilterOutOfPolicyCheckBox.IsChecked = settings.FilterOutOfPolicy;
            ShowPreviewsCheckBox.IsChecked = settings.ShowPreviews;
            PreloadDataCheckBox.IsChecked = settings.PreloadData;
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
            SharedArea.NavigateRoot();
        }

        private void ReloadSharedSpaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            SharedArea.LoadRootItems();
            SharedArea.NavigateRoot();
        }

        private void FilterOutOfPolicyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Settings.Default.FilterOutOfPolicy = FilterOutOfPolicyCheckBox.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void ShowPreviewsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowPreviews = ShowPreviewsCheckBox.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void PreloadDataCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Settings.Default.PreloadData = ShowPreviewsCheckBox.IsChecked ?? false;
            Settings.Default.Save();
        }
    }
}