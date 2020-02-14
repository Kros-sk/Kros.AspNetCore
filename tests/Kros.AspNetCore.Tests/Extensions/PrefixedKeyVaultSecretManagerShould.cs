using Xunit;
using FluentAssertions;
using Microsoft.Azure.KeyVault.Models;
using Kros.AspNetCore.Extensions;

namespace Kros.AspNetCore.Tests.Extensions
{
    public class PrefixedKeyVaultSecretManagerShould
    {
        [Fact]
        public void LoadSecretWithNameStartingWithPrefix()
        {
            // arrange
            var secretNamePrefix = "Prefix";
            var secretManager = new PrefixedKeyVaultSecretManager(secretNamePrefix);
            var secret = new SecretItem($"https://keyVaultEndpoint/secrets/{secretNamePrefix}--SecretName");

            // act
            bool shouldLoad = secretManager.Load(secret);

            // assert
            shouldLoad.Should().BeTrue();
        }

        [Fact]
        public void NotLoadSecretWithNameNotStartingWithPrefix()
        {
            // arrange
            var secretNamePrefix = "Prefix";
            var badPrefix = "BadPrefix";
            var secretManager = new PrefixedKeyVaultSecretManager(secretNamePrefix);
            var secret = new SecretItem($"https://keyVaultEndpoint/secrets/{badPrefix}--SecretName");

            // act
            bool shouldLoad = secretManager.Load(secret);

            // assert
            shouldLoad.Should().BeFalse();
        }

        [Fact]
        public void RemovePrefixFromKeyName1()
        {
            // arrange
            var secretNamePrefix = "Prefix";
            var secretName = "SecretName";
            var secretManager = new PrefixedKeyVaultSecretManager(secretNamePrefix);
            var secret = new SecretBundle(id: $"https://keyVaultEndpoint/secrets/{secretNamePrefix}--{secretName}");

            // act
            string keyName = secretManager.GetKey(secret);

            // assert
            keyName.Should().BeEquivalentTo(secretName);
        }

        [Fact]
        public void RemovePrefixFromKeyName2()
        {
            // arrange
            var secretNamePrefix = "Prefix";
            var prefixToRemove = secretNamePrefix + "--";
            var secretName = "SecretName";
            var secretManager = new PrefixedKeyVaultSecretManager(prefixToRemove);
            var secret = new SecretBundle(id: $"https://keyVaultEndpoint/secrets/{secretNamePrefix}--{secretName}");

            // act
            string keyName = secretManager.GetKey(secret);

            // assert
            keyName.Should().BeEquivalentTo(secretName);
        }

        [Fact]
        public void RemovePrefixFromKeyName3()
        {
            // arrange
            var secretNamePrefix = "Prefix";
            var secretName = "SecretName";
            var prefixToRemove = "";
            var secretManager = new PrefixedKeyVaultSecretManager(prefixToRemove);
            var secret = new SecretBundle(id: $"https://keyVaultEndpoint/secrets/{secretNamePrefix}--{secretName}");

            // act
            string keyName = secretManager.GetKey(secret);

            // assert
            keyName.Should().BeEquivalentTo($"{secretNamePrefix}:{secretName}");
        }
    }
}
