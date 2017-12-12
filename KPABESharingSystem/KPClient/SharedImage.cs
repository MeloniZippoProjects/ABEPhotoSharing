using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    public class SharedImage : SharedItem
    {
        public static Task<DrawingImage> DefaultImageThumbnail;
        
        static SharedImage()
        {
            DefaultImageThumbnail = IconToDrawing(
                new PackIconModern() {Kind = PackIconModernKind.Image});
        }

        public SharedImage(string name, SharedArea sharedArea)
        {
            SetDefaultThumbnail();
            // ReSharper disable once VirtualMemberCallInConstructor
            Name = name;
            SharedArea = sharedArea;
        }

        public sealed override async void SetDefaultThumbnail()
        {
            Thumbnail = await DefaultImageThumbnail;
        }

        public override async void SetPreviewThumbnail()
        {
            if (!await IsPolicyVerified())
                return;

            var thumbnailBytes = await GetThumbnailBytes();

            if (thumbnailBytes == null)
            {
                Console.WriteLine($"Cannot decrypt: {Name}");
                return;
            }
            
            MemoryStream ms = new MemoryStream(thumbnailBytes);

            BitmapImage thumbnail = new BitmapImage();
            await Dispatcher.InvokeAsync(() =>
            {
                thumbnail.BeginInit();
                thumbnail.StreamSource = ms;
                thumbnail.EndInit();
            });

            Thumbnail = thumbnail;
        }

        public override string Name
        {
            set
            {
                Regex imageRegex = new Regex(@"^(?<name>.+).png.aes$");
                Match match = imageRegex.Match(value);
                base.Name = match.Groups["name"].Value;
            }
        }

        public override string ItemPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            $"{Name}.png.aes");

        public virtual string ThumbnailPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            $"{Name}.tmb.png.aes");

        public override string KeysPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{Name}.key.kpabe");

        public override bool IsValid => File.Exists(ItemPath) && File.Exists(KeysPath);

        private byte[] _thumbnailBytes;
        public async Task<byte[]> GetThumbnailBytes()
        {
            if (_thumbnailBytes != null)
                return _thumbnailBytes;

            ItemKeys itemKeys = await ItemKeys;
            _thumbnailBytes = await GetDecryptedBytes(ThumbnailPath, itemKeys.ThumbnailKey);
            return _thumbnailBytes;
        }


        private byte[] _imageBytes;
        public async Task<byte[]> GetImageBytes()
        {
            if (_imageBytes != null)
                return _imageBytes;

            ItemKeys itemKeys = await ItemKeys;
            _imageBytes = await GetDecryptedBytes(ItemPath, itemKeys.ImageKey);
            return _imageBytes;
        }

        protected async Task<byte[]> GetDecryptedBytes(string filePath, SymmetricKey key)
        {
            if (!await IsPolicyVerified())
                return null;

            try
            {
                using (Stream inputStream = new FileStream(filePath, FileMode.Open),
                    outputStream = new MemoryStream())
                {
                    await key.Decrypt(inputStream, outputStream);
                    return ((MemoryStream) outputStream).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
                return null;
            }
        }

        public override async void PreloadThumbnail()
        {
            if (await IsPolicyVerified())
                await GetThumbnailBytes();
        }
    }
}