using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace AsyncShadowRun.Commands;

public class Roll : CommandBase
{
    public Roll(Program program) : base(program)
    {
    }

    public override string Name => "roll";

    public override async Task Execute(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var count = (long)command.Data.Options.First().Value;
        if (count > 100 || count <= 0)
        {
            await command.RespondAsync(
                "Invalid number of dice. Valid is between 1 and 100.",
                ephemeral: true
            );
            return;
        }

        var rolls = GenerateDiceRolls(count).Take((int)count).ToArray();
        var succeeds = rolls.Where(x => x == 4 || x == 5).Count();
        var fails = rolls.Where(x => x == 0).Count();

        var sb = new StringBuilder();
        for (int i = 0; i < rolls.Length; ++i)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(rolls[i] switch
            {
                0 => $"*1*",
                4 => $"**5**",
                5 => $"**6**",
                _ => $"{rolls[i] + 1}",
            });
        }
        sb.Append($"\nErfolge: {succeeds}\nPatzer: {fails}");
        string state;
        Color color;
        if (fails * 2 >= count)
        {
            if (succeeds == 0)
                (state, color) = ("Kritischer Patzer", Color.Red);
            else (state, color) = ("Patzer", Color.Orange);
        }
        else
        {
            if (succeeds == 0)
                (state, color) = ("Fehlschlag", Color.Gold);
            else (state, color) = ("Erfolg", Color.Green);
        }


        await command.ModifyOriginalResponseAsync(
            mes => mes.Embed = new EmbedBuilder()
                .WithTitle($"Würfle {count}D6: {state}")
                .WithColor(color)
                .WithDescription(sb.ToString())
                .Build()
        );
    }

    private IEnumerable<int> GenerateDiceRolls(long count)
    {
        using var rng = RandomNumberGenerator.Create();
        Memory<byte> buffer = new byte[1];
        while (count > 0)
        {
            rng.GetBytes(buffer.Span);
            var num = buffer.Span[0];

            // 0..215 => 3 digits
            if (num < 216)
            {
                count -= 3;
                yield return num % 6;
                num /= 6;
                yield return num % 6;
                num /= 6;
                yield return num % 6;
                continue;
            }
            // 216..251 => 2 digits
            if (num < 252)
            {
                count -= 2;
                num -= 252;
                yield return num % 6;
                num /= 6;
                yield return num % 6;
                continue;
            }
        }
    }

    public override SlashCommandProperties GetSlashCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("rolls a single dice and counts the success")
            .AddOption(
                name: "count",
                type: ApplicationCommandOptionType.Integer,
                description: "The number of dice to roll",
                isRequired: true
            )
            .Build();
    }
}