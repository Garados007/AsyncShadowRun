using System.Text.Json.Serialization;

namespace AsyncShadowRun;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Data.Characters.Character))]
[JsonSerializable(typeof(Data.Attributes.Group))]
[JsonSerializable(typeof(Data.Cards.Card))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}