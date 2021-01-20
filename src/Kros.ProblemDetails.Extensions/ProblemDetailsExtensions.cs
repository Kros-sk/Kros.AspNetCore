using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Hellang problem details service extensions.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Registers Hellang problem details to IoC container.
        /// It creates problem details for fluent <see cref="ValidationException"/>.
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="environment">Current environment.</param>
        /// <param name="configAction">Additional action for configuring Hellang problem details.</param>
        public static IServiceCollection AddCustomProblemDetails(
            this IServiceCollection services,
            IWebHostEnvironment environment,
            Action<ProblemDetailsOptions> configAction = null)
            =>  services.AddProblemDetails(p =>
            {
                p.IncludeExceptionDetails = (context, ex) => IncludeExceptionDetails(environment, ex);
                p.Map<ValidationException>(SetProblemDetailsForFluentValidationException);

                configAction?.Invoke(p);
            });

        private static bool IncludeExceptionDetails(IWebHostEnvironment environment, Exception ex)
            => environment.IsTestOrDevelopment() && !IsExceptionWithoutExceptionDetails(ex) ? true : false;

        private static bool IsExceptionWithoutExceptionDetails(Exception ex)
            => ex.GetType() == typeof(ValidationException);

        private static ValidationProblemDetails SetProblemDetailsForFluentValidationException(ValidationException ex)
            => new ValidationProblemDetails(ex.Errors, StatusCodes.Status400BadRequest);
    }
}
