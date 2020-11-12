using System;

namespace Blowin.Result
{
    public class ResultException : Exception
    {
        public ResultException(string message) : base(message)
        {
        }
    }
}