using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KPServices
{
    
    public abstract class UniverseAttribute
    {
        static Regex ValidName = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
        static string[] keywords = { "and", "or", "of" };

        private string name = null;

        public UniverseAttribute() { }

        public UniverseAttribute(string name)
        {
            Name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (ValidName.IsMatch(value))
                {
                    if(!keywords.Contains(value))
                        name = value;
                    else
                        throw new ArgumentException("The attribute name cannot be a keyword. Keywords are 'and', 'or' and 'of'");
                }
                else
                {
                    throw new ArgumentException("The attribute name must start with a letter " +
                        "and must be composed only of letters, digits and underscore. " +
                        $"{value} is not a valid name.");
                }
            }
        }

        abstract override public String ToString();
    }
}
