using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class UserIdFromUserAgentProcessorShould
    {
        [Fact]
        public void AddUserIdFromAgentHeaderToTelemetry()
        {
            using TestActivity activity = new("request");

            new UserIdFromUserAgentProcessor(FakeHttpContextAccessor(true)).OnEnd(activity.Activity);

            Assert.Equal("User-Agent", activity.Activity.GetTagItem(ActivityTags.EnduserId));
        }

        [Fact]
        public void NoUserIdIfUserAgentHeaderIsEmpty()
        {
            using TestActivity activity = new("request");

            new UserIdFromUserAgentProcessor(FakeHttpContextAccessor(false)).OnEnd(activity.Activity);

            Assert.Null(activity.Activity.GetTagItem(ActivityTags.EnduserId));
        }

        [Fact]
        public void NoUserIdForDependencyTelemetry()
        {
            using TestActivity activity = new("dependency", ActivityKind.Client);

            new UserIdFromUserAgentProcessor(FakeHttpContextAccessor(true)).OnEnd(activity.Activity);

            Assert.Null(activity.Activity.GetTagItem(ActivityTags.EnduserId));
        }

        private static HttpContextAccessor FakeHttpContextAccessor(bool addUserAgent)
        {
            DefaultHttpContext httpContext = new();
            HttpContextAccessor context = new()
            {
                HttpContext = httpContext
            };
            if (addUserAgent)
            {
                context.HttpContext.Request.Headers.Append("User-Agent", "User-Agent");
            }

            return context;
        }
    }
}
