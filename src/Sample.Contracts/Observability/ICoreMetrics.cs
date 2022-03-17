using System;

namespace Sample.Observability
{
    public interface ICoreMetrics
    {
        void ApplicationInfo();

        void OnException(Exception exception);
    }
}
