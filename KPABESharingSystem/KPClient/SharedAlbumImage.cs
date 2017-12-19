using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace KPClient
{
    public class SharedAlbumImage : SharedImage
    {
        public SharedAlbum ParentAlbum { get; set; }
        public int ImageId;

        public SharedAlbumImage(string name, SharedArea sharedArea, SharedAlbum parentAlbum, int imageId) : base(name,
            sharedArea)
        {
            ParentAlbum = parentAlbum;
            ImageId = imageId;
        }

        public override string ImagePath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            ParentAlbum.Name,
            $"{ParentAlbum.Name}.{ImageId}.png.aes");

        public override string ThumbnailPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            ParentAlbum.Name,
            $"{ParentAlbum.Name}.{ImageId}.tmb.png.aes");

        public override string KeysPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{ParentAlbum.Name}.keys.kpabe");

        public override bool IsValid
        {
            get
            {
                if (ParentAlbum.IsValid)
                {
                    var siblings = Directory.GetFiles(ParentAlbum.AlbumPath)
                        .Select(Path.GetFileName)
                        .ToList().AsReadOnly();

                    for (int i = 0; i <= ImageId; i++)
                    {
                        if (!siblings.Contains($"{ParentAlbum.Name}.{i}.png.aes"))
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
        }

        public override Task<bool> IsPolicyVerified() 
            => ParentAlbum.IsPolicyVerified();

        protected override async Task<ItemKeys> GetItemKeys()
        {
            if (!IsValid)
                return null;

            if (ImageId == 0)
                return await ParentAlbum.ItemKeys;
            else
            {
                var siblings = await ParentAlbum.GetChildren();
                SharedAlbumImage precedent = siblings[ImageId - 1];
                ItemKeys precedentKeys = await precedent.ItemKeys;
                ItemKeys itemKeys = new ItemKeys
                {
                    ThumbnailKey = precedentKeys.ThumbnailKey.GetNextKey(),
                    ImageKey = precedentKeys.ImageKey.GetNextKey()
                };
                return itemKeys;
            }
        }
    }
}