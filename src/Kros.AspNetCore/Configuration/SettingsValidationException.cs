using System;

namespace Kros.AspNetCore.Configuration
{
    /// <summary>
    /// Standard exception for settings validation: <see cref="IValidatable"/>.
    /// </summary>
    public class SettingsValidationException : Exception
    {
        /// <summary>
        /// Create a new instance of <see cref="SettingsValidationException"/>
        /// </summary>
        /// <param name="message">Exception message.</param>
        public SettingsValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create a new instance of <see cref="SettingsValidationException"/>
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public SettingsValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
