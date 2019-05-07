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
        /// which request implement <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <param name="services">Service container.</param>
        /// <exception cref="InvalidOperationException">When number of implementation
        /// <typeparamref name="TRequest"/> and response are different.</exception>
        public static IServiceCollection AddPipelineBehaviorsForRequest<TRequest>(this IServiceCollection services)
        {
            var requestType = typeof(TRequest);
            var pipeLineType = typeof(IPipelineBehavior<,>);
            var requestInterfaceName = typeof(IRequest<>).Name;

            var requests = GetTypes(requestType);
            IEnumerable<Type> pipelineBehaviors = GetPipelineBehaviors(requestType, pipeLineType);

            foreach (var behavior in pipelineBehaviors)
            {
                var behaviorGenericArgumentsCount = behavior.GetGenericArguments().Length;

                foreach (var request in requests)
                {
                    var interfaceType = request.GetInterface(requestInterfaceName);
                    var responseType = interfaceType.GetGenericArguments()[0];
                    Type genericBehaviorType = null;

                    if (behaviorGenericArgumentsCount == 1)
                    {
                        genericBehaviorType = behavior.MakeGenericType(request);
                    }
                    else
                    {
                        genericBehaviorType = behavior.MakeGenericType(request, responseType);
                    }

                    services.AddTransient(
                        pipeLineType.MakeGenericType(request, responseType),
                        genericBehaviorType);
                }
            }

            return services;
        }

        private static IEnumerable<Type> GetPipelineBehaviors(Type requestType, Type pipeLineType)
            => Assembly.GetAssembly(requestType).GetTypes()
            .Where(t
                => t.GetInterface(pipeLineType.Name) != null
                && t.GetGenericArguments()[0].GetInterface(requestType.Name) != null);

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
            => services.AddSingleton(NullCheckPostProcessorOptions.Default)
            .AddSingleton(typeof(IRequestPostProcessor<,>), typeof(NullCheckPostProcessor<,>));

        /// <summary>
        /// Add <see cref="NullCheckPostProcessor{TRequest, TResponse}"/> for MediatR.
        /// </summary>
        /// <param name="services">Service container.</param>
        /// <param name="optionAction">Configure which requests will be ignore.</param>
        public static IServiceCollection AddMediatRNullCheckPostProcessor(
            this IServiceCollection services,
            Action<NullCheckPostProcessorOptions> optionAction)
        {
            var options = new NullCheckPostProcessorOptions();

            optionAction(options);

            services.AddSingleton(options);
            services.AddSingleton(typeof(IRequestPostProcessor<,>), typeof(NullCheckPostProcessor<,>));

            return services;
        }
    }
}
