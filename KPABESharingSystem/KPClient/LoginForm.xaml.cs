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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Password;

            RestRequest request = new RestRequest
            {
                Payload = JsonConvert.SerializeObject(
                    new {
                        Username = username,
                        Password = password
                    }),
                Encoding = Encoding.UTF8,
                HttpMethod = HttpMethod.POST,
                ContentType = ContentType.TXT,
                RequestUri = new Uri("/login")
            };
            var response = ((App) Application.Current).RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
                this.Close();
            else
                ErrorLabel.Content = "Wrong Username or Password";
        }
    }
}
