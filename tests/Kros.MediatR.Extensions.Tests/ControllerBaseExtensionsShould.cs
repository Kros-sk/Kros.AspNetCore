using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kros.MediatR.Extensions.Tests
{
    public class ControllerBaseExtensionsShould
    {
        #region Nested classes

        public class TestController : ControllerBase
        {
        }

        public class Request : IRequest<Request.Response>
        {
            public int Value { get; set; }

            public class Response
            {
                public int Value { get; set; }
            }
        }

        public class RequestHandler : IRequestHandler<Request, Request.Response>
        {
            public Task<Request.Response> Handle(Request request, CancellationToken cancellationToken)
                => Task.FromResult(new Request.Response() { Value = request.Value });
        }

        #endregion

        [Fact]
        public void GetMediatR()
        {
            var controller = CreateController();

            controller.Mediator().Should().NotBeNull();
        }

        [Fact]
        public async Task SentRequest()
        {
            var controller = CreateController();

            var response = await controller.SendRequest(new Request() { Value = 22 });

            response.Value.Should().Be(22);
        }

        [Fact]
        public async Task SentCreateCommand()
        {
            var controller = CreateController();

            var response = await controller.SendCreateCommand(new Request() { Value = 22 });

            response.StatusCode.Should().Be(201);
        }

        private static TestController CreateController()
        {
            var service = new ServiceCollection();
            service.AddMediatR();

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = service.BuildServiceProvider()
                },
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            };

            var controller = new TestController()
            {
                ControllerContext = new ControllerContext(actionContext)
            };
            return controller;
        }
    }
}
