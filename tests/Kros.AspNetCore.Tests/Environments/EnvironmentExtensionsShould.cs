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

            Assert.True(env.IsTest());
        }

        [Fact]
        public void IsTestReturnFalseIfDevelopmentEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Development);
            Assert.False(env.IsTest());
        }

        [Fact]
        public void IsTestOrDevelopmentReturnTrueIfDevelopmentEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Development);
            Assert.True(env.IsTestOrDevelopment());
        }

        [Fact]
        public void IsTestOrDevelopmentReturnFalseIfStagingEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Staging);
            Assert.False(env.IsTestOrDevelopment());
        }

        [Fact]
        public void IsStagingOrProductionReturnTrueIfStagingEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(Microsoft.Extensions.Hosting.Environments.Staging);
            Assert.True(env.IsStagingOrProduction());
        }

        [Fact]
        public void IsStagingOrProductionReturnFalseIfTestEnvironment()
        {
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.EnvironmentName.Returns(EnvironmentsEx.Test);
            Assert.False(env.IsStagingOrProduction());
        }
    }
}
