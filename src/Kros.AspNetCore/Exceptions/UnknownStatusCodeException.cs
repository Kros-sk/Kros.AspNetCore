using System;
using System.Net;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception thrown, when request returned unsuccessful status code, witch does not have exact exception.
    /// </summary>
    public class UnknownStatusCodeException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statusCode">Http response status code.</param>
        public UnknownStatusCodeException(HttpStatusCode statusCode) :
            base(string.Format(Properties.Resources.UnknownStatusCode, (int)statusCode))
        {
        }
    }
}
