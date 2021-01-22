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
        /// </summary>
        /// <param name="services">IoC container.</param>
        /// <param name="configAction">Additional action for configuring Hellang problem details.</param>
        public static IServiceCollection AddKrosProblemDetails(
            this IServiceCollection services,
            Action<ProblemDetailsOptions> configAction = null)
            =>  services.AddProblemDetails(p =>
            {
                p.IncludeExceptionDetails = (context, ex) => IncludeExceptionDetails(context, ex);

                p.Map<ValidationException>((ex)
                    => new ValidationProblemDetails(ex.Errors, StatusCodes.Status400BadRequest));

                configAction?.Invoke(p);
            });

        private static bool IncludeExceptionDetails(HttpContext context, Exception ex)
            => context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsTestOrDevelopment()
            && !IsExceptionWithoutExceptionDetails(ex) ? true : false;

        private static bool IsExceptionWithoutExceptionDetails(Exception ex)
            => ex.GetType() == typeof(ValidationException);
    }
}
