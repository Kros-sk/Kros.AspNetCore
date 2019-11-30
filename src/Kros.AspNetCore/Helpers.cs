using System;

namespace Kros.AspNetCore
{
    /// <summary>
    /// General helpers.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get section name by <typeparamref name="TOptions"/> type. Section name is class name without <c>Options</c>
        /// or <c>Settings</c> suffix. So if class name is <c>SmtpOptions</c>, the name is <c>Smtp</c>.
        /// </summary>
        /// <typeparam name="TOptions">Option type.</typeparam>
        /// <returns>Class name without Options suffix.</returns>
        public static string GetSectionName<TOptions>() where TOptions : class
            => GetSectionName(typeof(TOptions));

        internal static string GetSectionName(Type optionsType)
        {
            const string uselessSuffixOptions = "Options";
            const string uselessSuffixSettings = "Settings";
            string sectionName = optionsType.Name;

            if (sectionName.EndsWith(uselessSuffixOptions, StringComparison.OrdinalIgnoreCase))
            {
                return sectionName.Substring(0, sectionName.Length - uselessSuffixOptions.Length);
            }
            else if (sectionName.EndsWith(uselessSuffixSettings, StringComparison.OrdinalIgnoreCase))
            {
                return sectionName.Substring(0, sectionName.Length - uselessSuffixSettings.Length);
            }
            return sectionName;
        }
    }
}
