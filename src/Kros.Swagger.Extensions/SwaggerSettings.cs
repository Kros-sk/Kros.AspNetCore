using System;

namespace Kros.Swagger.Extensions
{
    /// <summary>
    /// Settings for Swagger documentation.
    /// </summary>
    public class SwaggerSettings
    {
        #region Nested types

        /// <summary>
        /// API contact settings.
        /// </summary>
        public class ContactSettings
        {
            /// <summary>
            /// The identifying name of the contact person/organization.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// The URL pointing to the contact information. MUST be in the format of a URL.
            /// </summary>
            public string Url { get; set; } = string.Empty;

            /// <summary>
            /// The email address of the contact person/organization.
            /// MUST be in the format of an email address.
            /// </summary>
            public string Email { get; set; } = string.Empty;
        }

        /// <summary>
        /// API license settings.
        /// </summary>
        public class LicenseSettings
        {
            /// <summary>
            /// REQUIRED. The license name used for the API.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// The URL pointing to the contact information. MUST be in the format of a URL.
            /// </summary>
            public string Url { get; set; } = string.Empty;
        }

        #endregion Nested types

        /// <summary>
        /// The title of the application.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// A short description of the application.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// REQUIRED. The version of the OpenAPI document.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// A URL to the Terms of Service for the API. MUST be in the format of a URL.
        /// </summary>
        public string TermsOfService { get; set; } = string.Empty;

        /// <summary>
        /// The contact information for the exposed API.
        /// </summary>
        public ContactSettings Contact { get; } = new ContactSettings();

        /// <summary>
        /// The license information for the exposed API.
        /// </summary>
        public LicenseSettings License { get; } = new LicenseSettings();
    }
}
