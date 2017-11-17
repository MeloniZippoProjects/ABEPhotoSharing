using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KPServices
{
    public class SimpleAttribute : UniverseAttribute
    {
        public SimpleAttribute(string name) : base(name){}

        override public string ToString()
        {
            return Name;
        }
    }
}
