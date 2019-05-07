using System;
using System.Collections.Generic;

namespace Kros.MediatR.PostProcessors
{
    /// <summary>
    /// Configuration for <see cref="NullCheckPostProcessor{TRequest, TResponse}"/>
    /// </summary>
    public class NullCheckPostProcessorOptions
    {
        private HashSet<Type> _ignoredRequests = new HashSet<Type>();

        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly NullCheckPostProcessorOptions Default = new NullCheckPostProcessorOptions();

        /// <summary>
        /// Do not check <see langword="null"/> response for request type <typeparamref name="TRequest"/>.
        /// This request will be ignored for checking <see langword="null"/> response.
        /// </summary>
        /// <typeparam name="TRequest">Ignored request type.</typeparam>
        public NullCheckPostProcessorOptions IgnoreRequest<TRequest>()
        {
            _ignoredRequests.Add(typeof(TRequest));

            return this;
        }

        /// <summary>
        /// Determine if should check response of <paramref name="request"/> for <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <param name="request">Request.</param>
        /// <returns><see langword="true"/> if can check response. Otherwise <see langword="false"/>.</returns>
        public bool CanCheckResponse<TRequest>(TRequest request) => !_ignoredRequests.Contains(request.GetType());
    }
}
