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

        public override string ItemPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "items",
            ParentAlbum.Name,
            $"{ParentAlbum.Name}.{ImageId}.png.aes");

        public override string KeyPath => Path.Combine(
            SharedArea.SharedFolderPath,
            "keys",
            $"{ParentAlbum.Name}.key.kpabe");

        public override bool IsValid
        {
            get
            {
                if (ParentAlbum.IsValid)
                {
                    var siblings = Directory.GetFiles(ParentAlbum.ItemPath)
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

        protected override async Task<SymmetricKey> GetSymmetricKey()
        {
            if (!IsValid)
                return null;

            if (ImageId == 0)
                return await ParentAlbum.SymmetricKey;
            else
            {
                var siblings = await ParentAlbum.Children;
                SharedAlbumImage precedent = siblings[ImageId - 1];
                SymmetricKey precedentKey = await precedent.SymmetricKey;
                SHA256Cng sha = new SHA256Cng();
                SymmetricKey symmetricKey = new SymmetricKey
                {
                    Key = sha.ComputeHash(precedentKey.Key),
                    Iv = sha.ComputeHash(precedentKey.Iv).Take(128 / 8).ToArray()
                };
                return symmetricKey;
            }
        }
    }
}