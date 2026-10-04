# Third-party components

The mod's original source is MIT licensed. Other components retain their licences.

- **BepInEx 6.0.0-be.788+5b766a3**, LGPL-2.1, official unmodified Unity.IL2CPP win-x64 archive. Licence in `licenses/BepInEx-LICENSE.txt`. Source: https://github.com/BepInEx/BepInEx/tree/5b766a3
- **Prism 0.18.3 Windows x64**, MPL-2.0, official unmodified `dynamic/release/bin/prism.dll`. Release: https://github.com/ethindp/prism/releases/tag/v0.18.3 . Source: https://github.com/ethindp/prism/tree/v0.18.3 . Notices are in `licenses/Prism-NOTICE`; upstream license texts are retained under `licenses/Prism/`, and the pinned Prism source archive accompanies the player package. Prism embeds its NVDA RPC implementation; its NVDA attribution is required, but a separate NVDA controller DLL is not bundled. Compatibility-shim DLLs and development files are not shipped.
- BepInEx's archive includes .NET 6.0.7, HarmonyX 2.10.2, Il2CppInterop 1.5.3, Cpp2IL, AsmResolver, Mono.Cecil, MonoMod, AssetRipper libraries, Disarm, Iced, Capstone, SemanticVersioning, and native Dobby. These binaries are unmodified. The official loader archive did not include their license texts, so upstream licenses and .NET third-party notices are now included explicitly in `licenses/`. Version/source evidence is recorded in `docs/DEPENDENCY-SOURCES.json`.
- **Unity Doorstop 4.5.0** — loader entry point, upstream license in `licenses/Doorstop-LICENSE`; source: https://github.com/NeighTools/UnityDoorstop/tree/v4.5.0 . Dobby license is in `licenses/Dobby-LICENSE`.

LGPL components remain separate libraries. Their license texts and corresponding BepInEx, Il2CppInterop, Unity Doorstop source archives accompany the player package under `third-party-sources/`; users may replace/rebuild these libraries under their licenses, including debugging modifications. GNU GPL texts required by the LGPL are also included. Other upstream license/copyright notices are retained. These notices do not license the original Sticky Business game.

Local analysis tools and generated game references are not bundled. No Sticky Business code, metadata, art, saves or generated game assemblies are included.

