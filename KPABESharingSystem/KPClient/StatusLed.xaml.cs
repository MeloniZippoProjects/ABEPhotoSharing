using System;
using System.Windows;

namespace KPClient
{
    /// <summary>
    /// Interaction logic for StatusLed.xaml
    /// </summary>
    public partial class StatusLed
    {
        public enum StatusEnum
        {
            Ok,
            Working,
            Error
        }

        public StatusEnum Status
        {
            get => (StatusEnum) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                "Status",
                typeof(StatusEnum),
                typeof(StatusLed),
                new PropertyMetadata(
                    StatusEnum.Ok,
                    OnStatusChanged)
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