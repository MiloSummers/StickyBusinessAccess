namespace StickyBusinessAccess;

// Pure state rules shared by the navigator and regression checks.
internal static class NavigationRules
{
    internal static int Move(int index,int count,int direction)=>count==0?-1:index<0?(direction>0?0:count-1):(index+direction+count)%count;
    internal static IEnumerable<string> Pages(string text,int limit)
    {
        while(text.Length>limit)
        {
            int split=text.LastIndexOf(' ',limit);if(split<1)split=limit;
            yield return text[..split].Trim();text=text[split..].TrimStart();
        }
        if(text.Length>0)yield return text;
    }
}
