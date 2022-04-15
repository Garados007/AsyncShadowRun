namespace AsyncShadowRun.Configs;

public class KommlinkConfig
{
    public ulong? Group { get; set; }

    public Dictionary<ulong, Dictionary<ulong, Data.KommlinkChat>> DirectChats { get; set; }
        = new Dictionary<ulong, Dictionary<ulong, Data.KommlinkChat>>();
}