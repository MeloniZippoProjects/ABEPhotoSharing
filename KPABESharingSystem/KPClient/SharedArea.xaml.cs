using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace KPClient
{
    /// <summary>
    /// Interaction logic for SharedArea.xaml
    /// </summary>
    public partial class SharedArea
    {
        public ObservableCollection<SharedItem> DisplayedItems { get; set; } =
            new ObservableCollection<SharedItem>();

        public List<SharedItem> RootItems { get; set; } =
            new List<SharedItem>();

        public List<SharedAlbumImage> AlbumImages { get; set; } =
            new List<SharedAlbumImage>();

        private List<SharedItem> _filteredItems;

        private string _currentAlbum;

        private static void SharedFolderPath_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sharedArea = (SharedArea) d;
            sharedArea.LoadRootItems();
        }

        private static void FilterOutOfPolicy_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sa = (SharedArea) d;
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
            SharedArea sa = (SharedArea) d;
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
                string[] sharedItemPaths = Directory.GetFileSystemEntries(
                    Path.Combine(SharedFolderPath, "items"));
                foreach (string sharedItemPath in sharedItemPaths)
                {
                    string itemName = Path.GetFileName(sharedItemPath);
                    FileAttributes fileAttributes = File.GetAttributes(sharedItemPath);
                    SharedItem item;
                    if (fileAttributes.HasFlag(FileAttributes.Directory))
                        item = new SharedAlbum(
                            name: itemName,
                            sharedArea: this);
                    else
                        item = new SharedImage(
                            name: itemName,
                            sharedArea: this);

                    item.SharedArea = this;
                    item.Name = itemName;

                    if (item.IsValid)
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
            List<SharedItem> tmp = RootItems.Where(item => !_filteredItems.Contains(item)).ToList();
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
                string[] subdirs = Directory.GetDirectories(sharedFolderPath);
                List<string> requiredDirs = new List<string>()
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
            SharedAreaItemButton button = sender as SharedAreaItemButton;
            Debug.Assert(button != null, nameof(button) + " != null");
            SharedItem item = button.Item;
            if (!item.IsPolicyVerified) return;
            switch (item)
            {
                case SharedImage _:
                    OpenImage(item as SharedImage);
                    break;
                case SharedAlbum _:
                    LoadAlbumImages(item as SharedAlbum);
                    break;
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