using System;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception that is thrown when user requesting resource for which has not permission.
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
