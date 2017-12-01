﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KPServices;
using MahApps.Metro.IconPacks;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Path = System.IO.Path;

namespace KPClient
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OpenUploadImagesWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var uploadImagesWindow = new UploadImagesWindow();
            uploadImagesWindow.ShowDialog();
            SharedArea.LoadRootItems();
        }

        private void SetSharedSpaceLocationButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.SharedFolderPath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    //UpdateSharedArea();
                }
            }
        }

        private void ReloadSharedSpaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            SharedArea.LoadRootItems();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = KPClient.Properties.Settings.Default;
            settings.Universe = null;
            File.Delete(settings.PublicKeyPath);
            settings.PublicKeyPath = null;
            File.Delete(settings.PrivateKeyPath);
            settings.PrivateKeyPath = null;
            settings.Save();

            Application.Current.Shutdown();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            //todo: implement home button
        }
    }
}
