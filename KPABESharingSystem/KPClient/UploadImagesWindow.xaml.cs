using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per UploadImagesWindow.xaml
    /// </summary>
    public partial class UploadImagesWindow
    {
        public ObservableCollection<ImageItem> ImageItems { get; private set; } = new ObservableCollection<ImageItem>();

        public UploadImagesWindow()
        {
            InitializeComponent();

            ImageItems.CollectionChanged += UpdateClearAllButtonStatus;
            ImageItems.CollectionChanged += UpdateUploadButtonStatus;
            TagsSelector.ValidityChanged += UpdateUploadButtonStatus;
        }

        private void UpdateUploadButtonStatus(object sender, EventArgs e)
        {
            if (ImageItems.Count > 0 && TagsSelector.IsValid)
                UploadButton.IsEnabled = true;
            else
                UploadButton.IsEnabled = false;
        }

        private void UpdateClearAllButtonStatus(object sender, EventArgs e)
        {
            ClearAllButton.IsEnabled = ImageItems.Count > 0;
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            ImageItemButton clickedButton = sender as ImageItemButton;
            ImageItems.Remove(clickedButton?.Item);
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ImageItems.Clear();
        }

        private void AddImagesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files(*.jpg, *.jpeg, *.png, *.bmp)|*.jpg; *.jpeg; *.png; *.bmp",
                Multiselect = true
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    ImageItems.Add(new ImageItem() {ImagePath = filename});
                }
            }
        }

        //todo: add control to choose image/album name. Use current settings as defaults
        //todo: catch IO errors, delete files eventually written
        private async void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            SymmetricKey imageKey = new SymmetricKey();
            imageKey.GenerateKey();
            SymmetricKey thumbnailKey = new SymmetricKey();
            thumbnailKey.GenerateKey();
            ItemKeys itemKeys = new ItemKeys
            {
                ImageKey = imageKey,
                ThumbnailKey = thumbnailKey
            };
            
            var tasks = new List<Task>();

            if (ImageItems.Count == 1)
            {
                string imagePath = ImageItems.First().ImagePath;
                string imageName = Path.GetFileNameWithoutExtension(imagePath);
                tasks.Add(UploadKeys(itemKeys, imageName));
                tasks.Add(UploadImage(
                    imagePath,
                    imageName,
                    itemKeys)
                );
            }
            else
            {
                string albumName = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss-ff");
                string albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
                Directory.CreateDirectory(albumPath);
                tasks.Add(UploadKeys(itemKeys, albumName));
                tasks.Add(UploadAlbum(
                    ImageItems.Select(item => item.ImagePath).ToArray(),
                    albumName,
                    itemKeys));
            }

            await Task.WhenAll(tasks.ToArray());

            Close();
        }

        private async Task UploadKeys(ItemKeys itemKeys, string name)
        {
            string keysPath = Path.GetTempFileName();
            using (FileStream fs = new FileStream(keysPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string serializedKeys = await Dispatcher.InvokeAsync(() =>
                        JsonConvert.SerializeObject(itemKeys));
                    await sw.WriteAsync(serializedKeys);
                }
            }

            string encryptedKeysPath = Path.GetTempFileName();

            App app = (App)Application.Current;

            app.KpService.Encrypt(
                sourceFilePath: keysPath,
                destFilePath: encryptedKeysPath,
                attributes: TagsSelector.GetTagsString());

            string keysName = $"{name}.keys.kpabe";

            string keysDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", keysName);

            File.Move(sourceFileName: encryptedKeysPath,
                destFileName: keysDestPath);
        }

        private static async Task UploadImage(string sourceImagePath, string imageName, ItemKeys itemKeys)
        {
            try
            {
                string encryptedImagePath = await EncryptImage(sourceImagePath, itemKeys.ImageKey);
                string encryptedThumbnailPath = await EncryptThumbnail(sourceImagePath, itemKeys.ThumbnailKey);

                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.png.aes");
                string thumbnailDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.tmb.png.aes");

                File.Move(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath);
                File.Move(sourceFileName: encryptedThumbnailPath,
                    destFileName: thumbnailDestPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }


        private static async Task UploadAlbum(IReadOnlyList<string> imagePaths, string albumName, ItemKeys itemKeys)
        {
            var uploadTasks = new List<Task>();
            for (int imageId = 0; imageId < imagePaths.Count(); imageId++)
            {
                uploadTasks.Add( 
                    UploadAlbumImage(
                        sourceImagePath: imagePaths[imageId],
                        albumName: albumName,
                        imageId: imageId,
                        itemKeys: itemKeys)
                );
                itemKeys = new ItemKeys
                {
                    ImageKey = itemKeys.ImageKey.GetNextKey(),
                    ThumbnailKey = itemKeys.ThumbnailKey.GetNextKey()
                };
            }
            await Task.WhenAll(uploadTasks.ToArray());
        }

        private static async Task UploadAlbumImage(string sourceImagePath, string albumName, int imageId, ItemKeys itemKeys)
        {
            try
            {
                string encryptedImagePath = await EncryptImage(sourceImagePath, itemKeys.ImageKey);
                string encryptedThumbnailPath = await EncryptThumbnail(sourceImagePath, itemKeys.ThumbnailKey);

                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.png.aes");
                string thumbnailDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.tmb.png.aes");

                File.Move(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath);
                File.Move(sourceFileName: encryptedThumbnailPath,
                    destFileName: thumbnailDestPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private static async Task<string> EncryptImage(string sourceImagePath, SymmetricKey imageKey)
        {
            try
            {
                string encryptedImagePath = Path.GetRandomFileName();

                Bitmap b = new Bitmap(sourceImagePath);
                using (MemoryStream ms = new MemoryStream())
                {
                    //todo: could be optimized by converting in background after add
                    b.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    using (var outputStream = new FileStream(encryptedImagePath, FileMode.Create))
                    {
                        await imageKey.Encrypt(ms, outputStream);
                    }
                }
                return encryptedImagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
                return null;
            }
        }

        private static async Task<string> EncryptThumbnail(string sourceImagePath, SymmetricKey thumbnailKey)
        {
            try
            {
                string encryptedThumbnailPath = Path.GetRandomFileName();

                Bitmap image = new Bitmap(sourceImagePath);
                using (MemoryStream ms = new MemoryStream())
                {
                    //todo: could be optimized by converting in background after add
                    Bitmap thumbnail;
                    const int largestThumbnailDimension = 150;
                    if (image.Height > image.Width)
                    {
                       double ratio = (double) image.Height / largestThumbnailDimension;
                       thumbnail = new Bitmap(
                           image,
                           (int)(image.Width / ratio),
                           largestThumbnailDimension);
                    }
                    else
                    {
                        double ratio = (double)image.Width / largestThumbnailDimension;
                        thumbnail = new Bitmap(
                            image,
                            largestThumbnailDimension,
                            (int)(image.Height / ratio));
                    }

                    thumbnail.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    using (var outputStream = new FileStream(encryptedThumbnailPath, FileMode.Create))
                    {
                        await thumbnailKey.Encrypt(ms, outputStream);
                    }
                }
                return encryptedThumbnailPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
                return null;
            }
        }

    }

    public class ImageItem
    {
        public string ImagePath { get; set; }
    }

    public class ImageItemButton : Button
    {
        public ImageItem Item
        {
            get => (ImageItem) GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ImageItem), typeof(ImageItemButton), null);
    }
}