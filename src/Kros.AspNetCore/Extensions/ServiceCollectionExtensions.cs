using Kros.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Default MVC compatibility version for <see cref="AddWebApi(IServiceCollection)"/>.
        /// </summary>
        public static CompatibilityVersion DefaultCompatibilityVersion => CompatibilityVersion.Version_2_2;

        /// <summary>
        /// Configure options of type <typeparamref name="TOptions"/> and binds it to section with the name
        /// same as the class <typeparamref name="TOptions"/> without Options suffix in <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TOptions">Type of the options.</typeparam>
        /// <param name="services">Service collection where the options are registered.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <returns>Returns input <paramref name="services"/>.</returns>
        public static IServiceCollection ConfigureOptions<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration) where TOptions : class
            => ConfigureOptions<TOptions>(services, configuration, Helpers.GetSectionName<TOptions>());

        /// <summary>
        /// Configure options of type <typeparamref name="TOptions"/> and binds it to section <paramref name="sectionName"/>
        /// in <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TOptions">Type of the options.</typeparam>
        /// <param name="services">Service collection where the options are registered.</param>
        /// <param name="configuration">Configuration from which the options are loaded.</param>
        /// <param name="sectionName">Section name in configuration.</param>
        /// <returns>Returns input <paramref name="services"/>.</returns>
        public static IServiceCollection ConfigureOptions<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName) where TOptions : class
            => services.Configure<TOptions>(options => configuration.GetSection(sectionName).Bind(options));

        /// <summary>
        /// Adds the minimum essential MVC services to the DI container for web API services.
        /// (MVC Core, JSON Formatters, CORS, API Explorer)
        /// Additional services must be added separately using the <see cref="IMvcCoreBuilder"/> returned from this method.
        /// As compatibility version for MVC is used <see cref="DefaultCompatibilityVersion"/>.
        /// </summary>
        /// <param name="services">MVC Core builder.</param>
        public static IMvcCoreBuilder AddWebApi(this IServiceCollection services)
            => AddWebApi(services, DefaultCompatibilityVersion);

        /// <summary>
        /// Adds the minimum essential MVC services to the DI container for web API services.
        /// (MVC Core, JSON Formatters, CORS, API Explorer)
        /// Additional services must be added separately using the <see cref="IMvcCoreBuilder"/> returned from this method.
        /// </summary>
        /// <param name="services">MVC Core builder.</param>
        /// <param name="compatibilityVersion">Compatibility version for MVC.</param>
        public static IMvcCoreBuilder AddWebApi(this IServiceCollection services, CompatibilityVersion compatibilityVersion)
        {
            var builder = services.AddMvcCore().SetCompatibilityVersion(compatibilityVersion);

            return builder.AddFormatterMappings()
                .AddJsonFormatters()
                .AddCors()
                .AddApiExplorer();
        }
    }
}
