using System.Collections.Generic;

namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Problem details containing information about failed validation of referenced entities existence.
    /// </summary>
    internal class EntitiesDoNotExistProblemDetails : ProblemDetailsBase<EntitiesDoNotExistError>
    {
        public EntitiesDoNotExistProblemDetails(
            IDictionary<string, IEnumerable<long>> ids,
            string errorCode,
            string message,
            int statusCode)
            : base(statusCode)
        {
            Errors = new []
            {
                new EntitiesDoNotExistError()
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message,
                    Ids = ids
                }
            };
        }
    }
}
