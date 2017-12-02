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
using System.Windows.Shapes;
using Grapevine.Client;
using Grapevine.Shared;
using Newtonsoft.Json;


namespace KPClient
{
    /// <summary>
    /// Logica di interazione per LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        //todo: should work also with enter in the textboxes
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Password;
            App app = (App) Application.Current;

            if (app.KPRestClient.Login(username, password))
                this.Close();
            else
                ErrorLabel.Content = "Wrong Username or Password";
        }
    }
}
