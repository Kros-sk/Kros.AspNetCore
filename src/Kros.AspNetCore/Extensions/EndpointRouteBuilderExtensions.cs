using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace Kros.AspNetCore.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps signalr hub with options from configuration.
        /// </summary>
        /// <param name="endpoints"><see cref="IEndpointRouteBuilder"/> where middleware is added.</param>
        /// <param name="configuration">Configuration from which the options are loaded.
        /// Configuration must contains ClientCredentialsAuthorization section.</param>
        /// <param name="pattern">Signalr hub endpoint route pattern.</param>
        /// <exception cref="InvalidOperationException">
        /// When `HttpConnectionDispatcher` section is missing in configuration.
        /// </exception>
        public static void MapSignalRHubWithOptions<THub>(
            this IEndpointRouteBuilder endpoints,
            IConfiguration configuration,
            [StringSyntax("Route")] string pattern)  where THub : Hub
        {
            HttpConnectionDispatcherOptions options = configuration.GetSection<HttpConnectionDispatcherOptions>();

            if (options is null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.OptionMissingSection,
                        Helpers.GetSectionName<HttpConnectionDispatcherOptions>()));
            }

            endpoints.MapHub<THub>(pattern, o =>
            {
                o = options;
            });
        }
    }
}
