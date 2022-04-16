using System.Collections;

namespace AsyncShadowRun.Data.Attributes;

public class GroupCollection : IEnumerable<Group>
{
    readonly Dictionary<string, Group> groups = new();

    private GroupCollection() {}

    private static GroupCollection? current;

    public static async ValueTask<GroupCollection> Load()
    {
        if (current is not null)
            return current;
        await Group.LoadAll();
        var result = new GroupCollection();
        foreach (var (key, value) in Group.EnumerateCachedData())
            result.groups.Add(key, value);
        current = result;
        return result;
    }

    public Group? Get(string key)
    {
        return groups.TryGetValue(key, out Group? value) ? value : null;
    }

    public IEnumerator<Group> GetEnumerator()
    {
        return groups.Values.GetEnumerator();
    }

    public Group? GetTemplate(string key)
    {
        var group = Get(key);
        if (group is not null && group.Template)
            return group;
        return null;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    public Attribute? GetAttribute(string path)
    {
        foreach (var group in this)
        {
            var prefix = "";
            if (group.Root is not null)
            {
                prefix = $"{group.Root}.";
            }
            if (group.Template)
            {
                prefix = $"{prefix}@.";
            }

            if (prefix.Length > 0 && !path.StartsWith(prefix))
                continue;

            foreach (var (key, attr) in group.Attributes.Number)
                if (path == $"{prefix}{key}")
                    return attr;

            foreach (var (key, attr) in group.Attributes.Text)
                if (path == $"{prefix}{key}")
                    return attr;

            foreach (var (key, attr) in group.Attributes.NumberList)
                if (path == $"{prefix}{key}")
                    return attr;
        }

        return null;
    }

    public async ValueTask<Characters.Character> BuildDefault(string key)
    {
        var character = new Characters.Character();
        foreach (var (_, group) in groups)
        {
            var prefix = group.Root is null ? "" : $"{group.Root}.";
            if (group.Template)
            {
                character.Numbers[$"{prefix}@next"] = 0;
                character.NumberLists[$"{prefix}@list"] = Array.Empty<long>();
            }
            else
            {
                foreach (var (path, _) in group.Attributes.Number)
                    character.Numbers[$"{prefix}{path}"] = 0;
                foreach (var (path, _) in group.Attributes.Text)
                    character.Texts[$"{prefix}{path}"] = "";
                foreach (var (path, _) in group.Attributes.NumberList)
                    character.NumberLists[$"{prefix}{path}"] = Array.Empty<long>();
            }
        }
        character.SetBufferKey(key);
        await character.Save();
        return character;
    }
}