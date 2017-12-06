using System.IO;
using System.Linq;
using System.Security.Cryptography;

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

        public override bool IsPolicyVerified => ParentAlbum.IsPolicyVerified;

        protected override SymmetricKey GetSymmetricKey()
        {
            if (!IsValid)
                return null;

            if (ImageId == 0)
                return ParentAlbum.SymmetricKey;
            else
            {
                SharedAlbumImage precedent = ParentAlbum.Children[ImageId - 1];
                SHA256Cng sha = new SHA256Cng();
                SymmetricKey symmetricKey = new SymmetricKey
                {
                    Key = sha.ComputeHash(precedent.SymmetricKey.Key),
                    Iv = sha.ComputeHash(precedent.SymmetricKey.Iv).Take(128 / 8).ToArray()
                };
                return symmetricKey;
            }
        }
    }
}