using System;
using System.Collections.Generic;
using System.Text;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception is thrown when the user request is invalid.
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BadRequestException"/> class.
        /// </summary>
        public BadRequestException()
            : this(Properties.Resources.BadRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public BadRequestException(string message)
            : base(message)
        {
        }
    }
}
