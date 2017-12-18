using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;

namespace KPClient
{
    public abstract class SharedItem : DependencyObject
    {
        public static Task<DrawingImage> UndefinedThumbnail;

        static SharedItem()
        {
            UndefinedThumbnail = IconToDrawing(
                    new PackIconModern() {Kind = PackIconModernKind.ImageBacklight});
        }

        protected static async Task<DrawingImage> IconToDrawing(PackIconModern icon)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
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
            });
        }

        public SharedArea SharedArea { get; set; }

        public abstract string KeysPath { get; }

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

        public virtual async Task<bool> IsPolicyVerified()
        {
            return (await ItemKeys) != null;
        }

        private Task<ItemKeys> _itemKeys;
        public Task<ItemKeys> ItemKeys => _itemKeys ?? (_itemKeys = GetItemKeys());

        public void PreloadItemKeys()
        {
            if(_itemKeys == null)
                _itemKeys = GetItemKeys();
        }

        protected virtual async Task<ItemKeys> GetItemKeys()
        {
            string decryptedKeyPath = null;
            try
            {
                decryptedKeyPath = Path.GetTempFileName();
                App app = (App) Application.Current;
                app.KpService.Decrypt(
                    sourceFilePath: KeysPath,
                    destFilePath: decryptedKeyPath);

                using (FileStream fs = new FileStream(decryptedKeyPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string serializedKeys = await sr.ReadToEndAsync();
                        ItemKeys itemKeys = await Task.Run(() =>
                            JsonConvert.DeserializeObject<ItemKeys>(serializedKeys));
                        return itemKeys;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Operation failed: {e}");
                return null;
            }
            finally
            {
                if(decryptedKeyPath != null)
                    File.Delete(decryptedKeyPath);
            }
        }

        public abstract void PreloadThumbnail();
    }
}