using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun.Data.Characters;

public class Character : BufferedJson<Character>
{
    public ulong? ControllingUser { get; set; }

    public Dictionary<string, long> Numbers { get; set; }
        = new Dictionary<string, long>();
    public Dictionary<string, string> Texts { get; set; }
        = new Dictionary<string, string>();
    public Dictionary<string, long[]> NumberLists { get; set; }
        = new Dictionary<string, long[]>();

    protected override JsonTypeInfo<Character> GetJsonTypeInfo(SourceGenerationContext ctx)
    {
        return ctx.Character;
    }
}