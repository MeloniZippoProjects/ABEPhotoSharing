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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KPServices;

namespace KPClient
{
    /// <summary>
    /// Interaction logic for TagsSelector.xaml
    /// </summary>
    public partial class TagsSelector : UserControl
    {
        private static Regex TagSequence = new Regex(@"^\s*(?<attribute>[a-zA-Z][a-zA-Z0-9_]*(?:\s*=\s*\d+)?\s+)*\s*$");
        private static Regex Tag = new Regex(@"(?<name>[a-zA-Z][a-zA-Z0-9_]*)(?:\s*=\s*(?<value>\d+))?");

        public event EventHandler ValidityChanged;

        private bool _isValid;
        public bool IsValid
        {
            get=>_isValid;
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    ValidityChanged?.Invoke(this, null);
                }
            }
        }
        
        public TagsSelector()
        {
            InitializeComponent();

            ValidityChanged += (sender, args) =>
            {
                if (IsValid)
                    TagsTextBox.Background = new SolidColorBrush(Colors.LightGreen);
                else
                    TagsTextBox.Background = new SolidColorBrush(Colors.IndianRed);
            };
        }
        
        private void TagsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            tags.Clear();
            string tagsText = TagsTextBox.Text + " ";
            if (TagSequence.IsMatch(tagsText))
            {
                Match tagSequenceMatch = TagSequence.Match(tagsText);

                var tags = tagSequenceMatch.Groups["attribute"].Captures
                    .Cast<Capture>()
                    .Select(tag => Tag.Match(tag.Value))
                    .Select(tagMatch => new
                    {
                        name = tagMatch.Groups["name"].Value,
                        value = tagMatch.Groups["value"].Success
                            ? Int32.Parse(tagMatch.Groups["value"].Value)
                            : (int?)null
                    });

                bool duplicateCheck = tags.GroupBy(tag => tag.name)
                    .Any(group => group.Count() > 1);

                Universe universe = ((App)Application.Current).Universe;
                var validTags = tags.Where(tag => universe.Contains(tag.name, tag.value));

                if (duplicateCheck || validTags.Count() < tags.Count())
                    IsValid = false;

                //todo: use validTags for the AvailableTagsControl
            }
            else
                IsValid = false;
        }
    }
}
