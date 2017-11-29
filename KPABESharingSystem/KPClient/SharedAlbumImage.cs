using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KPClient
{
    class SharedAlbumImage : SharedImage
    {
        public SharedAlbum ParentAlbum { get; set; }
        public int ImageId;

        public SharedAlbumImage(string Name, SharedArea SharedArea, SharedAlbum ParentAlbum, int ImageId) : base(Name,
            SharedArea)
        {
            this.ParentAlbum = ParentAlbum;
            this.ImageId = ImageId;
        }
        
        public override string ItemPath
        {
            get => Path.Combine(
                SharedArea.SharedFolderPath,
                "items",
                ParentAlbum.Name,
                $"{ParentAlbum.Name}.{ImageId}.png.aes");
        }

        public override string KeyPath
        {
            get => Path.Combine(
                SharedArea.SharedFolderPath,
                "keys",
                $"{ParentAlbum.Name}.key.kpabe");
        }

        public override bool IsValid
        {
            get
            {
                if (ParentAlbum.IsValid)
                {
                    var siblings = Directory.GetFiles(ParentAlbum.ItemPath)
                        .Select(file => Path.GetFileName(file));
                    for (int i = 0; i < Int32.Parse(Name); i++)
                    {
                        if (!siblings.Contains($"{ParentAlbum.Name}.{i}.png.kpabe"))
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

            if(ImageId == 0)
                return ParentAlbum.SymmetricKey;
            else
            {
                var precedent = ParentAlbum.Children[ImageId - 1];
                var sha = new SHA256Cng();
                var symmetricKey = new SymmetricKey
                {
                    Key = sha.ComputeHash(precedent.SymmetricKey.Key),
                    IV = sha.ComputeHash(precedent.SymmetricKey.IV).Take(128/8).ToArray()
                };
                return symmetricKey;
            }
        }
    }
}
