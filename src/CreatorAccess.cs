using StickerGame;
using StickerGame.StickerCreator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StickyBusinessAccess;

internal static class CreatorAccess
{
    internal static StickerElement? Editing;
    private static float speechAt;
    private static bool movementSpeechPending;
    internal static StickerElement? Element(Selectable s)
    {
        var e=s.GetComponentInParent<StickerElement>();
        return e!=null&&(e.controllerButton==s||s.name=="StickerSprite")?e:null;
    }
    internal static IEnumerable<Selectable> Elements(GameObject root)
    {
        foreach(var e in root.GetComponentsInChildren<StickerElement>(false))
            if(e.controllerButton!=null&&UiModel.Visible(e.gameObject)&&(e.moved||e.IsUnlocked()))yield return e.controllerButton;
    }
    internal static bool Activate(Selectable s)
    {
        var e=Element(s);if(e==null)return false;
        if(!s.IsInteractable()||!UiModel.Visible(e.gameObject)||!e.IsUnlocked()){Speech.Say("This element is not available.");return true;}
        if(!e.moved)
        {
            UiModel.Reveal(e.transform);if(!UiModel.CanActivate(s)){Speech.Say(UiModel.BlockReason(s));return true;}
            // The game's drag/drop creation method preserves the catalogue copy.
            // This is invoked only for an element explicitly chosen by the player.
            e.SpawnInWorld(Vector2.zero,out var catalogueCopy);
            UndoManager.Instance.Push(new UndoStepCreate(e));
            e.OnEndDrag(new PointerEventData(EventSystem.current));
        }
        e.Select();Editing=e;
        Speech.Say("Editing "+State(e),true);
        return true;
    }
    internal static string State(StickerElement e)
    {
        var p=e.transform.localPosition;var scale=e.image.transform.localScale;var rot=e.image.transform.localEulerAngles;
        return UiModel.ElementName(e)+$". Position X {Math.Round(p.x)}, Y {Math.Round(p.y)}, relative to canvas centre. Rotation {Math.Round(rot.z)} degrees. Scale {Math.Round(Math.Abs(scale.x)*100)} percent. Layer {e.transform.GetSiblingIndex()+1}";
    }
    internal static bool Handle(HashSet<int> keys,bool shift)
    {
        var e=Editing;if(e==null||!UiModel.Visible(e.gameObject)){Editing=null;return false;}
        if(movementSpeechPending&&!MenuAccess.PlacementHeld){movementSpeechPending=false;Speech.Say(State(e),true);}
        if(keys.Contains(9)||keys.Contains(13)){Editing=null;e.Deselect();return false;}
        bool changed=false;
        int x=keys.Contains(37)?-1:keys.Contains(39)?1:0,y=keys.Contains(38)?1:keys.Contains(40)?-1:0;
        if(x!=0||y!=0)
        {
            var parent=e.transform.parent;var local=e.transform.localPosition+new Vector3(x,y,0)*(shift?10:1);
            var world=parent.TransformPoint(local);
            if(!e.CheckIfNewPositionIsInBounds(world)){if(Time.unscaledTime>=speechAt){speechAt=Time.unscaledTime+.5f;Speech.Say("Canvas edge. Position unchanged.");}return true;}
            UndoManager.Instance.Push(new UndoStepTransform(e));e.MoveStickerWithWASD(world);e.StopMovingStickerWithWASD();changed=true;
        }
        if(keys.Contains(81)){e.Rotate(true);changed=true;}
        if(keys.Contains(69)){e.Rotate(false);changed=true;}
        if(keys.Contains(189)||keys.Contains(109)){e.ScaleSmaller();changed=true;}
        if(keys.Contains(187)||keys.Contains(107)){e.ScaleBigger();changed=true;}
        if(keys.Contains(219)){e.MoveLayerBackwards();changed=true;}
        if(keys.Contains(221)){e.MoveLayerForward();changed=true;}
        if(keys.Contains(46)){Delete(e);return true;}
        if(changed){if(!MenuAccess.PlacementHeld||Time.unscaledTime>=speechAt){speechAt=Time.unscaledTime+.5f;movementSpeechPending=false;Speech.Say(State(e),true);}else movementSpeechPending=true;return true;}
        return false;
    }
    internal static void Delete(StickerElement e)
    {
        if(!e.moved||!UiModel.Visible(e.gameObject))return;
        string name=UiModel.ElementName(e);e.ButtonClickDestroyElement();Editing=null;movementSpeechPending=false;Speech.Say(name+" deleted.",true);
    }
}
