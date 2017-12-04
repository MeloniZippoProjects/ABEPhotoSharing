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
        private static Regex TagSequence = new Regex(@"^\s*(?<attribute>[a-zA-Z][a-zA-Z0-9_]*(?:\s*=\s*\d+)?\s+)+\s*$");

        private List<TagSpecification> validTags = new List<TagSpecification>();

        public event EventHandler ValidityChanged;

        private bool? _isValid = null;

        public bool IsValid
        {
            get => _isValid ?? false;
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

            Universe universe = ((App)Application.Current).Universe;
            UniverseReminder.Text = $"The universe is: {universe}";

            ValidityChanged += (sender, args) =>
            {
                TagsTextBox.Background =
                    IsValid ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.IndianRed);
            };
        }

        private void TagsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string tagsText = TagsTextBox.Text + " ";
            if (TagSequence.IsMatch(tagsText))
            {
                Match tagSequenceMatch = TagSequence.Match(tagsText);

                var tags = tagSequenceMatch.Groups["attribute"].Captures
                    .Cast<Capture>()
                    .Select(tag => new TagSpecification(tag.Value));

                bool duplicateCheck = tags.GroupBy(tag => tag.Name)
                    .Any(group => group.Count() > 1);

                Universe universe = ((App) Application.Current).Universe;
                validTags = tags.Where(tag => universe.ValidateTag(tag)).ToList();

                if (duplicateCheck || validTags.Count() < tags.Count())
                    IsValid = false;
                else
                    IsValid = true;
            }
            else
                IsValid = false;
        }

        public string GetTagsString()
        {
            if (IsValid)
            {
                Universe universe = ((App)Application.Current).Universe;
                string ret = "";
                foreach (TagSpecification tag in validTags)
                {
                    ret += $" '{universe.GetTagString(tag)}'";
                }
                return ret;
            }
            else
            {
                throw new InvalidOperationException("The tags are not valid");
            }
        }
    }
}
