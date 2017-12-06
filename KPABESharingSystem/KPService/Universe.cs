using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KPServices
{
    public class Universe
    {
        readonly HashSet<UniverseAttribute> _attributes = new HashSet<UniverseAttribute>();

        public Universe()
        {
        }

        public Universe(IEnumerable<UniverseAttribute> attributes)
        {
            foreach (UniverseAttribute attribute in attributes)
            {
                AddAttribute(attribute);
            }
        }

        public Universe Copy()
        {
            // ReSharper disable once ArrangeThisQualifier
            return FromString(this.ToString());
        }

        public int Count => _attributes.Count;

        public override string ToString()
        {
            string universeString = "";
            foreach (UniverseAttribute attribute in _attributes)
            {
                universeString += "'" + attribute + "' ";
            }

            return universeString;
        }

        public void AddAttribute(UniverseAttribute attributeToAdd)
        {
            if (_attributes.Any(
                attribute => attribute.Name.Equals(attributeToAdd.Name)))
                throw new ArgumentNullException($"The attribute {attributeToAdd.Name} is already present.");

            _attributes.Add(attributeToAdd);
        }

        public void AddAttribute(string attributeString)
        {
            Regex isNumericalAttribute = new Regex("=");
            UniverseAttribute attributeToAdd;
            if (isNumericalAttribute.IsMatch(attributeString))
                attributeToAdd = new NumericalAttribute(attributeString);
            else
                attributeToAdd = new SimpleAttribute(attributeString);

            AddAttribute(attributeToAdd);
        }

        public bool RemoveAttribute(string attributeName)
        {
            int removed = _attributes.RemoveWhere(
                attribute => attribute.Name.Equals(attributeName));
            return (removed > 0);
        }

        public bool ValidateTag(TagSpecification tag)
        {
            var candidateAttribute = _attributes
                .Where(attr => attr.Name == tag.Name)
                .ToList().AsReadOnly();

            if (!candidateAttribute.Any())
                return false;

            UniverseAttribute attribute = candidateAttribute.First();

            if (attribute is SimpleAttribute)
            {
                return tag.Value == null;
            }
            else
            {
                // ReSharper disable once BuiltInTypeReferenceStyle
                return tag.Value != null && ((NumericalAttribute) attribute).CanBeValue((UInt64) tag.Value);
            }
        }

        public string GetTagString(TagSpecification tag)
        {
            if (!ValidateTag(tag))
                throw new ArgumentException("Invalid tag for this universe");

            if (tag.Value == null)
                return tag.Name;
            else
            {
                NumericalAttribute numericalAttribute =
                    _attributes.First(attr => attr.Name == tag.Name) as NumericalAttribute;
                return numericalAttribute?.GetTagString(tag);
            }
        }

        public static Universe FromString(string universeString, bool skipInvalidAttributes = true)
        {
            Universe universe = new Universe();

            Regex attributeFormat = new Regex("['\"](?<attribute>.+?)['\"]");
            IEnumerable<string> attributesStrings = attributeFormat.Matches(universeString)
                .Cast<Match>()
                .Select(match => match.Groups["attribute"].Value);

            List<Exception> exceptions = new List<Exception>();

            foreach (string attributeString in attributesStrings)
            {
                try
                {
                    universe.AddAttribute(attributeString);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!skipInvalidAttributes && exceptions.Count > 0)
                throw new AggregateException("String contained invalid attributes", exceptions);
            else if (universe.Count == 0)
                throw new ArgumentException("The resulting universe is void");
            else
                return universe;
        }

        /// <summary>
        /// Returns the string to which the universeString is set. Reads the file "universeString"
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown if file is locked by another thread</exception>
        /// <returns>The current universeString string</returns>
        public static Universe ReadFromFile(string filename, bool skipInvalidAttributes = true)
        {
            using (FileStream universeFile = File.Open(filename, FileMode.Open))
            {
                byte[] universeBytes = new byte[universeFile.Length];
                using (MemoryStream memStream = new MemoryStream(universeBytes))
                {
                    universeFile.CopyTo(memStream);
                    string universeString = Encoding.UTF8.GetString(memStream.ToArray());
                    return FromString(universeString, skipInvalidAttributes);
                }
            }
        }

        public void SaveToFile(string filename)
        {
            using (FileStream universeFile = File.Open(filename, FileMode.OpenOrCreate))
            {
                long oldLength = universeFile.Length;
                universeFile.Lock(0, oldLength);
                byte[] universeBytes = Encoding.UTF8.GetBytes(ToString());
                using (MemoryStream memStream = new MemoryStream(universeBytes))
                {
                    memStream.CopyTo(universeFile);
                }
                universeFile.Unlock(0, oldLength);
            }
        }
    }
}