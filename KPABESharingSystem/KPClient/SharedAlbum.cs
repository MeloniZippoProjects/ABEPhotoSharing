using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace KPClient
{
    public sealed class SharedAlbum : SharedItem
    {
        public static Task<DrawingImage> DefaultAlbumThumbnail;

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
        }

        private Task<List<SharedAlbumImage>> children;
        public async Task<List<SharedAlbumImage>> GetChildren()
        {
            if (children == null)
            {
                children = Dispatcher.InvokeAsync(() =>
                {
                    var taskChildren = new List<SharedAlbumImage>();
                    for (int childrenId = 0;; childrenId++)
                    {
                        string childrenName = $"{Name}.{childrenId}.png.aes";
                        string childrenPath = Path.Combine(AlbumPath, childrenName);
                        if (File.Exists(childrenPath))
                        {
                            taskChildren.Add(new SharedAlbumImage(
                                name: childrenName,
                                sharedArea: SharedArea,
                                parentAlbum: this,
                                imageId: childrenId));
                        }
                        else
                            return taskChildren;
                    }
                }).Task;
            }
            return await children;
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
                (await GetChildren()).ForEach(
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