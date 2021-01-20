using FluentValidation.Results;
using Hellang.Middleware.ProblemDetails;
using Mapster;
using System.Collections.Generic;

namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Problem details with informations about failed fluent validations.
    /// </summary>
    internal class ValidationProblemDetails : StatusCodeProblemDetails
    {
        public IEnumerable<ValidationError> Errors { get; }

        public ValidationProblemDetails(IEnumerable<ValidationFailure> errors, int statusCode) : base(statusCode)
        {
            Errors = errors.Adapt<IEnumerable<ValidationError>>();
        }
    }


}
