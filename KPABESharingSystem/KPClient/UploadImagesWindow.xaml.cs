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
            if (ImageItems.Count == 1)
                UploadImage(ImageItems.First().ImagePath);
            else
                UploadAlbum(ImageItems.Select(item => item.ImagePath).ToArray());
        }

        private void UploadImage(string imagePath)
        {
            try
            {
                string workingDir = Path.GetTempPath();
                Console.WriteLine(workingDir);
                string basename = Path.GetFileNameWithoutExtension(imagePath);
                string convertedFilepath = Path.Combine(workingDir, $"{basename}.png");

                Bitmap b = new Bitmap(imagePath);
                using (FileStream fs = new FileStream(convertedFilepath, FileMode.Create)) {
                    b.Save(fs, ImageFormat.Png);
                }

                Aes aes = new AesCng();
                aes.KeySize = 256;
                aes.GenerateIV();
                aes.GenerateKey();

                string keyPath = Path.Combine(workingDir, $"{basename}.key");
                var key = new
                {
                    IV = aes.IV,
                    Key = aes.Key
                };
                using (FileStream fs = new FileStream(keyPath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string serializedKey = JsonConvert.SerializeObject(key);
                        sw.Write(serializedKey);
                    }
                }

                string encryptedKeyPath = $"{keyPath}.kpabe";

                App app = (App) Application.Current;

                app.KpService.Encrypt(
                    sourceFilePath: keyPath, 
                    destFilePath: encryptedKeyPath,
                    attributes: TagsSelector.GetTagsString());
                
                var encryptor = aes.CreateEncryptor();

                string encryptedImagePath = $"{convertedFilepath}.aes";
                using (FileStream outputFS = new FileStream(encryptedImagePath, FileMode.Create))
                {
                    using (CryptoStream encryptCS = new CryptoStream(outputFS, encryptor, CryptoStreamMode.Write))
                    {
                        using (FileStream inputFS = new FileStream(convertedFilepath, FileMode.Open))
                        {
                            inputFS.CopyTo(encryptCS);
                        }
                    }
                }

                string keyDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "items", Path.GetFileName(encryptedImagePath));
                string imageDestPath = Path.Combine(Properties.Settings.Default.SharedFolderPath, "keys", Path.GetFileName(encryptedKeyPath));
                Console.WriteLine(keyDestPath);
                Console.WriteLine(imageDestPath);

                File.Copy(sourceFileName: encryptedKeyPath,
                            destFileName: keyDestPath,
                            overwrite: true);
                
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

        private void UploadAlbum(string[] imagePaths)
        {
            MessageBox.Show("Not yet implemented!");
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
