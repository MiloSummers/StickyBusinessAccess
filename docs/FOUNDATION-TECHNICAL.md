> Historical record of a pre-Prism release. Current speech and installation behavior are documented in README.md and PRISM-MIGRATION.md.

# Technical verification — 30 September 2026

## Installed game

Found through `C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf`, then `appmanifest_2303350.acf`. The library's embedded app list was stale, but the app manifest and installed files were present.

- Installation: `C:\Program Files (x86)\Steam\steamapps\common\Sticky Business`.
- Steam app: 2303350. Installed build: **24353709**.
- Executable: `StickyBusiness.exe`; PE machine **0x8664 / AMD64**.
- UnityPlayer product version: **2021.3.45f2 (88f88f591b2e)**.
- Backend: **IL2CPP**, with native `GameAssembly.dll` and `StickyBusiness_Data/il2cpp_data/Metadata/global-metadata.dat`.
- Metadata header: magic **0xFAB11BAF**, version **31**; metadata length 10,679,600 bytes.
- Appropriate loader family: **BepInEx 6 Unity.IL2CPP win-x64**, not BepInEx 5 Mono. Selected official build: **6.0.0-be.788+5b766a3**. It is a bleeding-edge build, not a stable release.

## Local analysis and provenance


Il2CppDumper **6.7.46** processed the native binary and metadata successfully. It identified metadata 31 and adjusted its native layout interpretation to 29. Its output provides signatures, fields and method addresses, not recovered C# implementations. Do not interpret dummy method bodies as the game's original source.

Il2CppInterop from the pinned loader generated local reference wrappers. UnityPy **1.25.3** plus TypeTreeGeneratorAPI **0.0.10** reconstructed serialized component layouts from local dummy assemblies. The generated trees needed corrections for the base enabled-field alignment and arrays of strings; the pure Python reader then validated full byte consumption. Main-menu serialized actions, upgrade categories and localization records were successfully read. No extracted assets or binaries are part of the source repository or release.

The pinned loader's own Cpp2IL pipeline was also executed locally against the original native binary/metadata, producing **74 assemblies** and completing Il2CppInterop generation with Unity 2021.3.45 reference libraries. The final plugin was rebuilt against these Cpp2IL-generated wrappers: **zero warnings, zero errors**. This verifies offline metadata/interop compatibility, not successful injection or in-game behavior.

## Menu and input evidence

The game has `StickerGame.MainMenu.MainMenuScreen`. Its `ButtonGroup` points at `Buttons_01` in level0. Core buttons have serialized `SpellgardenButton` callbacks to `ClickedOnButton` with the arguments `newgame`, `loadgame`, `settings`, `credits`, `quit`, `continue`. `SpellgardenButton` inherits Unity UI `Button`, so invoking its existing `onClick` once preserves the serialized action.

Three icon-only social buttons map to `twitter`, `tiktok`, `discord`; the prototype names these from their actual action bindings. More-games, news and promotional content can also be present. Button availability is evaluated at runtime: active hierarchy, interactability, CanvasGroup visibility, screen bounds and the top raycast hit. The prototype does not click through an overlay when no reachable control is found.

The game uses Unity's Input System with mouse and controller UI modules and its own InputManager. Serialized MouseInputActions includes WASD/arrow navigation; ControllerInputActions has no direct keyboard paths in the inspected asset. Therefore the prototype patches InputSystemUIInputModule.Process to temporarily suppress native move/submit/cancel navigation while its navigator is active, restoring the original flag after processing. The mod itself leaves Escape to the game's input handling. Callback-level conflicts and frame ordering remain live-test items.

## Preservation

Installer checks for an existing loader, checks every destination for collisions, hashes original game files and records all added files. It overwrites no original game files. Uninstaller removes only unchanged receipt-owned files and only removes empty directories. Save storage is never read or written by the installer or plugin. Normal actions selected by the player still have normal game effects, including starting/loading games.

No executable patch, permanent registry change, screen-reader add-on or save conversion is used. Private analysis and generated references live outside the Git project and distributable.

## Runtime validation boundary

The NVDA controller library could be loaded, but `nvdaController_testIfRunning` returned 1722 from this session. Live screen-reader success is not claimed. The plugin logs every speech request, but a logged line alone does not prove that it was heard.

The built menu prototype still needs an in-game pass for load announcement, correct button set/order, single activation, popup filtering, Alt+Tab, late NVDA startup and help/repeat behavior. Settings, save slots, credits reading and gameplay are outside this stage. If the game is updated, regenerate local interop and repeat verification before treating this build as compatible.

## Completed checks and installation

- Nine catalogue resolver/validation checks passed, including variant precedence, game-title fallback, no invented details, case-sensitive IDs, malformed JSON, duplicate keys and unsupported schema rejection.
- Reversible-installer tests passed in a private fixture: clean install/removal restored the original file set and hashes; existing-loader collision was refused; modified plugin files were preserved; the loader injection DLL was removed.
- Release archive checked: 247 entries, plugin/controller present, no GameAssembly, Assembly-CSharp, global metadata, data.unity3d, interop or dummy DLL output.
- Installed the release in the discovered Steam game directory with granted filesystem access. Original game hashes were checked after installation. Saves were not accessed. The game was left closed for the user's NVDA test.

The installer receipt is `StickyBusinessAccess-install.json` in the game folder. The local source is a separate Git repository. No remote repository or publication was created.

