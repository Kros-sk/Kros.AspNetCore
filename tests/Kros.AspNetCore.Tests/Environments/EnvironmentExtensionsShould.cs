using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Xunit;

namespace Kros.AspNetCore.Tests.Environments
{
    public class EnvironmentExtensionsShould
    {
        [Fact]
        public void IsTestReturnTrueIfTestEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Test);

            env.IsTest().Should().BeTrue();
        }

        [Fact]
        public void IsTestReturnFalseIfDevelopmentEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Development);
            env.IsTest().Should().BeFalse();
        }

        [Fact]
        public void IsTestOrDevelopmentReturnTrueIfDevelopmentEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Development);
            env.IsTestOrDevelopment().Should().BeTrue();
        }

        [Fact]
        public void IsTestOrDevelopmentReturnFalseIfStagingEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Staging);
            env.IsTestOrDevelopment().Should().BeFalse();
        }

        [Fact]
        public void IsStagingOrProductionReturnTrueIfStagingEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Staging);
            env.IsStagingOrProduction().Should().BeTrue();
        }

        [Fact]
        public void IsStagingOrProductionReturnFalseIfTestEnvironment()
        {
            IHostingEnvironment env = Substitute.For<IHostingEnvironment>();
            env.EnvironmentName.Returns(Microsoft.AspNetCore.Hosting.Environments.Test);
            env.IsStagingOrProduction().Should().BeFalse();
        }
    }
}
