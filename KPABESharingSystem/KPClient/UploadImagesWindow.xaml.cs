using System;
using System.Collections.Generic;
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
        private List<ImageItem> imageItems;

        public UploadImagesWindow()
        {
            InitializeComponent();
            imageItems = new List<ImageItem>();
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
        }
        
    }

    public class ImageItem
    {
        public string ImagePath { get; set; }
    }
}
