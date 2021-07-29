using System;

namespace LapinMQ.Exceptions
{
    public class RedeliveryException : Exception
    {
        public RedeliveryException(string message) : base(message)
        {
            
        }
    }
}