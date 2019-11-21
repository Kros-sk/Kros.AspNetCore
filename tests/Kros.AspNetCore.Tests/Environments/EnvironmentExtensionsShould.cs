using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace Kros.AspNetCore.Tests.Environments
{
    public class EnvironmentExtensionsShould
    {
        [Fact]
        public void IsTestReturnTrueIfTestEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(EnvironmentsEx.Test);

            env.IsTest().Should().BeTrue();
        }

        [Fact]
        public void IsTestReturnFalseIfDevelopmentEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Development);
            env.IsTest().Should().BeFalse();
        }

        [Fact]
        public void IsTestOrDevelopmentReturnTrueIfDevelopmentEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Development);
            env.IsTestOrDevelopment().Should().BeTrue();
        }

        [Fact]
        public void IsTestOrDevelopmentReturnFalseIfStagingEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Staging);
            env.IsTestOrDevelopment().Should().BeFalse();
        }

        [Fact]
        public void IsStagingOrProductionReturnTrueIfStagingEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Staging);
            env.IsStagingOrProduction().Should().BeTrue();
        }

        [Fact]
        public void IsStagingOrProductionReturnFalseIfTestEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(EnvironmentsEx.Test);
            env.IsStagingOrProduction().Should().BeFalse();
        }
    }
}
