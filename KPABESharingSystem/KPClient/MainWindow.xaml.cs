using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
#if DEBUG
            OpenUploadImagesWindowButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));    //Force a click() event on the button
#endif
        }

        private void OpenUploadImagesWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var uploadImagesWindow = new UploadImagesWindow();
            uploadImagesWindow.ShowDialog();
        }


        private void SetSharedSpaceLocationButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ReloadSharedSpaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void HideOutOfPolicyCheckBox_OnChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
