> Historical record of a pre-Prism release. Current speech and installation behavior are documented in README.md and PRISM-MIGRATION.md.

# Existing-loader setup, revision 3

Setup.bat now checks an existing loader before offering a plain text keyboard choice. Y/Yes accepts reuse; N, any other answer, or Enter declines without game-folder changes. The prompt is explicitly printed before input so it remains visible in both ordinary console sessions and redirected test output.

Reuse requires the pinned BepInEx Unity.IL2CPP win-x64 be.788 version, matching loader/runtime files, enabled Doorstop, and supported IL2CPP target/runtime paths. User BepInEx.cfg settings and unrelated Doorstop options are preserved. Other versions, Mono/BepInEx 5, missing or modified runtime files, and disabled/incompatible entry points are explained and rejected. No acceptance bypasses these checks.

Accepted reuse copies only StickyBusinessAccess.dll, nvdaControllerClient64.dll, and descriptions.en.json. The receipt records loaderOwnership=external and owns only these plugin files. Update replaces the owned plugin only and rechecks the external loader. Uninstall removes this mod's unchanged files and only cleans its own empty plugin directory; external loader files, configurations, other mods, and their directories remain. Reinstall asks again before reusing the retained loader. Modified/unmanaged Sticky Access binaries are not silently overwritten.

tests/ExistingLoaderTests.py exercises the actual Setup.bat and Uninstall.bat using redirected Y/N/Enter input and private copies of original game files. Sixteen assertions cover decline, default decline, acceptance, exact preservation of all pre-existing file hashes, plugin-only ownership, settings-preserving update, uninstall preserving external loader/other mods/empty directories, reinstall, and refusal of disabled, modified-runtime, or wrong-family loaders. The ordinary clean-install lifecycle is also checked with tests/InstallerTests.ps1.

These are automated setup checks. They do not certify audible NVDA prompt delivery, compatibility with every other mod, or every BepInEx version. The gameplay DLL and dependencies are unchanged from setup2. Send StickyBusinessAccess-0.7.0-setup3.zip.

