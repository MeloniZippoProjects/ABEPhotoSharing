using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    public class SuiteExeNotFound : SuiteErrorException
    {
        public SuiteExeNotFound() { }

        public SuiteExeNotFound(String message) : base(message) { }
    }
}
