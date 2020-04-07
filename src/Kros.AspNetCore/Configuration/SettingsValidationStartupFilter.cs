using Kros.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Kros.AspNetCore.Configuration
{
    /// <summary>
    /// An <see cref="IStartupFilter"/> that validates all <see cref="IValidatable"/> objects in DI container during
    /// application startup.
    /// Turn on validation by using <see cref="ServiceCollectionExtensions.UseConfigurationValidation(IServiceCollection)"/>
    /// extension method in your <c>ConfigureServices</c> method.
    /// </summary>
    public class SettingsValidationStartupFilter : IStartupFilter
    {
        private readonly IEnumerable<IValidatable> _validatableObjects;

        /// <summary>
        /// Create a new instance of <see cref="SettingsValidationStartupFilter"/>
        /// </summary>
        /// <param name="validatableObjects">List of objects to validate.</param>
        public SettingsValidationStartupFilter(IEnumerable<IValidatable> validatableObjects)
        {
            _validatableObjects = Check.NotNull(validatableObjects, nameof(validatableObjects));
        }

        /// <inheritdoc />
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach (IValidatable validatableObject in _validatableObjects)
            {
                validatableObject.Validate();
            }
            return next;
        }
    }
}
