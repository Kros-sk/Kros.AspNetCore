using System.Collections.Generic;

namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Class containing information about non-existent entities error.
    /// </summary>
    internal class EntitiesDoNotExistError: ErrorBase
    {
        /// <summary>
        /// Non existent entity ids. Dictionary with key = entity name
        /// and value = enumerable of non existent entity ids.
        /// </summary>
        public IDictionary<string, IEnumerable<long>> Ids { get; set; }
    }
}
