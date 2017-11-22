using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.IconPacks;
using Button = System.Windows.Controls.Button;
using Path = System.IO.Path;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<SharedAreaItem> SharedAreaItems { get; set; } = new ObservableCollection<SharedAreaItem>();
        private List<SharedAreaItem> filteredSharedAreaItems = new List<SharedAreaItem>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSharedArea();
        }

        private void OpenUploadImagesWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var uploadImagesWindow = new UploadImagesWindow();
            uploadImagesWindow.ShowDialog();
        }

        private void SetSharedSpaceLocationButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.SharedFolderPath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    UpdateSharedArea();
                }
            }
        }

        private void ReloadSharedSpaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSharedArea();
        }

        private void HideOutOfPolicyCheckBox_OnChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ShowPreviewCheckBox_OnChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPreviewCheckBox.IsChecked ?? false)
                ShowPreviews();
            else
                ShowDefaultThumbnails();
        }

        //todo: consider generalization for albums
        private void UpdateSharedArea()
        {
            SharedAreaItems.Clear();

            string sharedFolderPath = Properties.Settings.Default.SharedFolderPath;

            if (checkSharedFolderStructure(sharedFolderPath))
            {
                var sharedItemPaths = Directory.GetFileSystemEntries(
                    Path.Combine(sharedFolderPath, "items"));
                foreach (string sharedItemPath in sharedItemPaths)
                {
                    SharedAreaItem item = new SharedAreaItem()
                    {
                        Path = sharedItemPath,
                        Name = Path.GetFileNameWithoutExtension(sharedItemPath)
                    };
                    SharedAreaItems.Add(item);    
                }

                if (ShowPreviewCheckBox.IsChecked ?? false)
                    ShowPreviews();
                else
                    ShowDefaultThumbnails();

                if (HideOutOfPolicyCheckBox.IsChecked ?? false)
                    FilterOutOfPolicy();
            }
        }

        private void ShowDefaultThumbnails()
        {
            foreach (SharedAreaItem sharedAreaItem in SharedAreaItems)
            {
                var fileAttributes = File.GetAttributes(sharedAreaItem.Path);
                if (fileAttributes.HasFlag(FileAttributes.Directory))
                {
                    sharedAreaItem.Type = SharedAreaItem.SharedItemType.Album;
                    sharedAreaItem.Thumbnail = SharedAreaItem.DefaultAlbumThumbnail;
                }
                else
                {
                    sharedAreaItem.Type = SharedAreaItem.SharedItemType.Image;
                    sharedAreaItem.Thumbnail = SharedAreaItem.DefaultImageThumbnail;
                }
            }
        }

        //todo: consider albums, policy verification, decryption...
        private void ShowPreviews()
        {
            SharedAreaItems
                .Where(sharedItem => /*sharedItem.IsPolicyVerified == true &&*/
                                                sharedItem.Type == SharedAreaItem.SharedItemType.Image)
                .ToList()
                .ForEach(sharedItem =>
                {
                    BitmapImage thumbnail = new BitmapImage(new Uri(sharedItem.Path));
                    sharedItem.Thumbnail = thumbnail;
                });
        }

        //todo: consider generalization for albums
        private void FilterOutOfPolicy()
        {
            filteredSharedAreaItems.AddRange(
                SharedAreaItems.Where(sharedItem => sharedItem.IsPolicyVerified == false));
            foreach (SharedAreaItem filteredItem in filteredSharedAreaItems)
            {
                SharedAreaItems.Remove(filteredItem);
            }
        }

        private bool checkSharedFolderStructure(string sharedFolderPath)
        {
            if (Directory.Exists(sharedFolderPath))
            {
                var subdirs = Directory.GetDirectories(sharedFolderPath);
                var requiredDirs = new List<string>()
                {
                    "items",
                    "keys"
                };

                foreach (string requiredDir in requiredDirs)
                {
                    if (!subdirs.Any(
                            dir => Path.GetFileName(dir) == requiredDir))
                    {
                        return false;
                    }
                }

                //todo: further checking? Maybe regexes on the file names?

                return true;
            }
            else
                return false;
        }
    }
}
