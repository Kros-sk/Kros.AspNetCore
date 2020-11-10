using Kros.Utils;
using MassTransit.Topology;

namespace Kros.MassTransit.AzureServiceBus
{
    /// <summary>
    /// Entity name formatter for MassTransit with prefixes.
    /// </summary>
    class PrefixEntityNameFormatter: IEntityNameFormatter
    {
        private readonly IEntityNameFormatter _originalFormatter;
        private readonly string _prefix;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="originalFormatter">Original formatter.</param>
        /// <param name="messageTypePrefix">Prefix for message type.</param>
        public PrefixEntityNameFormatter(IEntityNameFormatter originalFormatter, string messageTypePrefix)
        {
            _originalFormatter = Check.NotNull(originalFormatter, nameof(originalFormatter));
            _prefix = Check.NotNullOrWhiteSpace(messageTypePrefix, nameof(messageTypePrefix));
        }

        /// <inheritdoc/>
        public string FormatEntityName<T>()
            => $"{_prefix}-{_originalFormatter.FormatEntityName<T>()}";
    }
}
