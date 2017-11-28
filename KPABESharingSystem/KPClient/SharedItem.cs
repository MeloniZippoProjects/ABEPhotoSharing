using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KPServices;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    public abstract class SharedItem : DependencyObject
    {
        public static DrawingImage UndefinedThumbnail;

        static SharedItem()
        {
            UndefinedThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.ImageBacklight });
        }

        public SharedArea SharedArea { get; set; }

        public abstract string ItemPath { get; }
        public abstract string KeyPath { get; }

        public abstract bool IsValid { get; }

        //todo: could be cached
        public bool IsPolicyVerified
        {
            get => VerifyPolicy();
        }

        protected abstract bool VerifyPolicy();
        
        private byte[] _symmetricKey;
        public byte[] SymmetricKey
        {
            get
            {
                if(_symmetricKey == null)
                    _symmetricKey = GetSymmetricKey();
                return _symmetricKey;
            }
        }

        protected abstract byte[] GetSymmetricKey();

        public virtual string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(SharedItem), new PropertyMetadata(""));

        public ImageSource Thumbnail
        {
            get { return (ImageSource)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(ImageSource), typeof(SharedItem), new PropertyMetadata(UndefinedThumbnail));

        public abstract void SetDefaultThumbnail();

        protected static DrawingImage IconToDrawing(MahApps.Metro.IconPacks.PackIconModern icon)
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

    public class SharedAreaItemButton : Button
    {
        public SharedItem Item
        {
            get { return (SharedItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(SharedItem), typeof(SharedAreaItemButton), new PropertyMetadata(null));
    }

    public class DesignTimeSharedAreaContext
    {
        public List<SharedItem> SharedItems { get; set; } = new List<SharedItem>()
        {
            new SharedAlbum()
            {
                Name = "album",
            },
            new SharedImage()
            {
                Name = "this is a very very long image name.png.aes",
            },
            new SharedImage()
            {
                Name = "image.png.aes",
            },
            new SharedAlbum()
            {
                Name = "album",
            },
            new SharedImage()
            {
                Name = "this is a very very long image name.png.aes",
            },
            new SharedImage()
            {
                Name = "image.png.aes",
            },
            new SharedAlbum()
            {
                Name = "album",
            },
            new SharedImage()
            {
                Name = "this is a very very long image name.png.aes",
            },
            new SharedImage()
            {
                Name = "image.png.aes",
            }
        };
    }
}
