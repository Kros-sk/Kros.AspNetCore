using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Filter for allowing authorized access to operations.
    /// This filter is suitable, if by default all your endpoints allow anonymous access and you want just for some of them
    /// to be authorized. It sets security requirements for all operations which have <see cref="AuthorizeAttribute"/>
    /// and clears any security requirements from all other operations.
    /// </summary>
    internal class AuthorizeOperationFilter : IOperationFilter
    {
        private readonly IConfiguration _configuration;

        public AuthorizeOperationFilter(IConfiguration configuration)
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
                    : context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
                bool hasMethodAttribute = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
                bool hasMetadataAttribute = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any();

                operation.Security = hasClassAttribute || hasMethodAttribute || hasMetadataAttribute
                    ? settings.CreateSecurityRequirements()
                    : [];
            }
        }
    }
}
