using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;

namespace KPServices
{
    public class Universe
    {
        HashSet<UniverseAttribute> Attributes;

        public Universe() { }

        public Universe(IEnumerable<UniverseAttribute> attributes)
        {
            Attributes = new HashSet<UniverseAttribute>(attributes);
        }

        public Universe Copy()
        {
            return FromString(this.ToString());
        }

        public int Count
        {
            get => Attributes.Count;
        }

        override public string ToString()
        {
            string universe = "";
            foreach (UniverseAttribute attribute in Attributes)
            {
                universe += "'" + attribute + "' ";
            }

            return universe;
        }

        public bool AddAttribute(UniverseAttribute attributeToAdd)
        {
            if (Attributes.Any(
                attribute => attribute.Name.Equals(attributeToAdd.Name)))
                return false;
            else
                return Attributes.Add(attributeToAdd);
        }

        public Exception AddAttribute(string attributeString)
        {
            UniverseAttribute attributeToAdd = null;

            try
            {
                Regex isNumericalAttribute = new Regex("=");
                if (isNumericalAttribute.IsMatch(attributeString))
                    attributeToAdd = new NumericalAttribute(attributeString);
                else
                    attributeToAdd = new SimpleAttribute(attributeString);

                if (AddAttribute(attributeToAdd))
                    return null;
                else
                    throw new ArgumentNullException($"The attribute {attributeToAdd.Name} is already present.");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public bool RemoveAttribute(string attributeName)
        {
            int removed = Attributes.RemoveWhere(
                attribute => attribute.Name.Equals(attributeName));
            return (removed > 0);
        }

        public static Universe FromString(string universeString, bool skipInvalidAttributes = true)
        {
            Universe universe = new Universe();

            Regex attributeFormat = new Regex("['\"](.+?)['\"]");
            string[] attributesStrings = attributeFormat.Matches(universeString)
                                            .Cast<Match>()
                                            .Select(m => m.ToString())
                                            .ToArray();

            List<Exception> exceptions = new List<Exception>();

            foreach (string attributeString in attributesStrings)
            {
                try
                {
                    universe.AddAttribute(attributeString);
                }
                catch(Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!skipInvalidAttributes && exceptions.Count > 0)
                throw new AggregateException("String contained invalid attributes", exceptions);
            else
                return universe;
        }
        
        /// <summary>
        /// Returns the string to which the universeString is set. Reads the file "universeString"
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown if file is locked by another thread</exception>
        /// <returns>The current universeString string</returns>
        public static Universe ReadFromFile(string filename, bool skipInvalidAttributes = true )
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
