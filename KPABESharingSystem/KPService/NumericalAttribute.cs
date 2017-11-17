using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    public class NumericalAttribute : UniverseAttribute
    {
        public int Number;

        public NumericalAttribute(string name, int number) : base(name)
        {
            Number = number;
        }

        public override string ToString()
        {
            return Name + "=" + Number;
        }
    }
}
