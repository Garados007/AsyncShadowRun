using System.Text;

namespace AsyncShadowRun.Data.Cards;

public class CardValueBlock
{
    public string? Name { get; set; }

    public string? Value { get; set; }

    public string? Short { get; set; }

    public string? Category { get; set; }

    public string? Const { get; set; }

    public string? GetText(
        Characters.Character character,
        Attributes.GroupCollection attr,
        Dictionary<string, string>? arguments = null
    )
    {
        if (Name is not null)
            return attr.GetAttribute(Name)?.Name;
        if (Short is not null)
            return attr.GetAttribute(Short)?.Short;
        if (Category is not null)
            return attr.GetAttribute(Category)?.Category;
        if (Value is not null)
        {
            var sb = new StringBuilder(Value);
            if (arguments is not null)
                foreach (var (k, v) in arguments)
                    sb.Replace($"{{{k}}}", $"${v}");
            var path = sb.ToString();
            if (character.Numbers.TryGetValue(path, out long lv))
                return lv.ToString();
            if (character.Texts.TryGetValue(path, out string? sv))
                return sv;
            if (character.NumberLists.TryGetValue(path, out long[]? llv))
            {
                sb.Clear();
                sb.Append('[');
                for (int i = 0; i < 3 && i < llv.Length; ++i)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(llv[i]);
                }
                if (llv.Length > 3)
                    sb.Append(", ...");
                sb.Append(']');
                return sb.ToString();
            }
        }
        if (Const is not null)
            return Const;
        return null;
    }
}