using FluentAssertions;
using Kros.AspNetCore.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Kros.AspNetCore.Tests.Configuration
{
    public class DataAnnotatedSettingsTests
    {
        public class TestSettings : AnnotatedSettingsBase
        {
            [Required]
            public string Text { get; set; }

            public string NotValidated { get; set; }
        }

        public class NotAnnotatedTestSettings : AnnotatedSettingsBase
        {
            public string Text { get; set; }

            public string NotValidated { get; set; }
        }

        [Fact]
        public void ValidateMustThrowIfSettingsAreIncorrect()
        {
            var settings = new TestSettings();

            Action action = () => settings.Validate();

            action.Should().Throw<ValidationException>();
        }

        [Theory]
        [MemberData(nameof(ValidateMustNotThrowIfSettingsAreCorrect_Data))]
        public void ValidateMustNotThrowIfSettingsAreCorrect(IValidatable settings)
        {
            Action action = () => settings.Validate();

            action.Should().NotThrow();
        }

        public static IEnumerable<object[]> ValidateMustNotThrowIfSettingsAreCorrect_Data()
        {
            yield return new object[] { new TestSettings { Text = "Lorem Ipsum" } };
            yield return new object[] { new NotAnnotatedTestSettings() };
        }
    }
}
