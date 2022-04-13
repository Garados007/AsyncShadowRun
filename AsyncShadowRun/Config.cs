using System;
using System.IO;
using System.Text.Json;

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
        return await JsonSerializer.DeserializeAsync<Config>(stream, Options);
    }

    public async Task WriteTo(string file)
    {
        using var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, this, Options);
        await stream.FlushAsync();
        stream.SetLength(stream.Position);
    }
}