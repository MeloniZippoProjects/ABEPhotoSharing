using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPClient
{
    class SharedAlbumImage : SharedImage
    {
        public SharedItem ParentAlbum { get; set; }
        public int Id;

        public override string ItemPath
        {
            get => Path.Combine(
                SharedArea.SharedFolderPath,
                "items",
                ParentAlbum.Name,
                $"{ParentAlbum.Name}.{Id}.png.aes");
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

        protected override byte[] GetSymmetricKey()
        {
            if(Id == 0)
                return ParentAlbum.SymmetricKey;
            else
            {
                //todo: implement cascade key computation here
                return null;
            }
        }
    }
}
