using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;

namespace KPClient
{
    public sealed class SharedAlbum : SharedItem
    {
        public static DrawingImage DefaultAlbumThumbnail;

        public List<SharedAlbumImage> Children;

        static SharedAlbum()
        {
            DefaultAlbumThumbnail =
                IconToDrawing(
                    new PackIconModern() {Kind = PackIconModernKind.ImageMultiple});
        }

        public SharedAlbum(string name, SharedArea sharedArea)
        {
            SetDefaultThumbnail();
            Name = name;
            SharedArea = sharedArea;

            PopulateChildren();
        }

        private void PopulateChildren()
        {
            Children = new List<SharedAlbumImage>();
            int childrenId = 0;
            while (true)
            {
                string childrenName = $"{Name}.{childrenId}.png.aes";
                string childrenPath = Path.Combine(ItemPath, childrenName);
                if (File.Exists(childrenPath))
                {
                    Children.Add(new SharedAlbumImage(
                        name: childrenName,
                        sharedArea: SharedArea,
                        parentAlbum: this,
                        imageId: childrenId));
                    ++childrenId;
                }
                else
                    return;
            }
        }

        public override void SetDefaultThumbnail()
        {
            Thumbnail = DefaultAlbumThumbnail;
        }

        public override void SetPreviewThumbnail()
        {
            SetDefaultThumbnail();
        }

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
            && File.Exists(Path.Combine(ItemPath, $"{Name}.0.png.aes"));

        protected override SymmetricKey GetSymmetricKey()
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
    }
}