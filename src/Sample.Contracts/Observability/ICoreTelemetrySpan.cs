using System;

namespace Sample.Observability
{
    public interface ICoreTelemetrySpan : IDisposable
    {
        void SetTag(string key, object value);

        void SetBaggage(string key, string value);
    }
}
