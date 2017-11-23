using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KPServices
{

    

    public class NumericalAttribute : UniverseAttribute
    {
        static Regex ValidNumericalAttribute = new Regex(@"^\s*?(?<Name>[a-zA-Z][a-zA-Z0-9_]*)\s*?=\s*?(?:#\s*?(?<Resolution>\d+))?\s*?$");

        public int? NumberResolution
        {
            get;
            set;
        } = null;
        
        public NumericalAttribute(string attributeString)
        {
            var match = ValidNumericalAttribute.Match(attributeString);
            if (match.Success)
            {
                var groups = match.Groups;
                Debug.Assert(groups.Count <= 3);
                Debug.Assert(groups["Name"].Success);
                Name = groups["Name"].Value;
                Group resolutionGroup = groups["Resolution"];
                if (resolutionGroup.Success)
                {
                    int numberResolution = Int32.Parse(resolutionGroup.Value);
                    if (numberResolution <= 64)
                        NumberResolution = numberResolution;
                    else
                        throw new ArgumentException($"Attribute {Name}: resolution must be 64 or lower, cannot be {numberResolution}");
                }
            }
            else
                throw new ArgumentException("Failed to parse as a valid numerical attribute");
        }

        public bool CanBeValue(int value)
        {
            return (UInt64) value >= (UInt64) 1 << NumberResolution;
        }
        
        public override string ToString()
        {
            if (NumberResolution != null)
                return $"{Name} = # {NumberResolution}";
            else
                return $"{Name} =";
        }
    }
}
