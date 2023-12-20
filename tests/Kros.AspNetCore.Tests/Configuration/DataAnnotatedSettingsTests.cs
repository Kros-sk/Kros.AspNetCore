using FluentAssertions;
using Kros.AspNetCore.Configuration;
using System;
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
            TestSettings settings = new();

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

        public static TheoryData<IValidatable> ValidateMustNotThrowIfSettingsAreCorrect_Data()
            => new()
            {
                { new TestSettings() { Text = "Lorem Ipsum" } },
                { new NotAnnotatedTestSettings() }
            };
    }
}
