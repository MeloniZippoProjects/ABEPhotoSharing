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

        private List<SharedItem> _filteredSharedAreaItems;
        
        private string CurrentAlbum = null;

        private static void SharedFolderPath_OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SharedArea sharedArea = (SharedArea) d;
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
            SharedItems.Clear();
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
            foreach (SharedItem item in SharedItems)
            {
                item.SetDefaultThumbnail();
            }
        }

        private void ApplyShowPreviews()
        {
            SharedItems
                .Where(sharedItem => sharedItem.IsPolicyVerified)
                .ToList()
                .ForEach(sharedItem => sharedItem.SetPreviewThumbnail());
        }

        private void ApplyFilterOutOfPolicy()
        {
            _filteredSharedAreaItems = SharedItems.Where(item => !item.IsPolicyVerified).ToList();
            var tmp = SharedItems.Where(item => !_filteredSharedAreaItems.Contains(item)).ToList();
            SharedItems.Clear();
            tmp.ForEach(item => SharedItems.Add(item));
        }

        private void UnFilterOutOfPolicy()
        {
            _filteredSharedAreaItems.ForEach(item => SharedItems.Add(item));
            _filteredSharedAreaItems = null;
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
            //todo: this needs to be implemented

            var button = sender as SharedAreaItemButton;
            var item = button.Item;
            if (item.IsPolicyVerified)
            {
                
            }
        }
        
    }
}