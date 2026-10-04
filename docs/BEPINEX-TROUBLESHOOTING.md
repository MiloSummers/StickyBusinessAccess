# BepInEx startup and console

## Class::Init warning

`Class::Init signatures have been exhausted, using a substitute!` comes from the bundled Il2CppInterop runtime, before Sticky Access loads. Its pinned upstream implementation deliberately tries exported substitute functions when the usual native signatures do not match. If none is available, it throws a separate error. Source: [InjectorHelpers.cs](https://github.com/BepInEx/Il2CppInterop/blob/dbda1cb353b0f4253345dc45136d170b9e50a5a0/Il2CppInterop.Runtime/Injection/InjectorHelpers.cs).

On the tested game build, startup continues, MenuAccess registers, the chainloader completes, and keyboard gameplay works. This warning alone has not indicated a failure in these checks. Keep it in the diagnostic log; do not replace loader DLLs merely to remove the message. This is a build-specific observation, not a guarantee for future Unity/game updates.

## Extra console window

In the game's `BepInEx/config/BepInEx.cfg`, `[Logging.Console]` controls whether the console appears. Set only that section's `Enabled` value to `false`, with the game closed. Leave `[Logging.Disk]` enabled so error reports remain available. The player package already defaults to a hidden console; older installed configurations are deliberately preserved by updates and may still enable it.

The local installation audited on 3 October had console logging enabled. Only that setting was changed to false, with a private rollback copy retained. No mod/game binaries or save data were changed for this console fix.

## Game opens without speech

Start NVDA and check whether the game's `BepInEx/LogOutput.log` changes on the new launch. A logged speech line does not prove audible NVDA output. If the loader does not start, fully exit Steam and restart it before launching from the Steam library. A prior direct game launch can leave stale Doorstop flags inherited by Steam. See [Doorstop's Steam restart issue](https://github.com/NeighTools/UnityDoorstop/issues/34).

When reporting an error, include the warning/error and nearby startup lines, mod version, game build and keyboard sequence. Remove shop/customer names, account information and save details before sharing logs.

Current recheck, 3 October: a normal Steam launch of installed 0.8.0 completed loader startup and logged startup/main-menu speech. The official controller returned 0 (NVDA running). Steam had no stale Doorstop environment entries. Intermittent silence was not reproduced on this launch; audible speech still needs human confirmation.
