> Historical record of a pre-Prism release. Current speech and installation behavior are documented in README.md and PRISM-MIGRATION.md.

# Player setup revision 2 — verification, 2 October 2026

The prior player ZIP already bundled BepInEx and its runtime. Its existing-loader check did not block an unmodded game: it refused to overwrite another loader. The genuine lifecycle defect was that uninstall left generated files but removed provenance; subsequent fresh installation rejected the remaining BepInEx directory.

## Implemented changes

- Setup.bat runs ordinary Windows PowerShell text setup, automatically choosing install or plugin-only update. It is the only player entry point for these actions in the current package. Steam detection includes additional libraries and numbered selection for multiple candidates; failed detection offers a typed/pasted folder prompt.
- Exact loader: BepInEx Unity.IL2CPP win-x64 6.0.0-be.788+5b766a3; Doorstop 4.5.0; bundled .NET 6.0.7 runtime. The mod targets net6.0 and references the IL2CPP loader and locally generated interop at development time. Players need neither a SDK nor pre-generated game assemblies.
- Full fresh-install payload hashes, required-file checks, x64 game validation, write-access feedback, and link/path checks. Hashes catch damaged/incomplete extraction; they are not a signed authenticity guarantee.
- A completed uninstall keeps a small provenance receipt. Reinstall permits generated caches/interop/logs and preferences, preserves custom config/catalogue, and refuses retained executable or unknown loader files. Unknown independent loader installations remain protected.
- Plugin-only updates retain existing settings, catalogue, and loader with timestamped rollback. The gameplay DLL is unchanged from 0.7.0.
- Packaging uses ZipFile rather than Compress-Archive so hidden dependency files are included. The development tree and allowlisted player payload are separate.
- Official NVDA 2026.1 x64 controller DLL replaces the previously unidentified binary in fresh player packages. It is renamed, not modified. LGPL sources/licenses and other bundled component notices are included.

## Verification performed

Windows PowerShell 5.1 ran tests/InstallerTests.ps1 against copies of actual game originals and the complete player payload. Checks covered missing Steam detection; secondary library discovery with spaces; clean install including hidden Doorstop files and runtime; automatic update with custom key/config/catalogue preservation; rollback; uninstall disabling injection; reinstall with simulated generated files; unchanged hashes of five game originals; modified executable preservation/refusal; unrelated loader refusal without installation changes; and corrupted payload refusal.

An initial run caught PowerShell 5.1's non-enumerating ConvertFrom-Json array behavior in manifest validation. This was fixed and the lifecycle checks rerun successfully.

The extracted player ZIP passed 27 installer assertions, including missing-file extraction and manifest path traversal checks. Setup.bat itself was checked for successful update and a visible failure with a nonzero exit code. This exposed inherited PowerShell 7 module paths shadowing Windows PowerShell's utility module; shared setup now explicitly imports the inbox Windows modules. The final batch checks passed after correction.

A separate complete game copy started with no loader, plugin, cache, or interop. The packaged installer installed its dependencies. An unmodified player plugin was launched in the clean copy with Unity batchmode/nographics and a local Steam app-id file. BepInEx generated interop on first launch, initialized its chainloader, loaded Sticky Business Access 0.7.0, validated the catalogue, and completed chainloader startup. No test fixture code was injected into the player plugin. The copy was then stopped; uninstall and reinstall were tested against its actual generated files.

The official NVDA DLL loaded successfully on this machine, exposed all three API functions used by Speech.cs, and returned success for testIfRunning with NVDA running. This is API connectivity evidence, not an audible speech or keyboard accessibility certification.

## Limits

This verifies setup and startup on this Windows machine using the inspected owned game build. It does not establish a fresh physical friend's machine result, network behavior on every connection, full gameplay/DLC coverage, spoken installer usability, or an audible NVDA regression pass. No original installed game was uninstalled or replaced for these tests. Game files used for testing and generated interop remain private in work/ and are excluded from release packaging.

Send StickyBusinessAccess-0.7.0-setup2.zip. Players extract everything, run Setup.bat, then start NVDA and launch through Steam. Do not send the source checkout, test folders, or updater backup folders.

