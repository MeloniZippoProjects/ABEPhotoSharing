using System;
using System.Runtime.Serialization;

namespace KPServices
{
    [Serializable]
    public class UnsatisfiablePolicyException : Exception
    {
        public UnsatisfiablePolicyException()
        {
        }

        public UnsatisfiablePolicyException(string message) : base(message)
        {
        }

        public UnsatisfiablePolicyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsatisfiablePolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}