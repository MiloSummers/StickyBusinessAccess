using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickyBusinessAccess;

internal sealed class ScreenText
{
    private List<string> pages=new();
    private int page;
    private bool quietPosition;
    internal string Signature=>string.Join("|",pages);
    internal void Refresh(UiScreen screen)
    {
        quietPosition=screen.Kind=="sleep";
        var result=new List<string>();
        var texts=screen.Root.GetComponentsInChildren<TMP_Text>(false).ToArray()
            .OrderBy(t=>screen.Kind=="packing"&&UiModel.PathOf(t.transform).Contains("/Phone/")?0:1)
            .ThenBy(t=>UiModel.Order(t.transform),StringComparer.Ordinal);
        foreach(var t in texts)
        {
            if(screen.Kind=="packing"&&UiModel.PathOf(t.transform).Contains("/ExtrasArea/"))continue;
            if(!UiModel.Visible(t.gameObject)||t.GetComponentInParent<TMP_InputField>()!=null||UiModel.HasAncestor(t.transform,"Template")||UiModel.HasAncestor(t.transform,"Dropdown List"))continue;
            if(t.GetComponentInParent<Selectable>()!=null)continue;
            if(t.GetComponentInParent<StickerGame.PrinterInventoryStickerElement>()!=null)continue;
            if(screen.Kind=="sleep"&&t.GetComponentInParent<StickerGame.StickerUpgradePreview>()!=null)continue;
            if(screen.Kind=="sleep"&&t.transform.parent!=null&&t.name=="Text"&&t.transform.parent.Find("Amount")!=null)continue;
            string text=UiModel.Translate(t.text);
            if(screen.Kind=="tutorial"&&(text.Contains("clock",StringComparison.OrdinalIgnoreCase)))text+=" Press "+AccessBindings.Key(84)+" to read the current in-game time.";
            if((UiModel.PathOf(t.transform).Contains("/LetterSticker(")||UiModel.PathOf(t.transform).Contains("/SleepStickerPreview("))&&t.name.Contains("Amount"))
            {
                var row=t.transform;while(row.parent!=null&&!row.name.Contains("(Clone)"))row=row.parent;
                text=GameUi.StickerImage(row.gameObject)+(screen.Kind=="packing"?", remaining to pack ":", quantity ")+text;
            }
            if(screen.Kind=="packing")
            {
                if(UiModel.PathOf(t.transform).Contains("/Paper/")||UiModel.PathOf(t.transform).Contains("/Filling/"))continue;
                if(t.name=="MoneyText")text="Order value: "+text+" coins";
                if(t.name=="XPText")text="XP: "+text;
                if(t.name=="DayText")text="Days remaining: "+(text=="\u221e"?"no deadline":text);
            }
            if(t.name=="SheetCostMoneyText")text="Printing cost: "+text+" coins";
            if(screen.Kind=="sheet"&&t.name=="MoneyText")text="Potential turnover: "+text+" coins";
            if(screen.Kind=="sleep")
            {
                if(t.name=="Amount"&&t.transform.parent!=null)text=UiModel.Pretty(t.transform.parent.name)+": "+text;
                if(t.name=="Money Gained")text="Coins gained: "+text;
                if(t.name=="FameGained")text="Hearts gained: "+text;
            }
            if(text.Length==0||result.Contains(text))continue;
            foreach(string line in text.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries))foreach(string chunk in NavigationRules.Pages(line,650))result.Add(chunk);
        }
        foreach(var inventory in screen.Root.GetComponentsInChildren<StickerGame.PrinterInventoryStickerElement>(false))
            if(UiModel.Visible(inventory.gameObject)&&inventory.StickerItem!=null)
                result.Add("Inventory. "+inventory.StickerItem.Name+". Available "+UiModel.Translate(inventory.availableStickersTMP.text)+". Ordered "+UiModel.Translate(inventory.orderedStickersTMP.text));
        if(screen.Kind=="sleep")
            foreach(var preview in screen.Root.GetComponentsInChildren<StickerGame.StickerUpgradePreview>(false))
                if(UiModel.Visible(preview.gameObject))result.Add(GameUi.UpgradeImage(preview)+". XP: "+UiModel.Translate(preview.Counter.text)+". Progress "+Math.Round(preview.Percentage.fillAmount*100)+" percent"+(UiModel.Active(preview.upgradeOverlay)?". Upgrade bonus "+UiModel.Translate(preview.upgradeText.text):""));
        if(screen.Kind=="shop")
        {
            var empty=screen.Root.GetComponentsInChildren<StickerGame.Shop.StickerShopElement>(false).Where(c=>UiModel.Visible(c.gameObject)&&c.GetStickerItem()==null).ToArray();
            foreach(var group in empty.GroupBy(c=>UiModel.Text(c.gameObject)))
                if(group.Key.Length>0)result.Add(group.Key+": "+group.Count()+" slots");
        }
        string balance=GameUi.BalanceText();if(balance.Length>0)result.Add(balance);
        if(screen.Kind=="packing")result.Insert(0,GameUi.OrderText(screen.Root));
        pages=result;page=Math.Clamp(page,0,Math.Max(0,pages.Count-1));
    }
    internal string Intro(UiScreen screen)
    {
        page=0;
        if(screen.Kind=="newgame")return "Enter your name and shop name, choose day speed and whether to play the tutorial, then choose Create new game. Use a new shop name for a separate save.";
        if(screen.Kind=="credits")return "Up and Down read credits in sections. They stay open until you leave. Escape returns to the menu. "+Current();
        if(screen.Kind is "popup" or "tutorial" or "sleep" or "production-popup" or "messages" or "message-history" or "packing" or "sheet" or "production")return Current();
        if(screen.Kind=="creator")return "Creative Corner.";
        if(screen.Kind=="creator-save")return "Name your sticker, review the material, then choose Save sticker. Export sticker image is a separate optional action. Escape closes this panel.";
        if(screen.Kind is "hub" or "shop")return Current();
        return "";
    }
    internal string Current()=>pages.Count==0?"No additional screen text.":pages[page]+(quietPosition?"":$". Text section {page+1} of {pages.Count}.");
    internal void Read(UiScreen screen,int step)
    {
        Refresh(screen);page=Math.Clamp(page+step,0,Math.Max(0,pages.Count-1));
        if(screen.Kind=="credits")
            foreach(var sr in screen.Root.GetComponentsInChildren<ScrollRect>(false))
                if(sr.vertical){sr.StopMovement();sr.verticalNormalizedPosition=pages.Count<=1?1:1-(float)page/(pages.Count-1);}
        Speech.Say(Current()+(screen.Kind=="credits"&&page==pages.Count-1?" End of credits. Escape returns to the main menu.":""),true);
    }
}
