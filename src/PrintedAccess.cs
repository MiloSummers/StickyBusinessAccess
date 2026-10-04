using StickerGame.StickerLetter;
using StickerGame.ProductionScreen;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace StickyBusinessAccess;

internal static class PrintedAccess
{
    internal static PrintedStickerElement? Editing;
    private static PrintedStickerElement? pendingDrop;
    private static float dropAt;
    private static float speechAt;
    private static PrintedStickerElement? pendingMove;
    private static Vector3 previousPosition, previousRotation;
    private static float moveCheckAt;
    private static bool movementSpeechPending;
    private static string lastBlock="";
    internal static PrintedStickerElement? Element(Selectable s)=>s.GetComponentInParent<PrintedStickerElement>();
    internal static IEnumerable<Selectable> Elements(GameObject root)
    {
        foreach(var e in root.GetComponentsInChildren<PrintedStickerElement>(false))
            if(UiModel.Visible(e.gameObject)&&e.button!=null)yield return e.button;
    }
    internal static bool Activate(Selectable s)
    {
        var e=Element(s);if(e==null)return false;
        if(!s.IsInteractable()||!UiModel.Visible(e.gameObject)){Speech.Say("This sticker is unavailable.");return true;}
        if(!e.moved)
        {
            bool first=!UnityEngine.Object.FindObjectsOfType<PrintedStickerElement>().Any(other=>other!=e&&other.moved&&other.Phase==e.Phase&&UiModel.Visible(other.gameObject));
            if(e.Phase!=PrintedStickerElement.StickerElementType.PrintPhase&&UnityEngine.Object.FindObjectsOfType<GoodieElement>().Any(g=>g.moved&&UiModel.Visible(g.gameObject)))first=false;
            UiModel.Reveal(e.transform);if(!UiModel.CanActivate(s)){Speech.Say(UiModel.BlockReason(s));return true;}
            // Use the game's controller placement path for this player-selected sticker.
            e.OnBeginDrag(new PointerEventData(EventSystem.current));
            if(e.Phase==PrintedStickerElement.StickerElementType.PrintPhase)StickerSheetDesigner.Instance.PlaceOnSheet(e);
            else
            {
                PackingControllerManager.Instance.Place(e);
                if(e.body!=null)e.body.position=new Vector2(e.transform.position.x,e.transform.position.y);
                Physics2D.SyncTransforms();
            }
            // Keyboard placement changes only the initial position of the real copy.
            // Mouse placement and the game's inventory/validity callbacks stay native.
            if(Area(e,out var area,out var rect)&&first)
            {
                MoveTo(e,area.TransformPoint(new Vector3(rect.center.x,rect.center.y,0)));
            }
            else if(e.Phase==PrintedStickerElement.StickerElementType.PrintPhase&&Area(e,out area,out rect))
            {
                // Native packing chooses a random point. Sheets normally find a
                // free grid position; keyboard placement randomizes a legal point
                // using the same real geometry and native drop/validation path.
                MoveTo(e,area.TransformPoint(new Vector3(UnityEngine.Random.Range(rect.xMin+rect.width*.1f,rect.xMax-rect.width*.1f),UnityEngine.Random.Range(rect.yMin+rect.height*.1f,rect.yMax-rect.height*.1f),0)));
            }
            pendingDrop=e;dropAt=Time.unscaledTime+.15f;
        }
        Editing=e;movementSpeechPending=false;lastBlock="";if(pendingDrop!=e)Speech.Say(State(e),true);
        return true;
    }
    internal static void Tick()
    {
        if(pendingMove is {} moving && Time.unscaledTime>=moveCheckAt)
        {
            pendingMove=null;
            if(UiModel.Visible(moving.gameObject))
            {
                moving.CheckValidity();string invalid=PlacementProblem(moving);
                if(invalid.Length>0){moving.transform.localEulerAngles=previousRotation;MoveTo(moving,previousPosition);movementSpeechPending=false;if(lastBlock!=invalid){lastBlock=invalid;speechAt=Time.unscaledTime+.5f;Speech.Say("Movement blocked: "+invalid+". Position unchanged. "+Position(moving),true);}}
                else if(!MenuAccess.PlacementHeld||Time.unscaledTime>=speechAt){lastBlock="";speechAt=Time.unscaledTime+.5f;movementSpeechPending=false;Speech.Say(State(moving),true);}else movementSpeechPending=true;
            }
        }
        var e=pendingDrop;if(e==null||Time.unscaledTime<dropAt)return;
        pendingDrop=null;
        if(!UiModel.Visible(e.gameObject))return;
        e.CheckValidity();string problem=PlacementProblem(e);
        if(problem.Length>0){string name=e.StickerItem?.Name??"Sticker";int buttonId=e.button==null?0:e.button.GetInstanceID();e.DestroyElement();Editing=null;MenuAccess.Instance?.ReturnToChoice(false);MenuAccess.Instance?.RemoveControl(buttonId);Speech.Say(name+" placement blocked: "+problem+". Returned to sticker choices.",true);return;}
        e.OnEndDrag(new PointerEventData(EventSystem.current));Speech.Say(State(e),true);
    }
    internal static string State(PrintedStickerElement e)
    {
        return (e.StickerItem?.Name??"Sticker")+". "+Position(e)+$". Rotation {Math.Round(e.transform.localEulerAngles.z)} degrees. "+(e.Colliding?"Overlapping or invalid placement.":e.moved?"Placed.":"Not placed.")+(e.Phase!=PrintedStickerElement.StickerElementType.PrintPhase&&e.moved?(e.IsInLetter?" Inside the order box.":" Outside the order box."):"");
    }
    private static bool Area(PrintedStickerElement e,out Transform area,out Rect rect)
    {
        area=e.transform.parent;rect=default;
        if(e.Phase==PrintedStickerElement.StickerElementType.PrintPhase)
        {
            var sheet=StickerSheetDesigner.Instance;var rt=sheet?.Container?.TryCast<RectTransform>();
            if(rt==null)return false;area=rt;rect=rt.rect;return rect.width>0&&rect.height>0;
        }
        var box=PackingControllerManager.Instance?.mouseModerectCollider;
        if(box==null)return false;
        area=box.transform;var bounds=box.bounds;
        var lo=area.InverseTransformPoint(bounds.min);var hi=area.InverseTransformPoint(bounds.max);
        rect=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);return rect.width>0&&rect.height>0;
    }
    private static void MoveTo(PrintedStickerElement e,Vector3 world)
    {
        e.OnAttach(e.transform.position);e.OnMove(world);
        if(e.body!=null)e.body.position=new Vector2(e.transform.position.x,e.transform.position.y);
        Physics2D.SyncTransforms();e.CheckValidity();
    }
    private static string PlacementProblem(PrintedStickerElement e)
    {
        // Packing deliberately permits stacked stickers. Its validity is box
        // membership, unlike printing's native polygon-overlap restriction.
        if(!e.IsInLetter)return e.Phase==PrintedStickerElement.StickerElementType.PrintPhase?"it is outside the printable sheet":"it is outside the order box";
        if(e.Phase!=PrintedStickerElement.StickerElementType.PrintPhase)return "";
        // The shipped IL2CPP build strips polygon-query APIs. Consult the
        // game's real contact list after its physics step, never rectangle
        // approximations (which falsely reject transparent sticker corners).
        if(e.collissions!=null)
            foreach(var contact in e.collissions)
                if(contact!=null&&contact.GetComponentInParent<PrintedStickerElement>() is {} other&&other!=e&&other.moved)
                    return "it overlaps another sticker";
        if(e.Colliding)return "the game's placement rules reject this position";
        return "";
    }
    internal static void Delete(PrintedStickerElement e)
    {
        if(!e.moved||!UiModel.Visible(e.gameObject))return;
        string name=e.StickerItem?.Name??"Sticker";if(pendingDrop==e)pendingDrop=null;if(pendingMove==e)pendingMove=null;
        e.DestroyElement();Editing=null;movementSpeechPending=false;Speech.Say(name+" deleted.",true);
    }
    private static string Position(PrintedStickerElement e)
    {
        if(!Area(e,out var area,out var rect))return Coordinates(e);
        var p=area.InverseTransformPoint(e.transform.position);
        float x=(p.x-rect.xMin)/rect.width,y=(rect.yMax-p.y)/rect.height;
        string horizontal=x<.33f?"left":x>.67f?"right":"centre";
        string vertical=y<.33f?"top":y>.67f?"bottom":"middle";
        string region=horizontal=="centre"&&vertical=="middle"?"Centre":vertical+" "+horizontal;
        return region+$". {Math.Round(x*100,1)} percent from left, {Math.Round(y*100,1)} percent from top";
    }
    internal static string Coordinates(PrintedStickerElement e)
    {
        if(!Area(e,out var area,out var rect))return "Local coordinates unavailable";
        var p=area.InverseTransformPoint(e.transform.position)-new Vector3(rect.center.x,rect.center.y,0);
        return $"X {Math.Round(p.x,1)}, Y {Math.Round(p.y,1)} canvas units from the area centre. Positive X is right; positive Y is up";
    }
    internal static bool Handle(HashSet<int> keys,bool shift)
    {
        var e=Editing;if(e==null||!UiModel.Visible(e.gameObject)){Editing=null;return false;}
        if(!MenuAccess.PlacementHeld)lastBlock="";
        if(pendingDrop==e||pendingMove==e)return true;
        if(movementSpeechPending&&!MenuAccess.PlacementHeld){movementSpeechPending=false;Speech.Say(State(e),true);}
        if(keys.Contains(9)||keys.Contains(13)){e.OnEndDrag(new PointerEventData(EventSystem.current));Editing=null;return false;}
        if(keys.Contains(27)||keys.Contains(8)||keys.Contains(48)){Editing=null;return false;}
        int x=keys.Contains(37)?-1:keys.Contains(39)?1:0,y=keys.Contains(38)?1:keys.Contains(40)?-1:0;
        bool changed=false;
        var before=e.transform.position;var rotation=e.transform.localEulerAngles;
        if(x!=0||y!=0)
        {
            var local=e.transform.localPosition+new Vector3(x,y,0)*(shift?10:1);
            MoveTo(e,e.transform.parent.TransformPoint(local));changed=true;
        }
        if(keys.Contains(81)||keys.Contains(69)){e.Rotate();e.CheckValidity();changed=true;}
        if(changed){pendingMove=e;previousPosition=before;previousRotation=rotation;moveCheckAt=Time.unscaledTime+.06f;return true;}
        if(keys.Contains(46)){Delete(e);return true;}
        return false;
    }
}
