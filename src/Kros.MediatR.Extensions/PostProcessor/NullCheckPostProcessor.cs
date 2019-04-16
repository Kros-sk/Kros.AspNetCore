using Kros.AspNetCore.Exceptions;
using MediatR.Pipeline;
using System.Threading.Tasks;

namespace Kros.MediatR.PostProcessor
{
    /// <summary>
    /// MediatR post processor which check if response is
    /// <see langword="null"/> and throw <see cref="NotFoundException"/> if <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <exception cref="NotFoundException">If response is <see langword="null"/>.</exception>
    public class NullCheckPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    {
        /// <inheritdoc />
        public Task Process(TRequest request, TResponse response)
        {
            if (response == null)
            {
                throw new NotFoundException();
            }

            return Task.CompletedTask;
        }
    }
}
