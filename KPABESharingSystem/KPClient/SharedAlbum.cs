using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    class SharedAlbum : SharedItem
    {
        public static DrawingImage DefaultAlbumThumbnail;

        static SharedAlbum()
        {
            DefaultAlbumThumbnail =
                IconToDrawing(
                    new MahApps.Metro.IconPacks.PackIconModern() { Kind = PackIconModernKind.ImageMultiple });
        }

        public SharedAlbum() => Thumbnail = DefaultAlbumThumbnail;

        public override string ItemPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            Name);

        public override string KeyPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{Name}.key.kpabe");

        public override bool IsValid =>
            Directory.Exists(ItemPath) 
            && File.Exists(KeyPath)
            && File.Exists(Path.Combine(ItemPath, $"{Name}.0.png.kpabe"));

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
            Thumbnail = DefaultAlbumThumbnail;
        }
    }
}
