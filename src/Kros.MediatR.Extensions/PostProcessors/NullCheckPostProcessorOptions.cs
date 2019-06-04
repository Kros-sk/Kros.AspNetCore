using System;
using System.Collections.Generic;

namespace Kros.MediatR.PostProcessors
{
    /// <summary>
    /// Configuration for <see cref="NullCheckPostProcessor{TRequest, TResponse}"/>
    /// </summary>
    public class NullCheckPostProcessorOptions
    {
        private readonly HashSet<Type> _ignoredRequests = new HashSet<Type>();

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
        /// Determine if should check response for request type <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <returns><see langword="true"/> if can check response for request type <typeparamref name="TRequest"/>.
        /// Otherwise <see langword="false"/>.</returns>
        internal bool CanCheckResponseFor<TRequest>() => !_ignoredRequests.Contains(typeof(TRequest));
    }
}
