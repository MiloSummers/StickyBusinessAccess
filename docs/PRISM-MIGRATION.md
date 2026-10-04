# Prism migration: 0.8.2

The 0.8.1 source supplied from the earlier chat used a central Speech class which loaded nvdaControllerClient64.dll and called NVDA's running, cancel and speak APIs directly. It cleaned markup/whitespace, suppressed identical non-forced speech within 500 ms, retained the last message for repeat, interrupted by default and allowed selected validation/paged-text messages to append.

0.8.2 retains the Speech entry points and all existing call-site parameters. SpeechService owns cleanup, duplicate suppression, repeat and failure containment. PrismSpeechBackend is the only native reader interface. Its cdecl delegates, UTF-8 strings, one-byte C bool, size_t indexes and 64-bit IDs match the official Prism 0.18.3 header. It initializes a context, enumerates the registry, excludes synthesis engines, probes runtime availability, initializes one backend in Prism priority order and sends each message once with its existing interrupt flag. Failures release the backend for recreation on the existing one-second availability poll. Messages are not retried through another reader after a failed delivery, since partial delivery might duplicate them. Backend/context/library resources are released on game exit.

The migration changes Speech.cs, adds SpeechService.cs and PrismSpeechBackend.cs, and changes only the version strings and game-exit cleanup in Plugin.cs/MenuAccess.cs. The project version is 0.8.2. The source audit records 16 byte-identical existing accessibility source files and all 96 existing Speech.Say call sites. MenuAccess's only announcement text change is the version in the load announcement. Bindings, controls, feature logic, descriptions and game actions are preserved. See PRISM-SOURCE-AUDIT.json.

Packaging adds the unmodified official Prism Windows x64 dynamic release DLL, pinned archive/DLL/source checksums, all upstream license notices and the Prism source archive. The DLL's PE imports are Windows system libraries; it has no external MSVC runtime DLL import. Compatibility shims, debug DLLs, headers and import libraries are unnecessary for players. The standalone NVDA controller DLL, its download/source records and old standalone license/readme copies are removed. Prism's own NVDA attribution remains required. Historical release documents remain explicitly historical.

Setup.bat remains the entry point. Update now handles the mod and speech binaries transactionally, verifies ownership/checksums, backs up prior binaries and receipt, removes only an unchanged owned old controller, and supports restoring matching dependencies on rollback. Unowned or modified binaries are refused before changes. Existing BepInEx, settings, keybindings, catalogue, game files and saves keep their prior behavior. Installer checks now require prism.dll.

README.md lists Prism-supported Windows readers and platform limits. NVDA testing of earlier versions is historical evidence only. Following the automated migration checks, MiloSummers reported successful testing of 0.8.2 with NVDA on 4 October 2026. No testing of other readers or exhaustive feature coverage is claimed. UIA can serve Narrator and other notification-consuming readers; its availability signal is advisory. If no reader is available, mod speech is silent and keyboard access remains. There is no system-TTS fallback.

## Manual NVDA regression checklist

Use a separate test shop and the same installed game/version used for your earlier testing.

1. With NVDA running, install or upgrade using Setup.bat. Launch from Steam. Confirm the 0.8.2 load announcement and initial focus are spoken once. Check the log selects NVDA rather than also delivering through UIA.
2. Navigate menus rapidly and hold navigation keys. Confirm old focus speech is replaced and no backlog continues after you stop. Verify F1 contextual help, F2 repeat, list-controls commands, unavailable controls and screen transitions.
3. Check selected/unselected states, checkboxes, slider values, dropdown choices and text fields: normal typing, deletion, caret review, Escape restoration, Tab navigation, validation and passwords. Verify your NVDA typing echo preferences do not cause unexpected duplicate feedback.
4. Check tutorials, inventory counts/descriptions, Creative Corner, sticker placement, position/rotation announcements, held movement, boundaries and overlap warnings. Compare wording and information with 0.8.1.
5. Check production, packing, paper/filling/goodies, order/customer details, balance, end-of-day and status/error feedback. Verify appended paged text and validation are not lost while navigating.
6. Check Settings, your saved keybindings and game speed/day length. Restart the game and confirm preferences persist.
7. Exit NVDA while the game remains open, navigate, then restart NVDA. The game must remain usable and speech should return after an availability poll or the next F2/navigation request. Also launch with no reader and start NVDA later.
8. Alt+Tab away and back. Verify ordinary NVDA modifier commands remain available and focus speech is correct on returning.
9. Check a fresh install, an upgrade from the old NVDA version and, if needed, rollback. Confirm the complete package works without separately downloading DLLs. Only claim tested readers after listening to them in gameplay.

Automated verification results are recorded alongside the final deliverable; these do not certify audible behavior, every DLC/tutorial branch or another computer.

## Verification results

Release build succeeded with zero warnings and errors. All 32 existing C# checks and 22 speech checks passed. The installer suite passed 27 checks, the existing-loader suite 16 checks, and the new migration/rollback suite 10 checks. Pinned runtime/source fetches verified successfully. No game or audible screen-reader test was run during this migration.

Remaining NVDA-specific references were reviewed: only backward-compatible installer cleanup/rollback, its migration tests, upstream license attribution, screen-reader compatibility/test guidance, and historical records remain. No active mod source imports or calls the NVDA controller API. Native speech is confined to PrismSpeechBackend; all announcements still route through Speech/SpeechService.

## Subsequent human testing

On 4 October 2026, MiloSummers identified NVDA as the reader used and reported that the latest build was running smoothly. This user-reported testing followed the automated migration checks. The exact NVDA version and individual checklist coverage were not provided. Other readers remain supported through Prism but untested in this mod.
