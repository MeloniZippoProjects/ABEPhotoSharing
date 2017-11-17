using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KPServices
{
    public abstract class UniverseAttribute
    {

        private String _Name;

        public UniverseAttribute(String name)
        {
            Name = name;
        }

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                Regex regex = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
                if (regex.IsMatch(value))
                {
                    _Name = value;
                }
                else
                {
                    throw new ArgumentException(String.Format("The attribute name must start with a letter " +
                        "and must be composed only of letters, digits and underscore. " +
                        "{0} is not a valid name.", value), "value");
                }
            }
        }


        abstract override public String ToString();
    }
}
