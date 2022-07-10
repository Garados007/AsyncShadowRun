using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class ResyncKommlink : CommandBase
{
    public ResyncKommlink(Program program) : base(program)
    {
    }

    public override string Name => "resync-kommlink";

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

        if (Config.GuildId is not ulong guildId
            || Config.PlayerRole is not ulong playerId
            || Config.LeaderRole is not ulong leaderId
            || Config.BotsRole is not ulong botsId
            || Config.Kommlink.Group is not ulong groupId
        )
        {
            await command.RespondAsync(
                $"Bot needs setup",
                ephemeral: true
            );
            return;
        }

        var guild = Client.GetGuild(guildId);
        var players = new List<IGuildUser>();
        var playerHash = new HashSet<ulong>();
        foreach (var user in guild.Users)
        {
            if (user is null)
                continue;
            if (user.Roles.Any(x => x.Id == playerId))
            {
                players.Add(user);
                playerHash.Add(user.Id);
                Program.Log($"ResyncKommlink: Found {user.DisplayName} ({user.Id})");
            }
            else Program.Log($"ResyncKommlink: Skip {user.DisplayName} ({user.Id})");
        }
        players.Sort((x, y) => x.Id.CompareTo(y.Id));

        int updated = 0;
        int created = 0;
        int deleted = 0;

        for (int i = 0; i < players.Count; ++i)
        {
            if (!Config.Kommlink.DirectChats.TryGetValue(players[i].Id, out Dictionary<ulong, Data.KommlinkChat>? conf))
                Config.Kommlink.DirectChats.Add(players[i].Id, conf = new Dictionary<ulong, Data.KommlinkChat>());
            for (int j = i + 1; j < players.Count; ++j)
            {
                var name = Tools.Namings.FormatChannelName(
                    $"{players[i].DisplayName}-{players[j].DisplayName}"
                );
                var topic = $"Direct chat with {players[i].DisplayName} and {players[j].DisplayName}";
                Data.KommlinkChat? chat;
                if (conf.TryGetValue(players[j].Id, out chat))
                {
                    var room = guild.GetTextChannel(chat.RoomId);
                    if (room != null)
                    {
                        if (room.Name != name)
                        {
                            await room.ModifyAsync(x => 
                            {
                                x.Name = name;
                                x.Topic = topic;
                            });
                            await command.DeferAsync();
                            updated++;
                        }
                    }
                    else chat = null;
                }
                else chat = null;

                if (chat is null)
                {
                    var room = await guild.CreateTextChannelAsync(
                        name,
                        x =>
                        {
                            x.CategoryId = groupId;
                            x.PermissionOverwrites = new []
                            {
                                new Overwrite(leaderId, PermissionTarget.Role, 
                                    OverwritePermissions.InheritAll.Modify(
                                        viewChannel: PermValue.Allow
                                    )
                                ),
                                new Overwrite(botsId, PermissionTarget.Role, 
                                    OverwritePermissions.InheritAll.Modify(
                                        viewChannel: PermValue.Allow
                                    )
                                ),
                                new Overwrite(players[i].Id, PermissionTarget.User, 
                                    OverwritePermissions.InheritAll.Modify(
                                        viewChannel: PermValue.Allow
                                    )
                                ),
                                new Overwrite(players[j].Id, PermissionTarget.User, 
                                    OverwritePermissions.InheritAll.Modify(
                                        viewChannel: PermValue.Allow
                                    )
                                ),
                                new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, 
                                    OverwritePermissions.InheritAll.Modify(
                                        viewChannel: PermValue.Deny
                                    )
                                ),
                            };
                            x.Topic = topic;
                        }
                    );
                    await command.DeferAsync();
                    conf[players[j].Id] = new Data.KommlinkChat
                    {
                        RoomId = room.Id,
                    };
                    created++;
                }
            }
        
            // check for old rooms that are no longer allowed to exists
            foreach (var pid in conf.Keys.ToArray())
                if (!playerHash.Contains(pid))
                {
                    var room = guild.GetTextChannel(conf[pid].RoomId);
                    conf.Remove(pid);
                    if (room is null)
                        continue;
                    await room.DeleteAsync();
                    await command.DeferAsync();
                    deleted++;
                }
        }

        // check for old rooms that are no longer allowed to exists
        foreach (var pid in Config.Kommlink.DirectChats.Keys.ToArray())
            if (!playerHash.Contains(pid))
            {
                foreach (var (_, info) in Config.Kommlink.DirectChats[pid])
                {
                    var room = guild.GetTextChannel(info.RoomId);
                    if (room is null)
                        continue;
                    await room.DeleteAsync();
                    await command.DeferAsync();
                    deleted++;

                }
                Config.Kommlink.DirectChats.Remove(pid);
            }

        // finish
        var embed = new EmbedBuilder()
            .WithTitle("Kommlinks synced")
            .WithDescription(
                $"Player: {players.Count}\n" + 
                $"Room created: {created}\n" +
                $"Rooms updated: {updated}\n" +
                $"Rooms deleted: {deleted}"
            )
            .Build();
        if (command.HasResponded)
            await command.ModifyOriginalResponseAsync(x =>
            { 
                x.Embed = embed;
                x.Content = "";
            });
        else await command.RespondAsync(embed: embed);

        await SaveConfigAsync();
    }

    public override SlashCommandProperties GetSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Sync all kommlink channels with the current users")
            .Build();
    }
}