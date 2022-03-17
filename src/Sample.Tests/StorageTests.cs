using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sample.Storage;
using Xunit;

namespace Sample.Tests
{
    public class StorageTests : IClassFixture<StorageProvider>
    {
        private readonly IStorage storage;

        public StorageTests(StorageProvider storageProvider)
        {
            this.storage = storageProvider?.Storage;
        }

        [Fact]
        public async Task TestKeysAsync()
        {
            const int KeyCount = 10;

            for (int i = 0; i < KeyCount; ++i)
            {
                await AddRandomDataToStorageAsync(this.storage, $"key:{i}");
            }

            var keys = await storage.GetIdentifiersAsync().ToListAsync();

            var allKeys = string.Join(",", keys);

            Console.WriteLine(allKeys);

            Assert.Equal(KeyCount, keys.Count);
        }

        [Fact]
        public async Task TestValuesAsync()
        {
            const int ValueCount = 10;
            var validationDictionary = new Dictionary<string, byte[]>();

            for (int i = 0; i < ValueCount; ++i)
            {
                var key = $"key:{i}";
                var byteBuffer = await AddRandomDataToStorageAsync(this.storage, key);
                validationDictionary.Add(key, byteBuffer);
            }

            var allKeys = validationDictionary.Keys.ToList();
            foreach (var key in allKeys)
            {
                var stream = await storage.GetAsync(key);
                var byteBuffer = await StreamToBytesAsync(stream);

                Assert.True(validationDictionary[key].SequenceEqual(byteBuffer));

                validationDictionary.Remove(key);
            }

            var keys = await storage.GetIdentifiersAsync().ToListAsync();
            Assert.Equal(ValueCount, keys.Count);
        }

        [Fact]
        public async Task TestUpdateAsync()
        {
            var key = "testkey";

            var byteBuffer1 = await AddRandomDataToStorageAsync(this.storage, key);
            var keys1 = await storage.GetIdentifiersAsync().ToListAsync();
            Assert.Single(keys1);

            var storedByteBuffer1 = await StreamToBytesAsync(await storage.GetAsync(key));
            Assert.True(byteBuffer1.SequenceEqual(storedByteBuffer1));

            var byteBuffer2 = await AddRandomDataToStorageAsync(this.storage, key);
            var keys2 = await storage.GetIdentifiersAsync().ToListAsync();
            Assert.Single(keys2);

            var storedByteBuffer2 = await StreamToBytesAsync(await storage.GetAsync(key));
            Assert.True(byteBuffer2.SequenceEqual(storedByteBuffer2));

            Assert.False(byteBuffer1.SequenceEqual(byteBuffer2));
        }

        [Fact]
        public async Task TestRemoveAsync()
        {
            var key = "testkey";

            var byteBuffer1 = await AddRandomDataToStorageAsync(this.storage, key);
            var keys1 = await storage.GetIdentifiersAsync().ToListAsync();
            Assert.Single(keys1);

            var storedByteBuffer1 = await StreamToBytesAsync(await storage.GetAsync(key));
            Assert.True(byteBuffer1.SequenceEqual(storedByteBuffer1));

            await storage.RemoveAsync(key);

            var keys2 = await storage.GetIdentifiersAsync().ToListAsync();
            Assert.Empty(keys2);
        }

        private static async Task<byte[]> AddRandomDataToStorageAsync(IStorage storage, string key, int dataSize = 100)
        {
            var bytes = new byte[dataSize];
            new Random().NextBytes(bytes);

            using var stream = new MemoryStream(bytes);

            await storage.CreateAsync(key, stream);

            return bytes;
        }

        private static async Task<byte[]> StreamToBytesAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        private static string BytesToString(Memory<byte> bytes)
        {
            return BitConverter.ToString(bytes.ToArray());
        }
    }
}
