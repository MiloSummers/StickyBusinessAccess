using StickerGame;
using StickerGame.Management;
using StickerGame.StickerLetter;
using UnityEngine;
using UnityEngine.UI;
namespace StickyBusinessAccess;

internal static class GoodieAccess
{
    internal static GoodieElement? Editing;
    private static GoodieElement? pending;
    private static float settleAt,speechAt;
    internal static GoodieElement? Element(Selectable s)=>s.GetComponent<GoodieElement>();
    internal static int Count(string id){var game=GameManager.Instance?.Game;return game?.Goodies!=null&&game.Goodies.ContainsKey(id)?game.Goodies[id]:0;}
    internal static string Name(string id)=>GameUi.UpgradeName(string.IsNullOrEmpty(id)?null:UpgradeInfo.FromID(id),id);
    internal static void Ensure(GameObject root)
    {
        foreach(var spawner in root.GetComponentsInChildren<GoodieElementSpawner>(false))
        {
            if(!UiModel.Visible(spawner.gameObject)||string.IsNullOrEmpty(spawner.goodieUpgradeID))continue;
            // Mouse mode deliberately disables the controller button. An independent
            // keyboard proxy avoids changing that native input-mode decision.
            if(spawner.transform.Find("AccessGoodie")!=null)continue;
            var go=new GameObject("AccessGoodie",new[]{Il2CppInterop.Runtime.Il2CppType.Of<RectTransform>(),Il2CppInterop.Runtime.Il2CppType.Of<Button>()});go.transform.SetParent(spawner.transform,false);
            var rt=go.transform.Cast<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
            go.GetComponent<Button>().onClick=new Button.ButtonClickedEvent();
        }
        foreach(var e in root.GetComponentsInChildren<GoodieElement>(false))
            if(e.spawner!=null&&UiModel.Visible(e.gameObject)&&e.GetComponent<Button>()==null){var b=e.gameObject.AddComponent<Button>();b.targetGraphic=e.GetComponentInChildren<Image>();b.onClick=new Button.ButtonClickedEvent();}
    }
    internal static bool Proxy(Selectable s)=>s.name=="AccessGoodie"||Element(s)!=null;
    internal static bool InSelectedCategory(Selectable s)
    {
        if(Element(s)!=null)return true;
        var spawner=s.GetComponentInParent<GoodieElementSpawner>();if(spawner==null)return true;
        var manager=UnityEngine.Object.FindObjectOfType<PackingExtrasManager>();if(manager==null)return false;
        string[] groups={"Filling","Goodies","HalloweenGoodies","FestiveGoodies","PrideGoodies","PWMGoodies","CampGoodies","WitchyDLCGoodies","SeasideDLCGoodies"};
        int state=(int)manager.extrasState;
        return state>=0&&state<groups.Length&&UiModel.HasAncestor(spawner.transform,groups[state]);
    }
    internal static string? Label(Selectable s)
    {
        var e=Element(s);if(e!=null)return Name(e.spawner.goodieUpgradeID)+". Placed goodie. "+(Editing==e?"Selected. ":"Not selected. ")+(e.isInBox?"Inside order box. ":"Outside order box. ")+"Optional packing extra";
        var spawner=s.GetComponentInParent<GoodieElementSpawner>();
        return spawner==null?null:"Add goodie: "+Name(spawner.goodieUpgradeID)+$". Available {Count(spawner.goodieUpgradeID)}. Optional packing extra";
    }
    internal static bool Activate(Selectable s)
    {
        if(!Proxy(s))return false;
        if(!UiModel.CanActivate(s)){Speech.Say(UiModel.BlockReason(s));return true;}
        var e=Element(s);
        if(e!=null){Editing=e;Speech.Say(Label(s)!,true);return true;}
        var spawner=s.GetComponentInParent<GoodieElementSpawner>();if(spawner==null)return false;
        if(Count(spawner.goodieUpgradeID)<=0){Speech.Say(Name(spawner.goodieUpgradeID)+" is out of stock. Purchase more in Upgrades.",true);return true;}
        var before=UnityEngine.Object.FindObjectsOfType<GoodieElement>().Select(g=>g.GetInstanceID()).ToHashSet();
        spawner.OnClick();
        e=UnityEngine.Object.FindObjectsOfType<GoodieElement>().FirstOrDefault(g=>g.spawner==spawner&&!before.Contains(g.GetInstanceID()));
        if(e!=null){Editing=e;pending=e;settleAt=Time.unscaledTime+.2f;}return true;
    }
    internal static void Tick()
    {
        if(pending==null||Time.unscaledTime<settleAt)return;
        var e=pending;pending=null;
        if(UiModel.Visible(e.gameObject))Speech.Say(Name(e.spawner.goodieUpgradeID)+" placed. "+(e.isInBox?"Inside order box. ":"Outside order box. ")+$"Available {Count(e.spawner.goodieUpgradeID)}.",true);
    }
    internal static void Delete(GoodieElement e){string name=Name(e.spawner.goodieUpgradeID);e.DestroyElement();Editing=null;if(pending==e)pending=null;Speech.Say(name+" removed and returned to stock.",true);}
    internal static bool Handle(HashSet<int> keys,bool fast)
    {
        var e=Editing;if(e==null)return false;if(!UiModel.Visible(e.gameObject)){Editing=null;return false;}
        if(keys.Contains(9)||keys.Contains(13)){Editing=null;return keys.Contains(13);}
        int x=keys.Contains(37)?-1:keys.Contains(39)?1:0,y=keys.Contains(38)?1:keys.Contains(40)?-1:0;
        if(x==0&&y==0)return false;
        var box=PackingControllerManager.Instance?.mouseModerectCollider;if(box==null)return true;
        var local=e.transform.localPosition+new Vector3(x,y,0)*(fast?10:1);var world=e.transform.parent.TransformPoint(local);var bounds=box.bounds;
        if(world.x<bounds.min.x||world.x>bounds.max.x||world.y<bounds.min.y||world.y>bounds.max.y){if(Time.unscaledTime>=speechAt){Speech.Say("Order box edge. Position unchanged.");speechAt=Time.unscaledTime+.5f;}return true;}
        e.Attach(e.transform.position);e.OnMove(world);e.OnDetach();Physics2D.SyncTransforms();
        if(Time.unscaledTime>=speechAt){Speech.Say(Name(e.spawner.goodieUpgradeID)+$". X {Math.Round(local.x)}, Y {Math.Round(local.y)}.",true);speechAt=Time.unscaledTime+.5f;}return true;
    }
}
