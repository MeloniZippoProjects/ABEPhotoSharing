﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Newtonsoft.Json;

namespace KPClient
{
    public sealed class SharedAlbum : SharedItem
    {
        public static DrawingImage DefaultAlbumThumbnail;

        public readonly Task<List<SharedAlbumImage>> Children;

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

            Children = Dispatcher.InvokeAsync(PopulateChildren).Task;
        }

        private List<SharedAlbumImage> PopulateChildren()
        {
            var children = new List<SharedAlbumImage>();
            for(int childrenId = 0; ; childrenId++)
            {
                string childrenName = $"{Name}.{childrenId}.png.aes";
                string childrenPath = Path.Combine(ItemPath, childrenName);
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
    }
}