using System;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Sample.Functions.Startup))]

namespace Sample.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();
            var keyVaultName = builtConfig["KeyVaultNameFromDeployment"];

            if (!string.IsNullOrEmpty(keyVaultName))
            {
                builder.ConfigurationBuilder.AddAzureKeyVault(
                    new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());
            }
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
        }
    }
}