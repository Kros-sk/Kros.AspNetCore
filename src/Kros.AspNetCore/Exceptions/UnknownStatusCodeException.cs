using System;
using System.Net;

namespace Kros.AspNetCore.Exceptions
{
    class UnknownStatusCodeException : Exception
    {
        public UnknownStatusCodeException(HttpStatusCode statusCode) :
            base(string.Format(Properties.Resources.UnknownStatusCode, (int)statusCode))
        {
        }
    }
}
