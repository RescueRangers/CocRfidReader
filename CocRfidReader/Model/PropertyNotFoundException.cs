using System.Runtime.Serialization;

namespace CocRfidReader.Models
{
    [Serializable]
    internal class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException()
        {
        }

        public PropertyNotFoundException(string? message) : base(message)
        {
        }

        public PropertyNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}