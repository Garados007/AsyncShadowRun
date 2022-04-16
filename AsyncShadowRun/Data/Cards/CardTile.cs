using System.Text;
using System.Text.Json.Serialization;

namespace AsyncShadowRun.Data.Cards;

public class CardTile
{
    public bool Inline { get; set; } = true;

    public List<CardValueBlock> Title { get; set; } = new();

    [JsonPropertyName("title-name")]
    public string? TitleName { get => null; set => Set(Title, x => x.Name = value); }

    [JsonPropertyName("title-value")]
    public string? TitleValue { get => null; set => Set(Title, x => x.Value = value); }

    [JsonPropertyName("title-short")]
    public string? TitleShort { get => null; set => Set(Title, x => x.Short = value); }

    [JsonPropertyName("title-category")]
    public string? TitleCategory { get => null; set => Set(Title, x => x.Category = value); }

    [JsonPropertyName("title-const")]
    public string? TitleConst { get => null; set => Set(Title, x => x.Const = value); }

    public List<CardValueBlock> Value { get; set; } = new();

    [JsonPropertyName("value-name")]
    public string? ValueName { get => null; set => Set(Value, x => x.Name = value); }

    [JsonPropertyName("value-value")]
    public string? ValueValue { get => null; set => Set(Value, x => x.Value = value); }

    [JsonPropertyName("value-short")]
    public string? ValueShort { get => null; set => Set(Value, x => x.Short = value); }

    [JsonPropertyName("value-category")]
    public string? ValueCategory { get => null; set => Set(Value, x => x.Category = value); }

    [JsonPropertyName("value-const")]
    public string? ValueConst { get => null; set => Set(Value, x => x.Const = value); }

    private void Set(List<CardValueBlock> list, Action<CardValueBlock> updater)
    {
        if (list.Count > 1)
            list.RemoveRange(1, list.Count - 1);
        if (list.Count == 0)
            list.Add(new CardValueBlock());
        updater(list[0]);
    }

    private string Build(
        List<CardValueBlock> list,
        Characters.Character character,
        Attributes.GroupCollection attr,
        Dictionary<string, string>? arguments = null
    )
    {
        var sb = new StringBuilder();
        foreach (var item in list)
            sb.Append(item.GetText(character, attr, arguments));
        if (sb.Length == 0)
            return "<not set>";
        return sb.ToString();
    }

    public string GetTitle(
        Characters.Character character,
        Attributes.GroupCollection attr,
        Dictionary<string, string>? arguments = null
    )
        => Build(Title, character, attr, arguments);

    public string GetValue(
        Characters.Character character,
        Attributes.GroupCollection attr,
        Dictionary<string, string>? arguments = null
    )
        => Build(Value, character, attr, arguments);
}
