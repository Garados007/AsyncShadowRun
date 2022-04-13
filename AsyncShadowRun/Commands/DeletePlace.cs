using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class DeletePlace : CommandBase
{
    public DeletePlace(Program program)
        : base(program)
    {}

    public override string Name => "delete-place";

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

        var conf = Config.AutoChatRoom.Rooms
            .Where(x => x.RoomName == command.Data.Options.First().Value.ToString())
            .FirstOrDefault();
        if (conf is null)
        {
            await command.RespondAsync(
                $"Room {command.Data.Options.First().Value.ToString()} not found",
                ephemeral: true
            );
            return;
        }

        if (Config.GuildId is not ulong guildId)
        {
            await command.RespondAsync(
                $"Bot needs setup",
                ephemeral: true
            );
            return;
        }
        var guild = Client.GetGuild(guildId);

        var role = guild.GetRole(conf.RoleId);
        if (role is not null)
            await role.DeleteAsync();

        var channel = guild.GetChannel(conf.RoomId);
        if (channel is not null)
            await channel.DeleteAsync();
        
        Config.AutoChatRoom.Rooms.Remove(conf);
        await SaveConfigAsync();

        await new Tools.AutoRoleForRoom(Config, Client).RepopulateMessages();

        await command.RespondAsync(
            $"Place {conf.Name} was deleted."
        );
    }

    public override async Task AutoComplete(SocketAutocompleteInteraction interaction)
    {
        await interaction.RespondAsync(
            Config.AutoChatRoom.Rooms
                .Where(x => x.RoomName.StartsWith(interaction.Data.Current.Value?.ToString() ?? ""))
                .Select(
                    x => new AutocompleteResult(x.Name, x.RoomName)
                )
        );
    }

    public override SlashCommandProperties GetSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Delete a room that was created with `/create-place`")
            .AddOption(
                name: "name",
                type: ApplicationCommandOptionType.String,
                description: "The name of the channel to be deleted",
                isRequired: true,
                isAutocomplete: true,
                channelTypes: new List<ChannelType> { ChannelType.Text }
            )
            .Build();
    }
}