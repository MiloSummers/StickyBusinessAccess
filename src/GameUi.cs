using StickerGame;
using StickerGame.Management;
using StickerGame.StickerCreator;
using StickerGame.UpgradeScreen;
using StickerGame.StickerLetter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace StickyBusinessAccess;

// Labels read live game data; no replacement controls or game progression.
internal static class GameUi
{
    internal static string? Label(Selectable s)
    {
        string? binding=BindingUi.Label(s);if(binding!=null)return binding;
        string path=UiModel.PathOf(s.transform);
        if(s.name=="MoreFromSpellgardenBtn")return "More from Spellgarden Games, external Steam publisher page";
        if(s.name=="NextOrder"&&path.Contains("StickerSendingCanvas"))return "Next Order";
        if(s.name=="PrevOrder"&&path.Contains("StickerSendingCanvas"))return "Previous Order";
        string? tab=PackingTab(s);if(tab!=null)return tab+" tab";
        if(s.name=="Inventory"&&path.Contains("StickerCuttingCanvas"))return "Inventory";
        if(path.Contains("/ContextMenu/")&&s.GetComponentInParent<StickerElement>()!=null)
        {
            if(s.name=="Back")return "Move element one layer back";
            if(s.name=="Forward")return "Move element one layer forward";
            if(s.name=="To Back")return "Move element to bottom layer";
            if(s.name=="To Front")return "Move element to top layer";
        }
        if(path.Contains("/Downloaded/")&&s.name=="SaveButton")return "Close export confirmation";
        if(path.Contains("/SheetDesigner/LowerButtonGroup/")&&s.name=="Print Button")return "Export sticker sheet image";
        if(path.Contains("/SheetDesigner/LowerButtonGroup/")&&s.name=="SaveButton")return "Save sheet design";
        var tabs=s.GetComponentInParent<StickerGame.UserInterface.UpgradesTopBar>();
        if(tabs!=null)
        {
            if(s==tabs.StickersGameObjectBtn)return "Sticker elements tab";
            if(s==tabs.ColorsGameObjectBtn)return "Element colours tab";
            if(s==tabs.ColorsOutlinesGameObjectBtn)return "Outline colours tab";
            if(s==tabs.FillingGameObjectBtn)return "Packing filling tab";
            if(s==tabs.PapersGameObjectBtn)return "Packing paper tab";
            if(s==tabs.UpgradesViewBtn)return "Shop upgrades tab";
            if(s==tabs.GlitterViewBtn)return "Special sticker materials tab";
            if(s==tabs.GoodiesViewBtn)return "Goodies tab";
        }
        if(path.Contains("Hub Screen/"))
        {
            if(path.EndsWith("/MessageButton"))return "Messages";
            if(path.EndsWith("/Send"))return "Send packed orders. "+UiModel.Text(s.gameObject)+" packages";
            if(path.EndsWith("/StickerSending"))return "Order packing";
        }
        var sheet=s.GetComponentInParent<StickerGame.ProductionScreen.StickerSheetOverviewElement>();
        if(sheet!=null&&sheet.item!=null)
        {
            var entries=new List<string>();foreach(var pair in sheet.item.GetStickerList())entries.Add(pair.Value+" "+pair.Key.Name);
            return "Sticker sheet: "+string.Join(", ",entries)+". Printing cost "+sheet.item.GetPricePoint()+" coins. Open sheet actions";
        }
        if(s.name.StartsWith("NewStickerSheetPrefab"))return "Create new sticker sheet";
        var color=s.GetComponent<ColorButton>();
        if(color!=null&&color.customColor!=null)return "Shop colour: "+ShopColour(color.customColor);
        var deco=s.GetComponent<BackgroundDecoButton>();
        if(deco!=null&&deco.customBackgroundDeco!=null)return "Shop background: "+UiModel.Pretty(deco.customBackgroundDeco.name).Replace("Custom Background ","");
        if(path.Contains("/BackgroundColor/Colors/OutlineColorElement"))
        {
            var graphics=s.GetComponentsInChildren<Image>(false);
            var graphic=graphics.FirstOrDefault(g=>g.color.r<.95f||g.color.g<.95f||g.color.b<.95f)??graphics.FirstOrDefault();
            return "Outline colour: "+(graphic==null?"swatch":Colour(graphic.color));
        }
        var u=s.GetComponentInParent<UpgradeElement>();
        if(u!=null&&u.upgrade!=null)return UiModel.Translate(u.upgrade.GetLocalizationTitleKey());
        var message=s.GetComponentInParent<OverviewCustomerMessage>();
        if(message!=null)return UiModel.Text(message.gameObject).Replace("???","Unknown customer")+
            (UiModel.Active(message.actionRequiredIndicator)?". Action required":"")+
            (UiModel.Active(message.storyDoneIndicator)?". Story complete":"")+
            (UiModel.Active(message.indicator)?". New message":"")+(message.customerNameText!=null&&message.customerNameText.text.Contains("???")?". Customer has not been revealed":". Open customer history");
        var paper=s.GetComponentInParent<PaperSelector>();
        if(paper!=null&&paper.paperData!=null)return "Packing paper: "+UpgradeName(paper.paperData.GetUpgrade(),paper.paperData.name);
        var filling=s.GetComponentInParent<FillingSelector>();
        if(filling!=null&&filling.fillingData!=null)return "Packing filling: "+UpgradeName(filling.fillingData.GetUpgrade(),filling.fillingData.name);
        var goodie=s.GetComponentInParent<GoodieElementSpawner>();
        if(goodie!=null)return "Add goodie: "+UpgradeName(string.IsNullOrEmpty(goodie.goodieUpgradeID)?null:UpgradeInfo.FromID(goodie.goodieUpgradeID),goodie.name);
        var card=s.GetComponentInParent<StickerGame.Shop.StickerShopElement>();
        if(card!=null&&card.GetStickerItem()!=null)
        {
            var item=card.GetStickerItem();
            if(s.name.Contains("Delete"))return "Delete sticker: "+item.Name;
            if(s==card.inShopToggle)return item.Name+", sale selection: "+(item.InShop?"in shop":"not in shop");
            if(card.editNameButton!=null&&UiModel.Within(s.transform,card.editNameButton.transform))return "Rename sticker: "+item.Name;
            if(card.saveButton!=null&&UiModel.Within(s.transform,card.saveButton.transform))return "Export sticker image: "+item.Name;
            return item.Name+". Price "+item.MoneyPrice+" coins. "+(item.InShop?"In shop":"Not in shop")+". Sticker preview";
        }
        var printed=s.GetComponentInParent<PrintedStickerElement>();
        if(printed!=null&&printed.StickerItem!=null)return (printed.moved?"Placed sticker: ":"Add sticker: ")+printed.StickerItem.Name;
        return null;
    }
    internal static string? Describe(Selectable s)
    {
        if(UiModel.PathOf(s.transform).Contains("/BackgroundSize/"))return UiModel.Label(s)+", button. "+BorderWidth();
        var inventory=Inventory(s);
        if(inventory!=null)return "Inventory, button. "+(UiModel.Active(inventory.inventoryScreen)?"Open":"Closed");
        string? packing=PackingSelection(s);if(packing!=null)return packing;
        var copy=s.GetComponentInParent<PrintedStickerElement>();
        if(copy!=null&&!copy.moved&&copy.Phase!=PrintedStickerElement.StickerElementType.PrintPhase&&copy.textElement!=null)
            return Label(s)+", button. Available printed copies: "+UiModel.Translate(copy.textElement.text);
        var print=s.GetComponentInParent<StickerGame.ProductionScreen.SheetPrintPopup>();
        if(print!=null&&s.name=="Create Btn")return "Print sheet, button. Printing cost "+(print.priceElement==null?"not displayed":UiModel.Translate(print.priceElement.text))+" coins";
        var tabs=s.GetComponentInParent<StickerGame.UserInterface.UpgradesTopBar>();
        if(tabs!=null)return Label(s)+", tab"+(tabs.lastSelectedButton==s?", selected":"");
        var u=s.GetComponentInParent<UpgradeElement>();
        if(u!=null&&u.upgrade!=null)
        {
            var info=u.upgrade;var stats=info.GetStatistics();
            string state=info.IsUnlocked()?"owned":info.IsBlocked()?"locked":"purchase";
            string cost=UiModel.Active(u.priceGo)?". Cost "+Price(u)+" "+(UiModel.Active(u.moneyIcon)?"coins":UiModel.Active(u.fameIcon)?"hearts":""):"";
            var hint=u.GetComponentInChildren<StickerGame.UserInterface.UIHoverInfoText>(false);
            string description=hint==null?"":UiModel.Translate(hint.Text);
            string progress=u.FameProgressText!=null&&UiModel.Visible(u.FameProgressText.gameObject)?". Progress "+UiModel.Translate(u.FameProgressText.text):"";
            return Label(s)+". "+state+cost+(stats==null?"":". Level "+stats.Level)+progress+(description.Length==0?"":". "+description);
        }
        var c=s.GetComponent<ColorButton>();
        if(c!=null&&c.customColor!=null)return Label(s)+", colour selection"+(GameManager.Instance?.Game?.ShopColor==c.customColor.name?", selected":"");
        var d=s.GetComponent<BackgroundDecoButton>();
        if(d!=null&&d.customBackgroundDeco!=null)return Label(s)+", background selection"+(GameManager.Instance?.Game?.ShopBG==d.customBackgroundDeco.name?", selected":"");
        if(UiModel.PathOf(s.transform).Contains("/BackgroundColor/Colors/OutlineColorElement"))
            return Label(s)+", selection. Current outline: "+Colour(BorderColorChanger.Instance.color);
        return null;
    }
    internal static PrinterInventoryLoader? Inventory(Selectable s)=>s.name=="Inventory"&&UiModel.PathOf(s.transform).Contains("StickerCuttingCanvas")?s.transform.root.GetComponentInChildren<PrinterInventoryLoader>(false):null;
    internal static string InventoryRows(PrinterInventoryLoader loader)
    {
        if(!UiModel.Active(loader.inventoryScreen))return "";
        var rows=loader.Container.GetComponentsInChildren<PrinterInventoryStickerElement>(false).Where(e=>UiModel.Visible(e.gameObject)&&e.StickerItem!=null)
            .Select(e=>e.StickerItem.Name+". Available "+UiModel.Translate(e.availableStickersTMP.text)+". Ordered "+UiModel.Translate(e.orderedStickersTMP.text));
        return string.Join(". ",rows);
    }
    internal static string? PackingSelection(Selectable s)
    {
        var paper=s.GetComponentInParent<PaperSelector>();
        if(paper?.paperData!=null&&paper.fillingController!=null)
        {
            var image=paper.fillingController.paperOpen;var data=paper.paperData;
            return Label(s)+", selection, "+(image!=null&&image.sprite==data.paperOpen&&image.color.Equals(data.color)?"selected":"not selected");
        }
        var fill=s.GetComponentInParent<FillingSelector>();
        if(fill?.fillingData!=null&&fill.fillingController!=null)
        {
            var image=fill.fillingController.filling;var data=fill.fillingData;
            return Label(s)+", selection, "+(image!=null&&image.sprite==data.fillingImage&&image.color.Equals(data.color)?"selected":"not selected");
        }
        var tabs=s.GetComponentInParent<TopBarController>();if(tabs==null)return null;
        int index=s==tabs.fillingsSelectable?0:s==tabs.goodiesSelectable?1:s==tabs.halloweenGoodiesSelectable?2:s==tabs.festiveSelectable?3:s==tabs.prideSelectable?4:s==tabs.PWMSelectable?5:s==tabs.campSelectable?6:s==tabs.witchyDLCGoodiesSelectable?7:s==tabs.seasideDLCGoodiesSelectable?8:-1;
        var manager=UnityEngine.Object.FindObjectOfType<PackingExtrasManager>();
        if(index<0||manager==null)return null;
        string[] names={"Filling","Goodies","Halloween goodies","Festive goodies","Pride goodies","PWM goodies","Camp goodies","Witchy goodies","Seaside goodies"};
        return names[index]+", tab, "+((int)manager.extrasState==index?"selected":"not selected");
    }
    internal static string? PackingTab(Selectable s)
    {
        var tabs=s.GetComponentInParent<TopBarController>();if(tabs==null)return null;
        return s==tabs.fillingsSelectable?"Filling":s==tabs.goodiesSelectable?"Goodies":s==tabs.halloweenGoodiesSelectable?"Halloween goodies":s==tabs.festiveSelectable?"Festive goodies":s==tabs.prideSelectable?"Pride goodies":s==tabs.PWMSelectable?"PWM goodies":s==tabs.campSelectable?"Camp goodies":s==tabs.witchyDLCGoodiesSelectable?"Witchy goodies":s==tabs.seasideDLCGoodiesSelectable?"Seaside goodies":null;
    }
    internal static string BalanceText()
    {
        var game=GameManager.Instance?.Game;
        return game==null?"":$"Balance: {game.Money} coins. {game.Fame} hearts";
    }
    internal static string OrderText(GameObject root)
    {
        var letter=root.GetComponentInChildren<LetterManager>(false);if(letter==null||!UiModel.Visible(letter.gameObject)||letter.currentCustomer==null)return "No customer order is displayed here. Open Order packing from the hub with 5.";
        string customer=letter.fromTextPhone!=null&&UiModel.Visible(letter.fromTextPhone.gameObject)?UiModel.Translate(letter.fromTextPhone.text):UiModel.Translate(letter.fromTextMessage?.text??letter.currentCustomer.Name);
        var parts=new List<string>{"Customer order from "+customer};
        if(letter.orderAmountText!=null&&UiModel.Visible(letter.orderAmountText.gameObject))parts.Add("Open orders: "+UiModel.Translate(letter.orderAmountText.text));
        if(letter.messageText!=null&&UiModel.Visible(letter.messageText.gameObject))parts.Add(UiModel.Translate(letter.messageText.text));
        foreach(var t in letter.stickerOrderContainer.GetComponentsInChildren<TMP_Text>(false))
            if(UiModel.Visible(t.gameObject)&&t.name.Contains("Amount"))
            {var row=t.transform;while(row.parent!=null&&!row.name.Contains("(Clone)"))row=row.parent;parts.Add(StickerImage(row.gameObject)+", remaining to pack "+UiModel.Translate(t.text));}
        if(letter.moneyText!=null&&UiModel.Visible(letter.moneyText.gameObject))parts.Add("Order value "+UiModel.Translate(letter.moneyText.text)+" coins");
        if(letter.fameText!=null&&UiModel.Visible(letter.fameText.gameObject))parts.Add("XP: "+UiModel.Translate(letter.fameText.text));
        if(letter.dayText!=null&&UiModel.Visible(letter.dayText.gameObject)){string day=UiModel.Translate(letter.dayText.text);parts.Add(day=="∞"?"No deadline":"Days remaining "+day);}
        parts.Add(PackingMaterials(letter.fillingController));
        return string.Join(". ",parts);
    }
    internal static string PackingMaterials(FillingController? controller)
    {
        if(controller==null)return "";
        var paper=controller.paperDatas.FirstOrDefault(d=>d!=null&&controller.paperOpen!=null&&d.paperOpen==controller.paperOpen.sprite&&d.color.Equals(controller.paperOpen.color));
        var fill=controller.fillingDatas.FirstOrDefault(d=>d!=null&&controller.filling!=null&&d.fillingImage==controller.filling.sprite&&d.color.Equals(controller.filling.color));
        return "Packing choices. Paper: "+(paper==null?"none selected":UpgradeName(paper.GetUpgrade(),paper.name))+". Filling: "+(fill==null?"none selected":UpgradeName(fill.GetUpgrade(),fill.name))+". These are packing options; any customer requests are in the order message";
    }
    internal static string OrderIdentity(GameObject root)
    {
        var letter=root.GetComponentInChildren<LetterManager>(false);
        return letter?.currentCustomer==null?"":letter.currentOrderIndex+"|"+UiModel.Translate(letter.fromTextPhone?.text??letter.currentCustomer.Name);
    }
    internal static string? OrderSwitchReason(Selectable s)
    {
        var letter=s.GetComponentInParent<LetterManager>();
        if(letter==null||letter.nextPrevOrderButtons==null||!letter.nextPrevOrderButtons.Any(b=>b==s))return null;
        if(letter.elementsInLetter!=null&&letter.elementsInLetter.Count>0)return "Order switching is unavailable while stickers are in the box. Pack this order or remove its stickers first";
        if(!letter.packingEnabled)return "Packing is unavailable. Check the remaining day time and any current tutorial instruction";
        if(!letter.switchingOrdersEnabled)return "Order switching is temporarily unavailable while the order is opening";
        return "No other customer order is currently available";
    }
    private static string Price(UpgradeElement element)
    {
        string price=element.priceElement==null?"":UiModel.Translate(element.priceElement.text);
        if(price.Length==0)foreach(var t in element.GetComponentsInChildren<TMP_Text>(false))
            if(UiModel.Visible(t.gameObject)&&t.name.Contains("Coin Text")){price=UiModel.Translate(t.text);break;}
        return price.Length==0?element.upgrade.Price.ToString():price;
    }
    internal static string UpgradeName(UpgradeInfo? info,string fallback)=>info==null?UiModel.Pretty(fallback):UiModel.Translate(info.GetLocalizationTitleKey());
    internal static string UpgradeImage(StickerGame.StickerUpgradePreview preview)
    {
        var game=GameManager.Instance?.Game;if(game!=null&&preview.StickerPreview!=null&&preview.StickerPreview.sprite!=null)
            foreach(var id in game.StickerElementUpgrades){var info=UpgradeInfo.FromID(id);if(info!=null&&info.sprite==preview.StickerPreview.sprite)return UiModel.Translate(info.GetLocalizationTitleKey());}
        return "Element upgrade";
    }
    internal static string StickerImage(GameObject root)
    {
        var game=GameManager.Instance?.Game;if(game==null)return "Sticker";
        foreach(var image in root.GetComponentsInChildren<Image>(false))
            if(image.sprite!=null)foreach(var item in game.stickers)
                if(!item.MarkAsDeleted&&item.Sprite==image.sprite)return item.Name;
        return "Sticker";
    }
    private static string ShopColour(CustomColor c)
    {string name=c.name;return name.StartsWith("CustomColor")?UiModel.Pretty(name[11..]):Colour(c.mainColor);}
    internal static string Colour(Color c)
    {
        float hi=Math.Max(c.r,Math.Max(c.g,c.b)),lo=Math.Min(c.r,Math.Min(c.g,c.b));
        if(hi-lo<.09f)return hi>.88f?"White":hi<.13f?"Black":hi>.65f?"Light grey":hi<.35f?"Dark grey":"Grey";
        Color.RGBToHSV(c,out float h,out float sat,out float value);
        string name=h<.045f||h>=.95f?"Red":h<.12f?"Orange":h<.19f?"Yellow":h<.45f?"Green":h<.55f?"Cyan":h<.72f?"Blue":h<.84f?"Purple":"Pink";
        if(name=="Red"&&c.g>.4f&&c.b>.4f)name="Pink";
        return (value<.45f?"Dark ":sat<.35f&&name!="Pink"?"Light ":"")+name;
    }
    internal static void Clock()
    {
        var manager=GameManager.Instance;var game=manager==null?null:manager.Game;
        if(game==null){Speech.Say("No game clock on this screen.");return;}
        int minutes=Math.Max(0,game.MinutesLeft);
        Speech.Say($"Day {game.Day}. {minutes/60} hours, {minutes%60} minutes remaining in the working day.",true);
    }
    internal static string BorderWidth()=>"Border width: "+BorderColorChanger.GetPixelBorderSize()+" pixels";
}
