using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPClient
{
    public class DesignTimeSharedAreaContext
    {
        public List<SharedItem> DisplayedItems { get; set; } = new List<SharedItem>()
        {
            new SharedAlbum(
                Name : "album",
                SharedArea: null),
            new SharedImage(
                Name: "this is a very very long image name.png.aes",
                SharedArea: null),
            new SharedImage(
                Name : "image.png.aes",
                SharedArea: null),
            new SharedAlbum(
                Name : "album",
                SharedArea: null),
            new SharedImage(
                Name: "this is a very very long image name.png.aes",
                SharedArea: null),
            new SharedImage(
                Name : "image.png.aes",
                SharedArea: null),
            new SharedAlbum(
                Name : "album",
                SharedArea: null),
            new SharedImage(
                Name: "this is a very very long image name.png.aes",
                SharedArea: null),
            new SharedImage(
                Name : "image.png.aes",
                SharedArea: null)
        };
    }
}
