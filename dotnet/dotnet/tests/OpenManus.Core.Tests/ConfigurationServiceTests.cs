using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace OpenManus.Core.Tests
{
    public class ConfigurationServiceTests
    {
        private readonly IConfiguration _configuration;

        public ConfigurationServiceTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        [Fact]
        public void Configuration_ShouldLoad_ValidSettings()
        {
            // Arrange
            var expectedValue = "ExpectedValue"; // Replace with actual expected value

            // Act
            var actualValue = _configuration["SomeSettingKey"]; // Replace with actual setting key

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Configuration_ShouldThrow_WhenInvalidJson()
        {
            // Arrange
            var invalidJsonPath = "invalidappsettings.json"; // Path to an invalid JSON file

            // Act & Assert
            var exception = Assert.Throws<FormatException>(() =>
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(invalidJsonPath, optional: false, reloadOnChange: true)
                    .Build();
            });

            Assert.Contains("The JSON value could not be converted", exception.Message);
        }

        // Additional tests can be added here
    }
}