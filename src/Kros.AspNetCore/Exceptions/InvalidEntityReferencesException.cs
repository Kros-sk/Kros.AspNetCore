using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception is thrown when entity references in user request are invalid.
    /// </summary>
    public class InvalidEntityReferencesException : Exception
    {
        /// <summary>
        /// Ids of invalid entities. Dictionary with key = entity name and value = enumerable of invalid entity ids.
        /// </summary>
        public IDictionary<string, IEnumerable<long>> Ids { get; }

        /// <summary>
        /// Type of entity references problem. The value should follow the rules for problem
        /// types defined in https://datatracker.ietf.org/doc/html/rfc7807.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">Type of entity references problem. <see cref="Type"/></param>
        /// <param name="ids">Ids of invalid entities. <see cref="Ids"/></param>
        /// <param name="message">Message with detail of exception.</param>
        public InvalidEntityReferencesException(string type, IDictionary<string, IEnumerable<long>> ids, string message = null)
            : base(message)
        {
            Type = type;
            Ids = ids;
        }
    }
}
