using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable
namespace iBMSApp.Services
{
    public class UnityDbAccessor
    {
        private readonly string _basePath;

        // File path 對應的鎖定物件
        private static readonly Dictionary<string, SemaphoreSlim> _fileLocks = new();
        private static readonly string CursorIdProperty = "CursorId";

        public UnityDbAccessor()
        {
            _basePath = Application.persistentDataPath;
        }

        private string GetTablePath(string tableName)
        {
            string path = Path.Combine(_basePath, tableName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetItemPath(string tableName, string key)
        {
            return Path.Combine(GetTablePath(tableName), $"{key}.json");
        }

        private SemaphoreSlim GetLockForFile(string path)
        {
            lock (_fileLocks)
            {
                if (!_fileLocks.TryGetValue(path, out var sem))
                {
                    sem = new SemaphoreSlim(1, 1);
                    _fileLocks[path] = sem;
                }
                return sem;
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task createDB<T>(string dbName, int version, T tables) => Task.CompletedTask;

        public async Task<T?> GetValueAsync<T>(string tableName, string key)
        {
            string path = GetItemPath(tableName, key);
            if (!File.Exists(path)) return default;
            string json = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task SetCursorValueAsync(string tableName, Dictionary<string, string> data)
        {
            string key = Guid.NewGuid().ToString();

            data[CursorIdProperty] = key;

            string path = GetItemPath(tableName, key);
            string json = JsonConvert.SerializeObject(data);

            var sem = GetLockForFile(path);
            await sem.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(path, json);
            }
            finally
            {
                sem.Release();
            }
        }

        public async Task DeleteCursorTaskAsync(string tableName, Dictionary<string, string> cursorTask)
        {
            if (!cursorTask.TryGetValue(CursorIdProperty, out string key))
            {
                string trackedTime = cursorTask.TryGetValue("NowTime", out string cursorTime) ? cursorTime : "untracked";
                throw new ArgumentException($"CursorTask trackedTime = {trackedTime}, has no key \"CursorId\"");
            }
            await DeleteDataAsync(tableName, key);
        }

        public async Task<Dictionary<string, string>> GetCursorValueAsync(string tableName)
        {
            List<Dictionary<string, string>> results = await GetAllDataAsync<Dictionary<string, string>>(tableName);
            return results
                .OrderBy(item =>
                {
                    if (item.TryGetValue("NowTime", out string? nowTimeStr) &&
                        DateTime.TryParseExact(nowTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        return dt;
                    }
                    return DateTime.MaxValue; // 若無法轉換，就排在最後
                })
                .FirstOrDefault();
        }

        public async Task<List<T>> GetAllDataAsync<T>(string tableName)
        {
            string folder = GetTablePath(tableName);
            var results = new List<T>();
            foreach (string file in Directory.GetFiles(folder, "*.json"))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file);
                    var item = JsonConvert.DeserializeObject<T>(json);
                    if (item != null)
                        results.Add(item);
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"讀取檔案失敗：{file}，錯誤：{ex.Message}");
                }
            }
            return results;
        }

        public Task<int> GetAllDataCountAsync(string tableName)
        {
            string folder = GetTablePath(tableName);
            int count = Directory.GetFiles(folder, "*.json").Length;
            return Task.FromResult(count);
        }

        /// <summary>
        /// 將資料存入本地 DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="value">Property: Sn 必須存在，否則會亂數產生 Key 值，可能無法精準追蹤</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SetValueAsync<T>(string tableName, T value)
        {
            string key = value?.GetType().GetProperty("Sn")?.GetValue(value)?.ToString() ?? Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Value must have Sn property or provide key");
            string path = GetItemPath(tableName, key);
            string json = JsonConvert.SerializeObject(value);

            var sem = GetLockForFile(path);
            await sem.WaitAsync();
            try
            {
                await File.WriteAllTextAsync(path, json);
            }
            finally
            {
                sem.Release();
            }
        }

        /// <summary>
        /// 以清單的方式存入所有資料 (會先清空 table)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public async Task SetAllValueAsync<T>(string tableName, IEnumerable<T> dataList)
        {
            await ClearDataAsync(tableName);
            foreach (var data in dataList)
            {
                await SetValueAsync(tableName, data);
            }
        }

        public async Task DeleteDataAsync(string tableName, string key)
        {
            string path = GetItemPath(tableName, key ?? "");
            if (!File.Exists(path)) return;

            var sem = GetLockForFile(path);
            await sem.WaitAsync();
            try
            {
                File.Delete(path);
            }
            finally
            {
                sem.Release();
            }
        }

        public async Task ClearDataAsync(string tableName)
        {
            string folder = GetTablePath(tableName);
            foreach (string file in Directory.GetFiles(folder, "*.json"))
            {
                var sem = GetLockForFile(file);
                await sem.WaitAsync();
                try
                {
                    File.Delete(file);
                }
                finally
                {
                    sem.Release();
                }
            }
        }
    }
}
