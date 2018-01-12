using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KPServices
{
    public abstract class UniverseAttribute
    {
        static readonly Regex ValidName = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
        static readonly string[] Keywords = {"and", "or", "of"};

        private string _name;

        protected UniverseAttribute() {}

        protected UniverseAttribute(string name)
        {
            Name = name;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (ValidName.IsMatch(value))
                {
                    if (!Keywords.Contains(value))
                        _name = value;
                    else
                        throw new ArgumentException(
                            "The attribute name cannot be a keyword. Keywords are 'and', 'or' and 'of'");
                }
                else
                {
                    throw new ArgumentException("The attribute name must start with a letter " +
                                                "and must be composed only of letters, digits and underscore. " +
                                                $"{value} is not a valid name.");
                }
            }
        }

        public abstract override String ToString();
    }
}