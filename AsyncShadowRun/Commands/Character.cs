using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class Character : CommandBase
{
    public Character(Program program) : base(program)
    {
    }

    public override string Name => "character";

    public override async Task Execute(SocketSlashCommand command)
    {
        var first = command.Data.Options.First();
        switch (first.Name)
        {
            case "sys":
                await ExecuteSys(command, first);
                break;
            default:
                await command.RespondAsync($"Unknown group `{first.Name}`");
                break;
        }
    }

    private async Task ExecuteSys(SocketSlashCommand command, SocketSlashCommandDataOption option)
    {
        var first = option.Options.First();
        switch (first.Name)
        {
            case "create":
                await ExecuteSysCreate(command, first);
                break;
            
            default:
                await command.RespondAsync($"Unknown command: `sys {first.Name}`");
                break;
        }
    }

    private async Task ExecuteSysCreate(SocketSlashCommand command, SocketSlashCommandDataOption option)
    {
        IUser? owner = null;
        string? name = null;
        foreach (var opt in option.Options)
            switch (opt.Name)
            {
                case "owner": owner = opt.Value as IUser; break;
                case "name": name = opt.Value as string; break;
                default:
                    await command.RespondAsync($"Unknown option `{opt.Name}`");
                    return;
            }
        
        if (owner is null || name is null)
        {
            await command.RespondAsync($"Both arguments `name` and `owner` must be set.");
            return;
        }

        var guid = Guid.NewGuid();
        var group = await Data.Attributes.GroupCollection.Load();
        var character = await group.BuildDefault(guid.ToString());

        var cardInfo = await Data.Cards.Card.Load("bio-meta");
        var cardAttr = await Data.Cards.Card.Load("attributes");
        if (cardInfo is null || cardAttr is null)
            return;
        await command.RespondAsync(
            embeds: new[]
            {
                new EmbedBuilder()
                    .WithDescription($"Character created. Id: `{guid}`")
                    .Build(),
                cardInfo.BuildEmbed(character, group),
                cardAttr.BuildEmbed(character, group),
            }
        );
    }

    public override SlashCommandProperties GetSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("character settings")
            .AddOption(
                name: "sys",
                type: ApplicationCommandOptionType.SubCommandGroup,
                description: "The subcommand to run",
                options: new()
                {
                    new SlashCommandOptionBuilder()
                        .WithName("create")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .WithDescription("Creates a new character")
                        .AddOption(
                            name: "owner",
                            type: ApplicationCommandOptionType.User,
                            description: "The controlling user",
                            isRequired: true
                        )
                        .AddOption(
                            name: "name",
                            type: ApplicationCommandOptionType.String,
                            description: "The name of the new character",
                            isRequired: true
                        )
                }
            )
            .Build();
    }
}