using BepInEx.Configuration;
using System.Runtime.InteropServices;
using System.Text;
namespace StickyBusinessAccess;

internal sealed record AccessAction(int Command,string Name,ConfigEntry<int> Entry);
internal static class AccessBindings
{
    internal static readonly List<AccessAction> Actions=new();
    private static ConfigFile config=null!;
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetKeyNameText(int param,StringBuilder text,int size);
    [DllImport("user32.dll")] private static extern uint MapVirtualKey(uint code,uint type);
    internal static void Initialize(ConfigFile file)
    {
        config=file;
        bool hadInformation=file.Keys.Any(d=>d.Section=="AccessibilityKeys"&&d.Key=="Action116");
        foreach(var pair in new (int,string)[]{(9,"Next control / finish editing"),(13,"Activate / finish adjustment"),(32,"Activate alternate"),(27,"Back or pause"),(8,"Back alternate"),(48,"Back shortcut"),
            (37,"Left / decrease / move left"),(39,"Right / increase / move right"),(38,"Up / previous control or text"),(40,"Down / next control or text"),(36,"First control"),(35,"Last control"),(33,"Previous text section"),(34,"Next text section"),
            (112,"Contextual help"),(72,"Help alternate"),(113,"Repeat announcement"),(82,"Repeat alternate"),(114,"List controls"),(115,"Read screen information"),(119,"Write diagnostics"),
            (116,"Game information"),(84,"Read game time"),(66,"Read balance"),(79,"Read current order"),(67,"Read placed sticker coordinates"),(117,"Jump to categories"),(118,"Jump to sticker choices"),(120,"Jump to placed stickers"),
            (46,"Delete selected placed item"),(81,"Rotate left / rotate copy"),(69,"Rotate right / rotate copy"),(189,"Resize smaller"),(109,"Resize smaller alternate"),(187,"Resize bigger"),(107,"Resize bigger alternate"),(219,"Move layer back"),(221,"Move layer forward"),
            (49,"Hub Shop"),(50,"Hub Creative Corner"),(51,"Hub Upgrades"),(52,"Hub Production"),(53,"Hub Order packing"),(54,"Hub Messages"),(55,"Hub Send packed orders"),(56,"Hub Sleep")})
            Actions.Add(new(pair.Item1,pair.Item2,file.Bind("AccessibilityKeys","Action"+pair.Item1,pair.Item1,pair.Item2+". Windows virtual-key code; editable in Settings > Accessibility controls.")));
        // Adding a new shortcut must not reset existing valid custom bindings.
        if(!hadInformation)
        {
            var information=Actions.First(a=>a.Command==116);
            if(Actions.Any(a=>a!=information&&a.Entry.Value==information.Entry.Value))
                information.Entry.Value=Enumerable.Range(112,24).Concat(Enumerable.Range(65,26)).First(k=>Allowed(k)&&!Actions.Any(a=>a!=information&&a.Entry.Value==k));
        }
        var used=new HashSet<int>();
        if(Actions.Any(a=>!Allowed(a.Entry.Value)||!used.Add(a.Entry.Value)))
        {foreach(var a in Actions)a.Entry.Value=a.Command;config.Save();Plugin.Logger.LogWarning("Invalid or conflicting accessibility keys. Restored defaults.");}
    }
    internal static bool Allowed(int key)=>key>=8&&key<=254&&key is not 16 and not 17 and not 18 and not 20 and not 45 and not 91 and not 92 and not 93 and not 160 and not 161 and not 162 and not 163 and not 164 and not 165;
    internal static int Physical(int command)=>Actions.FirstOrDefault(a=>a.Command==command)?.Entry.Value??command;
    internal static string Key(int command)=>Name(Physical(command));
    internal static string Name(int key)
    {
        uint scan=MapVirtualKey((uint)key,0);bool extended=key is 33 or 34 or 35 or 36 or 37 or 38 or 39 or 40 or 45 or 46;
        var text=new StringBuilder(80);GetKeyNameText((int)(scan<<16)|(extended?1<<24:0),text,80);
        return text.Length>0?text.ToString():"Key "+key;
    }
    internal static HashSet<int> Translate(HashSet<int> physical)=>Actions.Where(a=>physical.Contains(a.Entry.Value)).Select(a=>a.Command).ToHashSet();
    internal static string? Conflict(int key,AccessAction action)
    {
        if(!Allowed(key))return "That key is reserved for modifiers, NVDA or Windows. Choose another single key.";
        var other=Actions.FirstOrDefault(a=>a!=action&&a.Entry.Value==key);
        return other==null?null:Name(key)+" is already assigned to "+other.Name+". Choose another key.";
    }
    internal static void Assign(AccessAction action,int key){action.Entry.Value=key;config.Save();}
    internal static void Defaults(){foreach(var a in Actions)a.Entry.Value=a.Command;config.Save();}
    internal static string Help(string text)
    {
        text=text.Replace("Left and right brackets","§219§ and §221§").Replace("minus and plus","§189§ and §187§").Replace("Minus and plus","§189§ and §187§");
        text=System.Text.RegularExpressions.Regex.Replace(text,@"\barrows\b",m=>"movement keys "+string.Join(", ",new[]{37,39,38,40}.Select(k=>"§"+k+"§")),System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var tokens=new Dictionary<string,int>{{"0",48},{"1",49},{"2",50},{"3",51},{"4",52},{"5",53},{"6",54},{"7",55},{"8",56},{"F1",112},{"F2",113},{"F3",114},{"F4",115},{"F6",117},{"F7",118},{"F8",119},{"F9",120},{"Tab",9},{"Enter",13},{"Space",32},{"Escape",27},{"Backspace",8},{"Delete",46},{"Left",37},{"Right",39},{"Up",38},{"Down",40},{"Q",81},{"E",69},{"T",84},{"B",66},{"O",79},{"C",67},{"H",72},{"R",82}};
        text=System.Text.RegularExpressions.Regex.Replace(text,@"\b([0-8]|F[1-9]|Tab|Enter|Space|Escape|Backspace|Delete|Left|Right|Up|Down|Q|E|T|B|O|C|H|R)\b",m=>tokens.TryGetValue(m.Value,out var key)?Key(key):m.Value);
        return System.Text.RegularExpressions.Regex.Replace(text,@"§(\d+)§",m=>Key(int.Parse(m.Groups[1].Value)));
    }
}
