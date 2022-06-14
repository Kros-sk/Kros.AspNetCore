using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using Kros.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class PrefixKeyVaultSecretManagerShould
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" \t \n")]
        public void ThrowArgumentExceptionIfPrefixNotSet(string prefix)
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager(prefix);
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(ThrowArgumentExceptionIfNoValidPrefix_Data))]
        public void ThrowArgumentExceptionIfNoValidPrefix(IEnumerable<string> prefix)
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager(prefix);
            action.Should().Throw<ArgumentException>();
        }

        public static IEnumerable<object[]> ThrowArgumentExceptionIfNoValidPrefix_Data()
        {
            yield return new object[] { null };
            yield return new object[] { Array.Empty<string>() };
            yield return new object[] { new string[] { "", "   ", "\t" } };
        }

        [Theory]
        [InlineData("lorem-ipsum1", true)]
        [InlineData("Lorem-ipsum2", true)]
        [InlineData("LOREM-ipsum3", true)]
        [InlineData("lorem-hierarchey--key--one", true)]
        [InlineData("lorem-hierarchey--key--two", true)]
        [InlineData("lorem-non-hierarchy-key", true)]
        [InlineData("loremWithoutDash", false)]
        [InlineData("invalidPrefix-keyName", false)]
        [InlineData("noPrefix", false)]
        [InlineData("dolor-sitAmet", true)]
        public void LoadPrefixedValue(string secretName, bool loadIt)
        {
            PrefixKeyVaultSecretManager manager = CreateManager();
            SecretProperties secret = new(secretName);
            manager.Load(secret).Should().Be(loadIt);
        }

        [Theory]
        [InlineData("lorem-ipsum1", "ipsum1")]
        [InlineData("Lorem-ipsum2", "ipsum2")]
        [InlineData("LOREM-ipsum3", "ipsum3")]
        [InlineData("lorem-hierarchy--key--one", "hierarchy:key:one")]
        [InlineData("lorem-hierarchy--key--two", "hierarchy:key:two")]
        [InlineData("lorem-non-hierarchy-key", "non-hierarchy-key")]
        [InlineData("dolor-sitAmet", "sitAmet")]
        [InlineData("noPrefix", "noPrefix")]
        public void CreateCorrectConfigKey(string secretName, string configKey)
        {
            PrefixKeyVaultSecretManager manager = CreateManager();
            KeyVaultSecret secret = new(secretName, string.Empty);
            manager.GetKey(secret).Should().Be(configKey);
        }

        [Fact]
        public void ThrowWhenPrefixIsInvalid()
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager("invalid-prefix");
            action.Should().Throw<ArgumentException>()
                .WithMessage("*invalid-prefix*");
        }

        [Fact]
        public void ThrowWhenSomeOfPrefixesIsInvalid()
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager(
                new[] { "validPrefix1", "validPrefix2", "invalid-prefix", "validPrefix3" });
            action.Should().Throw<ArgumentException>()
                .WithMessage("*invalid-prefix*");
        }

        private static PrefixKeyVaultSecretManager CreateManager() => new(new[] { "Lorem", "DOLOR" });
    }
}
