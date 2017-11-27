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
    public class SharedItem:DependencyObject
    {
        public static DrawingImage UndefinedThumbnail;
        public static DrawingImage DefaultAlbumThumbnail;
        public static DrawingImage DefaultImageThumbnail;

        public enum SharedItemType
        {
            Image,
            Album,
            AlbumImage
        };

        public SharedItemType Type { get; set; }
        public String Name { get; set; }
        public SharedArea SharedArea { get; set; }
        public SharedItem ParentAlbum { get; set; }

        public string ItemPath
        {
            get
            {
                switch (Type)
                {
                    case SharedItemType.Image:
                        return Path.Combine(SharedArea.SharedFolderPath, "items", $"{Name}.png.aes");
                    case SharedItemType.Album:
                        return Path.Combine(SharedArea.SharedFolderPath, "items", Name);
                    case SharedItemType.AlbumImage:
                        return Path.Combine(SharedArea.SharedFolderPath, "items", ParentAlbum.Name, $"{ParentAlbum.Name}.{Name}.png.aes");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string KeyPath
        {
            get
            {
                switch (Type)
                {
                    case SharedItemType.Image:
                    case SharedItemType.Album:
                        return Path.Combine(SharedArea.SharedFolderPath, "keys", $"{Name}.key.kpabe");
                    
                    case SharedItemType.AlbumImage:
                        return Path.Combine(SharedArea.SharedFolderPath, "keys", $"{ParentAlbum.Name}.key.kpabe");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool IsValid
        {
            get
            {
                switch (Type)
                {
                    case SharedItemType.Image:
                        return File.Exists(ItemPath) && File.Exists(KeyPath);
                    case SharedItemType.Album:
                    {
                        if (Directory.Exists(ItemPath) && File.Exists(KeyPath))
                            return File.Exists(Path.Combine(ItemPath, $"{Name}.0.png.kpabe"));
                        else
                            return false;
                    }
                    case SharedItemType.AlbumImage:
                    {
                        if (ParentAlbum.IsValid)
                        {
                            var siblings = Directory.GetFiles(ParentAlbum.ItemPath)
                                .Select(file => Path.GetFileName(file));
                            for (int i = 0; i < Int32.Parse(Name); i++)
                            {
                                if (!siblings.Contains($"{ParentAlbum.Name}.{i}.png.kpabe"))
                                    return false;
                            }
                            return true;
                        }
                        else
                            return false;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //todo: could be cached
        public bool IsPolicyVerified
        {
            get => VerifyPolicy();
        }

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
        
        //public string Path { get; set; }
        
        public ImageSource Thumbnail
        {
            get { return (ImageSource)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(ImageSource), typeof(SharedItem), new PropertyMetadata(UndefinedThumbnail));

        private bool VerifyPolicy()
        {
            switch (Type)
            {
                case SharedItemType.Image:
                case SharedItemType.Album:
                    return GetSymmetricKey() != null;

                case SharedItemType.AlbumImage:
                    return ParentAlbum.IsPolicyVerified;
            }

            //How do you reach this?
            return false;
        }

        private byte[] GetSymmetricKey()
        {
            switch (Type)
            {
                case SharedItemType.Image:
                case SharedItemType.Album:
                {
                    string encKeyPath = Path.Combine(SharedArea.SharedFolderPath, "keys", Name, ".key.kpabe");
                    try
                    {
                        string workingDir = Path.GetTempPath();
                        string keyPath = Path.Combine(workingDir, Name, ".key");
                        App app = (App)Application.Current;
                        app.KpService.Decrypt(
                            sourceFilePath: encKeyPath,
                            destFilePath: keyPath);
                        byte[] key = File.ReadAllBytes(keyPath);
                        return key;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Operation failed: {e}");
                        return null;
                    }
                }

                case SharedItemType.AlbumImage:
                {
                    return ParentAlbum.GetSymmetricKey();
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static SharedItem()
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
    public class DesignTimeWindowContext
    {
        public List<SharedItem> SharedAreaItems { get; set; } = new List<SharedItem>()
        {
            new SharedItem()
            {
                Name = "album",
                Thumbnail = SharedItem.DefaultAlbumThumbnail
            },
            new SharedItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedItem.DefaultImageThumbnail
            },
            new SharedItem()
            {
                Name = "image",
                Thumbnail = SharedItem.DefaultImageThumbnail
            },
            new SharedItem()
            {
                Name = "album",
                Thumbnail = SharedItem.DefaultAlbumThumbnail
            },
            new SharedItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedItem.DefaultImageThumbnail
            },
            new SharedItem()
            {
                Name = "image",
                Thumbnail = SharedItem.DefaultImageThumbnail
            },
            new SharedItem()
            {
                Name = "album",
                Thumbnail = SharedItem.DefaultAlbumThumbnail
            },
            new SharedItem()
            {
                Name = "this is a very very long image name",
                Thumbnail = SharedItem.DefaultImageThumbnail
            },
            new SharedItem()
            {
                Name = "image",
                Thumbnail = SharedItem.DefaultImageThumbnail
            }
        };
    }
}
