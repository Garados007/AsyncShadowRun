using System.Text.Json.Serialization;

namespace AsyncShadowRun.Data.Attributes;

public class GroupAttributes
{
    public Dictionary<string, Attribute> Number { get; set; } = new();

    public Dictionary<string, Attribute> Text { get; set; } = new();

    [JsonPropertyName("number-list")]
    public Dictionary<string, Attribute> NumberList { get; set; } = new();
}