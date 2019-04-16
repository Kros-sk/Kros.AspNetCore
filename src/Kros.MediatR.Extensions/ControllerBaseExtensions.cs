using MediatR;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Extensions for better work with MediatR in controllers.
    /// </summary>
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// Access to <see cref="IMediator"/>.
        /// </summary>
        /// <param name="controller">Controller.</param>
        /// <returns>Mediator.</returns>
        public static IMediator Mediator(this ControllerBase controller)
            => (IMediator)controller.HttpContext.RequestServices.GetService(typeof(IMediator));

        /// <summary>
        /// Send <paramref name="request"/> through <see cref="IMediator"/>.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="controller">Controller.</param>
        /// <param name="request">Request.</param>
        /// <returns>Response from request.</returns>
        public static Task<TResponse> SendRequest<TResponse>(
            this ControllerBase controller,
            IRequest<TResponse> request)
            => controller.Mediator().Send(request);

        /// <summary>
        /// Send command for create resource.
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="controller">Controller.</param>
        /// <param name="command">Create command.</param>
        /// <param name="actionName">Action name to specify URI at which the content has been created.</param>
        /// <returns><see cref="CreatedResult"/> with resource id and location.</returns>
        public static async Task<CreatedResult> SendCreateCommand<TResponse>(
            this ControllerBase controller,
            IRequest<TResponse> command,
            string actionName = null)
        {
            var ret = new { id = await controller.Mediator().Send(command) };
            var url = actionName != null
                ? controller.Url.Link(actionName, ret)
                : string.Empty;

            return controller.Created(url, ret);
        }
    }
}
