using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;

namespace KPClient
{
    public abstract class SharedItem : DependencyObject
    {
        public static DrawingImage UndefinedThumbnail;

        static SharedItem()
        {
            UndefinedThumbnail =
                IconToDrawing(
                    new PackIconModern() {Kind = PackIconModernKind.ImageBacklight});
        }

        protected static DrawingImage IconToDrawing(PackIconModern icon)
        {
            Geometry geo = Geometry.Parse(icon.Data);
            GeometryDrawing gd = new GeometryDrawing
            {
                Geometry = geo,
                Brush = icon.BorderBrush,
                Pen = new Pen(Brushes.Black, 1)
            };
            DrawingImage geoImage = new DrawingImage(gd);
            return geoImage;
        }

        public SharedArea SharedArea { get; set; }

        public abstract string ItemPath { get; }
        public abstract string KeyPath { get; }

        public abstract bool IsValid { get; }

        public virtual string Name
        {
            get => (string) GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(SharedItem), new PropertyMetadata(""));

        public ImageSource Thumbnail
        {
            get => (ImageSource) GetValue(ThumbnailProperty);
            set => SetValue(ThumbnailProperty, value);
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(ImageSource), typeof(SharedItem),
                new PropertyMetadata(UndefinedThumbnail));

        public abstract void SetDefaultThumbnail();
        public abstract void SetPreviewThumbnail();

        public virtual bool IsPolicyVerified => SymmetricKey != null;

        private SymmetricKey _symmetricKey;

        protected virtual SymmetricKey GetSymmetricKey()
        {
            try
            {
                string decryptedKeyPath = Path.GetTempFileName();
                App app = (App) Application.Current;
                app.KpService.Decrypt(
                    sourceFilePath: KeyPath,
                    destFilePath: decryptedKeyPath);

                using (FileStream fs = new FileStream(decryptedKeyPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string serializedKey = sr.ReadToEnd();
                        SymmetricKey symmetricKey = JsonConvert.DeserializeObject<SymmetricKey>(serializedKey);
                        return symmetricKey;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Operation failed: {e}");
                return null;
            }
        }

        public SymmetricKey SymmetricKey => _symmetricKey ?? (_symmetricKey = GetSymmetricKey());
    }

    public class SharedAreaItemButton : Button
    {
        public SharedItem Item
        {
            get => (SharedItem) GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(SharedItem), typeof(SharedAreaItemButton),
                new PropertyMetadata(null));
    }
}