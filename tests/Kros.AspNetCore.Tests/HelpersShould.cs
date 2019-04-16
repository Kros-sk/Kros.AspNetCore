using FluentAssertions;
using Xunit;

namespace Kros.AspNetCore.Tests
{
    public class HelpersShould
    {
        #region Test classes

        class TestOptions
        {}

        class TestClass
        { }

        #endregion

        [Fact]
        public void GetSectionName()
        {
            Helpers.GetSectionName<TestOptions>()
                .Should()
                .Be("Test");
        }

        [Fact]
        public void GetSectionNameWhenDoNotEndWithOptions()
        {
            Helpers.GetSectionName<TestClass>()
                .Should()
                .Be("TestClass");
        }
    }
}
