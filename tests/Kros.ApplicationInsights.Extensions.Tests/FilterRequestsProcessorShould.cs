using System.Diagnostics;
using Xunit;

namespace Kros.ApplicationInsights.Extensions.Tests
{
    public class FilterRequestsProcessorShould
    {
        [Theory]
        [InlineData("/health")]
        [InlineData("/health/ready")]
        [InlineData("/signalR")]
        public void FilterOutSkippedRequestPath(string path)
            => Assert.True(Process(path: path).IsDropped);

        [Theory]
        [InlineData("/weather")]
        [InlineData("/optionsrequest")]
        [InlineData("/someRequest")]
        public void PassOtherRequestPaths(string path)
            => Assert.False(Process(path: path).IsDropped);

        [Fact]
        public void FilterOutRequestWhenOnlyDisplayNameContainsSkippedRequest()
            => Assert.True(Process(displayName: "GET /health").IsDropped);

        [Fact]
        public void FilterOutHttpOptionsRequest()
            => Assert.True(Process(path: "/someRequest", method: "OPTIONS").IsDropped);

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public void PassOtherHttpMethods(string method)
            => Assert.False(Process(path: "/someRequest", method: method).IsDropped);

        [Theory]
        [InlineData("postman")]
        [InlineData("PostmanRuntime/7.5")]
        public void FilterOutSkippedUserAgent(string userAgent)
            => Assert.True(Process(path: "/someRequest", userAgent: userAgent).IsDropped);

        [Theory]
        [InlineData("Safari/4.23")]
        [InlineData("Chrome/4.23")]
        [InlineData("Opera/4.23")]
        public void PassOtherUserAgents(string userAgent)
            => Assert.False(Process(path: "/someRequest", userAgent: userAgent).IsDropped);

        [Fact]
        public void PassDependencyTelemetryForSkippedPath()
        {
            using TestActivity activity = new("dependency", ActivityKind.Client);
            activity.WithTag(ActivityTags.UrlPath, "/health");

            new FilterRequestsProcessor().OnEnd(activity.Activity);

            Assert.False(activity.IsDropped);
        }

        private static TestActivity Process(
            string path = null,
            string method = null,
            string userAgent = null,
            string displayName = "request")
        {
            TestActivity activity = new(displayName);
            if (path is not null)
            {
                activity.WithTag(ActivityTags.UrlPath, path);
            }
            if (method is not null)
            {
                activity.WithTag(ActivityTags.HttpRequestMethod, method);
            }
            if (userAgent is not null)
            {
                activity.WithTag(ActivityTags.UserAgentOriginal, userAgent);
            }

            new FilterRequestsProcessor().OnEnd(activity.Activity);

            return activity;
        }
    }
}
