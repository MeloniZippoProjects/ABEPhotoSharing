using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace KPClient
{
    /// <summary>
    /// Interaction logic for SharedItemView.xaml
    /// </summary>
    public partial class SharedItemView : UserControl
    {
        public SharedArea ParentArea
        {
            get => (SharedArea)GetValue(ParentAreaProperty);
            set => SetValue(ParentAreaProperty, value);
        }

        public static readonly DependencyProperty ParentAreaProperty =
            DependencyProperty.Register("ParentArea", typeof(SharedArea), typeof(SharedItemView), new PropertyMetadata(null));
        
        public SharedItem Item
        {
            get => (SharedItem)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(SharedItem), typeof(SharedItemView), new PropertyMetadata(null));

        public SharedItemView()
        {
            InitializeComponent();
        }

        private async void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await Item.IsPolicyVerified())
                return;
            switch (Item)
            {
                case SharedImage _:
                    OpenImage(Item as SharedImage);
                    break;
                case SharedAlbum _:
                    ParentArea.LoadAlbumImages(Item as SharedAlbum);
                    break;
            }
        }

        private static async void OpenImage(SharedImage sharedImage)
        {
            string imagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Path.GetRandomFileName()}.png");
            using (FileStream fs = new FileStream(path: imagePath, mode: FileMode.Create))
            {
                byte[] imageBytes = await sharedImage.GetImageBytes();
                await fs.WriteAsync(
                    buffer: imageBytes,
                    count: imageBytes.Length,
                    offset: 0);
            }
            Process.Start(imagePath);
        }
    }
}
