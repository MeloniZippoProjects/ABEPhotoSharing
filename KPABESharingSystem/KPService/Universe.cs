﻿using System;
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
        HashSet<UniverseAttribute> Attributes = new HashSet<UniverseAttribute>();

        public Universe() { }

        public Universe(IEnumerable<UniverseAttribute> attributes)
        {
            foreach (UniverseAttribute attribute in attributes)
            {
                AddAttribute(attribute);
            }
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
            string universeString = "";
            foreach (UniverseAttribute attribute in Attributes)
            {
                universeString += "'" + attribute + "' ";
            }

            return universeString;
        }

        public void AddAttribute(UniverseAttribute attributeToAdd)
        {
            if (Attributes.Any(
                attribute => attribute.Name.Equals(attributeToAdd.Name)))
                throw new ArgumentNullException($"The attribute {attributeToAdd.Name} is already present.");

            Attributes.Add(attributeToAdd);
        }

        public void AddAttribute(string attributeString)
        {
            Regex isNumericalAttribute = new Regex("=");
            UniverseAttribute attributeToAdd = null;
            if (isNumericalAttribute.IsMatch(attributeString))
                attributeToAdd = new NumericalAttribute(attributeString);
            else
                attributeToAdd = new SimpleAttribute(attributeString);

            AddAttribute(attributeToAdd);
        }

        public bool RemoveAttribute(string attributeName)
        {
            int removed = Attributes.RemoveWhere(
                attribute => attribute.Name.Equals(attributeName));
            return (removed > 0);
        }

        public bool HasAttribute(string attributeName, int? numericalValue = null)
        {
            //search for attribute with correct name
            var correctNameAttributeList = Attributes.Where(attr => attr.Name == attributeName).ToList();

            //if no attribute has that name, then Universe doesn't have that attribute
            if (correctNameAttributeList.Count != 1)
                return false;

            var correctNameAttribute = correctNameAttributeList.First();

            if (numericalValue != null)
            {
                if (correctNameAttribute is NumericalAttribute)
                {
                    return (correctNameAttribute as NumericalAttribute).CanBeValue(numericalValue.Value);
                }

                return false;
            }

            return true;
        }

        public static Universe FromString(string universeString, bool skipInvalidAttributes = true)
        {
            Universe universe = new Universe();

            Regex attributeFormat = new Regex("['\"](?<attribute>.+?)['\"]");
            var attributesStrings = attributeFormat.Matches(universeString)
                .Cast<Match>()
                .Select(match => match.Groups["attribute"].Value);

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
