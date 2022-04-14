using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncShadowRun;

public class Config
{
    public string Token { get; set; } = "";

    /// <summary>
    /// There is only one guild for this bot.
    /// </summary>
    public ulong? GuildId { get; set; }

    public ulong? LeaderRole { get; set; }

    public ulong? BotsRole { get; set; }

    [JsonIgnore]
    public string? Path { get; private set; }

    public Configs.AutoChatRoomConfig AutoChatRoom { get; set; }
        = new Configs.AutoChatRoomConfig();

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
    };

    public static async ValueTask<Config?> LoadFrom(string file)
    {
        if (!File.Exists(file))
            return null;
        using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        var value = await JsonSerializer.DeserializeAsync<Config>(stream, Options);
        if (value is not null)
            value.Path = file;
        return value;
    }

    public async Task Save()
    {
        await WriteTo(Path ?? "config.json");
    }

    public async Task WriteTo(string file)
    {
        using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, this, Options);
        await stream.FlushAsync();
        stream.SetLength(stream.Position);
        Path = file;
    }
}