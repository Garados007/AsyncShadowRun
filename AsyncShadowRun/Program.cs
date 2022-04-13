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
        Commands = new[]
        {
            new Commands.CreatePlace(this),
        };
    }


    public static async Task Main(string[] args)
    {
        var config = await Config.LoadFrom("config.json");
        if (config is null)
        {
            config ??= new Config();
            await config.WriteTo("config.json");
            Console.WriteLine("Missing config");
            return;
        }
        await config.WriteTo("config.json");

        using var client = new DiscordSocketClient();
        var program = new Program(config, client);

        client.Log += Log;
        client.Ready += program.ClientReady;
        client.SlashCommandExecuted += program.SlashCommandHandler;

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

}