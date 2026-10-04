using BepInEx.Configuration;
using HarmonyLib;
using StickerGame.Management;
namespace StickyBusinessAccess;

internal static class DayLength
{
    private static ConfigEntry<int> choice=null!;
    internal static readonly string[] Names={"Normal","1.25 times day length","1.5 times day length","2 times day length","Unlimited"};
    internal static void Initialize(ConfigFile config)
    {choice=config.Bind("Accessibility","DayLength",0,"0 Normal, 1 1.25x, 2 1.5x, 3 2x, 4 Unlimited. Applies to native action time costs; does not change the save's difficulty.");if(choice.Value<0||choice.Value>=Names.Length)choice.Value=0;}
    internal static string Label=>"Game speed / day length: "+Names[choice.Value];
    internal static void Next()=>choice.Value=(choice.Value+1)%Names.Length;
    internal static int Cost(int cost,int option)=>DayLengthRules.Cost(cost,option);
    [HarmonyPatch(typeof(GameManager),nameof(GameManager.GetGameSpeed))]
    private static class NativeCosts
    {
        static void Postfix(GameManager __instance,ref GameSpeedValues __result)
        {
            int option=choice.Value;
            if(option==0||__result==null||__instance.Game==null||__instance.Game.GameSpeed==GameSpeedDifficulty.Limitless)return;
            // Return a new value. Never mutate shared ScriptableObject difficulty presets.
            __result=new GameSpeedValues{CreateStickerMinutes=Cost(__result.CreateStickerMinutes,option),PackLetterMinutes=Cost(__result.PackLetterMinutes,option),PrintStickerMinutes=Cost(__result.PrintStickerMinutes,option),PostOfficeMinutes=Cost(__result.PostOfficeMinutes,option)};
        }
    }
}
