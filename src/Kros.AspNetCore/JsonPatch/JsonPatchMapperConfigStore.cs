using System;
using System.Collections.Concurrent;

namespace Kros.AspNetCore.JsonPatch
{
    /// <summary>
    /// Internal store for JSON patch mapping configuration.
    /// </summary>
    internal class JsonPatchMapperConfigStore
    {
        private readonly ConcurrentDictionary<Type, object> _configs = new();
        private static JsonPatchMapperConfigStore _instance;

        /// <summary>
        /// Instance.
        /// </summary>
        public static JsonPatchMapperConfigStore Instance => _instance ?? (_instance = new JsonPatchMapperConfigStore());

        private JsonPatchMapperConfigStore()
        {
        }

        /// <summary>
        /// Get configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </summary>
        /// <typeparam name="TSource">Type of model.</typeparam>
        /// <returns>Configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.</returns>
        public JsonPatchMapperConfig<TSource> GetConfig<TSource>() where TSource : class
            => _configs.GetOrAdd(typeof(TSource), (t) => new JsonPatchMapperConfig<TSource>()) as JsonPatchMapperConfig<TSource>;

        /// <summary>
        /// Store configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </summary>
        /// <typeparam name="TSource">Type of model.</typeparam>
        /// <param name="jsonPatchMapperConfig">
        /// Configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </param>
        public void Add<TSource>(JsonPatchMapperConfig<TSource> jsonPatchMapperConfig) where TSource: class
            => _configs.TryAdd(typeof(TSource), jsonPatchMapperConfig);
    }
}
