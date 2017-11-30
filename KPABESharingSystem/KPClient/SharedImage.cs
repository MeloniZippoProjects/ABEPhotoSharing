using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.Image });
        }

        public SharedImage(string Name, SharedArea SharedArea)
        {
            SetDefaultThumbnail();
            this.Name = Name;
            this.SharedArea = SharedArea;
        }

        public override void SetDefaultThumbnail()
        {
            Thumbnail = DefaultImageThumbnail;
        }

        public override void SetPreviewThumbnail()
        {
            MemoryStream ms = new MemoryStream(DecryptedBytes);
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
                var match = imageRegex.Match(value);
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

        public override bool IsValid => 
            File.Exists(ItemPath) && File.Exists(KeyPath);

        public byte[] _decryptedBytes = null;
        public byte[] DecryptedBytes => _decryptedBytes ?? (_decryptedBytes = GetDecryptedBytes());

        protected byte[] GetDecryptedBytes()
        {
            if (!IsPolicyVerified)
                return null;

            try
            {
                Aes aes = new AesCng();
                aes.KeySize = 256;
                aes.Key = SymmetricKey.Key;
                aes.IV = SymmetricKey.IV;

                var decryptor = aes.CreateDecryptor();

                using (Stream inputStream = new FileStream(ItemPath, FileMode.Open),
                    outputStream = new MemoryStream())
                {
                    SymmetricKey.DecryptFile(inputStream, outputStream);
                    return ((MemoryStream)outputStream).ToArray();
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
