using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Interface for JWT token caching service.
    /// </summary>
    public interface IJwtCachingService
    {
        /// <summary>
        /// Gets or creates a cached JWT token using the provided factory function.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="token">The authorization token.</param>
        /// <param name="jwtTokenFactory">Factory function to create JWT token if not cached.</param>
        /// <returns>The JWT token.</returns>
        Task<string> GetOrCreateJwtTokenAsync(
            HttpContext httpContext,
            StringValues token,
            Func<Task<string>> jwtTokenFactory);

        /// <summary>
        /// Gets or creates a cached JWT token for hash-based authorization.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="hashValue">The hash value.</param>
        /// <param name="jwtTokenFactory">Factory function to create JWT token if not cached.</param>
        /// <returns>The JWT token.</returns>
        Task<string> GetOrCreateJwtTokenForHashAsync(
            HttpContext httpContext,
            StringValues hashValue,
            Func<Task<string>> jwtTokenFactory);
    }
}
