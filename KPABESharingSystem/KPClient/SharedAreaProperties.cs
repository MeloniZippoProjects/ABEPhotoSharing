using System.Windows;

namespace KPClient
{
    public partial class SharedArea
    {
        public bool FilterOutOfPolicy
        {
            get => (bool) GetValue(FilterOutOfPolicyProperty);
            set => SetValue(FilterOutOfPolicyProperty, value);
        }

        public static readonly DependencyProperty FilterOutOfPolicyProperty =
            DependencyProperty.Register("FilterOutOfPolicy", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false, FilterOutOfPolicy_OnChange));

        public bool ShowThumbnails
        {
            get => (bool) GetValue(ShowThumbnailsProperty);
            set => SetValue(ShowThumbnailsProperty, value);
        }

        public static readonly DependencyProperty ShowThumbnailsProperty =
            DependencyProperty.Register("ShowThumbnails", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false, ShowThumbnails_OnChange));

        public bool PreloadThumbnails
        {
            get => (bool)GetValue(PreloadThumbnailsProperty);
            set => SetValue(PreloadThumbnailsProperty, value);
        }

        public static readonly DependencyProperty PreloadThumbnailsProperty =
            DependencyProperty.Register("PreloadThumbnails", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false, PreloadThumbnails_OnChange));

        public bool IsValidSharedFolder
        {
            get => (bool) GetValue(IsValidSharedFolderProperty);
            private set => SetValue(IsValidSharedFolderProperty, value);
        }

        public static readonly DependencyProperty IsValidSharedFolderProperty =
            DependencyProperty.Register("IsValidSharedFolder", typeof(bool), typeof(SharedArea),
                new PropertyMetadata(false));

        public string SharedFolderPath
        {
            get => (string) GetValue(SharedFolderPathProperty);
            set => SetValue(SharedFolderPathProperty, value);
        }

        public static readonly DependencyProperty SharedFolderPathProperty = DependencyProperty.Register(
            "SharedFolderPath",
            typeof(string),
            typeof(SharedArea),
            new PropertyMetadata(
                null,
                SharedFolderPath_OnChange)
        );

        public string CurrentAlbum
        {
            get => (string) GetValue(CurrentAlbumProperty);
            set => SetValue(CurrentAlbumProperty, value);
        }

        public static readonly DependencyProperty CurrentAlbumProperty =
            DependencyProperty.Register("CurrentAlbum", typeof(string), typeof(SharedArea),
                new PropertyMetadata("Main folder"));

        public SharedArea()
        {
            InitializeComponent();
        }
    }
}