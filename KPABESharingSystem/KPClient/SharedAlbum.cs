﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    public sealed class SharedAlbum : SharedItem
    {
        public static Task<DrawingImage> DefaultAlbumThumbnail;

        public readonly Task<List<SharedAlbumImage>> Children;

        static SharedAlbum()
        {
            DefaultAlbumThumbnail = IconToDrawing(
                    new PackIconModern() {Kind = PackIconModernKind.ImageMultiple});
        }

        public SharedAlbum(string name, SharedArea sharedArea)
        {
            SetDefaultThumbnail();
            Name = name;
            SharedArea = sharedArea;

            Children = Dispatcher.InvokeAsync(PopulateChildren).Task;
        }

        private List<SharedAlbumImage> PopulateChildren()
        {
            var children = new List<SharedAlbumImage>();
            for(int childrenId = 0; ; childrenId++)
            {
                string childrenName = $"{Name}.{childrenId}.png.aes";
                string childrenPath = Path.Combine(AlbumPath, childrenName);
                if (File.Exists(childrenPath))
                {
                    children.Add(new SharedAlbumImage(
                        name: childrenName,
                        sharedArea: SharedArea,
                        parentAlbum: this,
                        imageId: childrenId));
                }
                else
                    return children;
            }
        }

        public override async void SetDefaultThumbnail()
        {
            Thumbnail = await DefaultAlbumThumbnail;
        }

        public override void SetPreviewThumbnail()
        {
            SetDefaultThumbnail();
        }

        public override async void PreloadThumbnail()
        {
            if(await IsPolicyVerified())
                (await Children).ForEach(
                    child => child.PreloadThumbnail());
        }

        public string AlbumPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            Name);

        public override string KeysPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{Name}.keys.kpabe");

        public override bool IsValid =>
            Directory.Exists(AlbumPath)
            && File.Exists(KeysPath)
            && File.Exists(Path.Combine(AlbumPath, $"{Name}.0.png.aes"));
    }
}