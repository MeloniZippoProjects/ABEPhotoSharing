using System;

namespace KPServices
{
    class UniverseNotDefinedException : Exception
    {
        public UniverseNotDefinedException()
        { }

        public UniverseNotDefinedException(String message) : base(message) { }
    }
}
