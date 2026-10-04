using System.Text.RegularExpressions;
using StickerGame;
using StickerGame.Input;
using StickerGame.Management;
using StickerGame.MainMenu;
using StickerGame.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace StickyBusinessAccess;

internal sealed record UiScreen(GameObject Root,string Kind,string Title,GameObject? ActionsRoot=null)
{
    internal int Id=>Root.GetInstanceID();
}

internal static class UiModel
{
    internal static bool Active(GameObject? go)=>go!=null&&go.activeInHierarchy;
    internal static bool Within(Transform child,Transform parent)=>child==parent||child.IsChildOf(parent);
    internal static string PathOf(Transform t)
    {string p=t.name;while(t.parent!=null){t=t.parent;p=t.name+"/"+p;}return p;}
    internal static string Order(Transform t)
    {string p=t.GetSiblingIndex().ToString("D5");while(t.parent!=null){t=t.parent;p=t.GetSiblingIndex().ToString("D5")+"/"+p;}return p;}
    internal static bool Visible(GameObject? go)
    {
        if(go==null||!Active(go))return false;
        foreach(var c in go.GetComponentsInParent<CanvasGroup>(false))
        {if(c.alpha<0.05f)return false;if(c.ignoreParentGroups)break;}
        var canvas=go.GetComponentInParent<Canvas>();
        return canvas==null||canvas.isActiveAndEnabled;
    }
    internal static GameObject? Child(GameObject root,string name)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(false))
            if(t.name==name&&Visible(t.gameObject))return t.gameObject;
        return null;
    }
    internal static UiScreen? Resolve()
    {
        // Modal canvas owns focus even when an underlying screen remains active.
        var popups=Object.FindObjectOfType<Popups>();
        if(popups!=null)
        {
            if(Active(popups.loadScreen))return new(popups.loadScreen,"loading","Loading");
            if(popups.Container!=null)
                for(int i=popups.Container.childCount-1;i>=0;i--)
                {
                    var t=popups.Container.GetChild(i);
                    if(Visible(t.gameObject)&&t.GetComponentsInChildren<Selectable>(false).Length>0)
                        return new(t.gameObject,"popup",t.name.Contains("Question")?"Confirmation":"Message");
                }
        }
        // Native ScreenSwitcher is authoritative while its old/new roots overlap in an animation.
        var manager=Object.FindObjectOfType<CanvasScreenManager>();
        var switcher=manager==null?null:manager.CurrentSwitcher;
        if(switcher!=null&&switcher.InAnimation)return null;
        var menu=Object.FindObjectOfType<MainMenuScreen>();
        if(menu!=null&&menu.isActiveAndEnabled&&(switcher==null||switcher.Screen==GameScreen.Menu))
        {
            if(menu.settings!=null&&menu.settings.isActiveAndEnabled)return Settings(menu.settings.gameObject);
            if(Active(menu.credits))return new(menu.credits,"credits","Credits");
            if(Active(menu.SaveGames))return new(menu.SaveGames,"saves","Load game");
            if(Active(menu.ButtonGroup))
            {
                var promo=Visible(menu.WishlistPopup)?menu.WishlistPopup:null;
                if(promo!=null)return new(promo,"popup","Announcement");
                return new(menu.ButtonGroup,"menu","Main menu");
            }
        }
        var newGame=Object.FindObjectOfType<NewGameScreen>();
        if(newGame!=null&&newGame.isActiveAndEnabled&&(switcher==null||switcher.Screen==GameScreen.NewGame))
            return new(newGame.gameObject,"newgame","New game setup");
        var settings=Object.FindObjectOfType<SettingsScreen>();
        if(settings!=null&&settings.isActiveAndEnabled)return Settings(settings.gameObject);
        if(switcher!=null&&switcher.Screen==GameScreen.PauseScreen)
            return new(switcher.gameObject,"pause","Pause menu");
        if(switcher!=null)
        {var downloaded=Child(switcher.gameObject,"Downloaded");if(downloaded!=null&&downloaded.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(downloaded,"popup","Image exported");}
        if(switcher!=null&&switcher.Screen==GameScreen.StickerCreator)
        {
            var save=Child(switcher.gameObject,"SaveScreen");
            if(save!=null&&save.GetComponentsInChildren<Selectable>(false).Any(s=>Hit(s)))return new(save,"creator-save","Save sticker");
        }
        // Read the first tutorial and hub without enabling sticker-editing shortcuts.
        foreach(var t in Object.FindObjectsOfType<StickerGame.Tutorial.TutorialTrigger>())
            if(t.isActiveAndEnabled&&Visible(t.gameObject)&&PathOf(t.transform).Contains("TutorialTextBox"))return new(t.gameObject,"tutorial","Tutorial",t.gameObject.GetComponentsInChildren<Selectable>(false).Any(s=>s.IsInteractable()&&Hit(s))?null:switcher?.gameObject);
        if(switcher!=null&&Active(switcher.gameObject))
        {
            if(switcher.Screen==GameScreen.StickerShop)
            {
                var custom=Child(switcher.gameObject,"Customization");if(custom!=null&&custom.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(custom,"shop-custom","Customize shop");
                var shop=Object.FindObjectOfType<StickerGame.Shop.StickerShopManager>();
                if(shop!=null)
                {
                    if(Active(shop.downloadedPopup)&&shop.downloadedPopup.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(shop.downloadedPopup,"popup","Image exported");
                    if(shop.renameStickerScreen!=null&&Visible(shop.renameStickerScreen.gameObject)&&shop.renameStickerScreen.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(shop.renameStickerScreen.gameObject,"shop-rename","Rename sticker");
                    if(Active(shop.detailedStickerView)&&shop.detailedStickerView.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(shop.detailedStickerView,"shop-preview","Sticker preview");
                }
                return new(switcher.gameObject,"shop","Sticker shop");
            }
            if(switcher.Screen==GameScreen.UpgradeScreen)return new(switcher.gameObject,"upgrades","Upgrades");
            if(switcher.Screen==GameScreen.SleepScreen)return new(switcher.gameObject,"sleep","End of day");
            if(switcher.Screen==GameScreen.StickerSending)return new(switcher.gameObject,"packing","Order packing");
            if(switcher.Screen==GameScreen.StickerCutter)
            {
                foreach(string panelName in new[]{"SheetPrintPopup","SheetDesignerPopup","SheetDesigner"})
                {var panel=Child(switcher.gameObject,panelName);if(panel!=null&&panel.GetComponentsInChildren<Selectable>(false).Any(Hit))return new(panel,panelName=="SheetDesigner"?"sheet":"production-popup",Pretty(panelName));}
                return new(switcher.gameObject,"production","Production");
            }
            if(switcher.Screen==GameScreen.Hubscreen)
            {
                var board=Object.FindObjectOfType<MessageBoardController>();
                if(board!=null&&Active(board.detailView))return new(board.detailView,"message-history","Customer history",Child(switcher.gameObject,"MessageBoard"));
                foreach(string name in new[]{"ThriftyPopup","HalloweenPopup","AdventPopup","MessageBoard"})
                {var panel=Child(switcher.gameObject,name);if(panel!=null)return new(panel,name=="MessageBoard"?"messages":"popup",Pretty(name));}
                return new(switcher.gameObject,"hub","Your shop");
            }
            if(switcher.Screen==GameScreen.PauseScreen)return new(switcher.gameObject,"pause","Pause menu");
            if(switcher.Screen==GameScreen.StickerCreator)return new(switcher.gameObject,"creator","Creative Corner");
            return new(switcher.gameObject,"unsupported",Pretty(switcher.Screen.ToString()));
        }
        return null;
    }
    private static UiScreen Settings(GameObject root)
    {
        BindingUi.Ensure(root);
        if(BindingUi.Panel is {} bindings)return new(bindings,"access-controls","Accessibility controls");
        foreach(string n in new[]{"SendLogsScreen","TwitchSettings","Cheats"})
        {var child=Child(root,n);if(child!=null)return new(child,"settings-sub",n=="TwitchSettings"?"Streamer settings":Pretty(n));}
        return new(root,"settings","Settings");
    }
    internal static List<Selectable> Gather(UiScreen screen)
    {
        InventoryAccess.Ensure(screen.ActionsRoot??screen.Root);
        GoodieAccess.Ensure(screen.ActionsRoot??screen.Root);
        var result=new List<Selectable>();
        var seen=new HashSet<int>();
        if(screen.Kind is "loading" or "unsupported")return result;
        foreach(var s in (screen.ActionsRoot??screen.Root).GetComponentsInChildren<Selectable>(false).Concat(screen.ActionsRoot==null?Enumerable.Empty<Selectable>():screen.Root.GetComponentsInChildren<Selectable>(false)).GroupBy(s=>s.GetInstanceID()).Select(g=>g.First()))
        {
            if(!s.isActiveAndEnabled||!Visible(s.gameObject)||s.TryCast<Scrollbar>()!=null)continue;
            if(s.GetComponentInParent<StickerGame.StickerLetter.GoodieElementSpawner>()!=null&&!GoodieAccess.Proxy(s))continue;
            if(!GoodieAccess.InSelectedCategory(s))continue;
            if(screen.Kind=="tutorial"&&!s.IsInteractable())continue;
            if(s.name=="FakeButton")continue; // tutorial's controller proxy duplicates the real Toggle
            if(HasAncestor(s.transform,"Dropdown List")||HasAncestor(s.transform,"Template"))continue;
            bool supported=s.TryCast<Button>()!=null||s.TryCast<Slider>()!=null||s.TryCast<Toggle>()!=null||s.TryCast<TMP_Dropdown>()!=null||s.TryCast<TMP_InputField>()!=null;
            if(!supported)continue;
            // ScrollRect children remain in the list even when clipped; focus scrolls them into view.
            var scroll=s.GetComponentInParent<ScrollRect>();
            if(scroll==null||scroll.content==null||!Within(s.transform,scroll.content))
            {if(!Hit(s)&&!GoodieAccess.Proxy(s)&&InventoryAccess.Label(s)==null)continue;}
            result.Add(s);seen.Add(s.GetInstanceID());
        }
        if(screen.Kind is "creator" or "tutorial")
            foreach(var s in CreatorAccess.Elements(screen.ActionsRoot??screen.Root))
                if((screen.Kind!="tutorial"||s.IsInteractable())&&seen.Add(s.GetInstanceID()))result.Add(s);
        if(screen.Kind is "sheet" or "packing" or "tutorial")
            foreach(var s in PrintedAccess.Elements(screen.ActionsRoot??screen.Root))
                if(seen.Add(s.GetInstanceID()))result.Add(s);
        return result.OrderBy(s=>Rank(s,screen)).ThenBy(s=>Order(s.transform),StringComparer.Ordinal).ToList();
    }
    internal static bool HasAncestor(Transform t,string name)
    {for(var p=t;p!=null;p=p.parent)if(p.name==name)return true;return false;}
    private static int Rank(Selectable s,UiScreen screen)
    {
        if(screen.Kind=="menu")return s.name switch {"Continue Button"=>0,"New Game Button"=>1,"Load Game"=>2,"Settings"=>3,"Credits"=>4,"Quit Game"=>5,_=>10};
        if(screen.Kind=="newgame")return s.name switch {"UserName"=>0,"Shop Name"=>1,"Back"=>20,"Continue Button_01"=>10,_=>5};
        if(screen.Kind=="production-popup"&&PathOf(s.transform).Contains("SheetPrintPopup"))return s.name=="Create Btn"?0:s.name=="Edit Btn"?5:s.name=="Delete Btn"?15:20;
        if(screen.Kind=="creator-save")return s.TryCast<TMP_InputField>()!=null?0:s.TryCast<TMP_Dropdown>()!=null?1:IsBack(s)?20:5;
        if(screen.Kind=="creator")return s.GetComponent<CategoryIcon>()!=null?0:CreatorAccess.Element(s)!=null?10:30;
        // Keep a cancel action available but at the end of the form.
        return IsBack(s)?20:0;
    }
    internal static bool Hit(Selectable s)
    {
        var es=EventSystem.current;var rt=s.transform.TryCast<RectTransform>();var canvas=s.GetComponentInParent<Canvas>();
        if(es==null||rt==null||canvas==null)return false;
        Camera? cam=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        var local=rt.rect;
        // Sliders often have no raycastable centre; test their graphic as well as the centre.
        var positions=new List<Vector3>{rt.TransformPoint(local.center)};
        if(s.targetGraphic!=null)positions.Add(s.targetGraphic.transform.position);
        foreach(var position in positions)
        {
            Vector2 p=RectTransformUtility.WorldToScreenPoint(cam,position);
            if(p.x<0||p.y<0||p.x>Screen.width||p.y>Screen.height)continue;
            var hits=new Il2CppSystem.Collections.Generic.List<RaycastResult>();
            es.RaycastAll(new PointerEventData(es){position=p},hits);
            if(hits.Count>0&&Within(hits[0].gameObject.transform,s.transform))return true;
        }
        return false;
    }
    internal static bool CanActivate(Selectable s)
    {
        var screen=Resolve();if(screen==null||!Visible(s.gameObject)||!GoodieAccess.InSelectedCategory(s))return false;
        bool belongs=Within(s.transform,screen.Root.transform)||(screen.ActionsRoot!=null&&Within(s.transform,screen.ActionsRoot.transform));
        if(!belongs)return false;
        // The controller proxies and read-only rows need not be mouse-raycast
        // targets. Screen resolution supplies actual popup/tutorial ownership.
        if(CreatorAccess.Element(s)!=null||PrintedAccess.Element(s)!=null||GoodieAccess.Proxy(s)||InventoryAccess.Label(s)!=null)return true;
        return Hit(s);
    }
    internal static string BlockReason(Selectable s)
    {
        var screen=Resolve();
        return screen?.Kind is "popup" or "tutorial" or "creator-save" or "production-popup"?screen.Title+" is open. "+Text(screen.Root)+". Use its controls to continue or close it.":"This item is not available on the current screen. Wait for the screen to finish opening.";
    }
    internal static void Reveal(Transform target)
    {
        var scroll=target.GetComponentInParent<ScrollRect>();var rt=target.TryCast<RectTransform>();
        if(scroll==null||rt==null||scroll.content==null||scroll.viewport==null||!Within(rt,scroll.content))return;
        // Catalogue browsing uses the layout already produced by the game's UI
        // frame. Forcing every canvas twice for each arrow scales with all DLC
        // elements. Other controls (including inventory rows) retain their flush.
        if(target.GetComponentInParent<StickerGame.StickerCreator.StickerElement>()==null&&target.GetComponentInParent<StickerGame.StickerLetter.PrintedStickerElement>()==null)Canvas.ForceUpdateCanvases();
        var a=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
        var b=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(4);
        rt.GetWorldCorners(a);scroll.viewport.GetWorldCorners(b);
        var shift=Vector3.zero;
        if(scroll.vertical){if(a[1].y>b[1].y)shift.y=b[1].y-a[1].y;else if(a[0].y<b[0].y)shift.y=b[0].y-a[0].y;}
        if(scroll.horizontal){if(a[0].x<b[0].x)shift.x=b[0].x-a[0].x;else if(a[2].x>b[2].x)shift.x=b[2].x-a[2].x;}
        if(shift.sqrMagnitude>0){scroll.StopMovement();scroll.content.position+=shift;Canvas.ForceUpdateCanvases();}
    }
    internal static bool IsBack(Selectable s)
    {
        if(s.TryCast<Button>()==null||s.GetComponentInParent<StickerGame.StickerCreator.StickerElement>()!=null)return false;
        var capture=s.GetComponent<ControllerButtonCapture>();
        return (PathOf(s.transform).Contains("/Customization/")&&s.name=="CloseButton")||(PathOf(s.transform).Contains("/Downloaded/")&&s.name=="SaveButton")||(capture!=null&&capture.useCancel)||s.name is "Back" or "Close" or "NoButton" or "Cancel" or "back btn";
    }
    internal static string Translate(string s)
    {
        if(string.IsNullOrEmpty(s))return "";
        try {var loca=StickerGame.Extern.Spellgarden.Localization.SpellLoca.Instance;if(loca!=null&&loca.HasKey(s))s=loca.Translate(s);}catch{}
        return Speech.Clean(s).Replace("\u200b","");
    }
    internal static string Pretty(string s)=>Regex.Replace(s.Replace("(Clone)","").Replace('_',' '),"(?<=[a-z])(?=[A-Z])"," ").Trim();
    internal static string Text(GameObject go,bool excludeInputs=false)
    {
        var texts=new List<string>();
        foreach(var t in go.GetComponentsInChildren<TMP_Text>(false))
        {
            if(!Visible(t.gameObject))continue;
            if(excludeInputs&&t.GetComponentInParent<TMP_InputField>()!=null)continue;
            string s=Translate(t.text);if(s.Length>0&&!texts.Contains(s))texts.Add(s);
        }
        return string.Join(". ",texts);
    }
    internal static string Label(Selectable s)
    {
        string path=PathOf(s.transform);
        string? inventory=InventoryAccess.Label(s);if(inventory!=null)return inventory;
        string? goodie=GoodieAccess.Label(s);if(goodie!=null)return goodie;
        string? specific=GameUi.Label(s);if(specific!=null)return specific;
        if(path.EndsWith("/Tablet/StickerCreator"))return "Creative Corner";
        var category=s.GetComponent<CategoryIcon>();
        if(category!=null)return "Category: "+Pretty(category.category?.name??s.name).Replace(" Items","").Replace("Scroll View ","");
        var element=s.GetComponentInParent<StickerGame.StickerCreator.StickerElement>();
        if(element!=null&&(s==element.controllerButton||s.name=="StickerSprite"))
            return (element.moved?"Placed element: ":"Add element: ")+ElementName(element);
        if(path.Contains("/BackgroundSize/"))return s.name=="ScaleBigger"?"Increase border width":"Decrease border width";
        if(s.name=="UpButton"&&path.Contains("IconScrollView"))return "Scroll categories up";
        if(s.name=="DownButton"&&path.Contains("IconScrollView"))return "Scroll categories down";
        if(path.Contains("Social Buttons Container/"))return s.name switch {"Social_01"=>"Twitter, external link","Social_02"=>"TikTok, external link","Social_03"=>"Discord, external link",_=>Text(s.gameObject)};
        var ng=s.GetComponentInParent<NewGameScreen>();
        if(ng!=null)
        {
            if(s==ng.personName)return "Your name";
            if(s==ng.shopName)return "Shop name";
            if(s==ng.Button)return "Create new game";
        }
        var save=s.GetComponentInParent<StickerGame.StickerCreator.SaveScreen>();
        if(save!=null)
        {
            if(s==save.nameInput)return "Sticker name";
            if(s==save.dropdown)return "Sticker material";
            if(s==save.saveButton)return "Save sticker";
            if(s.name=="DownloadButton")return "Export sticker image";
        }
        if(s.name is "YesButton" or "NoButton" or "OkButton")return s.name[..^6];
        if(IsBack(s))return s.name=="Close"?"Close":"Back";
        // A dropdown caption is the value. Its sibling text is the label.
        if(s.TryCast<Slider>()!=null||s.TryCast<TMP_Dropdown>()!=null||s.TryCast<Toggle>()!=null||s.TryCast<TMP_InputField>()!=null)
        {
            if(s.TryCast<Toggle>()!=null){string ownLabel=Text(s.gameObject);if(ownLabel.Length>0)return ownLabel;}
            var input=s.TryCast<TMP_InputField>();
            if(input!=null)
            {
                var hint=s.GetComponent<StickerGame.UserInterface.UIHoverInfoText>();
                if(hint!=null){string label=Translate(hint.Text);if(label.Length>0)return label;}
            }
            var root=s.transform.parent;
            if(root!=null)
                foreach(var t in root.GetComponentsInChildren<TMP_Text>(false))
                    if(!Within(t.transform,s.transform)&&t.GetComponentInParent<Selectable>()==null&&Visible(t.gameObject))
                    {string label=Translate(t.text);if(label.Length>0)return label;}
            if(input!=null&&input.placeholder!=null){string placeholder=Text(input.placeholder.gameObject);if(placeholder.Length>0)return placeholder;}
            return Pretty(root?.name??s.name);
        }
        var slot=s.GetComponentInParent<SaveGameSlot>();
        if(slot!=null)return (s==slot.deleteButton?"Delete save. ":"Load save. ")+Text(slot.gameObject);
        string own=Text(s.gameObject);if(own.Length>0)return own;
        var hover=s.GetComponent<StickerGame.UserInterface.UIHoverInfoText>();
        if(hover!=null){string help=Translate(hover.Text);if(help.Length>0)return help;}
        if(path.Contains("TutorialTextBox")&&s.TryCast<Button>()!=null)return "Continue tutorial";
        return Pretty(s.name);
    }
    internal static string Describe(Selectable s)
    {
        string label=Label(s);string disabled=s.IsInteractable()?"":". Unavailable";
        string? specific=GameUi.Describe(s);if(specific!=null)return specific+disabled;
        var element=CreatorAccess.Element(s);
        if(element!=null)return label+(element.moved?", placed element":", element choice");
        var slider=s.TryCast<Slider>();
        if(slider!=null)return label+", slider, "+Math.Round((slider.value-slider.minValue)/Math.Max(.0001f,slider.maxValue-slider.minValue)*100)+" percent of range"+disabled;
        var toggle=s.TryCast<Toggle>();
        if(toggle!=null)return label+", "+(toggle.isOn?"checked":"not checked")+disabled;
        var dd=s.TryCast<TMP_Dropdown>();
        if(dd!=null)return label+", dropdown, "+(dd.value>=0&&dd.value<dd.options.Count?Translate(dd.options[dd.value].text):"no selection")+disabled;
        var field=s.TryCast<TMP_InputField>();
        if(field!=null)return label+", edit field, "+(field.contentType==TMP_InputField.ContentType.Password||PathOf(field.transform).Contains("/AuthInput/")?"protected":string.IsNullOrEmpty(field.text)?"empty":field.text)+(field.characterLimit>0?", maximum "+field.characterLimit+" characters":"")+disabled;
        return label+", button"+disabled;
    }
    internal static string ElementName(StickerGame.StickerCreator.StickerElement element)
    {
        var upgrade=element.Upgrade;
        return upgrade==null?Pretty(element.name):Translate(upgrade.GetLocalizationTitleKey());
    }
}
