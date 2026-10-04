using StickerGame;
using StickerGame.Management;
using UnityEngine;
namespace StickyBusinessAccess;

// A spoken information view preserves the native layout and remembered focus.
// Sections and rows are rebuilt from the loaded game on each key press.
internal static class InformationAccess
{
    private static bool open;
    private static int section,row;
    private static readonly string[] Sections={"Shop status","Current order","Printed sticker inventory","Purchased goodies"};
    internal static void Close()=>open=false;
    internal static bool Handle(HashSet<int> keys,bool editing)
    {
        if(editing||MenuAccess.AccessibilityModifierDown)return false;
        if(keys.Contains(116))
        {open=!open;section=row=0;if(open){CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;PrintedAccess.Editing=null;GoodieAccess.Editing=null;Say();}else Speech.Say("Game information closed. Returned to previous control.",true);return true;}
        if(!open)return false;
        if(keys.Contains(27)||keys.Contains(8)||keys.Contains(48)){Close();Speech.Say("Game information closed. Returned to previous control.",true);return true;}
        if(keys.Contains(113)||keys.Contains(82)){Speech.Repeat();return true;}
        if(keys.Contains(112)||keys.Contains(72)){Speech.Say(AccessBindings.Help("Game information. Left and Right change section. Up and Down read items. Enter refreshes the current item. Escape returns to the previous control."),true);return true;}
        if(keys.Contains(37)||keys.Contains(39)||keys.Contains(9)){section=(section+(keys.Contains(37)?-1:1)+Sections.Length)%Sections.Length;row=0;Say();return true;}
        var rows=Rows();
        if(keys.Contains(38)||keys.Contains(33))row=Math.Max(0,row-1);
        if(keys.Contains(40)||keys.Contains(34))row=Math.Min(Math.Max(0,rows.Count-1),row+1);
        if(keys.Contains(36))row=0;
        if(keys.Contains(35))row=Math.Max(0,rows.Count-1);
        if(keys.Any(k=>k is 38 or 40 or 33 or 34 or 36 or 35 or 13 or 32 or 115))Say();
        if(keys.Contains(66))Speech.Say(GameUi.BalanceText(),true);
        if(keys.Contains(84))GameUi.Clock();
        return true;
    }
    private static List<string> Rows()
    {
        var game=GameManager.Instance?.Game;var result=new List<string>();
        if(game==null)return new(){"No shop is currently loaded"};
        if(section==0){result.Add(GameUi.BalanceText());result.Add($"Day {game.Day}. {game.MinutesLeft} working minutes remaining. Native game mode: {game.GameSpeed}. "+DayLength.Label);}
        if(section==1){var screen=UiModel.Resolve();result.Add(screen==null?"No order is displayed":GameUi.OrderText(screen.ActionsRoot??screen.Root));}
        if(section==2)
            foreach(var item in game.stickers)
                if(!item.MarkAsDeleted){int amount=game.printedStickers.ContainsKey(item.ID)?game.printedStickers[item.ID]:0;result.Add(item.Name+$". Printed copies available {amount}. "+(amount==0?"Print a sheet in Production to add stock":"Use in Order packing"));}
        if(section==3)
            foreach(var pair in game.Goodies){string name=GoodieAccess.Name(pair.Key);result.Add(name+$". Available {pair.Value}. Optional packing extra. Open Order packing, then its Goodies category to add it to the box");}
        if(result.Count==0)result.Add(section==3?"No purchased goodies":"No sticker designs");return result;
    }
    private static void Say(){var rows=Rows();row=Math.Clamp(row,0,rows.Count-1);Speech.Say(Sections[section]+". "+rows[row],true);}
}
