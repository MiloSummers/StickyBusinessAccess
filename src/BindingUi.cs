using Il2CppInterop.Runtime;
using StickerGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace StickyBusinessAccess;

// The game has an input-mode dropdown but no action-rebinding UI. This extension
// lives in its existing Settings canvas; it does not alter native input assets.
internal static class BindingUi
{
    private static readonly Dictionary<int,AccessAction> rows=new();
    private static readonly HashSet<int> entries=new();
    private static readonly Dictionary<int,string> labels=new();
    private static GameObject? panel;
    private static Button? template;
    private static TMP_Text? heading;
    private static AccessAction? capturing;
    private static int page,candidate;
    private static float blockedUntil;
    internal static string? PreferredFocus;
    internal static bool Capturing=>capturing!=null;
    internal static bool Open=>panel!=null&&UiModel.Active(panel);
    internal static GameObject? Panel=>Open?panel:null;
    internal static void Ensure(GameObject root)
    {
        var controls=root.GetComponentsInChildren<TMP_Dropdown>(false).FirstOrDefault(d=>d.name=="Dropdown"&&UiModel.PathOf(d.transform).Contains("Control Sheme"));
        if(controls==null)return;
        if(controls.transform.parent.parent.Find("AccessibilityControlsEntry")!=null)return;
        var back=root.GetComponentsInChildren<Button>(false).FirstOrDefault(b=>b.name=="Back");if(back==null)return;
        var parent=controls.transform.parent.parent;
        // Append a native-style button to the same scroll content. Existing rows
        // retain their positions and native settings callbacks.
        var entry=Clone(back,parent,"AccessibilityControlsEntry","Accessibility controls");
        var rt=entry.transform.Cast<RectTransform>();var content=parent.Cast<RectTransform>();
        float bottom=parent.GetComponentsInChildren<RectTransform>(false).Where(t=>t.parent==parent&&t!=rt).Select(t=>t.anchoredPosition.y-t.rect.height/2).DefaultIfEmpty(-content.rect.height).Min();
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(600,60);rt.anchoredPosition=new Vector2(0,bottom-45);
        if(parent.GetComponent<LayoutGroup>()==null)content.sizeDelta=new Vector2(content.sizeDelta.x,Math.Max(content.rect.height,-bottom+100));
        entries.Add(entry.GetInstanceID());entry.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>Show(root,back)));
    }
    internal static Button Clone(Button source,Transform parent,string name,string label)
    {
        var go=Object.Instantiate(source.gameObject,parent,false);go.name=name;
        foreach(var locale in go.GetComponentsInChildren<StickerGame.Extern.Spellgarden.Localization.Locale>(true))locale.enabled=false;
        foreach(var capture in go.GetComponentsInChildren<StickerGame.Input.ControllerButtonCapture>(true)){capture.enabled=false;capture.useCancel=false;}
        var button=go.GetComponent<Button>();button.onClick=new Button.ButtonClickedEvent();button.interactable=true;button.enabled=true;
        foreach(var text in go.GetComponentsInChildren<TMP_Text>(true)){text.text=label;text.fontSize=23;text.enableAutoSizing=true;text.fontSizeMin=17;text.fontSizeMax=23;}
        labels[button.GetInstanceID()]=label;go.SetActive(true);return button;
    }
    internal static void Place(Button button,int row)
    {
        var rt=button.transform.Cast<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(900,48);rt.anchoredPosition=new Vector2(0,-120-row*52);
    }
    private static void Show(GameObject settings,Button back)
    {
        if(panel!=null)Object.Destroy(panel);
        panel=new GameObject("AccessibilityControls",new[]{Il2CppType.Of<RectTransform>(),Il2CppType.Of<Image>()});panel.transform.SetParent(settings.transform,false);
        var rt=panel.transform.Cast<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
        panel.GetComponent<Image>().color=new Color(.98f,.9f,.86f,1);
        template=back;page=0;Build();
    }
    private static void Build()
    {
        if(panel==null||template==null)return;rows.Clear();capturing=null;candidate=0;
        foreach(var child in panel.GetComponentsInChildren<Transform>(true))if(child.parent==panel.transform){child.gameObject.SetActive(false);Object.Destroy(child.gameObject);}
        var title=new GameObject("Title",new[]{Il2CppType.Of<RectTransform>(),Il2CppType.Of<TextMeshProUGUI>()});title.transform.SetParent(panel.transform,false);
        var rt=title.transform.Cast<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.sizeDelta=new Vector2(1100,100);rt.anchoredPosition=new Vector2(0,-55);
        heading=title.GetComponent<TextMeshProUGUI>();heading.font=template.GetComponentInChildren<TMP_Text>().font;heading.fontSize=26;heading.alignment=TextAlignmentOptions.Center;heading.color=Color.black;heading.raycastTarget=false;
        heading.text=$"Accessibility controls — page {page+1} of {(AccessBindings.Actions.Count+7)/8}";
        var choices=AccessBindings.Actions.Skip(page*8).Take(8).ToArray();PreferredFocus??=choices.FirstOrDefault()?.Name;int row=0;
        foreach(var action in choices)
        {
            var a=action;var b=Clone(template,panel.transform,"AccessBinding"+a.Command,a.Name+": "+AccessBindings.Key(a.Command));Place(b,row++);rows[b.GetInstanceID()]=a;
            b.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>Begin(a)));
        }
        var prev=Clone(template,panel.transform,"Previous bindings page","Previous bindings page");Place(prev,8);prev.interactable=page>0;prev.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>{page--;PreferredFocus=null;Build();Speech.Say(heading!.text,true);}));
        var next=Clone(template,panel.transform,"Next bindings page","Next bindings page");Place(next,9);next.interactable=(page+1)*8<AccessBindings.Actions.Count;next.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>{page++;PreferredFocus=null;Build();Speech.Say(heading!.text,true);}));
        var reset=Clone(template,panel.transform,"Reset accessibility keys","Restore accessibility defaults");Place(reset,10);reset.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>{AccessBindings.Defaults();Build();Speech.Say("Accessibility default bindings restored and saved.",true);}));
        var speed=Clone(template,panel.transform,"DayLength",DayLength.Label);Place(speed,11);speed.onClick.AddListener((UnityEngine.Events.UnityAction)(()=>{DayLength.Next();PreferredFocus="Game speed / day length";Build();Speech.Say(DayLength.Label,true);}));
        var close=Clone(template,panel.transform,"Back","Back to Settings");Place(close,12);close.onClick.AddListener((UnityEngine.Events.UnityAction)Close);
    }
    internal static string? Label(Selectable s)=>s.name=="DayLength"?DayLength.Label:entries.Contains(s.GetInstanceID())?"Accessibility controls":rows.TryGetValue(s.GetInstanceID(),out var action)?action.Name+": "+AccessBindings.Key(action.Command):labels.TryGetValue(s.GetInstanceID(),out var label)?label:null;
    private static void Begin(AccessAction action)
    {capturing=action;candidate=0;heading!.text="Change "+action.Name+". Press a single key. Escape cancels.";Speech.Say(heading.text,true);}
    private static string NativeConflict(int key)
    {
        string path=key>=65&&key<=90?((char)key).ToString().ToLowerInvariant():key>=112&&key<=135?"f"+(key-111):key>=48&&key<=57?((char)key).ToString():key switch {8=>"backspace",9=>"tab",13=>"enter",27=>"escape",32=>"space",33=>"pageUp",34=>"pageDown",35=>"end",36=>"home",37=>"leftArrow",38=>"upArrow",39=>"rightArrow",40=>"downArrow",46=>"delete",187=>"equals",189=>"minus",219=>"leftBracket",221=>"rightBracket",_=>""};
        if(path.Length==0)return "";
        var control=UnityEngine.InputSystem.InputSystem.FindControl("<Keyboard>/"+path);
        var input=StickerGame.Input.InputManager.Instance;
        var assets=new List<UnityEngine.InputSystem.InputActionAsset>();
        if(input?.mapping?.asset is {} controllerAsset)assets.Add(controllerAsset);
        if(input?.mouseInput?.asset is {} mouseAsset)assets.Add(mouseAsset);
        foreach(var ui in Object.FindObjectsOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>())
        {
            if(ui.actionsAsset!=null)assets.Add(ui.actionsAsset);
            foreach(var reference in new[]{ui.move,ui.submit,ui.cancel})if(reference?.action?.actionMap?.asset is {} asset)assets.Add(asset);
        }
        foreach(var asset in assets)foreach(var map in asset.actionMaps)foreach(var binding in map.bindings)
            if(!string.IsNullOrEmpty(binding.effectivePath)&&(string.Equals(binding.effectivePath.TrimStart('/'),"<Keyboard>/"+path,StringComparison.OrdinalIgnoreCase)||(control!=null&&UnityEngine.InputSystem.InputControlPath.Matches(binding.effectivePath,control))))return "This key is also used by the game for "+binding.action+". Accessibility controls take priority on supported screens. Enter confirms; Escape cancels.";
        return "";
    }
    internal static bool Handle(HashSet<int> physical)
    {
        if(Time.unscaledTime<blockedUntil)return true;
        if(capturing==null)return false;
        if(MenuAccess.AccessibilityModifierDown)return true;
        if(!Open){capturing=null;return false;}
        if(physical.Contains(27)){PreferredFocus=capturing.Name;capturing=null;candidate=0;Build();blockedUntil=Time.unscaledTime+.25f;Speech.Say("Binding change cancelled.",true);return true;}
        if(candidate!=0)
        {if(physical.Contains(13))Finish(candidate);return true;}
        if(physical.Count==0)return true;
        int key=physical.Where(k=>k is not 16 and not 17 and not 18 and not 160 and not 161 and not 162 and not 163 and not 164 and not 165).OrderBy(k=>k).FirstOrDefault();if(key==0)return true;string? conflict=AccessBindings.Conflict(key,capturing);
        if(conflict!=null){heading!.text=conflict;Speech.Say(conflict,true);return true;}
        string warning=NativeConflict(key);
        if(warning.Length>0){candidate=key;heading!.text=warning;Speech.Say(warning,true);return true;}
        Finish(key);return true;
    }
    private static void Finish(int key)
    {
        var action=capturing!;PreferredFocus=action.Name;AccessBindings.Assign(action,key);Build();blockedUntil=Time.unscaledTime+.25f;
        Speech.Say(action.Name+" assigned to "+AccessBindings.Name(key)+". Saved.",true);
    }
    internal static void Close(){capturing=null;candidate=0;if(panel!=null)panel.SetActive(false);blockedUntil=Time.unscaledTime+.25f;}
}
