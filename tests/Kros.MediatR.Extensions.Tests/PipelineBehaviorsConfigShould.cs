using FluentAssertions;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Kros.MediatR.Extensions.Tests
{
    public class PipelineBehaviorsConfigShould
    {
        [Fact]
        public void IgnoreNullRequestAssembly()
        {
            PipelineBehaviorsConfig config = new();
            config.AddRequestAssembly(typeof(string).Assembly);
            config.AddRequestAssembly(GetType().Assembly);

            IEnumerable<Assembly> assemblies = config.GetRequestAssemblies();

            assemblies.Should().HaveCount(2);
        }
    }
}
