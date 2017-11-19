using System;

namespace KPServices
{
    public class SuiteErrorException : Exception
    {
        public SuiteErrorException() { }

        public SuiteErrorException(String message) : base(message) { }
    }
}
