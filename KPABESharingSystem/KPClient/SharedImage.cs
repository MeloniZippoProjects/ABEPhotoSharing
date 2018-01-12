using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KPServices;
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

            SecureBytes thumbnailBytes = await GetThumbnailBytes();

            if (thumbnailBytes == null)
            {
                Console.WriteLine($"Cannot decrypt: {Name}");
                return;
            }

            MemoryStream ms = new MemoryStream(thumbnailBytes.ProtectedBytes);
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

        public virtual string ImagePath => Path.Combine(
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
            $"{Name}.keys.kpabe");

        public override bool IsValid => File.Exists(ImagePath) && File.Exists(KeysPath);

        private Task<SecureBytes> thumbnailBytesTask;
        public async Task<SecureBytes> GetThumbnailBytes()
        {
            if (thumbnailBytesTask == null)
            {
                ItemKeys itemKeys = await ItemKeys;
                thumbnailBytesTask = GetDecryptedBytes(ThumbnailPath, itemKeys.ThumbnailKey);
            }

            return await thumbnailBytesTask;
        }


        private Task<SecureBytes> imageBytesTask;
        public async Task<SecureBytes> GetImageBytes()
        {
            if (imageBytesTask == null)
            {
                ItemKeys itemKeys = await ItemKeys;
                imageBytesTask = GetDecryptedBytes(ImagePath, itemKeys.ImageKey);
            }

            return await imageBytesTask;
        }

        protected static async Task<SecureBytes> GetDecryptedBytes(string filePath, SymmetricKey key)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await key.Decrypt(inputStream: fs, outputStream: ms);
                        using (TemporaryBytes tb = ms.GetBuffer())
                        {
                            return new SecureBytes{ProtectedBytes = tb};
                        }
                    }
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