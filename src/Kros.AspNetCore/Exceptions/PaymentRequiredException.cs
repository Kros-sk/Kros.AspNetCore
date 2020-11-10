using System;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception which is thrown when request cannot be completed because of missing payment.
    /// </summary>
    public class PaymentRequiredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PaymentRequiredException"/> class.
        /// </summary>
        public PaymentRequiredException()
            : this(Properties.Resources.PaymentRequired)
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
    }
}
