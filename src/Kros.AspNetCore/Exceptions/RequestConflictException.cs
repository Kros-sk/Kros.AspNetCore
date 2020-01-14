using System;
using System.Collections.Generic;
using System.Text;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception which is thrown when request cannot be completed because of conflict.
    /// </summary>
    public class RequestConflictException : Exception
    {

        /// <summary>
        /// Initializes a new instance of <see cref="RequestConflictException"/> class.
        /// </summary>
        public RequestConflictException()
            : this(Properties.Resources.Conflict)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestConflictException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public RequestConflictException(string message)
            : base(message)
        {
        }
    }
}
