using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class Reload : CommandBase
{
    public Reload(Program program) : base(program)
    {
    }

    public override string Name => "reload";

    public override async Task Execute(SocketSlashCommand command)
    {
        if (Config.LeaderRole is ulong leader && 
            !((command.User as IGuildUser)?.RoleIds.Contains(leader) ?? false)
        )
        {
            await command.RespondAsync(
                $"You are not allowed to use this command. Only <@&{leader}> can do this!"
            );
            return;
        }

        await Config.Reload();

        await command.RespondAsync(
            embed: new EmbedBuilder()
                .WithDescription("Configuration reloaded")
                .WithColor(Color.Green)
                .Build()
        );
    }

    public override SlashCommandProperties GetSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Reload internal configuration")
            .Build();
    }
}