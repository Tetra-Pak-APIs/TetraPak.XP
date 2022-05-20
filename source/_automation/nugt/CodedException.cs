using System;

namespace nugt
{
    sealed class CodedException : Exception
    {
        public int Code { get; }

        public CodedException(int code, string message, Exception? innerException = null) : base(message,
            innerException)
        {
            Code = code;
        }
    }
}