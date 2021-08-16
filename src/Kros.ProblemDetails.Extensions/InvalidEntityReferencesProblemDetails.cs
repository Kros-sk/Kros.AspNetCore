using Hellang.Middleware.ProblemDetails;
using System.Collections.Generic;

namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Problem details with information about invalid entity references.
    /// </summary>
    internal class InvalidEntityReferencesProblemDetails : StatusCodeProblemDetails
    {
        /// <summary>
        /// Ids of invalid entities. Dictionary with key = entity name and value = enumerable of invalid entity ids.
        /// </summary>
        public IDictionary<string, IEnumerable<long>> Ids { get; set; }

        public InvalidEntityReferencesProblemDetails(IDictionary<string, IEnumerable<long>> ids, int statusCode)
            : base(statusCode)
        {
            Ids = ids;
        }
    }
}
