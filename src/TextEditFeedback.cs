using System.Globalization;
namespace StickyBusinessAccess;
internal static class TextEditFeedback
{
    internal static string Deleted(string before,string after)
    {
        var old=Elements(before);var current=Elements(after);int prefix=0;
        while(prefix<old.Count&&prefix<current.Count&&old[prefix]==current[prefix])prefix++;
        int suffix=0;while(suffix<old.Count-prefix&&suffix<current.Count-prefix&&old[old.Count-1-suffix]==current[current.Count-1-suffix])suffix++;
        return string.Concat(old.Skip(prefix).Take(old.Count-prefix-suffix));
    }
    private static List<string> Elements(string text)
    {var result=new List<string>();var e=StringInfo.GetTextElementEnumerator(text);while(e.MoveNext())result.Add(e.GetTextElement());return result;}
    internal static string At(string text,int position)=>position>=text.Length?"End of text":Spoken(StringInfo.GetNextTextElement(text,Math.Max(0,position)));
    internal static string Spoken(string text)=>text switch
    {" "=>"space","\n"=>"new line","\t"=>"tab","."=>"period",","=>"comma",":"=>"colon",";"=>"semicolon","!"=>"exclamation mark","?"=>"question mark","-"=>"hyphen","_"=>"underscore","@"=>"at sign","#"=>"number sign","/"=>"slash","\\"=>"backslash","("=>"left parenthesis",")"=>"right parenthesis","'"=>"apostrophe","\""=>"quotation mark",_=>text};
}
