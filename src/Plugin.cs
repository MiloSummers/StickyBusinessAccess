using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace StickyBusinessAccess;

[BepInPlugin("local.stickybusiness.access", "Sticky Business Access", "0.8.2")]
[BepInProcess("StickyBusiness.exe")]
public sealed class Plugin : BasePlugin
{
    internal static ManualLogSource Logger = null!;
    public override void Load()
    {
        Logger = Log;
        #if SMOKE_TEST
        AccessBindings.Initialize(MenuSmoke.BindingConfig);
#else
        AccessBindings.Initialize(Config);
#endif
#if SMOKE_TEST
        DayLength.Initialize(MenuSmoke.BindingConfig);
#else
        DayLength.Initialize(Config);
#endif
        Speech.Initialize(Path.GetDirectoryName(GetType().Assembly.Location)!);
        string catalogue=Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!,"descriptions.en.json");
        try { _ = DescriptionCatalogue.Parse(File.ReadAllText(catalogue)); Log.LogInfo("Description catalogue validated. Element names currently use game localization."); }
        catch(Exception e) { Log.LogWarning("Description catalogue ignored: "+e.Message); }
        new Harmony("local.stickybusiness.access").PatchAll();
#if SMOKE_TEST
        Application.runInBackground=true;
#endif
        AddComponent<MenuAccess>();
        Log.LogInfo("Menu access 0.8.2 loaded: menus, tutorial, shop, upgrades, messages, production and order packing. Save creation uses the game's own controls.");
    }
}

// Suppress Unity's second move/submit path while our active-screen navigator owns input.
// Prefix runs before native UI processing, independent of MonoBehaviour update order.
[HarmonyPatch(typeof(InputSystemUIInputModule), nameof(InputSystemUIInputModule.Process))]
internal static class NativeNavigationGuard
{
    static void Prefix(out bool __state)
    {
        var es = EventSystem.current;
        __state = es != null && es.sendNavigationEvents;
        if (es != null && MenuAccess.OwnsNavigation) es.sendNavigationEvents = false;
    }
    static Exception? Finalizer(Exception? __exception, bool __state)
    {
        var es = EventSystem.current;
        if (es != null) es.sendNavigationEvents = __state;
        return __exception;
    }
}

// Game-level input callbacks otherwise run alongside our polling and can cancel twice.
// Mouse/gamepad callbacks and unsupported gameplay retain the game's normal behavior.
[HarmonyPatch]
internal static class GameKeyboardGuard
{
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        foreach(var pair in new (Type type,string name)[]{
            (typeof(StickerGame.Input.InputManager),"PauseMouseMenuPerformed"),
            (typeof(StickerGame.Input.InputManager),"PauseMenuPerformed"),
            (typeof(StickerGame.Input.ControllerButtonCapture),"Performed"),
            (typeof(StickerGame.Input.ConsoleTextField),"AcceptOnPerformed"),
            (typeof(StickerGame.Input.ConsoleTextField),"MoveUp"),
            (typeof(StickerGame.Input.ConsoleTextField),"MoveDown"),
            (typeof(StickerGame.Input.ConsoleTextField),"MoveLeft"),
            (typeof(StickerGame.Input.ConsoleTextField),"MoveRight"),
            (typeof(StickerGame.Utility.Popups),"AcceptOnPerformed"),
            (typeof(StickerGame.Utility.Popups),"CancelOnPerformed")})
            yield return AccessTools.Method(pair.type,pair.name,new[]{typeof(UnityEngine.InputSystem.InputAction.CallbackContext)});
        foreach(var type in new[]{typeof(StickerGame.Input.CreatorScreenInput),typeof(StickerGame.ProductionScreen.ProductionDesignerControllerManager),typeof(StickerGame.ProductionScreen.ProductionOverviewControllerManager),typeof(StickerGame.StickerLetter.PackingControllerManager),typeof(StickerGame.StickerLetter.TopBarController)})
        foreach(var method in type.GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly))
            if(method.GetParameters().Length==1&&method.GetParameters()[0].ParameterType==typeof(UnityEngine.InputSystem.InputAction.CallbackContext))yield return method;
    }
    static bool Prefix(UnityEngine.InputSystem.InputAction.CallbackContext __0)
    {
        return !MenuAccess.GuardNative||!MenuAccess.InForeground||
            (!MenuAccess.ControlKeyDown&&__0.control?.device?.TryCast<UnityEngine.InputSystem.Keyboard>()==null);
    }
}

[HarmonyPatch(typeof(StickerGame.ScreenSwitcher),"SwitchBack")]
internal static class EditingBackGuard
{
    static bool Prefix()=>!MenuAccess.ProtectEditExit;
}

[HarmonyPatch(typeof(StickerGame.NewGameScreen),"CreateGame")]
internal static class EditingSubmitGuard
{
    static bool Prefix()=>!MenuAccess.ProtectEditExit;
}

// The game also completes credits from its scrolling callbacks. Keep their
// normal Back button, but never treat reaching the last text section as Back.
[HarmonyPatch(typeof(StickerGame.Credits),"OnComplete")]
internal static class CreditsEndGuard
{
    static bool Prefix()=>false;
}

[HarmonyPatch(typeof(StickerGame.UserInterface.ScrollOnEnable),"OnEnable")]
internal static class CreditsScrollGuard
{
    static bool Prefix(StickerGame.UserInterface.ScrollOnEnable __instance)
        =>!UiModel.PathOf(__instance.transform).Contains("/Credits Screen/");
}

