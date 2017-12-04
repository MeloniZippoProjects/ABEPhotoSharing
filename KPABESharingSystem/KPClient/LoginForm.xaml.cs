using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        static Regex usernameRegex = new Regex(@"^.+$");
        static Regex passwordRegex = new Regex(@"^.{8,}$");

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Password;
            if (!usernameRegex.IsMatch(username))
            {
                ErrorLabel.Content = "Invalid username field";
                return;
            }
            if (!passwordRegex.IsMatch(password))
            {
                ErrorLabel.Content = "Invalid password field";
                return;
            }

            App app = (App) Application.Current;
            try
            {
                if (app.KPRestClient.Login(username, password))
                {
                    app.Username = username;
                    this.Close();
                }
                else
                    ErrorLabel.Content = "Wrong Username or Password";
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = "Can't connect: check the settings";
            }
        }
    }
}
