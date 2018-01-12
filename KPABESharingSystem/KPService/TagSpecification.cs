using System;
using System.Text.RegularExpressions;

// ReSharper disable BuiltInTypeReferenceStyle

namespace KPServices
{
    public class TagSpecification
    {
        private static readonly Regex TagStructure = new Regex(@"(?<name>[a-zA-Z][a-zA-Z0-9_]*)(?:\s*=\s*(?<value>\d+))?");

        public string Name { get; }
        public UInt64? Value { get; }

        public TagSpecification(string tagSpecification)
        {
            Match tagMatch = TagStructure.Match(tagSpecification);
            Name = tagMatch.Groups["name"].Value;
            Value = tagMatch.Groups["value"].Success
                ? UInt64.TryParse(tagMatch.Groups["value"].Value, out ulong parsed)
                    ? parsed
                    : (UInt64?) null
                : null;
        }
    }
}