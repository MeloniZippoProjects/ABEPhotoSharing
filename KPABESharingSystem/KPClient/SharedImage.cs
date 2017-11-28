using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    class SharedImage : SharedItem
    {
        public static DrawingImage DefaultImageThumbnail;

        static SharedImage()
        {
            DefaultImageThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.Image });
        }

        public SharedImage() => Thumbnail = DefaultImageThumbnail;

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

        protected override bool VerifyPolicy()
        {
            return GetSymmetricKey() != null;
        }

        protected override byte[] GetSymmetricKey()
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
            catch (Exception e)
            {
                Console.WriteLine($"Operation failed: {e}");
                return null;
            }
        }

        public override void SetDefaultThumbnail()
        {
            Thumbnail = DefaultImageThumbnail;
        }
    }
}
