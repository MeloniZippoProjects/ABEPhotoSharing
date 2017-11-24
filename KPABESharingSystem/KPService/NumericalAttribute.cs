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
                    if (numberResolution <= 64 && numberResolution > 0)
                        NumberResolution = numberResolution;
                    else
                        throw new ArgumentException($"Attribute {Name}: resolution must a positive integer equal to 64 or lower, cannot be {numberResolution}");
                }
            }
            else
                throw new ArgumentException("Failed to parse as a valid numerical attribute");
        }

        public bool CanBeValue(UInt64 value)
        {
            int resolution = NumberResolution ?? 64;
            if (resolution == 64)
                return true;    //Result is insignificant, value may have been truncated
            else
                return value < (UInt64) 1 << resolution;
        }
        
        public override string ToString()
        {
            if (NumberResolution != null)
                return $"{Name} = # {NumberResolution}";
            else
                return $"{Name} =";
        }

        internal string GetTagString(TagSpecification tag)
        {
            if(tag.Name != Name)
                throw new ArgumentException("This tag is not related to this attribute");
            if(tag.Value == null)
                throw new ArgumentException("Tag has not a specified value");
            if(!CanBeValue((UInt64)tag.Value))
                throw new ArgumentException($"Tag's specified value cannot be represented on {NumberResolution} bits");

            return NumberResolution == null
                ? $"{Name} = {tag.Value}"
                : $"{Name} = {tag.Value} # {NumberResolution}";
        }
    }
}
