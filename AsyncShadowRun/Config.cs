using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun;

public class Config : Data.JsonData<Config>
{
    public string Token { get; set; } = "";

    /// <summary>
    /// There is only one guild for this bot.
    /// </summary>
    public ulong? GuildId { get; set; }

    public ulong? LeaderRole { get; set; }

    public ulong? BotsRole { get; set; }

    public ulong? PlayerRole { get; set; }

    public Configs.AutoChatRoomConfig AutoChatRoom { get; set; }
        = new Configs.AutoChatRoomConfig();

    public Configs.KommlinkConfig Kommlink { get; set; }
        = new Configs.KommlinkConfig();
    
    public Configs.TwitterConfig Twitter { get; set; } = new();

    public async Task Reload()
    {
        var conf = await LoadFrom(Path ?? throw new InvalidOperationException());
        if (conf is null)
            return;
        Token = conf.Token;
        GuildId = conf.GuildId;
        LeaderRole = conf.LeaderRole;
        BotsRole = conf.BotsRole;
        PlayerRole = conf.PlayerRole;
        AutoChatRoom = conf.AutoChatRoom;
        Kommlink = conf.Kommlink;
        Twitter = conf.Twitter;
    }

    protected override JsonTypeInfo<Config> GetJsonTypeInfo(SourceGenerationContext ctx)
    {
        return ctx.Config;
    }
}