using FluentValidation.Results;
using Hellang.Middleware.ProblemDetails;
using System.Collections.Generic;
using System.Linq;

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
            Errors = errors.Select(error => new ValidationError
            {
                ErrorCode = error.ErrorCode,
                ErrorMessage = error.ErrorMessage,
                PropertyName = error.PropertyName
            });
        }
    }
}
