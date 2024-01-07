using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.UnitTest.Helpers
{
    public class MockSession:ISession
    {
        private Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public byte[] this[string key]
        {
            get => _sessionStorage.TryGetValue(key, out var value) ? value : null;
            set => _sessionStorage[key] = value;
        }

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public string Id => throw new NotImplementedException();

        public bool IsAvailable => true;

        public IEnumerable<KeyValuePair<string, byte[]>> GetStoredData()
        {
            return _sessionStorage.ToList();
        }

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public void Load(IEnumerable<KeyValuePair<string, byte[]>> storedData)
        {
            _sessionStorage = storedData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // Perform any necessary commit operations (if needed)
            return Task.CompletedTask;
        }
        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            // Perform any necessary load operations (if needed)
            return Task.CompletedTask;
        }
        public bool TryGetValue(string key, out byte[] value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }
    }
}
