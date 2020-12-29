using System.Collections.Generic;
using System.Reflection;

namespace Kros.MediatR.Extensions
{
    /// <summary>
    /// Config for pipeline behaviours.
    /// </summary>
    public class PipelineBehaviorsConfig
    {
        private readonly List<Assembly> _pipelineBahviourAssemblies = new List<Assembly>();
        private readonly List<Assembly> _requestAssemblies = new List<Assembly>();

        /// <summary>
        /// Assembly with pipeline behaviours.
        /// </summary>
        public Assembly PipelineBehaviorsAssembly { get; set; }

        /// <summary>
        /// Assembly with requests.
        /// </summary>
        public Assembly RequestsAssembly { get; set; }

        /// <summary>
        /// Adds the assembly with pipeline behaviours.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public PipelineBehaviorsConfig AddPipelineBehaviourAssembly(Assembly assembly)
        {
            _pipelineBahviourAssemblies.Add(assembly);
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
        /// Gets assemblies with pipeline behaviours.
        /// </summary>
        public IEnumerable<Assembly> GetPipelineBehaviourAssemblies()
            => UnionAssemblies(_pipelineBahviourAssemblies, PipelineBehaviorsAssembly);

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
