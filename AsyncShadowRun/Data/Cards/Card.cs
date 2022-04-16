using System.Text.Json.Serialization.Metadata;
using Discord;

namespace AsyncShadowRun.Data.Cards;

public class Card : BufferedJson<Card>
{
    protected override JsonTypeInfo<Card> GetJsonTypeInfo(SourceGenerationContext ctx)
    {
        return ctx.Card;
    }

    protected override string GetRoot()
    {
        return "resource/cards";
    }

    public string Name { get; set; } = "";

    public List<CardTile> Rows { get; set; } = new();

    public Embed BuildEmbed(
        Characters.Character character,
        Attributes.GroupCollection attr,
        Dictionary<string, string>? arguments = null
    )
    {
        return new EmbedBuilder()
            .WithTitle(Name)
            .WithFields(
                Rows.Select(
                    x => new EmbedFieldBuilder()
                        .WithIsInline(x.Inline)
                        .WithName(x.GetTitle(character, attr, arguments))
                        .WithValue(x.GetValue(character, attr, arguments))
                )
            )
            .Build();
    }
}