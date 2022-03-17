using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Sample
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync().ConfigureAwait(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.Build();
                    var keyVaultName = builtConfig["KeyVaultNameFromDeployment"];

                    if (!string.IsNullOrEmpty(keyVaultName))
                    {
                        // Since KeyVault is deployed along side our WebApp, it injects the name into appsettings resource
                        // so you will not find this key in any appsettings*.json file.
                        // See 'Configuration -> Application settings' in the Azure Portal
                        config.AddAzureKeyVault(
                            new Uri($"https://{keyVaultName}.vault.azure.net/"),
                            new DefaultAzureCredential());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
