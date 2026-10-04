using System.Text.Json;

namespace StickyBusinessAccess;

/// <summary>Independent of Unity. Future sticker adapters provide the game's localized title.</summary>
public sealed class DescriptionCatalogue
{
    public sealed record Description(string ShortName, string? Detail);
    private readonly Dictionary<string,Description> entries=new(StringComparer.Ordinal);
    private readonly Dictionary<(string,string),Description> variants=new();
    public string Locale { get; private set; } = "en";
    public static DescriptionCatalogue Parse(string json)
    {
        using var document=JsonDocument.Parse(json,new JsonDocumentOptions{AllowTrailingCommas=false,CommentHandling=JsonCommentHandling.Disallow});
        var root=document.RootElement;
        Unique(root);
        if(root.GetProperty("schemaVersion").GetInt32()!=1)throw new FormatException("Unsupported catalogue schema version.");
        var result=new DescriptionCatalogue {Locale=root.GetProperty("locale").GetString()??""};
        if(string.IsNullOrWhiteSpace(result.Locale))throw new FormatException("Locale is required.");
        var all=root.GetProperty("entries");Unique(all);
        foreach(var entry in all.EnumerateObject())
        {
            if(string.IsNullOrWhiteSpace(entry.Name))throw new FormatException("An upgrade ID is empty.");
            result.entries.Add(entry.Name,Read(entry.Value));
            if(entry.Value.TryGetProperty("variants",out var vv))
            {
                Unique(vv);
                foreach(var v in vv.EnumerateObject())
                {
                    if(string.IsNullOrWhiteSpace(v.Name))throw new FormatException("A variant ID is empty.");
                    result.variants.Add((entry.Name,v.Name),Read(v.Value));
                }
            }
        }
        return result;
    }
    private static void Unique(JsonElement e)
    {
        var keys=new HashSet<string>(StringComparer.Ordinal);
        foreach(var p in e.EnumerateObject()) if(!keys.Add(p.Name))throw new FormatException("Duplicate catalogue key: "+p.Name);
    }
    private static Description Read(JsonElement e)
    {
        Unique(e);
        string name=e.GetProperty("shortName").GetString()??"";
        string? detail=e.TryGetProperty("description",out var d)?d.GetString():null;
        if(string.IsNullOrWhiteSpace(name)||name.Length>160||(detail?.Length??0)>4000)throw new FormatException("Invalid name or description length.");
        return new Description(name.Trim(),detail?.Trim());
    }
    public Description Resolve(string id,string? variant,string? localizedTitle)
    {
        if(variant!=null&&variants.TryGetValue((id,variant),out var v))return v;
        if(entries.TryGetValue(id,out var e))return e;
        return new Description(string.IsNullOrWhiteSpace(localizedTitle)?"Unlabelled sticker "+id:localizedTitle,null);
    }
}
