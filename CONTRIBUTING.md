# Contributing

Update root README.md alongside changes to accessibility controls, major features, installation, compatibility, or limitations. Verify against the implementation and ship matching release documentation. Distinguish human screen-reader testing from AI coding and automated checks.

Issue reports should include game build, mod version, the screen/action, expected speech, actual speech, and a short keyboard sequence. Remove personal names, save data and account details from logs before sharing them. Twitch account connection is not verified.

Build on Windows with a .NET 9 or newer SDK (the gameplay project targets .NET 6), BepInEx 6 IL2CPP, and locally generated wrappers from your own Sticky Business installation:

```powershell
./build.ps1 -LoaderDir 'D:\LocalDependencies\BepInEx' -InteropDir 'D:\LocalDependencies\StickyBusinessInterop'
dotnet run --project tests/CatalogueTests.csproj -c Release
```

Do not commit game assemblies, generated interop, decompiled code, extracted sprites/assets, saves, loader payloads, private fixtures, credentials or personal logs. Keep local testing outside this repository. The normal build must exclude `SMOKE_TEST`; the optional `SmokeSource` build property is only for an external private harness.

Run fetch-distribution-sources.ps1 and fetch-prism.ps1 to obtain pinned source archives and the official Prism 0.18.3 Windows x64 runtime after a clean clone. Package with package.ps1 -LoaderDir <pinned be.788 loader directory> -Destination <new release folder>; PrismDll defaults to dependencies/prism/prism.dll. Packaging verifies runtime/source checksums and ships Prism licenses and sources. The upstream release statically links its MSVC CRT. Players use Setup.bat, including upgrades from the old backend. Never ship game or interop assemblies.

Preserve native game actions, mouse controls, player choices and save validation. Do not generate designs or advance tutorial steps automatically. Document what was build-checked, exercised through native runtime handlers, visually inspected, and tested with physical keyboard/NVDA. Preserve working Settings sliders and text-entry behavior when changing other screens.

