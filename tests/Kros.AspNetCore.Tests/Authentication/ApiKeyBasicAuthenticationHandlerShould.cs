using FluentAssertions;
using Kros.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Kros.AspNetCore.Tests.Authentication
{
    public class ApiKeyBasicAuthenticationHandlerShould
    {
        private readonly ApiKeyBasicAuthenticationOptions _options;
        private readonly IOptionsMonitor<ApiKeyBasicAuthenticationOptions> _optionsMonitor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ApiKeyBasicAuthenticationHandler _handler;

        public ApiKeyBasicAuthenticationHandlerShould()
        {
            _options = new ApiKeyBasicAuthenticationOptions()
            {
                Scheme = "Basic.ApiKey",
                ApiKey = "key2"
            };
            _optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyBasicAuthenticationOptions>>();
            _optionsMonitor.Get(_options.Scheme).Returns(_options);
            _loggerFactory = Substitute.For<ILoggerFactory>();
            _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
            _handler = new ApiKeyBasicAuthenticationHandler(
                _optionsMonitor,
                _loggerFactory,
                Substitute.For<UrlEncoder>(),
                Substitute.For<ISystemClock>()
                );
        }

        [Fact]
        public async Task NotProcessRequestsWithoutAuthHeader()
        {
            DefaultHttpContext context = new();
            await _handler.InitializeAsync(
                new AuthenticationScheme(_options.Scheme, null, typeof(ApiKeyBasicAuthenticationHandler)),
                context);
            AuthenticateResult result = await _handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.None.Should().BeTrue();
        }

        [Fact]
        public async Task NotProcessRequestsWithoutBasicAuthHeader()
        {
            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, "Bearer key2");
            await _handler.InitializeAsync(
                new AuthenticationScheme(_options.Scheme, null, typeof(ApiKeyBasicAuthenticationHandler)),
                context);
            AuthenticateResult result = await _handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.None.Should().BeTrue();
        }

        [Fact]
        public async Task RejectRequestsWithIncorrectApiKey()
        {
            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, "Basic wrong_key");
            await _handler.InitializeAsync(
                new AuthenticationScheme(_options.Scheme, null, typeof(ApiKeyBasicAuthenticationHandler)),
                context);
            AuthenticateResult result = await _handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.Failure.Should().NotBeNull();
        }

        [Fact]
        public async Task AcceptRequestsWithCorrectApiKey()
        {
            DefaultHttpContext context = new();
            context.Request.Headers.Add(HeaderNames.Authorization, "Basic key2");
            await _handler.InitializeAsync(
                new AuthenticationScheme(_options.Scheme, null, typeof(ApiKeyBasicAuthenticationHandler)),
                context);
            AuthenticateResult result = await _handler.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.None.Should().BeFalse();
            result.Failure.Should().BeNull();
        }
    }
}
