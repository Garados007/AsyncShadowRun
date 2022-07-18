using System.Text.Json.Serialization;

namespace AsyncShadowRun;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = false,
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true
)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Data.Characters.Character))]
[JsonSerializable(typeof(Data.Attributes.Group))]
[JsonSerializable(typeof(Data.Cards.Card))]
[JsonSerializable(typeof(Twitter.User.Rewe.RewePriceData))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}