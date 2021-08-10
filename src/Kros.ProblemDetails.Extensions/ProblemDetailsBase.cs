using Hellang.Middleware.ProblemDetails;
using System.Collections.Generic;

namespace Kros.ProblemDetails.Extensions
{

    internal class ProblemDetailsBase<TErrorType>: StatusCodeProblemDetails
        where TErrorType: ErrorBase
    {
        public IEnumerable<TErrorType> Errors { get; protected set; }

        public ProblemDetailsBase(int statusCode) : base(statusCode)
        {
        }
    }
}
