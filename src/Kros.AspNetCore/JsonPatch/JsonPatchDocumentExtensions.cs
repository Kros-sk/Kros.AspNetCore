using Kros.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Linq;

namespace Kros.AspNetCore.JsonPatch
{
    /// <summary>
    /// Extensions method for <see cref="JsonPatchDocument{TModel}"/>
    /// </summary>
    public static class JsonPatchDocumentExtensions
    {
        /// <summary>
        /// Get columns names from JSON patch operations path.
        /// </summary>
        /// <typeparam name="TModel">Patch model type.</typeparam>
        /// <param name="jsonPatch">JSON patch document.</param>
        /// <returns>Columns names.</returns>
        /// <remarks>
        /// Use default configuration created by <see cref="JsonPatchMapperConfig{TSource}.NewConfig"/>.
        /// </remarks>
        public static IEnumerable<string> GetColumnsNames<TModel>(this JsonPatchDocument<TModel> jsonPatch) where TModel : class
            => GetColumnsNames(jsonPatch, JsonPatchMapperConfigStore.Instance.GetConfig<TModel>());

        /// <summary>
        /// Get columns names from JSON patch operations path.
        /// </summary>
        /// <typeparam name="TModel">Patch model type.</typeparam>
        /// <param name="jsonPatch">JSON patch document.</param>
        /// <param name="patchMapperConfig">Mapping configuration.</param>
        /// <returns>Columns names.</returns>
        public static IEnumerable<string> GetColumnsNames<TModel>(
            this JsonPatchDocument<TModel> jsonPatch,
            JsonPatchMapperConfig<TModel> patchMapperConfig) where TModel : class
            => jsonPatch.Operations
                .Select(o => ((IJsonPatchMapper)patchMapperConfig).GetColumnName(o.path))
                .Where(p => !p.IsNullOrEmpty());
    }
}
