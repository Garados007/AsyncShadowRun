using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace AsyncShadowRun.Data;

public abstract class BufferedJson<T> : JsonData<T>
    where T : BufferedJson<T>, new()
{
    [JsonIgnore]
    private string? Key { get; set; }

    private static readonly ConcurrentDictionary<string, T?> buffer
        = new ConcurrentDictionary<string, T?>();
    private static readonly SemaphoreSlim loadLock
        = new SemaphoreSlim(1, 1);

    protected virtual string GetRoot()
        => $"data/{typeof(T).Name}";
    
    public static async ValueTask<T?> Load(string key)
    {
        if (buffer.TryGetValue(key, out T? value))
            return value;
        
        await loadLock.WaitAsync();
        try
        {
            value = await LoadFrom($"{new T().GetRoot()}/{key}.json");
        }
        finally { loadLock.Release(); }

        buffer.AddOrUpdate(key, value, (_, _) => value);
        if (value is not null)
            value.Key = key;
        return value;
    }

    public static async ValueTask LoadAll()
    {
        var root = new T().GetRoot();
        if (!Directory.Exists(root))
            return;
        foreach (var file in Directory.EnumerateFiles(root, "*.json"))
        {
            await Load(System.IO.Path.GetFileNameWithoutExtension(file));
        }
    }

    public static IEnumerable<(string, T)> EnumerateCachedData()
    {
        foreach (var (key, value) in buffer)
            if (value is not null)
                yield return (key, value);
    }

    public void SetBufferKey(string key)
    {
        if (Key is not null && Key != key)
            throw new NotSupportedException("key already set");
        Key = key;
        SetPath($"{new T().GetRoot()}/{key}.json");
    }

    public override async Task WriteTo(string file)
    {
        await base.WriteTo(file);
        if (Key is not null)
            buffer.AddOrUpdate(Key, (T)this, (_, _) => (T)this);
    }
}