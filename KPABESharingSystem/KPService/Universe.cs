using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KPServices
{
    public class Universe
    {
        HashSet<UniverseAttribute> Attributes;

        public Universe(IEnumerable<UniverseAttribute> attributes)
        {
            Attributes = new HashSet<UniverseAttribute>(attributes);
        }

        override public String ToString()
        {
            String universe = "";
            foreach(UniverseAttribute attribute in Attributes)
            {
                universe += "'" + attribute + "' ";
            }

            return universe;
        }

        private static Universe FromString(String universe)
        {
            //remove starting and trailing spaces
            universe = universe.Trim();

            //remove excess whitespace between attributes
            universe = new Regex(@"\s+").Replace(universe, " ");

            //remove whitespace between name, number and "=" for numerical attributes
            universe = new Regex(@"\s*=\s*").Replace(universe, "=");

            String[] attributesStrings = universe.Split(" ".ToCharArray());

            HashSet<UniverseAttribute> attributesSet = new HashSet<UniverseAttribute>();

            foreach(String attributeIterator in attributesStrings)
            {
                String attributeString = attributeIterator;
                UniverseAttribute attribute;
                if (new Regex("=").IsMatch(attributeString))
                {
                    //we found a numerical attribute
                    if (new Regex(@"'[\w'=]+'").IsMatch(attributeString))
                        attributeString = attributeString.Substring(1, attributeString.Length - 2);
                    String[] sides = attributeString.Split("=".ToCharArray());
                    if(sides.Length != 2)
                    {
                        //too many "=" in string, error
                        throw new ArgumentException(String.Format("Universe syntax error, too many equals in {0}", attributeString), "universe");
                    }

                    //only one "=" in string
                    if (sides[1] != "")
                        attribute = new NumericalAttribute(sides[0], UInt64.Parse(sides[1]));
                    else
                        attribute = new NumericalAttribute(sides[0]);
                }
                else
                {
                    //we found a simple attribute
                    attribute = new SimpleAttribute(attributeString);
                }

                attributesSet.Add(attribute);
            }

            return new Universe(attributesSet);
        }
        
        /// <summary>
        /// Returns the string to which the universe is set. Reads the file "universe"
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown if file is locked by another thread</exception>
        /// <returns>The current universe string</returns>
        public static Universe ReadFromFile(String filename)
        {       
            using (FileStream universeFile = File.Open(filename, FileMode.Open))
            {
                byte[] universeBytes = new byte[universeFile.Length];
                using (MemoryStream memStream = new MemoryStream(universeBytes))
                {
                    universeFile.CopyTo(memStream);
                    return FromString(Encoding.UTF8.GetString(memStream.ToArray()));
                }
            }
        }

        public void SaveToFile(String filename)
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
