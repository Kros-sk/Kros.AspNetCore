namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Validation error.
    /// </summary>
    internal class ValidationError: ErrorBase
    {
        /// <summary>
        /// Name of the property whose validation test failed.
        /// </summary>
        public string PropertyName { get; set; }
    }
}
