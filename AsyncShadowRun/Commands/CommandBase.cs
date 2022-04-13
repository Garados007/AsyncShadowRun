using System;
using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public abstract class CommandBase
{
    protected Config Config { get; }
    protected DiscordSocketClient Client { get; }

    public CommandBase(Program program)
    {
        Config = program.Config;
        Client = program.Client;
    }

    protected async Task SaveConfigAsync()
    {
        await Config.WriteTo("config.json");
    }

    public abstract string Name { get; }

    public abstract SlashCommandProperties GetSlashCommand();

    public abstract Task Execute(SocketSlashCommand command);

    public virtual Task AutoComplete(SocketAutocompleteInteraction interaction)
    {
        return Task.CompletedTask;
    }
}