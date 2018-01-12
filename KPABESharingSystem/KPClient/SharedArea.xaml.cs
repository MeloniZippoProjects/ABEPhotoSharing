using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            sharedArea._currentAlbum = null;
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

        private static void PreloadThumbnails_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sa = (SharedArea)d;
            if (sa.PreloadThumbnails)
            {
                sa.RootItems.ForEach(item => item.PreloadThumbnail());
            }
            else
            {
                sa.LoadRootItems();   
            }
        }

        private static void ShowThumbnails_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sa = (SharedArea) d;
            if (sa.ShowThumbnails)
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

            if (ShowThumbnails)
                ApplyShowPreviews();
            else
                ShowDefaultThumbnails();

            foreach (SharedItem displayedItem in DisplayedItems)
            {
                displayedItem.PreloadItemKeys();
            }
        }

        public void LoadRootItems()
        {
            RootItems.Clear();
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

                    if (PreloadThumbnails)
                        item.PreloadThumbnail();
                }

                if (FilterOutOfPolicy)
                    ApplyFilterOutOfPolicy();
                else
                    UnFilterOutOfPolicy();
            }
            ReloadView();
        }

        public void NavigateRoot()
        {
            _currentAlbum = null;
            ReloadView();
        }

        public async void LoadAlbumImages(SharedAlbum sharedAlbum)
        {
            if (sharedAlbum.IsValid && await sharedAlbum.IsPolicyVerified())
            {
                AlbumImages.Clear();
                var albumImages = await sharedAlbum.GetChildren();
                albumImages.ForEach(
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
                .OrderBy(sharedItem => sharedItem.Name)
                .ToList()
                .ForEach(sharedItem => sharedItem.SetPreviewThumbnail());
        }

        private async void ApplyFilterOutOfPolicy()
        {
            await Task.WhenAll(RootItems
                .Select(item => item.IsPolicyVerified())
                .ToArray()
            );

            _filteredItems = RootItems.Where(item => !item.IsPolicyVerified().Result).ToList();
            List<SharedItem> tmp = RootItems.Where(item => item.IsPolicyVerified().Result).ToList();
            RootItems.Clear();
            tmp.ForEach(item => RootItems.Add(item));
            ReloadView();
        }

        private void UnFilterOutOfPolicy()
        {
            _filteredItems?.ForEach(item => RootItems.Add(item));
            _filteredItems = null;
            ReloadView();
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
    }
}