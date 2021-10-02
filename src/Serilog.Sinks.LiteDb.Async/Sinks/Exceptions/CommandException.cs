using System;

namespace Serilog.Sinks.LiteDb.Async.Exceptions
{
    [Serializable]
    public class LiteDbSinkException : Exception
    {
        public LiteDbSinkException()
        {
        }

        public LiteDbSinkException(string message) : base(message)
        {
        }

        public LiteDbSinkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}