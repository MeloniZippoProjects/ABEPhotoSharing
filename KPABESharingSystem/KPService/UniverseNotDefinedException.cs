using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    class UniverseNotDefinedException : Exception
    {
        public UniverseNotDefinedException() : base() { }

        public UniverseNotDefinedException(String message) : base(message) { }
    }
}
