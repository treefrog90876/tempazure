using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Observability
{
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection AddCoreTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddSingleton<ICoreTelemetry, ApplicationInsightsAdapter>();

            return services;
        }
    }
}