using System;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception is thrown when resource is not found.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
            : this(Properties.Resources.NotFound)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NotFoundException" /> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
