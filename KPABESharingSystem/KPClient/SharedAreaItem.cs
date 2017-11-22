using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    public class SharedAreaItem:DependencyObject
    {
        public static DrawingImage UndefinedThumbnail;
        public static DrawingImage DefaultAlbumThumbnail;
        public static DrawingImage DefaultImageThumbnail;

        public enum SharedItemType
        {
            Image,
            Album
        };

        public SharedItemType Type { get; set; }
        public string Path { get; set; }
        public bool? IsPolicyVerified { get; set; } = null;

        public String Name { get; set; }
        
        public ImageSource Thumbnail
        {
            get { return (ImageSource)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Thumbnail.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(ImageSource), typeof(SharedAreaItem), new PropertyMetadata(UndefinedThumbnail));



        //        public ImageSource Thumbnail { get; set; } = UndefinedThumbnail;

        static SharedAreaItem()
        {
            UndefinedThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.ImageBacklight });

            DefaultAlbumThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.ImageMultiple });

            DefaultImageThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.Image });
        }
        private static DrawingImage IconToDrawing(MahApps.Metro.IconPacks.PackIconModern icon)
        {
            Geometry geo = Geometry.Parse(icon.Data);
            GeometryDrawing gd = new GeometryDrawing();
            gd.Geometry = geo;
            gd.Brush = icon.BorderBrush;
            gd.Pen = new Pen(Brushes.Black, 1);
            DrawingImage geoImage = new DrawingImage(gd);
            return geoImage;
        }
    }
    public class DesignTimeWindowContext
    {
        public List<SharedAreaItem> SharedAreaItems { get; set; } = new List<SharedAreaItem>()
        {
            new SharedAreaItem()
            {
                Name = "album",
                Thumbnail = SharedAreaItem.DefaultAlbumThumbnail
            },
            new SharedAreaItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            },
            new SharedAreaItem()
            {
                Name = "image",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            },
            new SharedAreaItem()
            {
                Name = "album",
                Thumbnail = SharedAreaItem.DefaultAlbumThumbnail
            },
            new SharedAreaItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            },
            new SharedAreaItem()
            {
                Name = "image",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            },
            new SharedAreaItem()
            {
                Name = "album",
                Thumbnail = SharedAreaItem.DefaultAlbumThumbnail
            },
            new SharedAreaItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            },
            new SharedAreaItem()
            {
                Name = "image",
                Thumbnail = SharedAreaItem.DefaultImageThumbnail
            }
        };
    }
}
