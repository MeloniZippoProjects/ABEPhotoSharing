using System.Collections.Generic;

namespace KPClient
{
    public class DesignTimeSharedAreaContext
    {
        public List<SharedItem> DisplayedItems { get; set; } = new List<SharedItem>()
        {
            new SharedAlbum(
                name: "album",
                sharedArea: null),
            new SharedImage(
                name: "this is a very very long image name.png.aes",
                sharedArea: null),
            new SharedImage(
                name: "image.png.aes",
                sharedArea: null),
            new SharedAlbum(
                name: "album",
                sharedArea: null),
            new SharedImage(
                name: "this is a very very long image name.png.aes",
                sharedArea: null),
            new SharedImage(
                name: "image.png.aes",
                sharedArea: null),
            new SharedAlbum(
                name: "album",
                sharedArea: null),
            new SharedImage(
                name: "this is a very very long image name.png.aes",
                sharedArea: null),
            new SharedImage(
                name: "image.png.aes",
                sharedArea: null)
        };
    }
}