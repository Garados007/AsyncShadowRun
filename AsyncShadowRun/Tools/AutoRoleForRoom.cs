using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Tools;

public class AutoRoleForRoom
{
    private static readonly ReadOnlyMemory<string> EmoteNames = new []
    {
        ":one:",
        ":two:",
        ":three:",
        ":four:",
        ":five:",
        ":six:",
        ":seven:",
        ":eight:",
        ":nine:",
        ":keycap_ten:"
    };

    private static readonly IEmote[] Emotes = GenerateEmotes(EmoteNames);

    private static IEmote[] GenerateEmotes(ReadOnlyMemory<string> names)
    {
        var result = new IEmote[names.Length];
        for (int i = 0; i < names.Length; ++i)
            result[i] = Emoji.TryParse(names.Span[i], out Emoji e)
                ? e : throw new FormatException($"Emoji {names.Span[i]} cannot be parsed");
        return result;
    }

    public AutoRoleForRoom(Config config, DiscordSocketClient client)
    {
        Config = config;
        Client = client;
    }

    public Config Config { get; }

    public DiscordSocketClient Client { get; }

    public async Task RepopulateMessages()
    {
        if (Config.AutoChatRoom.AutoRoleRoom is not ulong id)
            return;
        
        var postChannel = await Client.GetChannelAsync(id) as ITextChannel;
        if (postChannel is null)
            return;

        var roomChunks = CreateChunks(10, Config.AutoChatRoom.Rooms.Select(x => x.Name));
        var messages = new List<IUserMessage>();
        foreach (var mid in Config.AutoChatRoom.RoomMsg.Keys)
        {
            var message = await postChannel.GetMessageAsync(mid) as IUserMessage;
            if (message is null)
            {
                Config.AutoChatRoom.RoomMsg.Remove(mid);
            }
            else
            {
                messages.Add(message);
            }
        }

        // create contents
        var contents = roomChunks
            .Select(x =>
            {
                return (
                    embed: new EmbedBuilder()
                        .WithAuthor(Client.CurrentUser)
                        .WithDescription(
                            string.Join("\n",
                                x.Select((v, i) => $"{EmoteNames.Span[i]} {v}")
                            )
                        )
                        .Build(),
                    names: x
                );
            })
            .ToList();

        // delete obsolete messages
        for (int i = roomChunks.Count; i < messages.Count; ++i)
        {
            Config.AutoChatRoom.RoomMsg.Remove(messages[i].Id);
            await messages[i].DeleteAsync();
        }

        // update or send messages
        for (int i = 0; i < contents.Count; ++i)
        {
            IUserMessage message;
            if (i < messages.Count)
            {
                await messages[i].ModifyAsync(x =>
                {
                    x.Embed = contents[i].embed;
                });
                message = messages[i];
            }
            else
            {
                message = await postChannel.SendMessageAsync(
                    embed: contents[i].embed,
                    allowedMentions: AllowedMentions.None
                );
            }
            Config.AutoChatRoom.RoomMsg[message.Id] = new Data.AutoChatMsg
            {
                MessageId = message.Id,
                RoomNames = contents[i].names,
            };
            var allowedReactions = Emotes.Take(contents[i].names.Length);
            foreach (var (emote, _) in message.Reactions)
                if (!allowedReactions.Contains(emote))
                    await message.RemoveAllReactionsForEmoteAsync(emote);
            await message.AddReactionsAsync(allowedReactions.Where(
                x => !message.Reactions.ContainsKey(x)
            ));
        }

        await Config.Save();
    }

    public async Task HandleReaction(SocketReaction reaction)
    {
        if (reaction.UserId == Client.CurrentUser.Id)
            return;
        
        try
        {
            // get emote
            var index = Array.IndexOf(Emotes, reaction.Emote);
            if (index < 0)
                return;
            
            // get name
            if (!Config.AutoChatRoom.RoomMsg.TryGetValue(reaction.MessageId, out Data.AutoChatMsg? msg))
                return;
            if (index >= msg.RoomNames.Length)
                return;
            
            // get room info
            var room = Config.AutoChatRoom.Rooms
                .Where(x => x.Name == msg.RoomNames[index])
                .FirstOrDefault();
            if (room is null)
                return;

            // get user
            var user = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
            if (user is null)
                return;
            var hadRole = false;
            foreach (var existingRoom in Config.AutoChatRoom.Rooms.Where(x => user.RoleIds.Contains(x.RoleId)))
            {
                if (existingRoom.RoomId == room.RoomId)
                {
                    hadRole = true;
                    continue;
                }
                var discordRoom = await Client.GetChannelAsync(existingRoom.RoomId);
                if (discordRoom is SocketTextChannel socketTextChannel)
                    await socketTextChannel.SendMessageAsync(
                        embed: new EmbedBuilder()
                            .WithTitle($"{user.Nickname} left the place")
                            .WithColor(Color.Red)
                            .Build()
                    );
            }
            await user.RemoveRolesAsync(Config.AutoChatRoom.Rooms
                .Select(x => x.RoleId)
                .Where(user.RoleIds.Contains)
            );
            await user.AddRoleAsync(room.RoleId);
            if (!hadRole && await Client.GetChannelAsync(room.RoomId) is SocketTextChannel textChannel)
                await textChannel.SendMessageAsync(
                    embed: new EmbedBuilder()
                        .WithTitle($"{user.Nickname} entered the place")
                        .WithColor(Color.Green)
                        .Build()
                );
        }
        finally
        {
            var message = await reaction.Channel.GetMessageAsync(reaction.MessageId);
            await message.RemoveReactionAsync(reaction.Emote, reaction.UserId);
        }
    }

    private List<T[]> CreateChunks<T>(int size, IEnumerable<T> items)
    {
        var result = new List<T[]>();
        var chunk = new List<T>(size);
        foreach (var item in items)
        {
            if (chunk.Count >= size)
            {
                result.Add(chunk.ToArray());
                chunk.Clear();
            }
            chunk.Add(item);
        }
        if (chunk.Count > 0)
            result.Add(chunk.ToArray());
        return result;
    }
}