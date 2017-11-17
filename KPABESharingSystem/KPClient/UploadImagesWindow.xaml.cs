using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per UploadImagesWindow.xaml
    /// </summary>
    public partial class UploadImagesWindow : Window
    {
        private ObservableCollection<ImageItem> imageItems;

        public UploadImagesWindow()
        {
            InitializeComponent();
            imageItems = new ObservableCollection<ImageItem>();
            ImagesToUploadControl.ItemsSource = imageItems;

            imageItems.CollectionChanged += UpdateClearAllButtonStatus;

#if DEBUG
            for (int i = 0; i < 100; i++)
            {
                imageItems.Add(new ImageItem() { ImagePath = "D:\\Users\\Raff\\OneDrive\\Immagini\\face.png" });
                imageItems.Add(new ImageItem() { ImagePath = "D:\\Users\\Raff\\OneDrive\\Immagini\\maxine.png" });
            }
#endif
        }

        private void UpdateClearAllButtonStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (imageItems.Count > 0)
                ClearAllButton.IsEnabled = true;
            else
                ClearAllButton.IsEnabled = false;
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            ImageItemButton clickedButton = sender as ImageItemButton;
            imageItems.Remove(clickedButton.Item);
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            imageItems.Clear();
        }

        private void AddImagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files(*.jpg, *.jpeg, *.png, *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            openFileDialog.Multiselect = true;

            // Show open file dialog box
            Nullable<bool> result = openFileDialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Add selected images
                foreach (string filename in openFileDialog.FileNames)
                {
                    imageItems.Add(new ImageItem() { ImagePath = filename});
                }
            }
        }
    }

    public class ImageItem
    {
        public string ImagePath { get; set; }
    }

    public class ImageItemButton : Button
    {
        public ImageItem Item
        {
            get { return (ImageItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ImageItem), typeof(ImageItemButton), null);
    }
}
