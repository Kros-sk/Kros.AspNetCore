namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Validation error.
    /// </summary>
    internal class ValidationError
    {
        /// <summary>
        /// Name of the property whose validation test failed.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Error message of failed validation test.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error code of failed validation test.
        /// </summary>
        public string ErrorCode { get; set; }
    }
}
