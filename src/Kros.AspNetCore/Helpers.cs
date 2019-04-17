namespace Kros.AspNetCore
{
    /// <summary>
    /// General helpers.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get section name by <typeparamref name="TOptions"/> type.
        /// Section name is class name without Options suffix.
        /// </summary>
        /// <typeparam name="TOptions">Option type.</typeparam>
        /// <returns>Class name without Options suffix.</returns>
        public static string GetSectionName<TOptions>() where TOptions : class
        {
            const string uselessSuffix = "Options";
            var sectionName = typeof(TOptions).Name;

            if (sectionName.EndsWith(uselessSuffix))
            {
                return sectionName.Substring(0, sectionName.Length - uselessSuffix.Length);
            }

            return sectionName;
        }
    }
}
