namespace StickyBusinessAccess;
internal static class DayLengthRules
{
    internal static int Cost(int cost,int option)=>cost<=0||option==0?cost:option==4?0:(int)Math.Ceiling(cost/(option==1?1.25:option==2?1.5:2));
}
