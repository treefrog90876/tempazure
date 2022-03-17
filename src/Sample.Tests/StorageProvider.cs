using Sample.Storage;
using Sample.Storage.Memory;

namespace Sample.Tests
{
    public class StorageProvider
    {
        public StorageProvider()
        {
            // TODO: Use this class to expose a configurable test.  For example, if we have
            // azure connection string set in config/env variables, use the real azure storage
            // implementation of IStorage
        }

        public IStorage Storage => new MemoryStorage();
    }
}
