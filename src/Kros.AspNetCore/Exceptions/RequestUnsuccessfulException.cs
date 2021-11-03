using System;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace Kros.AspNetCore.Exceptions
{
    /// <summary>
    ///  The exception thrown when request was not successful.
    /// </summary>
    public class RequestUnsuccessfulException : Exception
    {
        /// <summary>
        /// Response payload content type.
        /// </summary>
        public MediaTypeHeaderValue ResponseContentType { get; private set; }

        /// <summary>
        /// Serialized response content.
        /// </summary>
        public string ResponseContent { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUnsuccessfulException"/> class.
        /// </summary>
        public RequestUnsuccessfulException()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUnsuccessfulException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public RequestUnsuccessfulException(string message)
            : this(message, null, new MediaTypeHeaderValue(MediaTypeNames.Text.Plain))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUnsuccessfulException"/> class.
        /// </summary>
        /// <param name="responseContent">Serialized response content.</param>
        /// <param name="responseContentType">Response content type.</param>
        public RequestUnsuccessfulException(string responseContent, MediaTypeHeaderValue responseContentType)
            : this(string.Empty, responseContent, responseContentType)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RequestUnsuccessfulException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="responseContent">Serialized response content.</param>
        /// <param name="responseContentType">Response content type.</param>
        public RequestUnsuccessfulException(string message, string responseContent, MediaTypeHeaderValue responseContentType)
            : base(message)
        {
            AddPayload(responseContent, responseContentType);
        }

        /// <summary>
        /// Adds payload to exception.
        /// </summary>
        /// <param name="payload">Serialized payload.</param>
        /// <param name="payloadContentType">Payload content type.</param>
        internal void AddPayload(string payload, MediaTypeHeaderValue payloadContentType)
        {
            ResponseContent = payload;
            ResponseContentType = payloadContentType;
        }
    }
}
