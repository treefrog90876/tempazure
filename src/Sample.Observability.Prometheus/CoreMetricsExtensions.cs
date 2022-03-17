using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace Sample.Observability
{
    public static class CoreMetricsExtensions
    {
        public static IServiceCollection AddCoreMetrics(this IServiceCollection services, IHealthChecksBuilder healthChecksBuilder = null)
        {
            services.AddSingleton<ICoreMetrics, CoreMetrics>();
            healthChecksBuilder?.ForwardToPrometheus();

            return services;
        }

        public static IApplicationBuilder UseCoreMetricsMiddleware(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseHttpMetrics();
            applicationBuilder.UseMiddleware<ExceptionMetricsMiddleware>();

            return applicationBuilder;
        }

        public static IEndpointRouteBuilder MapCoreMetrics(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapMetrics();

            return endpoints;
        }
    }
}