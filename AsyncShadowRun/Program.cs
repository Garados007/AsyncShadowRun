using System;
using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun;

public class Program
{
    public Config Config { get; }
    public DiscordSocketClient Client { get; }
    public Commands.CommandBase[] Commands { get; }


    public Program(Config config, DiscordSocketClient client)
    {
        Config = config;
        Client = client;
        Commands = new Commands.CommandBase[]
        {
            new Commands.CreatePlace(this),
            new Commands.DeletePlace(this),
            new Commands.Roll(this),
        };
    }


    public static async Task Main(string[] args)
    {
        var config = await Config.LoadFrom("data/config.json");
        if (config is null)
        {
            config ??= new Config();
            await config.Save();
            Console.WriteLine("Missing config");
            return;
        }
        await config.Save();

        using var client = new DiscordSocketClient();
        var program = new Program(config, client);

        client.Log += Log;
        client.Ready += program.ClientReady;
        client.SlashCommandExecuted += program.SlashCommandHandler;
        client.ReactionAdded += program.ReactionAddedHandler;
        client.AutocompleteExecuted += program.AutoCompleteHandler;
        // client.AutocompleteExecuted += x => Task.Run(() => Console.WriteLine(ToJson(x.Data)));
        // client.InteractionCreated += x => Task.Run(() => Console.WriteLine(ToJson(x.Data)));

        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();
        await Task.Delay(-1);
    }

    private static async Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        await Task.CompletedTask;
    }

    private async Task ClientReady()
    {
        if (Config.GuildId is not ulong guildId)
            return;
        var guild = Client.GetGuild(guildId);

        try
        {
            for (int i = 0; i < Commands.Length; ++i)
                await guild.CreateApplicationCommandAsync(Commands[i].GetSlashCommand());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        for (int i = 0; i < Commands.Length; ++i)
            if (Commands[i].Name == command.Data.Name)
            {
                await Commands[i].Execute(command);
                return;
            }
        await command.RespondAsync(
            text: $"Unknown command `{command.Data.Name}`",
            ephemeral: true
        );
    }

    private async Task ReactionAddedHandler(
        Cacheable<IUserMessage, ulong> message, 
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        if (Config.AutoChatRoom.RoomMsg.ContainsKey(reaction.MessageId))
        {
            await new Tools.AutoRoleForRoom(Config, Client).HandleReaction(reaction);
            return;
        }
    }

    private async Task AutoCompleteHandler(SocketAutocompleteInteraction interaction)
    {
        for (int i = 0; i < Commands.Length; ++i)
            if (Commands[i].Name == interaction.Data.CommandName)
            {
                await Commands[i].AutoComplete(interaction);
                return;
            }
    }

    public static string ToJson<T>(T value)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            new { type = typeof(T).FullName, value = value }, 
            new System.Text.Json.JsonSerializerOptions 
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, 
            }
        );
        return json;

    }

}