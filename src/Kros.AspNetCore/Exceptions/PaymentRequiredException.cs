using System.Net.Http.Headers;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception which is thrown when request cannot be completed because of missing payment.
    /// </summary>
    public class PaymentRequiredException : RequestUnsuccessfulException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PaymentRequiredException"/> class.
        /// </summary>
        public PaymentRequiredException()
            : base(Properties.Resources.PaymentRequired)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PaymentRequiredException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public PaymentRequiredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PaymentRequiredException"/> class.
        /// </summary>
        /// <param name="responseContent">Serialized response content.</param>
        /// <param name="responseContentType">Response content type.</param>
        public PaymentRequiredException(string responseContent, MediaTypeHeaderValue responseContentType)
            : base(Properties.Resources.PaymentRequired, responseContent, responseContentType)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PaymentRequiredException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="responseContent">Serialized response content.</param>
        /// <param name="responseContentType">Response content type.</param>
        public PaymentRequiredException(string message, string responseContent, MediaTypeHeaderValue responseContentType)
            : base(message, responseContent, responseContentType)
        {
        }
    }
}
