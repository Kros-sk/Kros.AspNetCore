using Kros.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Kros.AspNetCore.JsonPatch
{
    /// <summary>
    ///JSON PATCH mapper configuration. Configuration for mapping <see cref="JsonPatchMapperConfig{TSource}"/>
    ///operation paths to database columns.
    /// </summary>
    /// <typeparam name="TSource">Source model type.</typeparam>
    public class JsonPatchMapperConfig<TSource> : IJsonPatchMapper where TSource : class
    {
        private Func<string, string> _pathMapping;
        private ConcurrentDictionary<string, string> _mapping =
            new ConcurrentDictionary<string, string>(StringComparer.InvariantCulture);

        /// <summary>
        /// Creates new config for <typeparamref name="TSource"/> and store it for non parametric  extension
        /// <see cref="JsonPatchDocumentExtensions.GetColumnsNames{TModel}(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument{TModel})"/>.
        /// </summary>
        /// <returns>New configuration.</returns>
        public static JsonPatchMapperConfig<TSource> NewConfig()
        {
            var config = new JsonPatchMapperConfig<TSource>();

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
