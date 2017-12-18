using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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

        private string defaultName = "";
        private bool isNameDefault = true;

        public UploadImagesWindow()
        {
            InitializeComponent();

            ImageItems.CollectionChanged += UpdateClearAllButtonStatus;
            ImageItems.CollectionChanged += UpdateUploadButtonStatus;
            ImageItems.CollectionChanged += UpdateDefaultName;
            TagsSelector.ValidityChanged += UpdateUploadButtonStatus;
        }

        private void NameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (NameTextBox.Text != defaultName)
                isNameDefault = false;
        }

        private void UpdateDefaultName(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ImageItems.Count == 0)
            {
                NameTextBox.Text = "";
                isNameDefault = true;
            }
            else
            {
                if (isNameDefault)
                {
                    if (ImageItems.Count == 1)
                    {
                        ImageItem item = ImageItems[0];
                        defaultName = Path.GetFileNameWithoutExtension(item.ImagePath);
                    }
                    else
                    {
                        defaultName = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss-ff");
                    }

                    if (defaultName != null) NameTextBox.Text = defaultName;
                }
            }
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
                    ImageItems.Add(new ImageItem(imagePath: filename));
                }
            }
        }

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
                ImageItem imageItem = ImageItems.First();
                string imageName = NameTextBox.Text;

                string destImagePath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.png.aes");
                string destThumbnailPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.tmb.png.aes");

                tasks.Add(UploadKeys(itemKeys, imageName));
                tasks.Add(
                    UploadImage(
                        imageItem: imageItem,
                        destImagePath: destImagePath,
                        destThumbnailPath: destThumbnailPath,
                        itemKeys: itemKeys)
                );
            }
            else
            {
                string albumName = NameTextBox.Name;
                string albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
                Directory.CreateDirectory(albumPath);

                tasks.Add(UploadKeys(itemKeys, albumName));
                tasks.Add(
                    UploadAlbum(
                        imageItems: ImageItems.ToArray(),
                        albumName: albumName,
                        itemKeys: itemKeys)
                    );
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
            File.Delete(keysPath);
        }

        private static async Task UploadImage(
            ImageItem imageItem,
            string destImagePath,
            string destThumbnailPath,
            ItemKeys itemKeys)
        {
            try
            {
                string encryptedImagePath = await EncryptImage(imageItem.Source, itemKeys.ImageKey);
                string encryptedThumbnailPath = await EncryptImage(imageItem.Thumbnail, itemKeys.ThumbnailKey);

                File.Move(sourceFileName: encryptedImagePath,
                    destFileName: destImagePath);
                File.Move(sourceFileName: encryptedThumbnailPath,
                    destFileName: destThumbnailPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }


        private static async Task UploadAlbum(IReadOnlyList<ImageItem> imageItems, string albumName, ItemKeys itemKeys)
        {
            var uploadTasks = new List<Task>();
            for (int imageId = 0; imageId < imageItems.Count(); imageId++)
            {
                string destImagePath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.png.aes");
                string destThumbnailPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.tmb.png.aes");

                uploadTasks.Add( 
                    UploadImage(
                        imageItem: imageItems[imageId],
                        destImagePath: destImagePath,
                        destThumbnailPath: destThumbnailPath,
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

        private static async Task<string> EncryptImage(BitmapSource image, SymmetricKey imageKey)
        {
            try
            {
                string encryptedImagePath = Path.GetTempFileName();

                using (MemoryStream ms = new MemoryStream())
                {
                    BitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(image));
                    pngEncoder.Save(ms);

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
    }

    public class ImageItem
    {
        private const int LargestThumbnailDimension = 150;
        
        public string ImagePath { get; private set; }
        public BitmapImage Source { get; private set; }
        public BitmapImage Thumbnail { get; private set; }

        public ImageItem(string imagePath)
        {
            ImagePath = imagePath;
            Source = new BitmapImage(new Uri(ImagePath));

            Thumbnail = new BitmapImage();
            Thumbnail.BeginInit();
            Thumbnail.UriSource = new Uri(ImagePath);
            if (Source.Height > Source.Width)
                Thumbnail.DecodePixelHeight = LargestThumbnailDimension;
            else
                Thumbnail.DecodePixelWidth = LargestThumbnailDimension;
            Thumbnail.EndInit();
        }
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