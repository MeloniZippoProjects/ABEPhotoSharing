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
        public static DrawingImage DefaultImageThumbnail;
        
        static SharedImage()
        {
            DefaultImageThumbnail =
                IconToDrawing(
                    new PackIconModern() {Kind = PackIconModernKind.Image});
        }

        public SharedImage(string name, SharedArea sharedArea)
        {
            SetDefaultThumbnail();
            // ReSharper disable once VirtualMemberCallInConstructor
            Name = name;
            SharedArea = sharedArea;
        }

        public sealed override void SetDefaultThumbnail()
        {
            Thumbnail = DefaultImageThumbnail;
        }

        public override async void SetPreviewThumbnail()
        {
            var imageBytes = await DecryptedBytes;

            if (imageBytes == null)
            {
                Console.WriteLine($"Cannot decrypt: {Name}");
                return;
            }
            
            MemoryStream ms = new MemoryStream(imageBytes);

            BitmapImage thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.StreamSource = ms;
            thumbnail.EndInit();

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

        public override string KeyPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{Name}.key.kpabe");

        public override bool IsValid => File.Exists(ItemPath) && File.Exists(KeyPath);

        private Task<byte[]> _decryptedBytes;
        public Task<byte[]> DecryptedBytes => _decryptedBytes ?? (_decryptedBytes = GetDecryptedBytes());
        
        protected async Task<byte[]> GetDecryptedBytes()
        {
            if (!IsPolicyVerified)
                return null;

            try
            {
                using (Stream inputStream = new FileStream(ItemPath, FileMode.Open),
                    outputStream = new MemoryStream())
                {
                    SymmetricKey symmetricKey = await SymmetricKey;
                    await symmetricKey.Decrypt(inputStream, outputStream);
                    return ((MemoryStream) outputStream).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
                return null;
            }
        }
    }
}