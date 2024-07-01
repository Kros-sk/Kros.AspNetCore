using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Filter for allowing anonymous access to operations.
    /// This filter is suitable, if by default all your endpoints are authorized and you want just for some of them
    /// to allow anonymous access. It clears any security requirements from operation if the operation
    /// has <see cref="AllowAnonymousAttribute"/> and sets security requirements for all other operations.
    /// </summary>
    internal class AllowAnonymousOperationFilter : IOperationFilter
    {
        private readonly IConfiguration _configuration;

        public AllowAnonymousOperationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            SwaggerSettings? settings = _configuration.GetSwaggerSettings();
            if (settings is not null)
            {
                bool hasClassAttribute = context.MethodInfo.DeclaringType is null
                    ? false
                    : context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
                bool hasMethodAttribute = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
                bool hasMetadataAttribute = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();

                operation.Security = hasClassAttribute || hasMethodAttribute || hasMetadataAttribute
                    ? []
                    : settings.CreateSecurityRequirements();
            }
        }
    }
}
