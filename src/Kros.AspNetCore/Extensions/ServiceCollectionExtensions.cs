using Kros.AspNetCore;
using Kros.AspNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add an <see cref="IStartupFilter"/> to the application that invokes <see cref="IValidatable.Validate"/>
        /// on all registered objects.
        /// </summary>
        public static IServiceCollection UseConfigurationValidation(this IServiceCollection services)
            => services.AddTransient<IStartupFilter, SettingsValidationStartupFilter>();

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
            IConfiguration configuration) where TOptions : class, new()
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
            string sectionName) where TOptions : class, new()
        {
            services.Configure<TOptions>(options => configuration.GetSection(sectionName).Bind(options));
            services.AddTransient<TOptions>(ctx => ctx.GetRequiredService<IOptionsSnapshot<TOptions>>().Value);
            if (typeof(IValidatable).IsAssignableFrom(typeof(TOptions)))
            {
                services.AddSingleton<IValidatable>(ctx => (IValidatable)ctx.GetRequiredService<IOptions<TOptions>>().Value);
            }
            return services;
        }

        /// <summary>
        /// Adds the minimum essential MVC services to the DI container for web API services.
        /// (MVC Core, JSON Formatters, CORS, API Explorer, Authorization)
        /// Additional services must be added separately using the <see cref="IMvcBuilder"/> returned from this method.
        /// </summary>
        /// <param name="services">MVC builder.</param>
        [Obsolete("AddWebApi() is deprecated, please use AddControllers() instead.")]
        public static IMvcBuilder AddWebApi(this IServiceCollection services)
            => services.AddControllers();

        /// <summary>
        /// Attempts to set proxy to HttpClient.
        /// </summary>
        /// <param name="services">DI container.</param>
        /// <param name="configuration">App configuration. A configuration section called "Proxy" with "Address"
        /// value is required.</param>
        public static IServiceCollection SetProxy(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("Proxy");
            if (section.Exists())
            {
                WebProxy proxy = section.Get<WebProxy>();
                if (!string.IsNullOrEmpty(proxy.Address?.AbsolutePath))
                {
                    HttpClient.DefaultProxy = proxy;
                }
            }

            return services;
        }
    }
}
