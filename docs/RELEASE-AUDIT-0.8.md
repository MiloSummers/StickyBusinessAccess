> Historical pre-Prism release audit. Current migration evidence is in PRISM-MIGRATION.md, PRISM-SOURCE-AUDIT.json and RELEASE-VERIFICATION.json.

# Current release: 0.8.1 shop navigation extension

The 0.8.1 setup1 package extends sections to upgrades, storefront and customization. Gameplay DLL changes are confined to MenuAccess section classification/help/category refresh and version metadata. InventoryAccess.cs is unchanged. RELEASE-VERIFICATION.json records current build and isolated runtime checks. The audit below describes the preceding 0.8.0 packaging cleanup.

# Publication and package audit — 3 October 2026

Gameplay version 0.8.0; player setup revision 4. This revision simplifies distribution without changing the gameplay DLL.

The player archive opens to Setup.bat, Uninstall.bat, START-HERE.txt and support/. Installer scripts, payload, notices, sources and detailed documentation live in support/. Wrappers support the new layout and the original flat development layout. Setup still verifies payload hashes and preserves native files, saves and preferences. No runtime libraries were removed. Duplicate catalogue copies, historical developer documents and four optional/redundant source or binary archives were omitted from the player ZIP; all notices and the four required LGPL source archives remain.

The original local Git repository has four historical commits containing a removed private test harness. The clean publication snapshot is exported from reviewed current files using an explicit source allowlist. It contains no .git history, private harness, bin/obj, generated interop, game assets/binaries, saves, logs, receipt/backups, runtime payload, or source-archive binaries. The original repository/history is retained locally. Use the clean source snapshot for a fresh GitHub repository. No remote or publication has been performed.

Development uses local game/interop files. Tests need a .NET 9 or newer SDK; the gameplay assembly targets .NET 6 for the bundled loader. `fetch-distribution-sources.ps1` verifies or downloads the pinned corresponding source archives before player packaging. These archives stay Git ignored.

Validation results are recorded in RELEASE-VERIFICATION.json and ACCESSIBILITY-0.8.md. Physical-keyboard and audible-NVDA coverage is distinguished from automated and isolated game checks. No universal compatibility claim is made.
