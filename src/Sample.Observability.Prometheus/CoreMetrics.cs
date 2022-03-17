using System;
using Prometheus;

namespace Sample.Observability
{
    internal class CoreMetrics : ICoreMetrics
    {
        private readonly Gauge applicationInfoCounter = Metrics.CreateGauge(
            "app_info",
            "Basic application runtime information",
            "version", "description");

        private readonly Counter totalExceptions = Metrics.CreateCounter(
            "sample_exceptions_total",
            "The total number of requests serviced.",
            "exception_type");

        public void ApplicationInfo()
        {
            this.applicationInfoCounter
                .WithLabels(System.Environment.Version.ToString(), System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Set(1);
        }

        public void OnException(Exception exception)
        {
            this.totalExceptions
                .WithLabels(exception.GetType().Name)
                .Inc();
        }
    }
}