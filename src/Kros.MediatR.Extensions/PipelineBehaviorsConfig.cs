using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kros.MediatR.Extensions
{
    /// <summary>
    /// Config for pipeline behaviors.
    /// </summary>
    public class PipelineBehaviorsConfig
    {
        private readonly HashSet<Assembly> _pipelineBehaviorAssemblies = new HashSet<Assembly>();
        private readonly HashSet<Assembly> _requestAssemblies = new HashSet<Assembly>();

        /// <summary>
        /// Assembly with pipeline behaviors.
        /// </summary>
        [Obsolete("Use AddPipelineBehaviorAssembly(Assembly assembly) method.")]
        public Assembly PipelineBehaviorsAssembly { set { _pipelineBehaviorAssemblies.Add(value); } }

        /// <summary>
        /// Assembly with requests.
        /// </summary>
        [Obsolete("Use AddRequestAssembly(Assembly assembly) method.")]
        public Assembly RequestsAssembly { set { _requestAssemblies.Add(value); } }

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
        public IEnumerable<Assembly> GetPipelineBehaviorAssemblies() => _pipelineBehaviorAssemblies;

        /// <summary>
        /// Gets assemblies with requests.
        /// </summary>
        public IEnumerable<Assembly> GetRequestAssemblies() => _requestAssemblies;
    }
}
