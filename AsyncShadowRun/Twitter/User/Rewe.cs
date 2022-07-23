using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun.Twitter.User;

public class Rewe : TwitterUserBase
{
    public class ReweData
    {
        public DateTime? NextPost { get; set; }

        public double TimeWindowStart { get; set; }

        public double TimeWindowEnd { get; set; }

        public double Chance { get; set; }
    }

    public class Entry
    {
        public string Name { get; set; } = "";
        
        public double Price { get; set; }

        public double Range { get; set; } = 0.1;

        public string Unit { get; set; } = "¥";

        public string Suffix { get; set; } = "";
    }

    public class RewePriceData : Data.BufferedJson<RewePriceData>
    {
        protected override string GetRoot()
        {
            return "resource/user/rewe";
        }

        public List<Entry> Prices { get; set; } = new();

        protected override JsonTypeInfo<RewePriceData> GetJsonTypeInfo(SourceGenerationContext ctx)
        {
            return ctx.RewePriceData;
        }
    }

    public ReweData Data { get; }

    public Rewe(Controller controller) : base(controller)
    {
        Data = GetConfig<ReweData>() ?? new();
        Data.NextPost ??= DateTime.UtcNow;
    }

    public override DateTime NextPostTime => Data.NextPost ?? DateTime.MaxValue;

    const string guid = "c6c01924-3c29-4251-b342-d1ff87abec67";
    public override Guid Id { get; } = new Guid(guid);

    public override string Name { get; } = "Rewe";

    public override string AvatarUrl { get; } = "https://cdn.discordapp.com/attachments/963383153555308604/998678492713599046/unknown.png";

    private static RewePriceData? rewePriceData;

    public override async Task Execute()
    {
        if (rewePriceData is null)
            rewePriceData = await RewePriceData.Load("base");
        if (rewePriceData is null)
        {
            Program.Log("Cannot load rewe price data");
            return;
        }
        var sb = new StringBuilder();
        var rng = new Random();
        var price = rewePriceData.Prices[rng.Next(rewePriceData.Prices.Count)];
        sb.AppendLine("**Neues Angebot reingekommen!**");
        sb.AppendLine();
        sb.Append(price.Name);
        sb.Append(" für nur ");
        sb.Append(Math.Round(price.Price * ((rng.NextDouble() - 0.5) * price.Range + 1), 2));
        sb.Append(price.Unit);
        sb.Append(price.Suffix);
        sb.AppendLine("! Schlag jetzt zu!");
        sb.AppendLine();
        sb.AppendLine("*Dein REWE - Immer ein Vergnügen wert.*");
        await Post(sb.ToString());
        await UpdateNextTime();
    }

    private async Task UpdateNextTime()
    {
        var rng = new Random();
        var now = DateTime.UtcNow;
        var date = now.Date.AddDays(1);
        while (rng.NextDouble() > Data.Chance)
            date = date.AddDays(1);
        var next = date + TimeSpan.FromHours(Data.TimeWindowStart +
            rng.NextDouble() * (Data.TimeWindowEnd - Data.TimeWindowStart)
        );
        Data.NextPost = next;
        await SetConfig(Data);
    }
}