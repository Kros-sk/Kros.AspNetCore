using FluentAssertions;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Kros.MediatR.Extensions.Tests
{
    public class PipelineBehaviorsConfigShould
    {
        [Fact]
        public void ReturnPipelineBehaviorAssembly()
        {
            var config = new PipelineBehaviorsConfig();
            config.PipelineBehaviorsAssembly = typeof(string).Assembly;

            IEnumerable<Assembly> assemblies = config.GetPipelineBehaviorAssemblies();

            assemblies.Should().HaveCount(1);
        }

        [Fact]
        public void ReturnRequestAssembly()
        {
            var config = new PipelineBehaviorsConfig();
            config.RequestsAssembly = typeof(string).Assembly;

            IEnumerable<Assembly> assemblies = config.GetRequestAssemblies();

            assemblies.Should().HaveCount(1);
        }

        [Fact]
        public void IgnoreNullRequestAssembly()
        {
            var config = new PipelineBehaviorsConfig();
            config.AddRequestAssembly(typeof(string).Assembly);
            config.AddRequestAssembly(GetType().Assembly);

            IEnumerable<Assembly> assemblies = config.GetRequestAssemblies();

            assemblies.Should().HaveCount(2);
        }
    }
}
