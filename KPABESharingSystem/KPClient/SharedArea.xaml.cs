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
        public ObservableCollection<SharedItem> SharedItems { get; set; } =
            new ObservableCollection<SharedItem>();  

        public ObservableCollection<SharedItem> AlbumImages { get; set; } =
            new ObservableCollection<SharedItem>();

        private List<SharedItem> filteredSharedAreaItems;
        
        private string CurrentAlbum = null;

        private static void SharedFolderPath_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sharedArea = (SharedArea) d;
            if (checkSharedFolderStructure(sharedArea.SharedFolderPath))
                sharedArea.IsFolderPathValid = true;
            sharedArea.LoadSharedArea();
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
        
        public void LoadSharedArea()
        {
            if (IsFolderPathValid)
            {
                var sharedItemPaths = Directory.GetFileSystemEntries(
                    Path.Combine(SharedFolderPath, "items"));
                foreach (string sharedItemPath in sharedItemPaths)
                {
                    string itemName = Path.GetFileNameWithoutExtension(sharedItemPath);
                    var fileAttributes = File.GetAttributes(sharedItemPath);

                    SharedItem item = new SharedItem {
                        SharedArea = this,
                        Name = itemName,
                        Type = fileAttributes.HasFlag(FileAttributes.Directory) 
                            ? SharedItem.SharedItemType.Album : SharedItem.SharedItemType.Image
                    };

                    if(item.IsValid)
                        SharedItems.Add(item);
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
            foreach (SharedItem sharedAreaItem in SharedItems)
            {
                var fileAttributes = File.GetAttributes(sharedAreaItem.ItemPath);
                if (fileAttributes.HasFlag(FileAttributes.Directory))
                {
                    sharedAreaItem.Type = SharedItem.SharedItemType.Album;
                    sharedAreaItem.Thumbnail = SharedItem.DefaultAlbumThumbnail;
                }
                else
                {
                    sharedAreaItem.Type = SharedItem.SharedItemType.Image;
                    sharedAreaItem.Thumbnail = SharedItem.DefaultImageThumbnail;
                }
            }
        }

        private void ApplyShowPreviews()
        {
            SharedItems
                .Where(sharedItem => sharedItem.IsPolicyVerified &&
                    sharedItem.Type == SharedItem.SharedItemType.Image)
                .ToList()
                .ForEach(sharedItem =>
                {
                    //todo: reimplement with decription
                    //BitmapImage thumbnail = new BitmapImage(new Uri(sharedItem.ItemPath));
                    //sharedItem.Thumbnail = thumbnail;
                });
        }

        private void ApplyFilterOutOfPolicy()
        {
            filteredSharedAreaItems = SharedItems.Where(item => !item.IsPolicyVerified).ToList();
            var tmp = SharedItems.Where(item => item.IsPolicyVerified).ToList();
            SharedItems.Clear();
            tmp.ForEach(item => SharedItems.Add(item));
        }

        private void UnFilterOutOfPolicy()
        {
            filteredSharedAreaItems.ForEach(item => SharedItems.Add(item));
            filteredSharedAreaItems = null;
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
        
        private void SharedAreaItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            //todo: this needs to be implemented

            var button = sender as SharedAreaItemButton;
            var item = button.Item;
            if (item.IsPolicyVerified)
            {
                switch (item.Type)
                {
                    case SharedItem.SharedItemType.Album:
                    {
                        break;
                    }
                    case SharedItem.SharedItemType.Image:
                    {
                        break;
                    }
                }
            }
        }
    }
}