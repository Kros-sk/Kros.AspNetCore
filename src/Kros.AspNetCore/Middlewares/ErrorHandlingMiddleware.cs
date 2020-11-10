using Kros.AspNetCore.Exceptions;
using Kros.AspNetCore.Extensions;
using Kros.AspNetCore.Properties;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Extensions.Logging;
using System;
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
            catch (JsonPatchException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status400BadRequest);
            }
            catch (BadRequestException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status400BadRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status401Unauthorized);
            }
            catch (PaymentRequiredException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status402PaymentRequired);
            }
            catch (ResourceIsForbiddenException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status403Forbidden);
            }
            catch (NotFoundException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status404NotFound);
            }
            catch (TimeoutException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status408RequestTimeout);
            }
            catch (RequestConflictException ex)
            {
                SetResponseType(context, ex, StatusCodes.Status409Conflict);
            }
            catch (Exception ex) when (!context.Response.HasStarted && IsSuccessStatusCode(context.Response.StatusCode))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                LogStatusCodeChange(ex, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        private void SetResponseType(HttpContext context, Exception ex, int statusCode)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ClearExceptCorsHeaders();
                context.Response.StatusCode = statusCode;
                context.Response.WriteAsync(ex.Message).Wait();
                LogStatusCodeChange(ex, statusCode);
            }
            _logger.LogError(ex, ex.Message);
        }

        private void LogStatusCodeChange(Exception ex, int statusCode)
            => _logger.LogDebug(Resources.ErrorHandlingMiddleware_StatusCodeChange, ex.GetType().FullName, statusCode);

        private static bool IsSuccessStatusCode(int statusCode)
            => (statusCode >= StatusCodes.Status200OK) && (statusCode < StatusCodes.Status300MultipleChoices);
    }
}
