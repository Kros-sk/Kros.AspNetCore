using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Extensions;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Kros.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware handles erros and sends exception to client.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next">Delegate for next middleware.</param>
        /// <param name="logger">Logger.</param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = Check.NotNull(next, nameof(next));
            _logger = Check.NotNull(logger, nameof(logger));
        }

        /// <summary>
        /// Invoke.
        /// </summary>
        /// <param name="context">Current http context.</param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (ResourceIsForbiddenException ex)
            {
                SetResponseType(context, ex, HttpStatusCode.Forbidden);
            }
            catch (NotFoundException ex)
            {
                SetResponseType(context, ex, HttpStatusCode.NotFound);
            }
            catch (TimeoutException ex)
            {
                SetResponseType(context, ex, HttpStatusCode.RequestTimeout);
            }
            catch (UnauthorizedAccessException ex)
            {
                SetResponseType(context, ex, HttpStatusCode.Unauthorized);
            }
            catch when (context.Response.StatusCode >= StatusCodes.Status200OK
                && context.Response.StatusCode < StatusCodes.Status300MultipleChoices)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                throw;
            }
        }

        private void SetResponseType(HttpContext context, Exception ex, HttpStatusCode statusCode)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ClearExceptCorsHeaders();
            context.Response.StatusCode = (int)statusCode;
        }
    }
}
