using StickerGame;
using UnityEngine;
using UnityEngine.UI;
namespace StickyBusinessAccess;

// Native inventory rows are images/text, without any Selectable. Give each row
// a read-only keyboard target. The game's quantities and purchase flow stay native.
internal static class InventoryAccess
{
    internal static void Ensure(GameObject root)
    {
        foreach(var row in root.GetComponentsInChildren<PrinterInventoryStickerElement>(false))
        {
            if(!UiModel.Visible(row.gameObject)||row.StickerItem==null)continue;
            if(row.GetComponent<Button>()==null){var b=row.gameObject.AddComponent<Button>();b.targetGraphic=row.image;b.onClick=new Button.ButtonClickedEvent();}
        }
    }
    internal static string? Label(Selectable s)
    {
        var row=s.GetComponent<PrinterInventoryStickerElement>();
        return row?.StickerItem==null?null:Row(row);
    }
    internal static string Row(PrinterInventoryStickerElement row)=>"Printed inventory: "+row.StickerItem.Name+". Available "+UiModel.Translate(row.availableStickersTMP?.text??row.availableStickers.ToString())+". Ordered "+UiModel.Translate(row.orderedStickersTMP?.text??row.orderedStickers.ToString());
    internal static bool Read(Selectable s){string? label=Label(s);if(label==null)return false;Speech.Say(UiModel.CanActivate(s)?label:UiModel.BlockReason(s),true);return true;}
}
