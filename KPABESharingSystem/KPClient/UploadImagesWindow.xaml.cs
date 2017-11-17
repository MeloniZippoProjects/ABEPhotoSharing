using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
#if DEBUG
            for (int i = 0; i < 100; i++)
            {
                imageItems.Add(new ImageItem() { ImagePath = "D:\\Users\\Raff\\OneDrive\\Immagini\\face.png" });
                imageItems.Add(new ImageItem() { ImagePath = "D:\\Users\\Raff\\OneDrive\\Immagini\\maxine.png" });
            }
#endif
            ImagesToUploadControl.ItemsSource = imageItems;
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            //todo: remove the specific image linked to the button

            ImageItemButton clickedButton = sender as ImageItemButton;
            imageItems.Remove(clickedButton.Item);
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
