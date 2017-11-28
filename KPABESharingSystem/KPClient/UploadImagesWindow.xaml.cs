using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KPServices;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per UploadImagesWindow.xaml
    /// </summary>
    public partial class UploadImagesWindow : Window
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
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files(*.jpg, *.jpeg, *.png, *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            openFileDialog.Multiselect = true;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    ImageItems.Add(new ImageItem() { ImagePath = filename});
                }
            }
        }

        private bool ValidateTags()
        {
            return false;
        }

        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var keyBytes = new byte[256/8];
            var iv = new byte[128/8];
            string workingDir = Path.GetTempPath();

            GenerateKeyAndIv(ref keyBytes, ref iv);

            string keyPath = Path.Combine(workingDir, Path.GetRandomFileName());
            var key = new
            {
                IV = iv,
                Key = keyBytes
            };

            using (FileStream fs = new FileStream(keyPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string serializedKey = JsonConvert.SerializeObject(key);
                    sw.Write(serializedKey);
                }
            }

            string encryptedKeyPath = Path.GetRandomFileName();

            App app = (App)Application.Current;

            app.KpService.Encrypt(
                sourceFilePath: keyPath,
                destFilePath: encryptedKeyPath,
                attributes: TagsSelector.GetTagsString());

            //string finalKeyPath = (albumName ?? basename) + ".key.kpabe";

            string finalKeyPath;

            if (ImageItems.Count == 1)
            {
                var imagePath = ImageItems.First().ImagePath;
                UploadImage(imagePath, null, null, keyBytes, iv);
                finalKeyPath = Path.GetFileNameWithoutExtension(imagePath) + ".key.kpabe";
            }
            else
            {
                var albumName = DateTime.Now.ToString("yyyy-M-d_HH-mm-ss-ff");
                var albumPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", albumName);
                Directory.CreateDirectory(albumPath);
                UploadAlbum(ImageItems.Select(item => item.ImagePath).ToArray(), albumName, keyBytes, iv);
                finalKeyPath = albumName + ".key.kpabe";
            }

            string keyDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", finalKeyPath);

            File.Copy(sourceFileName: encryptedKeyPath,
                destFileName: keyDestPath,
                overwrite: true);
        }

        private void GenerateKeyAndIv(ref byte[] keyBytes, ref byte[] ivBytes)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            if (keyBytes == null)
                throw new ArgumentException("keyBytes must be initialized");
            if (ivBytes == null)
                throw new ArgumentException("ivBytes must be initialized");
            rngCsp.GetBytes(keyBytes);
            rngCsp.GetBytes(ivBytes);
        }

        private void UploadImage(string imagePath, String albumName, int? imageId, byte[] keyBytes, byte[] ivBytes)
        {
            try
            {
                string workingDir = Path.GetTempPath();
                Console.WriteLine(workingDir);
                string basename = Path.GetFileNameWithoutExtension(imagePath);
                string convertedFilepath = Path.Combine(workingDir, Path.GetRandomFileName());

                Bitmap b = new Bitmap(imagePath);
                using (FileStream fs = new FileStream(convertedFilepath, FileMode.Create)) {
                    b.Save(fs, ImageFormat.Png);
                }

                Aes aes = new AesCng();
                aes.KeySize = 256;
                aes.Key = keyBytes;
                aes.IV = ivBytes; //always 128 bits

                
                var encryptor = aes.CreateEncryptor();

                string encryptedImagePath = Path.GetRandomFileName();
                using (FileStream outputFileStream = new FileStream(encryptedImagePath, FileMode.Create))
                {
                    using (CryptoStream encryptCryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (FileStream inputFileStream = new FileStream(convertedFilepath, FileMode.Open))
                        {
                            inputFileStream.CopyTo(encryptCryptoStream);
                        }
                    }
                }

                string finalImageName;

                if (albumName != null)
                {
                    finalImageName = albumName + $".{imageId}" + ".png.aes";
                }
                else
                {
                    finalImageName = $"{basename}.png.aes";
                }

                string finalImagePath = Path.Combine(albumName ?? "", finalImageName);

                string imageDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", finalImagePath);
                Console.WriteLine(imageDestPath);
                
                File.Copy(sourceFileName: encryptedImagePath,
                            destFileName : imageDestPath,
                            overwrite: true);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private void UploadAlbum(string[] imagePaths, string albumName, byte[] key, byte[] iv)
        {
            int i = 0;
            foreach(var imagePath in imagePaths)
            {
                var sha = new SHA256Cng();
                UploadImage(imagePath, albumName, i, key, iv);
                key = sha.ComputeHash(key);
                iv = sha.ComputeHash(iv).Take(128/8).ToArray();
                ++i;
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
            get { return (ImageItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ImageItem), typeof(ImageItemButton), null);
    }
}
