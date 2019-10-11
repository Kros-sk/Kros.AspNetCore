using System.Reflection;

namespace Kros.MediatR.Extensions
{
    /// <summary>
    /// Config for pipeline behaviours.
    /// </summary>
    public class PipelineBehaviorsConfig
    {
        /// <summary>
        /// Assembly with pipeline behaviours.
        /// </summary>
        public Assembly PipelineBehaviorsAssembly { get; set; }

        /// <summary>
        /// Assembly with requests.
        /// </summary>
        public Assembly RequestsAssembly { get; set; }
    }
}
