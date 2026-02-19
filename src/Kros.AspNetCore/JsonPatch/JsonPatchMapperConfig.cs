using Kros.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Kros.AspNetCore.JsonPatch
{
    /// <summary>
    /// Base class for generic <see cref="JsonPatchMapperConfig{TSource}"/>.
    /// </summary>
    public class JsonPatchMapperConfig
    {
        /// <summary>
        /// When using <see cref="JsonPatchMapperConfig{TSource}.NewConfig"/> method, only the first configuration for specified
        /// type is used. Another new config throws <see cref="InvalidOperationException"/>.
        /// If you do not want this exception to be thrown, set this property to 'false'."/>
        /// </summary>
        [Obsolete("This is only temporary for backward compatibility.")]
        public static bool ThrowExceptionOnDuplicateNewConfig { get; set; } = true;
    }

    /// <summary>
    ///JSON PATCH mapper configuration. Configuration for mapping <see cref="JsonPatchMapperConfig{TSource}"/>
    ///operation paths to database columns.
    /// </summary>
    /// <typeparam name="TSource">Source model type.</typeparam>
    public class JsonPatchMapperConfig<TSource> : JsonPatchMapperConfig, IJsonPatchMapper where TSource : class
    {
        private Func<string, string> _pathMapping;
        private readonly ConcurrentDictionary<string, string> _mapping = new(StringComparer.InvariantCulture);

        private JsonPatchMapperConfig()
        {
        }

        /// <summary>
        /// Creates new config for <typeparamref name="TSource"/> and store it for non parametric  extension
        /// <see cref="JsonPatchDocumentExtensions.GetColumnsNames{TModel}(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument{TModel})"/>.
        /// </summary>
        /// <returns>New configuration.</returns>
        public static JsonPatchMapperConfig<TSource> NewConfig()
        {
            JsonPatchMapperConfig<TSource> config = new();
            JsonPatchMapperConfigStore.Instance.Add(config);
            return config;
        }

        /// <summary>
        /// Defines mapping rule for JSON patch operation path to column name.
        /// </summary>
        /// <param name="pathMapping">Mapping rule function.</param>
        /// <returns>Configuration instance for next fluent configuration.</returns>
        /// <remarks>
        /// When you don't want a map path, then return <see langword="null"/> from <paramref name="pathMapping"/>.
        /// </remarks>
        public JsonPatchMapperConfig<TSource> Map(Func<string, string> pathMapping)
        {
            _pathMapping = pathMapping;
            return this;
        }

        string IJsonPatchMapper.GetColumnName(string path)
            => _mapping.GetOrAdd(path, GetColumnNameInternal);

        private string GetColumnNameInternal(string path)
        {
            if (_pathMapping != null)
            {
                path = _pathMapping(path);
            }

            return path != null
                ? string.Join(
                    string.Empty,
                    path.Split('/')
                        .Where(p => !p.IsNullOrWhiteSpace())
                        .Select(p => $"{char.ToUpper(p.First())}{p.Substring(1)}"))
                : null;
        }
    }
}
