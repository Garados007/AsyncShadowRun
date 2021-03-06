using System;
using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class CreatePlace : CommandBase
{
    public CreatePlace(Program program)
        : base(program)
    {}

    public override string Name => "create-place";

    public override SlashCommandProperties GetSlashCommand()
    {
        var command = new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Create now a room and associated role for a new place")
            .AddOption(
                name: "name",
                type: ApplicationCommandOptionType.String,
                description: "The name of the place to be created",
                isRequired: true
            );
        return command.Build();
    }

    public override async Task Execute(SocketSlashCommand command)
    {
        var name = command.Data.Options.First().Value.ToString();
        var channelName = name is null ? name : Tools.Namings.FormatChannelName(name);

        if (Config.LeaderRole is ulong leader && 
            !((command.User as IGuildUser)?.RoleIds.Contains(leader) ?? false)
        )
        {
            await command.RespondAsync(
                $"You are not allowed to use this command. Only <@&{leader}> can do this!"
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(channelName))
        {
            await command.RespondAsync(
                $"Invalid name: {name}",
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
        await command.DeferAsync();

        var guild = Client.GetGuild(guildId);
        var role = await guild.CreateRoleAsync(
            name: $"Ort: {name}"
        );
        var overwrites = new List<Overwrite>();
        if (Config.LeaderRole is ulong lid)
            overwrites.Add(new Overwrite(lid, PermissionTarget.Role,
                OverwritePermissions.InheritAll.Modify(
                    viewChannel: PermValue.Allow
                )
            ));
        if (Config.BotsRole is ulong bid)
            overwrites.Add(new Overwrite(bid, PermissionTarget.Role,
                OverwritePermissions.InheritAll.Modify(
                    viewChannel: PermValue.Allow
                )
            ));
        overwrites.Add(new Overwrite(role.Id, PermissionTarget.Role,
            OverwritePermissions.InheritAll.Modify(
                viewChannel: PermValue.Allow
            )
        ));
        overwrites.Add(new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
            OverwritePermissions.InheritAll.Modify(
                viewChannel: PermValue.Deny
            )
        ));

        var channel = await guild.CreateTextChannelAsync(
            name: channelName,
            func: x =>
            {
                x.PermissionOverwrites = Optional.Create<IEnumerable<Overwrite>>(
                    overwrites
                );
                if (Config.AutoChatRoom.Category is ulong id)
                    x.CategoryId = id;
                x.Topic = Optional.Create($"Ort: {name}");
            }
        );
        
        Config.AutoChatRoom.Rooms.Add(new Data.AutoChatRoom
        {
            Name = name ?? "",
            RoomName = channel.Name,
            RoleId = role.Id,
            RoomId = channel.Id,
        });
        await SaveConfigAsync();
        await command.ModifyOriginalResponseAsync(
            mes => 
            {
                mes.Embed = new EmbedBuilder()
                    .WithAuthor(
                        new EmbedAuthorBuilder()
                            .WithName(command.User.Username)
                            .WithIconUrl(command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    )
                    .WithTitle($"Ort {name} wurde erstellt")
                    .WithDescription(
                        $"Chat: <#{channel.Id}>\nRolle: <@&{role.Id}>"
                    )
                    .Build();
                mes.AllowedMentions = AllowedMentions.None;
            }
        );

        await new Tools.AutoRoleForRoom(Config, Client).RepopulateMessages();
    }
}