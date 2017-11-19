using System;

namespace KPServices
{
    public class UniverseNotDefinedException : Exception
    {
        public UniverseNotDefinedException()
        { }

        public UniverseNotDefinedException(String message) : base(message) { }
    }
}
