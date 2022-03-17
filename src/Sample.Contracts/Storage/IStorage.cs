using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sample.Storage
{
    public interface IStorage
    {
        IAsyncEnumerable<string> GetIdentifiersAsync();

        Task CreateAsync(string key, Stream value);

        Task<Stream> GetAsync(string key);

        Task RemoveAsync(string key);
    }
}
