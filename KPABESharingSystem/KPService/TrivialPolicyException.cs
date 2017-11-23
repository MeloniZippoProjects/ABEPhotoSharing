using System;
using System.Runtime.Serialization;

namespace KPServices
{
    [Serializable]
    public class TrivialPolicyException : Exception
    {
        public TrivialPolicyException()
        {
        }

        public TrivialPolicyException(string message) : base(message)
        {
        }

        public TrivialPolicyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TrivialPolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}