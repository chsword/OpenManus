using System;

namespace OpenManus.Core.Exceptions
{
    public class OpenManusException : Exception
    {
        public OpenManusException()
        {
        }

        public OpenManusException(string message)
            : base(message)
        {
        }

        public OpenManusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}