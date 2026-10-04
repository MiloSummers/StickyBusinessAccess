using System.Runtime.InteropServices;
using StickerGame;
using StickerGame.Input;
using StickerGame.UserInterface;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StickyBusinessAccess;

public sealed class MenuAccess : MonoBehaviour
{
    public MenuAccess(IntPtr pointer):base(pointer){}
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
    private readonly Dictionary<int,bool> held=new();
    private readonly Dictionary<int,bool> physicalHeld=new();
    private HashSet<int> physicalPressed=new();
    private static readonly int[] Keys={8,9,13,16,17,18,20,27,32,33,34,35,36,37,38,39,40,45,46,48,49,50,51,52,53,54,55,56,57,66,67,69,72,79,81,82,84,107,109,112,113,114,115,117,118,119,120,187,189,219,221};
    private readonly Dictionary<int,int> focusMemory=new();
    private List<Selectable> items=new();
    private readonly Dictionary<int,int> sectionMemory=new();
    private readonly Dictionary<int,int> sections=new();
    private int selectedCategory;
    private float gatherAt;
    private bool SectionNavigation=>screen?.Kind is "creator" or "sheet" or "packing" or "shop" or "shop-custom" or "upgrades";
    private string SectionName(int section)=>screen?.Kind switch
    {
        "shop"=>new[]{"Categories","Stickers","Placement area","Controls"}[section],
        "shop-custom"=>new[]{"Categories","Colours","Backgrounds","Controls"}[section],
        "upgrades"=>new[]{"Categories","Items","Placement area","Controls"}[section],
        _=>new[]{"Categories","Items","Placement area","Controls"}[section]
    };
    private string returnChoice="";
    private int returnChoiceId;
    private static string ChoiceKey(Selectable s)
    {
        if(CreatorAccess.Element(s) is {} e)return "element:"+(e.Upgrade?.ID??e.name);
        if(PrintedAccess.Element(s) is {} p)return "sticker:"+p.StickerItem?.ID;
        return "";
    }
    private int Section(Selectable s)
        =>sections.TryGetValue(s.GetInstanceID(),out int section)?section:Classify(s);
    private int Classify(Selectable s)
    {
        if(screen?.Kind=="upgrades")
        {
            if(s.GetComponentInParent<StickerGame.UserInterface.UpgradesTopBar>()!=null)return 0;
            if(s.GetComponentInParent<StickerGame.UpgradeScreen.UpgradeElement>()!=null)return 1;
        }
        if(screen?.Kind=="shop"&&s.GetComponentInParent<StickerGame.Shop.StickerShopElement>()!=null)return 1;
        if(screen?.Kind=="shop-custom")
        {
            if(s.GetComponent<ColorButton>()!=null)return 1;
            if(s.GetComponent<BackgroundDecoButton>()!=null)return 2;
        }
        if(s.GetComponent<CategoryIcon>()!=null||GameUi.PackingTab(s)!=null)return 0;
        if(CreatorAccess.Element(s) is {} e)return e.moved?2:1;
        if(PrintedAccess.Element(s) is {} p)return p.moved?2:1;
        if(GoodieAccess.Element(s)!=null)return 2;
        if(s.GetComponentInParent<StickerGame.StickerLetter.GoodieElementSpawner>()!=null||s.GetComponentInParent<StickerGame.StickerLetter.PaperSelector>()!=null||s.GetComponentInParent<StickerGame.StickerLetter.FillingSelector>()!=null)return 1;
        return 3;
    }
    private void JumpSection(int section)
    {
        readingText=false;
        int index=items.FindIndex(s=>Section(s)==section&&sectionMemory.GetValueOrDefault(section)==s.GetInstanceID());
        if(index<0)index=items.FindIndex(s=>Section(s)==section);
        string name=SectionName(section);
        if(index<0){Speech.Say(name+" has no available items.",true);return;}
        Focus(index,false);Speech.Say(name+". "+FocusDescription(),true);
    }
    private void CycleSection(int direction)
    {
        int section=Current==null?3:Section(Current);
        for(int step=1;step<=4;step++){int next=(section+direction*step+8)%4;if(items.Any(s=>Section(s)==next)){JumpSection(next);return;}}
    }
    internal void ReturnToChoice(bool announce=true)
    {
        gatherAt=0;Scan();
        int index=items.FindIndex(s=>Section(s)==1&&s.GetInstanceID()==returnChoiceId);
        if(index<0&&returnChoice.Length>0)index=items.FindIndex(s=>Section(s)==1&&ChoiceKey(s)==returnChoice);
        if(index>=0){Focus(index,announce);return;}
        int fallback=items.FindIndex(s=>Section(s)==1);
        if(fallback>=0)Focus(fallback,false);
        if(announce)Speech.Say("That sticker choice is no longer available. "+(fallback>=0?FocusDescription():"No item choices are available."),true);
    }
    internal void InvalidateControls(){gatherAt=0;scanAt=0;}
    internal void RemoveControl(int id){items.RemoveAll(s=>s==null||s.GetInstanceID()==id);sections.Remove(id);InvalidateControls();}
    private UiScreen? screen;
    private int focusId;
    private ScreenText reader=new();
    private string signature="",lastValue="",lastValidation="",pendingText="",spokenText="";
    private float scanAt,connectionAt,settleAt,transitionUntil,errorUntil;
    private bool loaded,foreground,screenReady;
    private float editGuardUntil,repeatAt,sliderSpeechAt;
    private int repeatDirection;
    private bool sliderPending;
    private string pendingOrder="",spokenOrder="";
    private float orderAt;
    internal static bool ProtectEditExit=>Instance!=null&&(Instance.editing!=null||Time.unscaledTime<Instance.editGuardUntil);
    internal static bool AccessibilityModifierDown=>new[]{17,18,20,45,91,92}.Any(k=>(GetAsyncKeyState(k)&0x8000)!=0);
    internal static bool ControlKeyDown=>new[]{8,9,13,27,32,37,38,39,40}.Any(k=>(GetAsyncKeyState(k)&0x8000)!=0);
    internal static bool GuardNative=>OwnsNavigation||ProtectEditExit;
    private TMP_InputField? editing;
    private TMP_Dropdown? dropdown;
    private int option;
    private string originalEdit="",reviewText="";
    private int reviewCaret;
    private bool readingText;
    internal static MenuAccess? Instance;
    internal static bool OwnsNavigation {get;private set;}
    internal static bool PlacementHeld=>Instance!=null&&new[]{37,38,39,40}.Any(k=>Instance.held.TryGetValue(k,out bool down)&&down);
    internal static bool InForeground
    {
        get {GetWindowThreadProcessId(GetForegroundWindow(),out uint p);return p==Environment.ProcessId;}
    }
    internal static bool KeyboardSubmitOrCancelHeld=>(GetAsyncKeyState(27)&0x8000)!=0||(GetAsyncKeyState(13)&0x8000)!=0;
    public void OnApplicationQuit() => Speech.Shutdown();
    public void Update()
    {
        Instance=this;
        if(Time.unscaledTime<errorUntil)return;
        try {Tick();}
        catch(Exception e)
        {
            Plugin.Logger.LogError("[access-error] "+e);
            OwnsNavigation=false;errorUntil=Time.unscaledTime+5;scanAt=0;
            Speech.Say("Menu access encountered an error. F8 records diagnostics. Please retain the log.");
        }
    }
    private void Tick()
    {
        physicalPressed=new HashSet<int>();
        for(int key=8;key<=254;key++)
        {bool down=(GetAsyncKeyState(key)&0x8000)!=0;
#if SMOKE_TEST
            down|=MenuSmoke.HeldKey==key&&Time.unscaledTime<MenuSmoke.HeldUntil;
#endif
            if(down&&(!physicalHeld.TryGetValue(key,out bool before)||!before))physicalPressed.Add(key);physicalHeld[key]=down;}
        foreach(int key in Keys)held[key]=physicalHeld.TryGetValue(AccessBindings.Physical(key),out bool down)&&down;
        bool movingHeld=new[]{37,38,39,40}.Any(k=>held[k]);
        if(!movingHeld)placementHeldSince=-1;else if(placementHeldSince<0)placementHeldSince=Time.unscaledTime;
        var pressed=AccessBindings.Translate(physicalPressed);
        foreground=InForeground;
#if SMOKE_TEST
        foreground=true;
#endif
        if(!foreground){OwnsNavigation=false;return;}
        if(!loaded)
        {loaded=true;Speech.CheckConnected();Speech.Say("Sticky Business Access 0.8.2 loaded.",true);}
        if(Time.unscaledTime>=connectionAt)
        {connectionAt=Time.unscaledTime+1;if(Speech.CheckConnected())Speech.Say("Sticky Business Access connected. "+(screen?.Title??"Waiting for a screen")+".",true);}
        if(Time.unscaledTime>=scanAt)
        {scanAt=Time.unscaledTime+.15f;Scan();}
        OwnsNavigation=screenReady&&screen!=null&&screen.Kind is not "unsupported" and not "loading";
        PrintedAccess.Tick();GoodieAccess.Tick();SaveAccess.Tick();ReviewEditing();
        if(!BindingUi.Handle(physicalPressed)&&!InformationAccess.Handle(pressed,editing!=null))HandleKeys(pressed);
#if SMOKE_TEST
        MenuSmoke.Tick(this);
#endif
    }
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    private void HandleKeys(HashSet<int> pressed)
    {
        if(pressed.Count>0&&items.Any(s=>s==null||!s.isActiveAndEnabled)){InvalidateControls();if(screen!=null)Scan();}
        bool modifier=held[17]||held[18]||held[20]||held[45];
        if(modifier)return;
        if(editing==null&&pressed.Contains(112)){Help();return;}
        if(editing==null&&pressed.Contains(113)){Speech.Repeat();return;}
        if(editing==null&&pressed.Contains(119)){Dump();return;}
        if(Time.unscaledTime<transitionUntil)return;
        // Retain an explicit editing session even if TMP consumes Enter/Escape before this Update.
        if(editing!=null)
        {
            if(physicalPressed.Contains(27)){FinishEditing(true);return;}
            if(physicalPressed.Contains(13)){FinishEditing(false);return;}
            if(physicalPressed.Contains(9))
            {
                FinishEditing(false,false);Move(held[16]?-1:1,false);
                var nextField=Current?.TryCast<TMP_InputField>();
                if(nextField!=null&&nextField.IsInteractable())BeginEditing(nextField,"Finished previous field. ");
                else Speech.Say("Finished editing. "+FocusDescription(),true);
                return;
            }
            // ConsoleTextField can lose TMP focus while its own input callbacks run.
            // Keep the editing session until an explicit finish or a real screen change.
            if(!editing.isFocused&&!physicalHeld.GetValueOrDefault(13)&&!physicalHeld.GetValueOrDefault(27))
            {EventSystem.current?.SetSelectedGameObject(editing.gameObject);editing.ActivateInputField();}
            return; // ordinary letters, arrows, space, home/end, delete and Backspace belong to TMP
        }
        var selected=EventSystem.current?.currentSelectedGameObject;
        var activeInput=selected==null?null:selected.GetComponent<TMP_InputField>();
        if(activeInput!=null&&activeInput.isFocused)
        {editing=activeInput;originalEdit=activeInput.text;reviewText=activeInput.text;reviewCaret=activeInput.stringPosition;focusId=activeInput.GetInstanceID();return;}
        if(pressed.Contains(72)){Help();return;}
        if(pressed.Contains(82)){Speech.Repeat();return;}
        if(dropdown!=null){HandleDropdown(pressed);return;}
        if(pressed.Contains(115)){if(screen!=null){CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;PrintedAccess.Editing=null;GoodieAccess.Editing=null;readingText=true;reader.Read(screen,0);}return;}
        if(pressed.Contains(114)){Speech.Say(items.Count==0?"No available controls.":string.Join(". ",items.Select(UiModel.Describe)),true);return;}
        if(pressed.Contains(33)||pressed.Contains(34)){if(screen!=null)reader.Read(screen,pressed.Contains(34)?1:-1);return;}
        if(!OwnsNavigation)return;
        if(pressed.Contains(46))
        {
            var goodie=GoodieAccess.Editing??(Current==null?null:GoodieAccess.Element(Current));
            if(goodie!=null){GoodieAccess.Delete(goodie);gatherAt=0;scanAt=0;return;}
            var printed=PrintedAccess.Editing??(Current==null?null:PrintedAccess.Element(Current));
            if(printed!=null&&printed.moved){PrintedAccess.Delete(printed);gatherAt=0;scanAt=0;return;}
            var element=CreatorAccess.Editing??(Current==null?null:CreatorAccess.Element(Current));
            if(element!=null&&element.moved){CreatorAccess.Delete(element);gatherAt=0;scanAt=0;return;}
        }
        if(pressed.Contains(84)){GameUi.Clock();return;}
        if(pressed.Contains(66)){string balance=GameUi.BalanceText();Speech.Say(balance.Length==0?"No balance displayed on this screen.":balance,true);return;}
        if(pressed.Contains(79)){if(screen!=null)Speech.Say(GameUi.OrderText(screen.Root),true);return;}
        if(pressed.Contains(67)&&(screen?.Kind is "sheet" or "packing"))
        {var copy=PrintedAccess.Editing??(Current==null?null:PrintedAccess.Element(Current));Speech.Say(copy!=null&&copy.moved?PrintedAccess.Coordinates(copy):"Select a placed sticker to read coordinates.",true);return;}
        if(pressed.Contains(48)){Back();return;}
        if(SectionNavigation&&(pressed.Contains(117)||pressed.Contains(118)||pressed.Contains(120)))
        {
            if(PrintedAccess.Editing!=null)PrintedAccess.Handle(new HashSet<int>{13},held[16]);
            if(PrintedAccess.Editing!=null)return;
            CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;GoodieAccess.Editing=null;
            JumpSection(pressed.Contains(117)?0:pressed.Contains(118)?1:screen?.Kind is "shop" or "upgrades"?3:2);return;
        }
        if(screen?.Kind=="hub"&&pressed.Any(k=>k>=49&&k<=56)){JumpArea(pressed.First(k=>k>=49&&k<=56)-49);return;}
        if(GoodieAccess.Handle(PlacementKeys(pressed),held[16]||FastPlacement))return;
        if(PrintedAccess.Editing!=null)
        {
            if(pressed.Contains(9)||pressed.Contains(13)){PrintedAccess.Handle(pressed,held[16]);if(PrintedAccess.Editing==null)ReturnToChoice();return;}
            if(PrintedAccess.Handle(PlacementKeys(pressed),held[16]||FastPlacement))return;
        }
        if(screen?.Kind is "sheet" or "packing")
        {
            if(pressed.Contains(118)){PrintedAccess.Editing=null;Focus(items.FindIndex(s=>PrintedAccess.Element(s) is {} e&&!e.moved));return;}
            if(pressed.Contains(120)){PrintedAccess.Editing=null;GoodieAccess.Editing=null;Focus(items.FindIndex(s=>(PrintedAccess.Element(s) is {} e&&e.moved)||GoodieAccess.Element(s)!=null));return;}
        }
        if(screen?.Kind=="creator"||CreatorAccess.Editing!=null)
        {
            if(CreatorAccess.Editing!=null&&(pressed.Contains(9)||pressed.Contains(13)))
            {CreatorAccess.Handle(pressed,held[16]);ReturnToChoice();return;}
            if(CreatorAccess.Handle(PlacementKeys(pressed),held[16]||FastPlacement))return;
            if(pressed.Contains(117)){CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;Focus(items.FindIndex(s=>s.GetComponent<CategoryIcon>()!=null));return;}
            if(pressed.Contains(118)){CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;Focus(items.FindIndex(s=>CreatorAccess.Element(s) is {} e&&!e.moved));return;}
            if(pressed.Contains(120)){CreatorAccess.Editing?.Deselect();CreatorAccess.Editing=null;Focus(items.FindIndex(s=>CreatorAccess.Element(s) is {} e&&e.moved));return;}
        }
        if(pressed.Contains(27)||pressed.Contains(8)){Back();return;}
        if(pressed.Contains(9)){readingText=false;if(SectionNavigation)CycleSection(held[16]?-1:1);else Move(held[16]?-1:1);return;}
        if(pressed.Contains(38)||pressed.Contains(40))
        {if(screen!=null&&(readingText||screen.Kind is "credits" or "sleep"))reader.Read(screen,pressed.Contains(38)?-1:1);else Move(pressed.Contains(38)?-1:1);return;}
        if(Current?.TryCast<Slider>()!=null)
        {
            int direction=held[37]==held[39]?0:held[37]?-1:1;
            if(pressed.Contains(37)||pressed.Contains(39)){direction=pressed.Contains(37)?-1:1;repeatDirection=direction;repeatAt=Time.unscaledTime+.35f;Adjust(direction,held[16]);return;}
            if(direction!=0&&direction==repeatDirection&&Time.unscaledTime>=repeatAt){repeatAt=Time.unscaledTime+.06f;Adjust(direction,held[16],true);return;}
            if(direction==0){repeatDirection=0;if(sliderPending){sliderPending=false;AnnounceValue();}}
        }
        else repeatDirection=0;
        if(pressed.Contains(37)||pressed.Contains(39)){Adjust(pressed.Contains(37)?-1:1,held[16]);return;}
        if(pressed.Contains(36)){Focus(SectionNavigation&&Current!=null?items.FindIndex(s=>Section(s)==Section(Current)):0);return;}
        if(pressed.Contains(35)){Focus(SectionNavigation&&Current!=null?items.FindLastIndex(s=>Section(s)==Section(Current)):items.Count-1);return;}
        if(pressed.Contains(13)||pressed.Contains(32)){Activate();return;}
        WatchChanges();
    }
    private float movementAt;
    private float placementHeldSince=-1;
    private bool FastPlacement=>placementHeldSince>=0&&Time.unscaledTime-placementHeldSince>=.75f;
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    private HashSet<int> PlacementKeys(HashSet<int> pressed)
    {
        var result=new HashSet<int>(pressed);
        if(pressed.Any(k=>k>=37&&k<=40))movementAt=Time.unscaledTime+.25f;
        else if(Time.unscaledTime>=movementAt&&new[]{37,38,39,40}.Any(k=>held[k]))
        {movementAt=Time.unscaledTime+.025f;foreach(int key in new[]{37,38,39,40})if(held[key])result.Add(key);}
        return result;
    }
    internal void Scan()
    {
        var next=UiModel.Resolve();
        screenReady=next!=null;
        if(next==null){OwnsNavigation=false;return;}
        bool changed=screen==null||screen.Id!=next.Id;
        if(!changed&&dropdown!=null)return;
        int categoryId=CategoryIcon.CurrentSelected==null?0:CategoryIcon.CurrentSelected.GetInstanceID();
        if(categoryId!=selectedCategory){selectedCategory=categoryId;gatherAt=0;}
        if(!changed&&Time.unscaledTime<gatherAt)return;
        gatherAt=Time.unscaledTime+(next.Kind is "creator" or "sheet" or "packing"?1f:.15f);
        if(changed)
        {
            sectionMemory.Clear();returnChoice="";returnChoiceId=0;
            CreatorAccess.Editing=null;PrintedAccess.Editing=null;GoodieAccess.Editing=null;InformationAccess.Close();
            Remember();
            if(dropdown!=null){dropdown.Hide();dropdown=null;}
            if(editing!=null){editing.DeactivateInputField();editing=null;EventSystem.current?.SetSelectedGameObject(null);}
            readingText=false;repeatDirection=0;sliderPending=false;screen=next;signature="";lastValidation="";pendingText="";spokenText="";
            reader=new ScreenText();reader.Refresh(screen);
            if(screen.Kind=="credits")
                foreach(var auto in screen.Root.GetComponentsInChildren<ScrollOnEnable>(false))auto.StopAllCoroutines();
        }
        bool previouslyEmpty=items.Count==0;
        items=UiModel.Gather(next);
        sections.Clear();if(SectionNavigation)foreach(var item in items)sections[item.GetInstanceID()]=Classify(item);
        string sig=string.Join(",",items.Select(s=>s.GetInstanceID()));
        if(changed||!items.Any(s=>s.GetInstanceID()==focusId))
        {
            int saved=focusMemory.TryGetValue(next.Id,out var id)?id:0;
            var first=items.FirstOrDefault(s=>s.GetInstanceID()==saved)??items.FirstOrDefault();
            focusId=first==null?0:first.GetInstanceID();
            lastValue=first==null?"":UiModel.Describe(first);
            if(first!=null)UiModel.Reveal(first.transform);
        }
        if(next.Kind=="access-controls"&&BindingUi.PreferredFocus is {} preferred)
        {
            var target=items.FirstOrDefault(s=>UiModel.Label(s).StartsWith(preferred+": "));
            if(target!=null){focusId=target.GetInstanceID();lastValue=UiModel.Describe(target);}
            BindingUi.PreferredFocus=null;
        }
        if(changed)
        {
            string intro=reader.Intro(next);
            if(next.Kind=="packing"){pendingOrder=spokenOrder=GameUi.OrderIdentity(next.Root);}
            if(next.Kind=="tutorial"&&items.Count>1)intro+=" Other available actions: "+string.Join(". ",items.Where(s=>s.GetInstanceID()!=focusId).Select(UiModel.Label))+".";
            lastValue=Current==null?"":UiModel.Describe(Current);
            if(next.Kind!="menu"||items.Count>0)Speech.Say(next.Title+". "+intro+" "+FocusDescription(),interrupt:true);
            Plugin.Logger.LogInfo("[screen] "+next.Kind+" | "+UiModel.PathOf(next.Root.transform));
            LogControls();
            if(next.Kind is "popup" or "tutorial")spokenText=reader.Signature;
        }
        else if(sig!=signature)
        {
            Plugin.Logger.LogInfo("[controls-changed] "+sig);
            if(previouslyEmpty&&items.Count>0&&next.Kind=="menu")Speech.Say(next.Title+". "+FocusDescription(),true);
            if(next.Kind is "tutorial" or "creator-save")Speech.Say("Available actions updated. "+string.Join(". ",items.Select(UiModel.Describe)),false);
        }
        signature=sig;
        // Recognize a dropdown opened with the mouse as the same keyboard sub-context.
        if(Time.unscaledTime>=transitionUntil)foreach(var s in items)
        {var dd=s.TryCast<TMP_Dropdown>();if(dd!=null&&dd.IsExpanded){dropdown=dd;option=dd.value;focusId=s.GetInstanceID();SayOption();break;}}
    }
    private Selectable? Current=>items.FirstOrDefault(s=>s!=null&&s.GetInstanceID()==focusId);
    private string FocusDescription()
    {
        int i=items.FindIndex(s=>s.GetInstanceID()==focusId);if(i<0)return "No selectable controls.";
        string description=UiModel.Describe(items[i]);
        if(SectionNavigation){int section=Section(items[i]);var group=items.Where(s=>Section(s)==section).ToList();return description+$". {group.FindIndex(s=>s.GetInstanceID()==focusId)+1} of {group.Count} in "+SectionName(section).ToLowerInvariant();}
        return description+(screen?.Kind=="sleep"?"":$". {i+1} of {items.Count}");
    }
    private void Remember(){if(screen!=null&&focusId!=0)focusMemory[screen.Id]=focusId;}
    internal void Move(int direction,bool announce=true)
    {
        if(SectionNavigation&&Current!=null)
        {int section=Section(Current);var indices=items.Select((s,i)=>(s,i)).Where(x=>Section(x.s)==section).Select(x=>x.i).ToList();int index=indices.IndexOf(items.FindIndex(s=>s.GetInstanceID()==focusId));Focus(indices[NavigationRules.Move(index,indices.Count,direction)],announce);}
        else Focus(NavigationRules.Move(items.FindIndex(s=>s.GetInstanceID()==focusId),items.Count,direction),announce);
    }
    internal void Focus(int index,bool announce=true)
    {
        if(index<0||index>=items.Count){Speech.Say("No selectable controls.");return;}
        readingText=false;repeatDirection=0;sliderPending=false;var s=items[index];focusId=s.GetInstanceID();Remember();UiModel.Reveal(s.transform);
        if(SectionNavigation)sectionMemory[Section(s)]=focusId;
        lastValue=UiModel.Describe(s);if(announce)Speech.Say(FocusDescription(),true);
    }
    internal void Activate()
    {
        var s=Current;if(s==null){Focus(0);return;}
        if(SectionNavigation&&Section(s) is 1 or 2&&ChoiceKey(s) is {Length:>0} key){returnChoice=key;returnChoiceId=Section(s)==1?s.GetInstanceID():0;}
        if(CreatorAccess.Element(s)!=null&&CreatorAccess.Activate(s)){lastValue=UiModel.Describe(s);gatherAt=0;scanAt=0;return;}
        if(PrintedAccess.Activate(s)){lastValue=UiModel.Describe(s);gatherAt=0;scanAt=0;return;}
        if(!s.IsInteractable()){Speech.Say(UiModel.Describe(s)+". "+(GameUi.OrderSwitchReason(s)??Validation()),true);return;}
        UiModel.Reveal(s.transform);
        if(InventoryAccess.Read(s)||GoodieAccess.Activate(s)){gatherAt=0;scanAt=0;return;}
        if(!UiModel.CanActivate(s)){Speech.Say("This control is covered or not yet visible. Please wait for the screen to finish opening.");scanAt=0;return;}
        var field=s.TryCast<TMP_InputField>();
        if(field!=null)
        {
            BeginEditing(field);return;
        }
        var dd=s.TryCast<TMP_Dropdown>();
        if(dd!=null){dropdown=dd;option=dd.value;Remember();dd.Show();SayOption();return;}
        var toggle=s.TryCast<Toggle>();
        if(toggle!=null){toggle.isOn=!toggle.isOn;AnnounceValue();return;}
        var button=s.TryCast<Button>();
        if(button!=null)
        {
            if(button.name=="MoreFromSpellgardenBtn")
            {
                // The native callback always requests the Steam overlay. It has
                // no browser fallback when the overlay is disabled/inaccessible.
                Application.OpenURL("https://store.steampowered.com/publisher/Spellgarden");
                Speech.Say("Opened the Spellgarden Games Steam publisher page in your browser.",true);return;
            }
            Remember();Plugin.Logger.LogInfo("[activate] "+UiModel.PathOf(button.transform)+" | "+UiModel.Label(button));
            var category=button.GetComponent<CategoryIcon>();
            string before=UiModel.Label(button);
            button.onClick.Invoke();
            bool categorySelection=category!=null||GameUi.PackingTab(button)!=null||(screen?.Kind=="upgrades"&&button.GetComponentInParent<StickerGame.UserInterface.UpgradesTopBar>()!=null);
            Transition(categorySelection?.05f:.25f);
            if(categorySelection)Scan();
            if(GameUi.Inventory(button) is {} inventory){lastValue=UiModel.Describe(button);Speech.Say(lastValue+". "+GameUi.InventoryRows(inventory),true);}
            else if(GameUi.PackingSelection(button) is {} packing){lastValue=UiModel.Describe(button);Speech.Say(packing,true);}
            else if(category!=null)Speech.Say(before.Replace("Category: ","")+" selected.",true);
            else if(before.StartsWith("Outline colour:"))Speech.Say("Outline colour "+GameUi.Colour(StickerGame.StickerCreator.BorderColorChanger.Instance.color)+" selected.",true);
            else if(before.Contains("border width")){lastValue=UiModel.Describe(button);Speech.Say(GameUi.BorderWidth(),true);}
            else if(before.StartsWith("Shop colour:")||before.StartsWith("Shop background:"))Speech.Say(UiModel.Describe(button),true);
            else if(before.StartsWith("Scroll categories"))Speech.Say("Category strip scrolled. Available categories: "+string.Join(". ",(screen?.ActionsRoot??screen?.Root)?.GetComponentsInChildren<CategoryIcon>(false).Where(c=>UiModel.Visible(c.gameObject)).Select(c=>UiModel.Pretty(c.category?.name??c.name).Replace(" Items", ""))??Enumerable.Empty<string>())+". Tab reaches the categories, including those outside the viewport.",true);
            return;
        }
        Speech.Say("Use Left and Right to adjust this control.");
    }
    private void Adjust(int direction,bool large,bool repeated=false)
    {
        var s=Current;if(s==null)return;
        var slider=s.TryCast<Slider>();
        if(slider!=null)
        {
            if(!slider.IsInteractable()){Speech.Say(UiModel.Describe(slider));return;}
            UiModel.Reveal(slider.transform);if(!UiModel.Hit(slider)){Speech.Say("This slider is covered.");return;}
            float step=slider.wholeNumbers?1:(slider.maxValue-slider.minValue)*(large?.1f:.01f);
            float before=slider.value;slider.value=Mathf.Clamp(before+direction*step,slider.minValue,slider.maxValue);
            if(Math.Abs(slider.value-before)<.00001f)return;
            lastValue=UiModel.Describe(slider);sliderPending=true;
            if(!repeated||Time.unscaledTime>=sliderSpeechAt){sliderSpeechAt=Time.unscaledTime+.65f;Speech.Say(lastValue);sliderPending=false;}
            return;
        }
        if(s.TryCast<TMP_Dropdown>()!=null){Activate();return;}
        if(s.TryCast<Toggle>()!=null){Activate();return;}
        Move(direction);
    }
    private void AnnounceValue(){if(Current!=null){lastValue=UiModel.Describe(Current);Speech.Say(lastValue,true);}}
    private void BeginEditing(TMP_InputField field,string prefix="")
    {
        editing=field;originalEdit=field.text;reviewText=field.text;reviewCaret=field.stringPosition;
        EventSystem.current?.SetSelectedGameObject(field.gameObject);field.ActivateInputField();
        Speech.Say(prefix+UiModel.Describe(field)+". Editing. Type normally. Enter finishes; Tab finishes and moves on; Escape cancels editing but stays on this screen.",true);
    }
    private void ReviewEditing()
    {
        var field=editing;if(field==null)return;
        string text=field.text;int caret=Math.Clamp(field.stringPosition,0,text.Length);
        bool protectedText=field.contentType==TMP_InputField.ContentType.Password||UiModel.PathOf(field.transform).Contains("/AuthInput/");
        if(!protectedText&&text!=reviewText&&text.Length<reviewText.Length&&(physicalHeld[8]||physicalHeld[46]))
        {
            string deleted=TextEditFeedback.Deleted(reviewText,text);
            Speech.Say("Deleted "+TextEditFeedback.Spoken(deleted),true);
        }
        else if(!protectedText&&text==reviewText&&caret!=reviewCaret&&(physicalHeld[37]||physicalHeld[39]||physicalHeld[36]||physicalHeld[35]))
            Speech.Say(TextEditFeedback.At(text,caret),true);
        reviewText=text;reviewCaret=caret;
    }
    private void JumpArea(int index)
    {
        string[] suffixes={"/Shop","/Tablet/StickerCreator","/Tablet/Upgrades","/Production","/StickerSending","/MessageButton","/Send","/Sleep"};
        int i=items.FindIndex(s=>UiModel.PathOf(s.transform).EndsWith(suffixes[index],StringComparison.Ordinal));
        if(i<0){Speech.Say("That area is not currently available.");return;}
        Focus(i,false);Activate();
    }
    private void FinishEditing(bool cancel,bool announce=true)
    {
        var field=editing;if(field==null)return;editGuardUntil=Time.unscaledTime+.35f;editing=null;
        field.DeactivateInputField();if(cancel)field.text=originalEdit;
        EventSystem.current?.SetSelectedGameObject(null);
        lastValue=UiModel.Describe(field);
        if(announce)Speech.Say((cancel?"Editing cancelled. ":"Finished editing. ")+lastValue,true);
        scanAt=0;settleAt=Time.unscaledTime+.25f;transitionUntil=Time.unscaledTime+.2f;
    }
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    private void HandleDropdown(HashSet<int> pressed)
    {
        if(dropdown==null)return;
        if(!dropdown.isActiveAndEnabled||!dropdown.IsExpanded){dropdown=null;Transition();return;}
        int count=dropdown.options.Count;
        if(pressed.Contains(27)||pressed.Contains(8)){dropdown.Hide();dropdown=null;Transition();Speech.Say("Selection cancelled. "+FocusDescription(),true);return;}
        if(pressed.Contains(13)||pressed.Contains(32))
        {
            var dd=dropdown;dropdown=null;
            if(count>0)dd.value=Math.Clamp(option,0,count-1);
            dd.Hide();lastValue=UiModel.Describe(dd);Speech.Say(lastValue,true);Transition();return;
        }
        int dir=pressed.Contains(38)||pressed.Contains(37)||(pressed.Contains(9)&&held[16])?-1:pressed.Contains(40)||pressed.Contains(39)||pressed.Contains(9)?1:0;
        if(dir!=0&&count>0){option=NavigationRules.Move(option,count,dir);SayOption();}
        if(pressed.Contains(36)&&count>0){option=0;SayOption();}
        if(pressed.Contains(35)&&count>0){option=count-1;SayOption();}
    }
    private void SayOption()
    {
        if(dropdown==null)return;
        string value=option>=0&&option<dropdown.options.Count?UiModel.Translate(dropdown.options[option].text):"No options";
        Speech.Say(UiModel.Label(dropdown)+". "+value+$". Option {option+1} of {dropdown.options.Count}. Enter chooses; Escape cancels.",true);
    }
    internal void Back()
    {
        if(screen==null)return;
        if(screen.Kind=="message-history"){var board=UnityEngine.Object.FindObjectOfType<StickerGame.MessageBoardController>();if(board!=null){Remember();board.EnableOverview();Transition();return;}}
        // Scope search to the active panel. Never jump to an underlying menu's Back button.
        var back=items.FirstOrDefault(s=>s.IsInteractable()&&UiModel.IsBack(s));
        if(back==null&&screen.Kind=="pause")back=items.FirstOrDefault(s=>s.IsInteractable()&&s.name=="Continue");
        if(back==null&&screen.Kind=="popup")back=items.FirstOrDefault(s=>s.IsInteractable()&&s.name=="OkButton");
        if(back==null&&(screen.Kind is "tutorial" or "hub" or "shop" or "creator" or "upgrades" or "production" or "sheet" or "packing"))
        {
            Remember();
            if(InputManager.TryPause()){Transition();return;}
            Speech.Say("The game cannot open its pause menu during this interaction. Finish the current dialogue or interaction, then try Escape again.",true);return;
        }
        if(back==null&&screen.Kind=="sleep"){int i=items.FindIndex(s=>s.IsInteractable()&&s.name=="Continue Button");if(i>=0){Focus(i);Speech.Say("End of day. Enter on "+UiModel.Label(items[i])+" continues through the game’s normal action.",true);}else Speech.Say("Day results are still appearing. Wait for the continuation button.");return;}
        if(back==null){Speech.Say(screen.Kind=="menu"?"Already at the main menu.":"This screen has no back or cancel action. Use Tab to review its available actions.",true);return;}
        UiModel.Reveal(back.transform);
        if(!UiModel.Hit(back)){Speech.Say("Back is not yet reachable. Please wait.");return;}
        Remember();back.Cast<Button>().onClick.Invoke();Plugin.Logger.LogInfo("[back] "+UiModel.PathOf(back.transform));Transition();
    }
    private void Transition(float delay=.25f){scanAt=0;gatherAt=0;transitionUntil=Time.unscaledTime+delay;}
    private string Validation()
    {
        if(screen?.Kind!="newgame")return "";
        var ng=screen.Root.GetComponent<NewGameScreen>();if(ng==null)return "";
        string error=ng.Savegameerrortext!=null&&UiModel.Visible(ng.Savegameerrortext.gameObject)?UiModel.Translate(ng.Savegameerrortext.text):"";
        if(error.Length>0)return error;
        if(string.IsNullOrWhiteSpace(ng.personName.text))return "Enter your name.";
        if(string.IsNullOrWhiteSpace(ng.shopName.text))return "Enter a shop name.";
        return ng.Button.IsInteractable()?"Ready to create a new game.":"Create new game is unavailable. Review the name fields.";
    }
    private void WatchChanges()
    {
        if(screen==null)return;
        if(screen.Kind=="packing")
        {
            string order=GameUi.OrderIdentity(screen.Root);
            if(order!=pendingOrder){pendingOrder=order;orderAt=Time.unscaledTime+.4f;}
            else if(order.Length>0&&order!=spokenOrder&&Time.unscaledTime>=orderAt){spokenOrder=order;Speech.Say(GameUi.OrderText(screen.Root),true);}
        }
        if(screen.Kind!="access-controls"&&Current!=null&&CreatorAccess.Editing==null&&PrintedAccess.Editing==null)
        {
            string value=UiModel.Describe(Current);
            if(value!=lastValue){lastValue=value;Speech.Say(value);}
        }
        string validation=Validation();
        if(validation!=lastValidation&&Time.unscaledTime>settleAt)
        {lastValidation=validation;if(validation.Length>0)Speech.Say(validation,interrupt:false);}
        if(screen.Kind is "popup" or "tutorial" or "settings-sub")
        {
            reader.Refresh(screen);string text=reader.Signature;
            if(text!=pendingText){pendingText=text;settleAt=Time.unscaledTime+.4f;}
            else if(text!=spokenText&&Time.unscaledTime>=settleAt){spokenText=text;Speech.Say(reader.Current(),interrupt:false);}
        }
    }
    private void Help()
    {
        if(editing!=null){Speech.Say("Editing "+UiModel.Label(editing)+". All text keys work normally, including H, R and Backspace. Enter finishes, Tab finishes and moves, Escape cancels editing. F2 repeats.",true);return;}

        if(PrintedAccess.Editing!=null){Speech.Say(AccessBindings.Help("Positioning a real sticker copy. The first copy starts centred; later copies start at random positions. Hold arrows to move. Printing rejects overlaps; packing permits native stacking. Arrows move; Shift moves farther. Position is read as percentages from the left and top. C reads coordinates from the area centre. Q or E rotates. Enter or Tab finishes and returns to the same item choice when available. Delete removes this copy. F2 repeats."),true);return;}
        if(CreatorAccess.Editing!=null){Speech.Say(AccessBindings.Help("Adjusting an element. Arrows move by one canvas unit, Shift by ten. Q and E rotate. Minus and plus resize. Left and right brackets change its layer. Delete removes it through the game's undo system. Enter or Tab finishes and returns to the same item choice when available. Escape requests pause. F2 repeats."),true);return;}
        if(dropdown!=null){Speech.Say("Dropdown choices. Arrows or Tab browse. Enter chooses and announces the value. Escape or Backspace cancels without changing the value. F2 repeats.",true);return;}
        string general=" Tab or Up and Down move through controls. Enter activates. Escape or Backspace uses this screen's Back or Cancel action and restores menu focus. H or F1 gives help; R or F2 repeats. F3 lists controls. F4 enters text reading; Up and Down read sections until Tab returns to controls. Page Down and Page Up also read text sections. F8 records diagnostics.";
        if(SectionNavigation)general=" Tab moves to the next main section; Shift+Tab moves back. Up and Down browse within the current section; Home and End reach its first and last item. F6 jumps to categories, F7 to items, F9 to placed items. Each section remembers its last focus. Enter or Tab finishes sticker positioning and returns to the same item choice, when available. Enter activates. Escape opens pause or uses Back. F1 gives help; F2 repeats; F4 reads screen text; F8 records diagnostics.";
        if(screen?.Kind is "shop" or "upgrades" or "shop-custom")general=" Tab moves to the next main section; Shift+Tab moves back. Up and Down browse within the section; Home and End reach its first and last item. Each section remembers its last focus. F7 jumps to items or colours. F9 jumps to controls, or backgrounds in Customize shop. F6 jumps to upgrade categories when available. Enter activates the native action. Escape uses Back or closes the panel. F1 gives help; F2 repeats; F3 lists controls; F4 reads screen text.";
        string detail=screen?.Kind switch
        {
            "access-controls"=>"Accessibility controls. Each action shows its assigned key. Activate an action and press a single key. Duplicate assignments are rejected; native game conflicts require confirmation. Bindings are saved. Previous and Next browse pages. Restore defaults resets only accessibility keys. Back returns to Settings.",
            "settings"=>"Settings. Hold Left or Right to adjust sliders; Shift uses larger steps. Values are spoken periodically and on release. Enter or Space toggles a checkbox. Enter opens a dropdown; arrows browse and Enter confirms. Values apply through the game's controls. Disabled controls are announced.",
            "settings-sub"=>screen.Title+". Use Enter on buttons or fields. Enter or Space changes checkboxes; Enter opens choices. Text entry temporarily disables letter shortcuts.",
            "credits"=>"Credits. Automatic scrolling is paused while you read. Up and Down read sections; Page Up and Page Down also work. Credits stay open until you leave; F4 repeats the current text section. Escape returns to the main menu.",
            "newgame"=>"New game setup. Fill Your name and Shop name: Enter starts editing, type normally, then Tab finishes and moves on. Choose day speed with Enter and arrows, and tutorial with Enter or Space. Review validation messages. Create new game uses the game's normal validation and creates a save only when you activate it. Use a new shop name to keep games separate.",
            "saves"=>"Load game. Each slot reads its information. Load and Delete are separate buttons. Any confirmation is read before you choose. Escape returns without loading or deleting.",
            "popup"=>screen.Title+". Focus is confined to this popup. Read its message before choosing a response. Escape uses No, Cancel or Close when provided.",
            "tutorial"=>"Tutorial. F4 repeats instructions. Tab browses the actual available actions, including Creative Corner when requested. Enter activates your choice. Escape asks the game to open its pause menu; the game may restrict it during a dialogue.",
            "creator"=>"Creative Corner. Tab switches between categories, items, placement and controls. F6 jumps to categories, F7 to available elements, F9 to placed elements. Enter chooses a category or adds an element at the canvas centre. Arrows then position it; Q and E rotate, minus and plus resize, brackets change layer. Enter or Tab finishes adjusting and returns to the same item choice. Use the game's Create button when you choose to finish the design. Escape opens pause when allowed.",
            "creator-save"=>"Save sticker. Tab browses the game's name, choices and confirmation controls. Enter edits a field. Tab finishes editing. Saving uses the game's normal button and validation.",
            "shop-custom"=>"Customize shop. Colour swatches and background patterns are selections and apply immediately. Enter announces the selected choice. Escape closes this panel.",
            "shop-preview"=>"Sticker preview. This enlarges the sticker image. Escape closes it and returns to the sticker list.",
            "shop-rename"=>"Rename sticker. Enter starts typing; Tab or Enter finishes editing. Use the actual confirmation button to apply the name. Escape leaves editing first, then closes this screen.",
            "shop"=>"Sticker shop. Tab switches between sticker actions and shop controls. Arrows browse within each section. F7 jumps to sticker actions; F9 to shop controls. Enter activates the selected native action. Customize has separate colour, background and control sections. Escape uses Back.",
            "hub"=>"Your shop. F4 reads its text, F3 lists current actions. Tab selects an action and Enter activates it. Escape requests the actual pause menu. On the hub, 1 Shop, 2 Creative Corner, 3 Upgrades, 4 Production, 5 Order packing, 6 Messages, 7 Send packed orders, 8 Sleep. Zero uses the current Back action. T reads remaining day time.",
            "sleep"=>"End of day. Up and Down read results item by item. Tab selects the actual continuation button; Enter continues. Escape focuses it when no Back action exists. After returning to the hub, Escape opens pause.",
            "messages" or "message-history"=>"Message Board. Tab selects customer entries. Enter opens the game’s customer history when available. F4 enters text reading; Up and Down read sections. Tab returns to buttons. Escape uses Close or Back.",
            "upgrades"=>"Upgrades. Tab switches between categories, items and controls. Arrows browse within the current section. F6 jumps to tabs and categories, F7 to upgrade cards, F9 to controls. Cards announce ownership, visible cost and progression. Enter attempts the game’s purchase action; review the price first. Escape returns using Back.",
            "production" or "production-popup" or "sheet"=>"Production. On sheets, Tab switches sections and arrows browse within each section; Enter activates. F4 reads costs and inventory, with Up and Down for text sections. Tab returns to controls. F7 jumps to sticker choices and F9 to placed copies. Enter adds a chosen copy; arrows adjust it, Q or E rotates, Enter or Tab finishes and returns to the same sticker choice. Save sheet design creates the production template; Export sticker sheet image writes an image. Printing inventory happens through the saved sheet’s Print action. Escape uses the current Close or Back action.",
            "packing"=>"Order packing. F4 reads the order and message. Up and Down read text after F4; Tab returns to available controls. F6 jumps to categories; F7 to item choices; F9 to placed copies. Enter places the chosen copy using the game’s placement action; arrows then adjust it, Q or E rotates, Enter or Tab finishes and returns to the same sticker choice. Choose Pack only when you want to complete the package. Escape uses Back.",
            "pause"=>"Pause menu. The game saves at end-of-day and next-day transitions; there is no manual game-save button here. Choose a listed action to resume, open settings or return to the main menu. Any confirmation opens its own navigation context.",
            "loading"=>"The game is loading. Wait for the next screen announcement.",
            "unsupported"=>"This gameplay screen is outside this version's menu support. F4 reads current text. Use F3 to inspect available controls; this screen has not been verified.",
            _=>"Main menu. Choose New Game, Load Game, Settings, Credits or Quit. External links follow the game buttons."
        };
        if(screen?.Kind is "sheet" or "packing")detail+=" The first keyboard placement starts centred; subsequent copies start randomly. Hold arrows to move. Sheet overlaps are blocked; native packing allows stacking. C reads coordinates for a placed copy. Overlap and box validity follow the game’s normal rules; reposition copies as needed.";
        if(screen?.Kind is "hub" or "shop" or "creator" or "production" or "sheet" or "packing" or "upgrades")detail+=" B reads the visible balance. O reads the displayed customer order; open Order packing with 5 from the hub. Inventory toggles the actual production sidebar and announces its state.";
        Speech.Say(AccessBindings.Help(detail+general),true);
    }
    private void LogControls(){foreach(var s in items)Plugin.Logger.LogInfo("[control] "+UiModel.PathOf(s.transform)+" | "+UiModel.Describe(s));}
    private void Dump(){Plugin.Logger.LogInfo("[diagnostics] screen="+screen?.Title+" editing="+(editing!=null)+" dropdown="+(dropdown!=null));LogControls();Speech.Say("Diagnostics written to the BepInEx log.",true);}
    public void OnDisable(){OwnsNavigation=false;Instance=null;}
#if SMOKE_TEST
    internal string TestKind=>screen?.Kind??"none";
    internal bool TestEditing=>editing!=null;
    internal bool TestDropdown=>dropdown!=null;
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    internal void TestKey(int key){physicalPressed=new HashSet<int>{key};var keys=AccessBindings.Translate(physicalPressed);if(!BindingUi.Handle(physicalPressed)&&!InformationAccess.Handle(keys,editing!=null))HandleKeys(keys);}
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    internal List<Selectable> TestItems=>items;
    internal string TestFocus=>Current==null?"":UiModel.Label(Current);
    internal void TestChoose(string label){int i=items.FindIndex(s=>UiModel.Label(s)==label);if(i<0)throw new Exception("Missing control: "+label);Focus(i);Activate();}
#endif
}



