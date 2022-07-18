using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace AsyncShadowRun.Twitter.User;

public class Rewe : TwitterUserBase
{
    public class ReweData
    {
        public DateTime? NextPost { get; set; }

        public List<double> PostTimes { get; set; } = new();
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
        var hour = now.TimeOfDay.Hours;
        var nextHour = Data.PostTimes
            .Where(x => x > hour)
            .OrderBy(x => x)
            .Select(x => (double?)x)
            .FirstOrDefault();
        DateTime next;
        if (nextHour is not null)
        {
            next = now.Date + TimeSpan.FromHours(nextHour.Value + rng.NextDouble());
        }
        else
        {
            if (Data.PostTimes.Count > 0)
            {
                next = now.Date + TimeSpan.FromHours(24 + Data.PostTimes[0] + rng.NextDouble());
            }
            else
            {
                next = now + TimeSpan.FromDays(1);
            }
        }
        Data.NextPost = next;
        await SetConfig(Data);
    }

}