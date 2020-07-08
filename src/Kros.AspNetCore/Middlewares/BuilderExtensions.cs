using Kros.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.BuilderMiddlewares
{
    /// <summary>
    /// Extensions for adding middlewares from this project.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Extension for adding error handling middleware to application.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <remarks>Consider the order of registration.
        /// To catching most of the exceptions, we recommend that you register this middleware as first.</remarks>
        public static void UseErrorHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ErrorHandlingMiddleware>();

        /// <summary>
        /// Extension for adding route pattern middleware to application.
        /// </summary>
        /// <param name="app">Application builder.</param>
        public static void UseRoutePattern(this IApplicationBuilder app)
            => app.UseMiddleware<RoutePatternMiddleware>();
    }
}
