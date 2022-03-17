using System;

namespace Sample.Observability
{
    public interface ICoreTelemetry
    {
        ICoreTelemetrySpan Start(string name);
    }
}
