using FluentAssertions;
using System;
using Xunit;

namespace Kros.AspNetCore.Tests
{
    public class HelpersShould
    {
        #region Test classes

        class TestOptions
        { }

        class Test2options
        { }

        class TestSettings
        { }

        class Test2settings
        { }

        class TestClass
        { }

        #endregion

        [Theory]
        [MemberData(nameof(GetSectionName_TestData))]
        public void GetSectionName(Type tOptions, string expectedSectionName)
        {
            Helpers.GetSectionName(tOptions)
                .Should()
                .Be(expectedSectionName);
        }

        public static TheoryData<Type, string> GetSectionName_TestData()
            => new()
        {
            { typeof(TestOptions), "Test" },
            { typeof(Test2options), "Test2" },
            { typeof(TestSettings), "Test" },
            { typeof(Test2settings), "Test2" },
            { typeof(TestClass), "TestClass" }
        };
    }
}
