using FluentAssertions;
using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> GetSectionName_TestData()
        {
            yield return new object[] { typeof(TestOptions), "Test" };
            yield return new object[] { typeof(Test2options), "Test2" };
            yield return new object[] { typeof(TestSettings), "Test" };
            yield return new object[] { typeof(Test2settings), "Test2" };
            yield return new object[] { typeof(TestClass), "TestClass" };
        }
    }
}
