using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

[assembly: InternalsVisibleTo("Kros.ApplicationInsights.Extensions.Tests")]
namespace Kros.ApplicationInsights.Extensions
{
    /// <summary>
    /// Configuration of user id in Application Insights telemetry.
    /// </summary>
    /// <seealso cref="Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer" />
    internal class UserCompanyInitializer : ITelemetryInitializer
    {
        private const string ClaimCompanyId = "company_id";
        private const string ClaimUserId = "user_id";

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCompanyInitializer"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of IHttpContextAccessor.</param>
        public UserCompanyInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes properties of the specified <see cref="T:Microsoft.ApplicationInsights.Channel.ITelemetry" /> object.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            var request = telemetry as RequestTelemetry;
            if (request != null)
            {
                AddProperty(request, ClaimCompanyId);
                AddProperty(request, ClaimUserId);
            }
        }

        private void AddProperty(RequestTelemetry requestTelemetry, string claimType)
        {
            string claimValue = GetClaimValue(claimType);
            if (!string.IsNullOrEmpty(claimValue))
            {
                requestTelemetry.Properties.Add(claimType, claimValue);
            }
        }

        private string GetClaimValue(string claimType)
        {
            Claim claim = _httpContextAccessor?.
                HttpContext?.
                User?.
                Identities?.
                FirstOrDefault()?.
                Claims?.
                FirstOrDefault(c => c.Type == claimType);
            return claim != null ? claim.Value : string.Empty;
        }


    }
}
