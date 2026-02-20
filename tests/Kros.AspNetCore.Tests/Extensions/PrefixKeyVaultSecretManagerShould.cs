using Azure.Security.KeyVault.Secrets;
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
            Assert.ThrowsAny<ArgumentException>(action);
        }

        [Theory]
        [MemberData(nameof(ThrowArgumentExceptionIfNoValidPrefix_Data))]
        public void ThrowArgumentExceptionIfNoValidPrefix(IEnumerable<string> prefix)
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager(prefix);
            Assert.ThrowsAny<ArgumentException>(action);
        }

        public static TheoryData<IEnumerable<string>> ThrowArgumentExceptionIfNoValidPrefix_Data()
            => new()
            {
                { null },
                { Array.Empty<string>() },
                { new string[] { "", "   ", "\t" } }
            };

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
            Assert.Equal(loadIt, manager.Load(secret));
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
            Assert.Equal(configKey, manager.GetKey(secret));
        }

        [Fact]
        public void ThrowWhenPrefixIsInvalid()
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager("invalid-prefix");
            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Contains("invalid-prefix", exception.Message);
        }

        [Fact]
        public void ThrowWhenSomeOfPrefixesIsInvalid()
        {
            Func<PrefixKeyVaultSecretManager> action = () => new PrefixKeyVaultSecretManager(
                new[] { "validPrefix1", "validPrefix2", "invalid-prefix", "validPrefix3" });
            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Contains("invalid-prefix", exception.Message);
        }

        private static PrefixKeyVaultSecretManager CreateManager() => new(new[] { "Lorem", "DOLOR" });
    }
}
