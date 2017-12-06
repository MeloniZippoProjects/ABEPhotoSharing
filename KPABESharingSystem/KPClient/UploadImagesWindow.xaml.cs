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
            SymmetricKey symmetricKey = new SymmetricKey();
            symmetricKey.GenerateKey();
            
            var tasks = new List<Task>();

            if (ImageItems.Count == 1)
            {
                string imagePath = ImageItems.First().ImagePath;
                string imageName = Path.GetFileNameWithoutExtension(imagePath);
                tasks.Add(UploadKey(symmetricKey, imageName));
                tasks.Add(UploadImage(
                    imagePath,
                    imageName,
                    symmetricKey)
                );
            }
            else
            {
                string albumName = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss-ff");
                string albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
                Directory.CreateDirectory(albumPath);
                tasks.Add(UploadKey(symmetricKey, albumName));
                tasks.Add(UploadAlbum(
                    ImageItems.Select(item => item.ImagePath).ToArray(),
                    albumName,
                    symmetricKey));
            }

            await Task.WhenAll(tasks.ToArray());

            Close();
        }

        private async Task UploadKey(SymmetricKey symmetricKey, string name)
        {
            string keyPath = Path.GetTempFileName();
            using (FileStream fs = new FileStream(keyPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string serializedKey = await Dispatcher.InvokeAsync(() =>
                        JsonConvert.SerializeObject(symmetricKey));
                    await sw.WriteAsync(serializedKey);
                }
            }

            string encryptedKeyPath = Path.GetTempFileName();

            App app = (App)Application.Current;

            app.KpService.Encrypt(
                sourceFilePath: keyPath,
                destFilePath: encryptedKeyPath,
                attributes: TagsSelector.GetTagsString());

            string keyName = $"{name}.key.kpabe";

            string keyDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", keyName);

            File.Move(sourceFileName: encryptedKeyPath,
                destFileName: keyDestPath);
        }

        private static async Task UploadImage(string sourceImagePath, string imageName, SymmetricKey symmetricKey)
        {
            try
            {
                string encryptedImagePath = await EncryptImage(sourceImagePath, symmetricKey);
                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.png.aes");
                Console.WriteLine(imageDestPath);

                File.Move(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private async Task UploadAlbum(string[] imagePaths, string albumName, SymmetricKey symmetricKey)
        {
            var uploadTasks = new List<Task>();
            for (int imageId = 0; imageId < imagePaths.Count(); imageId++)
            {
                uploadTasks.Add( UploadAlbumImage(
                    sourceImagePath: imagePaths[imageId],
                    albumName: albumName,
                    imageId: imageId,
                    symmetricKey: symmetricKey)
                );
                symmetricKey = symmetricKey.GetNextKey();
            }
            await Task.WhenAll(uploadTasks.ToArray());
        }

        private async Task UploadAlbumImage(string sourceImagePath, string albumName, int imageId, SymmetricKey symmetricKey)
        {
            try
            {
                string encryptedImagePath = await EncryptImage(sourceImagePath, symmetricKey);
                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.png.aes");
                Console.WriteLine(imageDestPath);

                File.Move(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private static async Task<string> EncryptImage(string sourceImagePath, SymmetricKey symmetricKey)
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
                        await symmetricKey.Encrypt(ms, outputStream);
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