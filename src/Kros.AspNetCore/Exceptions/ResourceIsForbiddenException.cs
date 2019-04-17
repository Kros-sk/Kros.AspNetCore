using System;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception which is thrown when user does not have permission for requested resource.
    /// </summary>
    public class ResourceIsForbiddenException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ResourceIsForbiddenException()
            : this(Properties.Resources.UserDoNotHavePermissionToAccessRequestedResource)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">Message.</param>
        public ResourceIsForbiddenException(string message)
            : base(message)
        {
        }
    }
}
