using Kros.Identity.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Kros.AspNetCore.Extensions
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddIdentityServerHealthChecks(this IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration)
        {
            var identityServerOptions = configuration.GetSection("IdentityServerHandlers").Get<IdentityServerOptions[]>().ToList();
            foreach (var option in identityServerOptions)
            {
                healthChecksBuilder.AddUrlGroup(
                    new Uri(option.AuthorityUrl),
                    name: $" IdentityServer handler for scheme {option.AuthenticationScheme}",
                    tags: new string[] { "url" });
            }

            return healthChecksBuilder;
        }
    }
}
