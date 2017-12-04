using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public ObservableCollection<SharedItem> DisplayedItems { get; set; } =
            new ObservableCollection<SharedItem>();

        public List<SharedItem> RootItems { get; set; } =
            new List<SharedItem>();  

        public List<SharedAlbumImage> AlbumImages { get; set; } =
            new List<SharedAlbumImage>();

        private List<SharedItem> _filteredItems;
        
        private string _currentAlbum = null;

        private static void SharedFolderPath_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sharedArea = (SharedArea) d;
            sharedArea.LoadRootItems();
        }

        private static void FilterOutOfPolicy_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sa = (SharedArea)d;
            if (sa.FilterOutOfPolicy)
            {
                sa.ApplyFilterOutOfPolicy();
            }
            else
            {
                sa.UnFilterOutOfPolicy();
            }
        }

        private static void ShowPreviews_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sa = (SharedArea)d;
            if (sa.ShowPreviews)
            {
                sa.ApplyShowPreviews();
            }
            else
            {
                sa.ShowDefaultThumbnails();
            }
        }

        public void ReloadView()
        {
            DisplayedItems.Clear();
            if (_currentAlbum != null)
            {
                CurrentAlbum = _currentAlbum;
                AlbumImages.ForEach(
                    image => DisplayedItems.Add(image));
            }
            else
            {
                CurrentAlbum = "Main folder";
                RootItems.ForEach(
                    item => DisplayedItems.Add(item));
            }

            if (ShowPreviews)
                ApplyShowPreviews();
            else
                ShowDefaultThumbnails();
        }

        public void LoadRootItems()
        {
            RootItems.Clear();
            _currentAlbum = null;
            IsValidSharedFolder = CheckSharedFolderStructure(SharedFolderPath);
            if (IsValidSharedFolder)
            {
                var sharedItemPaths = Directory.GetFileSystemEntries(
                    Path.Combine(SharedFolderPath, "items"));
                foreach (string sharedItemPath in sharedItemPaths)
                {
                    string itemName = Path.GetFileName(sharedItemPath);
                    var fileAttributes = File.GetAttributes(sharedItemPath);
                    SharedItem item;
                    if (fileAttributes.HasFlag(FileAttributes.Directory))
                        item = new SharedAlbum(
                            Name: itemName,
                            SharedArea: this);
                    else
                        item = new SharedImage(
                            Name: itemName,
                            SharedArea: this);

                    item.SharedArea = this;
                    item.Name = itemName;
                
                    if(item.IsValid)
                        RootItems.Add(item);
                }

                if (FilterOutOfPolicy)
                    ApplyFilterOutOfPolicy();
                else
                    UnFilterOutOfPolicy();
            }
            ReloadView();
        }

        private void LoadAlbumImages(SharedAlbum sharedAlbum)
        {
            if (sharedAlbum.IsValid && sharedAlbum.IsPolicyVerified)
            {
                AlbumImages.Clear();
                sharedAlbum.Children.ForEach(
                    image => AlbumImages.Add(image));
                _currentAlbum = sharedAlbum.Name;
            }
            ReloadView();
        }

        private void ShowDefaultThumbnails()
        {
            foreach (SharedItem item in DisplayedItems)
            {
                item.SetDefaultThumbnail();
            }
        }

        private void ApplyShowPreviews()
        {
            DisplayedItems
                .Where(sharedItem => sharedItem.IsPolicyVerified)
                .ToList()
                .ForEach(sharedItem => sharedItem.SetPreviewThumbnail());
        }

        private void ApplyFilterOutOfPolicy()
        {
            _filteredItems = RootItems.Where(item => !item.IsPolicyVerified).ToList();
            var tmp = RootItems.Where(item => !_filteredItems.Contains(item)).ToList();
            RootItems.Clear();
            tmp.ForEach(item => RootItems.Add(item));
        }

        private void UnFilterOutOfPolicy()
        {
            _filteredItems?.ForEach(item => RootItems.Add(item));
            _filteredItems = null;
        }

        private static bool CheckSharedFolderStructure(string sharedFolderPath)
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
                    if (subdirs.All(dir => Path.GetFileName(dir) != requiredDir))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
                return false;
        }
        
        private void SharedAreaItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as SharedAreaItemButton;
            var item = button.Item;
            if (item.IsPolicyVerified)
            {
                if (item is SharedImage)
                    OpenImage(item as SharedImage);
                else if (item is SharedAlbum)
                    LoadAlbumImages(item as SharedAlbum);
            }
        }

        private void OpenImage(SharedImage sharedImage)
        {
            string imagePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.png");
            File.WriteAllBytes(
                bytes: sharedImage.DecryptedBytes, 
                path: imagePath);

            Process.Start(imagePath);
        }
    }
}