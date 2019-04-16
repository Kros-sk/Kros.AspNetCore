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
        /// Configure options of type <typeparamref name="TOptions"/> and binds it to section with the same name as
        /// the  <typeparamref name="TOptions"/> type in <paramref name="configuration"/>.
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
        /// Adds web api dependencies.
        /// </summary>
        /// <param name="services">IoC container.</param>
        public static IMvcCoreBuilder AddWebApi(this IServiceCollection services)
        {
            var builder = services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            return builder.AddFormatterMappings()
                .AddJsonFormatters()
                .AddCors()
                .AddApiExplorer();
        }
    }
}
