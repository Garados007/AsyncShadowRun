using System.Text.Json.Serialization;

namespace AsyncShadowRun;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(Config))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}