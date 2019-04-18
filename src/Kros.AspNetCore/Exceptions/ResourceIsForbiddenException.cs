using System;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception which is thrown when user does not have permission for requested resource.
    /// </summary>
    public class ResourceIsForbiddenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ResourceIsForbiddenException"/> class.
        /// </summary>
        public ResourceIsForbiddenException()
            : this(Properties.Resources.UserDoNotHavePermissionToAccessRequestedResource)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ResourceIsForbiddenException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public ResourceIsForbiddenException(string message)
            : base(message)
        {
        }
    }
}
