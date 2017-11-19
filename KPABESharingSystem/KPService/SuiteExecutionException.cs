using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    class SuiteExecutionException : Exception
    {
        public SuiteExecutionException()
        { }

        public SuiteExecutionException(String message) : base(message) { }
    }
}
