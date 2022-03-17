using Microsoft.Extensions.DependencyInjection;
using Sample.Exceptions;
using Sample.Settings;
using Sample.Storage;
using Sample.Storage.Azure;
using Sample.Storage.Azure.Settings;
using Sample.Storage.Memory;

namespace Sample.Extensions
{
    public static class SampleStorageConfigurationExtensions
    {
        public static void AddSampleStorage(this IServiceCollection services, SampleSettings settings)
        {
            Guard.ThrowIfNull(settings, nameof(settings));

            if (settings.Features.UseStorageSimulator)
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
            else
            {
                services.AddSingleton<AzureStorageSettings>(settings.AzureStorageSettings);
                services.AddSingleton<IStorage, AzureBlobStorage>();
            }
        }
    }
}