using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Sample.Observability;

namespace Sample.Observability
{
    internal class ApplicationInsightsAdapter : ICoreTelemetry
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsAdapter(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public ICoreTelemetrySpan Start(string name)
        {
            return ApplicationInsightsSpanAdapter.StartSpan(this.telemetryClient, name);
        }

        private class ApplicationInsightsSpanAdapter : ICoreTelemetrySpan
        {
            private readonly Activity activity;
            private TelemetryClient telemetryClient;

            private IOperationHolder<DependencyTelemetry> request;

            public static ApplicationInsightsSpanAdapter StartSpan(TelemetryClient telemetryClient, string name)
            {
                return new ApplicationInsightsSpanAdapter(telemetryClient, name);
            }

            private ApplicationInsightsSpanAdapter(TelemetryClient telemetryClient, string name)
            {
                this.activity = new Activity(name);

                if (Activity.Current != null)
                {
                    this.activity.SetParentId(Activity.Current.Id);
                }

                this.telemetryClient = telemetryClient;

                this.request = this.telemetryClient.StartOperation<DependencyTelemetry>(activity);
                this.request.Telemetry.Type = name;
            }

            public void SetTag(string key, object value)
            {
                this.request.Telemetry.Properties.Add(key, value.ToString());
            }

            public void SetBaggage(string key, string value)
            {
                Activity.Current?.SetBaggage(key, value);
            }

            public void Dispose()
            {
                this.request.Dispose();
                this.activity.Dispose();
            }
        }
    }
}