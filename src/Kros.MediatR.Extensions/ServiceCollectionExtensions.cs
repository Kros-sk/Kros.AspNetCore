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
        /// <param name="configAction">Pipeline behaviors config.</param>
        /// <exception cref="InvalidOperationException">When number of implementation
        /// <typeparamref name="TRequest"/> and response are different.</exception>
        public static IServiceCollection AddPipelineBehaviorsForRequest<TRequest>(
            this IServiceCollection services,
            Action<PipelineBehaviorsConfig> configAction = null)
        {
            var config = new PipelineBehaviorsConfig();
            configAction?.Invoke(config);

            Type requestType = typeof(TRequest);
            Type pipeLineType = typeof(IPipelineBehavior<,>);
            string requestInterfaceName = typeof(IRequest<>).Name;

            IList<Type> requests = GetTypes(requestType, config.GetRequestAssemblies()).ToList();
            IEnumerable<Type> pipelineBehaviors = GetPipelineBehaviors(requestType, pipeLineType, config.GetPipelineBehaviorAssemblies());

            foreach (Type behavior in pipelineBehaviors)
            {
                int behaviorGenericArgumentsCount = behavior.GetGenericArguments().Length;

                foreach (Type request in requests)
                {
                    Type interfaceType = request.GetInterface(requestInterfaceName);
                    Type responseType = interfaceType.GetGenericArguments()[0];
                    Type genericBehaviorType;
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

        private static IEnumerable<Type> GetPipelineBehaviors(
            Type requestType,
            Type pipeLineType,
            IEnumerable<Assembly> behaviorsAssemblies)
        {
            if (behaviorsAssemblies is null || !behaviorsAssemblies.Any())
            {
                return GetPipelineBehaviors(requestType, pipeLineType, Assembly.GetAssembly(requestType));
            }
            return behaviorsAssemblies.SelectMany(a => GetPipelineBehaviors(requestType, pipeLineType, a));
        }

        private static IEnumerable<Type> GetPipelineBehaviors(Type requestType, Type pipeLineType, Assembly behaviorsAssembly)
            => behaviorsAssembly
            .GetTypes()
            .Where(t
                => (t.GetInterface(pipeLineType.Name) != null)
                && (t.GetGenericArguments().Length > 0)
                && (t.GetGenericArguments()[0].GetInterface(requestType.Name) != null));

        private static IEnumerable<Type> GetTypes(Type type, IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null || !assemblies.Any())
            {
                return GetTypes(type, Assembly.GetAssembly(type));
            }
            return assemblies.SelectMany(a => GetTypes(type, a));
        }

        private static IEnumerable<Type> GetTypes(Type type, Assembly assembly)
            => assembly
            .GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract & type.IsAssignableFrom(t));

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
