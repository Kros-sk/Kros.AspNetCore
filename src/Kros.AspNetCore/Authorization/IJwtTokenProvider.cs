using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Authorization;

/// <summary>
/// Interface for JWT token provider.
/// </summary>
internal interface IJwtTokenProvider
{
    /// <summary>
    /// Gets a JWT token for the given authorization token.
    /// </summary>
    /// <param name="token">The authorization token.</param>
    /// <returns>The JWT token.</returns>
    Task<string> GetJwtTokenAsync(StringValues token);

    /// <summary>
    /// Gets a JWT token for hash-based authorization.
    /// </summary>
    /// <param name="hashValue">The hash value.</param>
    /// <returns>The JWT token.</returns>
    Task<string> GetJwtTokenForHashAsync(StringValues hashValue);
}
