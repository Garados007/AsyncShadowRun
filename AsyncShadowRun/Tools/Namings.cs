namespace AsyncShadowRun.Tools;

public static class Namings
{
    public static string FormatChannelName(ReadOnlySpan<char> name)
    {
        Span<char> data = stackalloc char[name.Length];
        var special = false;
        int j = 0;
        for (int i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            switch (ch)
            {
                case 'ä':
                case 'ö':
                case 'ü':
                case (>= 'a' and <= 'z') or (>= '0' and <= '9') or '×' or '☎':
                    data[j++] = ch;
                    special = false;
                    break;
                case 'Ä':
                    data[j++] = 'ä';
                    special = false;
                    break;
                case 'Ö':
                    data[j++] = 'ö';
                    special = false;
                    break;
                case 'Ü':
                    data[j++] = 'ü';
                    special = false;
                    break;
                case >= 'A' and <= 'Z':
                    data[j++] = (char)((ch + 'a') - 'A');
                    special = false;
                    break;
                default:
                    if (!special)
                    {
                        special = true;
                        data[j++] = '-';
                    }
                    break;
            }
        }
        return new string(data[..j]);
    }
}