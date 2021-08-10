namespace Kros.ProblemDetails.Extensions
{
    /// <summary>
    /// Error base.
    /// </summary>
    internal class ErrorBase
    {
        /// <summary>
        /// Error code of failed validation test.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error message of failed validation test.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
