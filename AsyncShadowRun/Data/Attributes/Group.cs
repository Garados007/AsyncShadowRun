using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun.Data.Attributes;

public class Group : BufferedJson<Group>
{
    protected override JsonTypeInfo<Group> GetJsonTypeInfo(SourceGenerationContext ctx)
    {
        return ctx.Group;
    }

    protected override string GetRoot()
    {
        return "resource/attributes";
    }

    public string? Root { get; set; }

    public bool Template { get; set; }

    public GroupAttributes Attributes { get; set; }
        = new GroupAttributes();
}