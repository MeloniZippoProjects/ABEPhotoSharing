using System.Windows;

namespace KPClient
{
    public partial class SharedArea
    {
        public bool FilterOutOfPolicy
        {
            get { return (bool) GetValue(FilterOutOfPolicyProperty); }
            set { SetValue(FilterOutOfPolicyProperty, value); }
        }

        public static readonly DependencyProperty FilterOutOfPolicyProperty =
            DependencyProperty.Register("FilterOutOfPolicy", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false, FilterOutOfPolicy_OnChange));

        public bool ShowPreviews
        {
            get { return (bool) GetValue(ShowPreviewsProperty); }
            set { SetValue(ShowPreviewsProperty, value); }
        }

        public static readonly DependencyProperty ShowPreviewsProperty =
            DependencyProperty.Register("ShowPreviews", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false, ShowPreviews_OnChange));

        public bool IsFolderPathValid
        {
            get { return (bool) GetValue(IsFolderPathValidProperty); }
            private set { SetValue(IsFolderPathValidProperty, value); }
        }

        public static readonly DependencyProperty IsFolderPathValidProperty =
            DependencyProperty.Register("IsFolderPathValid", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false));

        public string SharedFolderPath
        {
            get { return (string) GetValue(SharedFolderPathProperty); }
            set { SetValue(SharedFolderPathProperty, value); }
        }

        public static readonly DependencyProperty SharedFolderPathProperty = DependencyProperty.Register(
            "SharedFolderPath",
            typeof(string),
            typeof(SharedArea),
            new PropertyMetadata(
                null,
                SharedFolderPath_OnChange)
        );

        public SharedArea()
        {
            InitializeComponent();
        }
    }
}