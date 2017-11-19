using System;
using System.Runtime.Serialization;

namespace KPServices
{
    [Serializable]
    internal class AttributeNotFoundException : Exception
    {
        public AttributeNotFoundException()
        {
        }

        public AttributeNotFoundException(string message) : base(message)
        {
        }

        public AttributeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AttributeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}