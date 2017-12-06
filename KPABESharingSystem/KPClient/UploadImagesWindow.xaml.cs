using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            //ImagesToUploadControl.ItemsSource = ImageItems;

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
        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            SymmetricKey symmetricKey = new SymmetricKey
            {
                Key = new byte[256 / 8],
                Iv = new byte[128 / 8]
            };

            GenerateKeyAndIv(symmetricKey);

            string keyPath = Path.GetTempFileName();

            using (FileStream fs = new FileStream(keyPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string serializedKey = JsonConvert.SerializeObject(symmetricKey);
                    sw.Write(serializedKey);
                }
            }

            string encryptedKeyPath = Path.GetRandomFileName();

            App app = (App) Application.Current;

            app.KpService.Encrypt(
                sourceFilePath: keyPath,
                destFilePath: encryptedKeyPath,
                attributes: TagsSelector.GetTagsString());

            string finalKeyPath;

            if (ImageItems.Count == 1)
            {
                string imagePath = ImageItems.First().ImagePath;
                string imageName = Path.GetFileNameWithoutExtension(imagePath);
                UploadImage(imagePath, imageName, symmetricKey);
                finalKeyPath = Path.GetFileNameWithoutExtension(imagePath) + ".key.kpabe";
            }
            else
            {
                string albumName = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss-ff");
                string albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
                Directory.CreateDirectory(albumPath);
                UploadAlbum(ImageItems.Select(item => item.ImagePath).ToArray(), albumName, symmetricKey);
                finalKeyPath = albumName + ".key.kpabe";
            }

            string keyDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", finalKeyPath);

            File.Copy(sourceFileName: encryptedKeyPath,
                destFileName: keyDestPath,
                overwrite: true);
        }

        private static void GenerateKeyAndIv(SymmetricKey symmetricKey)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(symmetricKey.Key);
            rngCsp.GetBytes(symmetricKey.Iv);
        }

        private static string EncryptImage(string sourceImagePath, SymmetricKey symmetricKey)
        {
            try
            {
                string convertedFilepath = Path.GetTempFileName();

                Bitmap b = new Bitmap(sourceImagePath);
                using (FileStream fs = new FileStream(convertedFilepath, FileMode.Create))
                {
                    b.Save(fs, ImageFormat.Png);
                }

                string encryptedImagePath = Path.GetRandomFileName();

                using (Stream inputStream = new FileStream(convertedFilepath, FileMode.Open),
                    outputStream = new FileStream(encryptedImagePath, FileMode.Create))
                {
                    symmetricKey.Encrypt(inputStream, outputStream);
                }

                return encryptedImagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
                return null;
            }
        }

        private void UploadImage(string sourceImagePath, string imageName, SymmetricKey symmetricKey)
        {
            try
            {
                string encryptedImagePath = EncryptImage(sourceImagePath, symmetricKey);
                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    $"{imageName}.png.aes");
                Console.WriteLine(imageDestPath);

                File.Copy(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath,
                    overwrite: true);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private void UploadAlbum(IEnumerable<string> imagePaths, string albumName, SymmetricKey symmetricKey)
        {
            int imageId = 0;
            foreach (string imagePath in imagePaths)
            {
                UploadAlbumImage(imagePath, albumName, imageId, symmetricKey);
                symmetricKey = symmetricKey.GetNextKey();
                ++imageId;
            }
        }

        private void UploadAlbumImage(string sourceImagePath, string albumName, int imageId, SymmetricKey symmetricKey)
        {
            try
            {
                string encryptedImagePath = EncryptImage(sourceImagePath, symmetricKey);
                string imageDestPath = Path.Combine(
                    Properties.Settings.Default.SharedFolderPath,
                    "items",
                    albumName,
                    $"{albumName}.{imageId}.png.aes");
                Console.WriteLine(imageDestPath);

                File.Copy(sourceFileName: encryptedImagePath,
                    destFileName: imageDestPath,
                    overwrite: true);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
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