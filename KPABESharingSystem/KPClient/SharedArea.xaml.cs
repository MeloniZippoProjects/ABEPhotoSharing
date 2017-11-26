using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace KPClient
{
    /// <summary>
    /// Interaction logic for SharedArea.xaml
    /// </summary>
    public partial class SharedArea : UserControl
    {
        public ObservableCollection<SharedAreaItem> SharedAreaItems { get; set; } =
            new ObservableCollection<SharedAreaItem>();  

        public ObservableCollection<SharedAreaItem> AlbumItems { get; set; } =
            new ObservableCollection<SharedAreaItem>();

        public bool FilterOutOfPolicy
        {
            get { return (bool)GetValue(FilterOutOfPolicyProperty); }
            set { SetValue(FilterOutOfPolicyProperty, value); }
        }
        public static readonly DependencyProperty FilterOutOfPolicyProperty =
            DependencyProperty.Register("FilterOutOfPolicy", typeof(bool), typeof(SharedArea), new PropertyMetadata(false));
        
        public bool ShowPreviews
        {
            get { return (bool)GetValue(ShowPreviewsProperty); }
            set { SetValue(ShowPreviewsProperty, value); }
        }
        public static readonly DependencyProperty ShowPreviewsProperty =
            DependencyProperty.Register("ShowPreviews", typeof(bool), typeof(SharedArea), new PropertyMetadata(false));

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(SharedArea), new PropertyMetadata(false));
        
        public string RootPath
        {
            get { return (string)GetValue(RootPathProperty); }
            set
            {
                SetValue(RootPathProperty, value);
            }
        }
        
        public static readonly DependencyProperty RootPathProperty = DependencyProperty.Register(
                "RootPath",
                typeof(string),
                typeof(SharedArea),
                new PropertyMetadata(
                    null,
                    UpdateRootPath)
                );

        private static void UpdateRootPath(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sharedArea = (SharedArea) d;
            if (checkSharedFolderStructure(sharedArea.RootPath))
                sharedArea.IsValid = true;
            sharedArea.LoadSharedArea();
        }


        public SharedArea()
        {
            InitializeComponent();
        }
        
        private List<SharedAreaItem> filteredSharedAreaItems = new List<SharedAreaItem>();

        public void LoadSharedArea()
        {
            if (checkSharedFolderStructure(RootPath))
            {
                var sharedItemPaths = Directory.GetFileSystemEntries(
                    Path.Combine(RootPath, "items"));
                foreach (string sharedItemPath in sharedItemPaths)
                {
                    string itemName = Path.GetFileNameWithoutExtension(sharedItemPath);

                    if (File.GetAttributes(sharedItemPath).HasFlag(FileAttributes.Directory))
                    {
                        if (!ValidAlbum(itemName))
                            continue;
                    }
                    else if (!ValidImage(itemName))
                            continue;

                    SharedAreaItem item = new SharedAreaItem()
                    {
                        Path = sharedItemPath,
                        Name = itemName
                    };
                    SharedAreaItems.Add(item);
                }

                if (ShowPreviews)
                    ApplyShowPreviews();
                else
                    ShowDefaultThumbnails();

                if (FilterOutOfPolicy)
                    ApplyFilterOutOfPolicy();
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

        private void ApplyShowPreviews()
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

        private void ApplyFilterOutOfPolicy()
        {
            filteredSharedAreaItems.AddRange(
                SharedAreaItems.Where(sharedItem => sharedItem.IsPolicyVerified == false));
            foreach (SharedAreaItem filteredItem in filteredSharedAreaItems)
            {
                SharedAreaItems.Remove(filteredItem);
            }
        }

        private void UnFilterOutOfPolicy()
        {
        }

        private static bool checkSharedFolderStructure(string sharedFolderPath)
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

        private bool ValidImage(string imageName)
        {
            string imagePath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items",
                $"{imageName}.kpabe");
            string keyPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", $"{imageName}.key");

            return File.Exists(imagePath) && File.Exists(keyPath);
        }

        private bool ValidAlbum(string albumName)
        {
            string albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
            string keyPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", $"{albumName}.key");
            if (Directory.Exists(albumPath) && File.Exists(keyPath))
            {
                string firstImagePath = Path.Combine(albumPath, $"{albumName}.0.png.kpabe");
                return File.Exists(firstImagePath);
            }
            else
                return false;
        }

        private void SharedAreaItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as SharedAreaItemButton;
            var item = button.Item;
            if (item.IsPolicyVerified ?? false)
            {
                switch (item.Type)
                {
                    case SharedAreaItem.SharedItemType.Album:
                    {
                        break;
                    }
                    case SharedAreaItem.SharedItemType.Image:
                    {
                        break;
                    }
                }
            }
        }
    }
}