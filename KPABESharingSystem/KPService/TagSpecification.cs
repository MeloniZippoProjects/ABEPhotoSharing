using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KPServices
{
    public class TagSpecification
    {
        private static Regex TagStructure = new Regex(@"(?<name>[a-zA-Z][a-zA-Z0-9_]*)(?:\s*=\s*(?<value>\d+))?");

        public string Name { get; private set; }
        public UInt64? Value { get; private set; }

        public TagSpecification(string tagSpecification)
        {
            var tagMatch = TagStructure.Match(tagSpecification);
            Name = tagMatch.Groups["name"].Value;
            Value = tagMatch.Groups["value"].Success
                ? UInt64.TryParse(tagMatch.Groups["value"].Value, out var parsed)
                    ? parsed
                    : (UInt64?) null
                : (UInt64?) null;
        }
    }
}
