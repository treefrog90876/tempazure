using System;

namespace Sample.Observability
{
    public class NullCoreTelemetry
    {
        private static readonly NullCoreTelemetrySpan NullSpan = new ();

        public ICoreTelemetrySpan Start(string name)
        {
            return NullSpan;
        }

        private class NullCoreTelemetrySpan : ICoreTelemetrySpan
        {
            public void SetTag(string key, object value)
            {
            }

            public void SetBaggage(string key, string value)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
