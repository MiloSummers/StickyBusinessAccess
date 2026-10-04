using HarmonyLib;
using UnityEngine;

namespace StickyBusinessAccess;

// Observe the synchronous native save after its file writes return successfully.
// Never initiate a save or change the game's save schedule.
[HarmonyPatch(typeof(StickerGame.Management.Game), nameof(StickerGame.Management.Game.Save))]
internal static class SaveAccess
{
    private static bool pending;
    private static float announceAt;
    static void Postfix(){pending=true;announceAt=Time.unscaledTime+.75f;}
    internal static void Tick()
    {
        if(!pending||Time.unscaledTime<announceAt)return;
        pending=false;Speech.Say("Game saved.",true,interrupt:false);
    }
}
