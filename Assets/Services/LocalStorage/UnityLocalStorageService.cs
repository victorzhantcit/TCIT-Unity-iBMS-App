using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace iBMSApp.Services
{
    public class UnityLocalStorageService : ILocalStorageService
    {
        public ValueTask ClearAsync(CancellationToken cancellationToken = default)
        {
            // 實作清除 PlayerPrefs
            UnityEngine.PlayerPrefs.DeleteAll();
            return new ValueTask();
        }

        public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return new ValueTask<bool>(UnityEngine.PlayerPrefs.HasKey(key));
        }

        public ValueTask<string> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default)
        {
            string result = UnityEngine.PlayerPrefs.GetString(key, "");
            return new ValueTask<string>(result);
        }

#nullable enable
        public ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            string json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json))
                return new ValueTask<T?>(Task.FromResult<T?>(default));

            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                return new ValueTask<T?>(Task.FromResult<T?>(result));
            }
            catch
            {
                return new ValueTask<T?>(Task.FromResult<T?>(default));
            }
        }
#nullable disable


        public ValueTask<string> KeyAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException(); // Unity 沒有暴露 key 索引
        }

        public ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException(); // Unity 沒有暴露所有 key
        }

        public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
            return new ValueTask();
        }

        public ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                UnityEngine.PlayerPrefs.DeleteKey(key);
            }
            return new ValueTask();
        }

        public ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default)
        {
            UnityEngine.PlayerPrefs.SetString(key, data);
            UnityEngine.PlayerPrefs.Save();
            return new ValueTask();
        }

        public ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(data);
            UnityEngine.PlayerPrefs.SetString(key, json);
            UnityEngine.PlayerPrefs.Save();
            return new ValueTask();
        }
    }
}