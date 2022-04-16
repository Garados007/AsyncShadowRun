using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun.Data;

public abstract class JsonData<T>
    where T : JsonData<T>, new()
{
    [JsonIgnore]
    public string? Path { get; private set; }

    protected void SetPath(string path)
    {
        if (Path is not null && path != Path)
            throw new NotSupportedException("path already set");
        Path = path;
    }

    private static readonly SourceGenerationContext Context = 
        new(new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        });

    protected abstract JsonTypeInfo<T> GetJsonTypeInfo(SourceGenerationContext ctx);

    public static async ValueTask<T?> LoadFrom(string file)
    {
        if (!File.Exists(file))
            return null;
        using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        var value = await JsonSerializer.DeserializeAsync<T>(
            stream,
            new T().GetJsonTypeInfo(Context)
        );
        if (value is not null)
            value.Path = file;
        return value;
    }

    public async Task Save()
    {
        await WriteTo(Path ?? throw new NotSupportedException());
    }

    public virtual async Task WriteTo(string file)
    {
        var dir = System.IO.Path.GetDirectoryName(file);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);
        using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializer.SerializeAsync<T>(stream, (T)this, jsonTypeInfo: GetJsonTypeInfo(Context));
        await stream.FlushAsync();
        stream.SetLength(stream.Position);
        Path = file;
    }
}