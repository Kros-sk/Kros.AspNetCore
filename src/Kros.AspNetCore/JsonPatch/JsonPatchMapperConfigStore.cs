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

        /// <summary>
        /// Instance.
        /// </summary>
        public static JsonPatchMapperConfigStore Instance { get; } = new();

        private JsonPatchMapperConfigStore()
        {
        }

        /// <summary>
        /// Get configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </summary>
        /// <typeparam name="TSource">Type of model.</typeparam>
        /// <returns>Configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.</returns>
        public JsonPatchMapperConfig<TSource> GetConfig<TSource>() where TSource : class
            => _configs.GetOrAdd(typeof(TSource), (t) => JsonPatchMapperConfig<TSource>.NewConfig()) as JsonPatchMapperConfig<TSource>;

        /// <summary>
        /// Store configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </summary>
        /// <typeparam name="TSource">Type of model.</typeparam>
        /// <param name="jsonPatchMapperConfig">
        /// Configuration for mapping JSON patch of <typeparamref name="TSource"/> model to database names.
        /// </param>
        public void Add<TSource>(JsonPatchMapperConfig<TSource> jsonPatchMapperConfig) where TSource : class
        {
            bool added = _configs.TryAdd(typeof(TSource), jsonPatchMapperConfig);
#pragma warning disable CS0618 // Type or member is obsolete
            if (!added && JsonPatchMapperConfig.ThrowExceptionOnDuplicateNewConfig)
            {
                string msg = $"Configuration for type {typeof(TSource)} already exists."
                    + " If you do not want this exception to be thrown, set JsonPatchMapperConfig.ThrowExceptionOnDuplicateNewConfig to 'false'."
                    + " But only the first added configuration for each type is used.";
                throw new InvalidOperationException(msg);
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
