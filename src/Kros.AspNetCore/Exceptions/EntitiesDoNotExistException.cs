using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception is thrown when entities referenced in user request do not exist.
    /// </summary>
    public class EntitiesDoNotExistException: Exception
    {
        /// <summary>
        /// Non existent entity ids. Dictionary with key = entity name,
        /// value = enumerable of non existent entity ids.
        /// </summary>
        public IDictionary<string, IEnumerable<long>> Ids { get; }

        /// <summary>
        /// Error code.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="ids">Non existent entity ids.</param>
        /// <param name="errorCode">Error code.</param>
        /// <param name="errorMessage">Error message.</param>
        public EntitiesDoNotExistException(IDictionary<string, IEnumerable<long>> ids, string errorCode, string errorMessage = null)
            : base(errorMessage)
        {
            Ids = ids;
            ErrorCode = errorCode;
        }
    }
}
