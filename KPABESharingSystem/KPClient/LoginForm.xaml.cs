using System;
using System.Text.RegularExpressions;
using System.Windows;


namespace KPClient
{
    /// <summary>
    /// Logica di interazione per LoginForm.xaml
    /// </summary>
    public partial class LoginForm
    {
        static readonly Regex UsernameRegex = new Regex(@"^.+$");
        static readonly Regex PasswordRegex = new Regex(@"^.{8,}$");

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;
            if (!UsernameRegex.IsMatch(username))
            {
                ErrorLabel.Content = "Invalid username field";
                return;
            }
            if (!PasswordRegex.IsMatch(password))
            {
                ErrorLabel.Content = "Invalid password field";
                return;
            }

            App app = (App) Application.Current;
            try
            {
                if (app.KpRestClient.Login(username, password))
                {
                    Properties.Settings.Default.CachedUsername = username;
                    Close();
                }
                else
                    ErrorLabel.Content = "Wrong Username or Password";
            }
            catch (Exception)
            {
                ErrorLabel.Content = "Can't connect: check the settings";
            }
        }
    }
}