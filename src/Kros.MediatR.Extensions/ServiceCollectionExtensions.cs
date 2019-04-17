using Kros.MediatR.PostProcessors;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kros.MediatR.Extensions
{
    /// <summary>
    /// Extensions to scan for MediatR pipeline behaviors and registers them.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan for MediatR pipeline behaviors
        /// which request implement <typeparamref name="TRequest"/> and response implement <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="services">Service container.</param>
        /// <exception cref="InvalidOperationException">When number of implementation
        /// <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/> are different.</exception>
        public static IServiceCollection AddPipelineBehaviorsForRequest<TRequest, TResponse>(this IServiceCollection services)
        {
            var requestType = typeof(TRequest);
            var responseType = typeof(TResponse);
            var pipeLineType = typeof(IPipelineBehavior<,>);
            (var requests, var responses) = GetTypes(requestType, responseType);

            Type MakeGenericType(Type type, int i)
                => type.MakeGenericType(requests[i], responses[i]);

            IEnumerable<Type> pipelineBehaviors = GetPipelineBehaviors(requestType, pipeLineType);

            foreach (var behavior in pipelineBehaviors)
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    services.AddTransient(MakeGenericType(pipeLineType, i), MakeGenericType(behavior, i));
                }
            }

            return services;
        }

        /// <summary>
        /// Scan for MediatR pipeline behaviors
        /// which request implement <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <param name="services">Service container.</param>
        /// <exception cref="InvalidOperationException">When number of implementation
        /// <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/> are different.</exception>
        public static IServiceCollection AddPipelineBehaviorsForRequest<TRequest>(this IServiceCollection services)
        {
            var requestType = typeof(TRequest);
            var pipeLineType = typeof(IPipelineBehavior<,>);
            var requests = GetTypes(requestType);
            var unitType = typeof(Unit);
            IEnumerable<Type> pipelineBehaviors = GetPipelineBehaviors(requestType, pipeLineType);

            foreach (var behavior in pipelineBehaviors)
            {
                foreach (var request in requests)
                {
                    services.AddTransient(
                        pipeLineType.MakeGenericType(request, unitType),
                        behavior.MakeGenericType(request));
                }
            }

            return services;
        }

        private static IEnumerable<Type> GetPipelineBehaviors(Type requestType, Type pipeLineType)
            => Assembly.GetAssembly(requestType).GetTypes()
            .Where(t
                => t.GetInterface(pipeLineType.Name) != null
                && t.GetGenericArguments()[0].GetInterface(requestType.Name) != null);

        private static (IList<Type>, IList<Type>) GetTypes(Type requestType, Type responseType)
        {
            var requests = GetTypes(requestType);
            var responses = GetTypes(responseType);

            if (requests.Count != responses.Count)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Resources.IncorrectNumberOfImplementation, requestType.Name, responseType.Name));
            }

            return (requests, responses);
        }

        private static IList<Type> GetTypes(Type type)
            => Assembly.GetAssembly(type)
            .GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract & type.IsAssignableFrom(t))
            .ToList();

        /// <summary>
        /// Add <see cref="NullCheckPostProcessor{TRequest, TResponse}"/> for MediatR.
        /// </summary>
        /// <param name="services">Service container.</param>
        public static IServiceCollection AddMediatRNullCheckPostProcessor(this IServiceCollection services)
            => services.AddSingleton(typeof(IRequestPostProcessor<,>), typeof(NullCheckPostProcessor<,>));
    }
}
