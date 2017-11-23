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
    /// Interaction logic for StatusLed.xaml
    /// </summary>
    public partial class StatusLed : UserControl
    {
        public enum StatusEnum
        {
            Ok,
            Working,
            Error
        }

        public StatusEnum Status
        {
            get { return (StatusEnum) GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                "Status",
                typeof(StatusEnum),
                typeof(StatusLed),
                new PropertyMetadata(
                    StatusEnum.Ok,
                    new PropertyChangedCallback(OnStatusChanged))
                );

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusLed led = (StatusLed) d;
            StatusEnum status = (StatusEnum) e.NewValue;

            switch (status)
            {
                case StatusEnum.Ok:
                {
                    led.OkLed.Visibility = Visibility.Visible;
                    led.WorkingLed.Visibility = Visibility.Hidden;
                    led.ErrorLed.Visibility = Visibility.Hidden;
                    break;
                }
                case StatusEnum.Working:
                {
                    led.OkLed.Visibility = Visibility.Hidden;
                    led.WorkingLed.Visibility = Visibility.Visible;
                    led.ErrorLed.Visibility = Visibility.Hidden;
                    break;
                }
                case StatusEnum.Error:
                {
                    led.OkLed.Visibility = Visibility.Hidden;
                    led.WorkingLed.Visibility = Visibility.Hidden;
                    led.ErrorLed.Visibility = Visibility.Visible;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public StatusLed()
        {
            InitializeComponent();
        }
    }
}
