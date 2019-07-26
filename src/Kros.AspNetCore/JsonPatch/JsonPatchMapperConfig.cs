using Kros.Extensions;
using System;
using System.Collections.Concurrent;

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
            new ConcurrentDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

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
        /// Defines mapping rule from JSON PATCH operation path to column name.
        /// </summary>
        /// <param name="pathMapping">Mapping rule function.</param>
        /// <returns>Configuration instance for next fluent configuration.</returns>
        public JsonPatchMapperConfig<TSource> Map(Func<string, string> pathMapping)
        {
            _pathMapping = pathMapping;

            return this;
        }

        string IJsonPatchMapper.GetColumnName(string path)
            => _mapping.GetOrAdd(path, GetColumnNameInternal);

        private string GetColumnNameInternal(string path)
        {
            string property = path.TrimStart('/').Replace("/", ".");

            if (_pathMapping != null)
            {
                string mappedProperty = _pathMapping(property);
                if (mappedProperty.IsNullOrEmpty()
                    || !property.Equals(mappedProperty, StringComparison.CurrentCultureIgnoreCase))
                {
                    return mappedProperty;
                }
            }

            return property.Replace(".", string.Empty);
        }
    }
}
