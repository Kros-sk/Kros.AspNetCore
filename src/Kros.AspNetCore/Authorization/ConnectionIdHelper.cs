using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Helper class for working with connection id.
    /// </summary>
    public abstract class ConnectionIdHelper
    {
        /// <summary>
        /// Connection ID.
        /// </summary>
        public const string ConnectionId = "Kros-Connection-Id";

        /// <summary>
        /// Gets the connection id from header associated with the specified key <see cref="ConnectionId"/>.
        /// </summary>
        /// <param name="headers">Http headers.</param>
        /// <param name="connectionId">When this method returns, the value associated with the connection id,
        /// if the key is found; otherwise null.</param>
        /// <returns>True if the header contains an element with the specified key<see cref="ConnectionId"/>.
        /// Otherwise false./returns>
        public static bool TryGetConnectionId(IHeaderDictionary headers, out string connectionId)
        {
            if (headers.TryGetValue(ConnectionId, out StringValues authHeader) && authHeader.Any())
            {
                connectionId = authHeader.First();
                return true;
            }
            connectionId = null;
            return false;
        }
    }
}
