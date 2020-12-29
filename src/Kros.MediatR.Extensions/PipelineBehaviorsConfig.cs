using System.Collections.Generic;
using System.Reflection;

namespace Kros.MediatR.Extensions
{
    /// <summary>
    /// Config for pipeline behaviors.
    /// </summary>
    public class PipelineBehaviorsConfig
    {
        private readonly List<Assembly> _pipelineBehaviorAssemblies = new List<Assembly>();
        private readonly List<Assembly> _requestAssemblies = new List<Assembly>();

        /// <summary>
        /// Assembly with pipeline behaviors.
        /// </summary>
        public Assembly PipelineBehaviorsAssembly { get; set; }

        /// <summary>
        /// Assembly with requests.
        /// </summary>
        public Assembly RequestsAssembly { get; set; }

        /// <summary>
        /// Adds the assembly with pipeline behaviors.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public PipelineBehaviorsConfig AddPipelineBehaviorAssembly(Assembly assembly)
        {
            _pipelineBehaviorAssemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Adds the assembly with request.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public PipelineBehaviorsConfig AddRequestAssembly(Assembly assembly)
        {
            _requestAssemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Gets assemblies with pipeline behaviors.
        /// </summary>
        public IEnumerable<Assembly> GetPipelineBehaviorAssemblies()
            => UnionAssemblies(_pipelineBehaviorAssemblies, PipelineBehaviorsAssembly);

        /// <summary>
        /// Gets assemblies with requests.
        /// </summary>
        public IEnumerable<Assembly> GetRequestAssemblies()
            => UnionAssemblies(_requestAssemblies, RequestsAssembly);

        private IEnumerable<Assembly> UnionAssemblies(IEnumerable<Assembly> assemblies, Assembly assembly)
        {
            if (assembly != null)
            {
                yield return assembly;
            }

            foreach (Assembly a in assemblies)
            {
                yield return a;
            }
        }
    }
}
