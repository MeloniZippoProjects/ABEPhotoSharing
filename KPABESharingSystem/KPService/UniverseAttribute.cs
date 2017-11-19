using System;
using System.Text.RegularExpressions;

namespace KPServices
{
    
    public abstract class UniverseAttribute
    {

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
                Regex regex = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
                if (regex.IsMatch(value))
                {
                    name = value;
                }
                else
                {
                    throw new ArgumentException(string.Format("The attribute name must start with a letter " +
                        "and must be composed only of letters, digits and underscore. " +
                        "{0} is not a valid name.", value), "value");
                }
            }
        }


        abstract override public String ToString();
    }
}
