using Kros.AspNetCore.Exceptions;
using Kros.Utils;
using MediatR.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace Kros.MediatR.PostProcessors
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
        private readonly NullCheckPostProcessorOptions _options;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="options">Configuration.</param>
        public NullCheckPostProcessor(NullCheckPostProcessorOptions options)
        {
            _options = Check.NotNull(options, nameof(options));
        }

        /// <inheritdoc />
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            if (_options.CanCheckResponseFor<TRequest>() && response == null)
            {
                throw new NotFoundException();
            }

            return Task.CompletedTask;
        }
    }
}
