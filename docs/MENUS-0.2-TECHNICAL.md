# Version 0.2.0 technical verification — 30 September 2026

## Compatibility and analysis

Installed Steam app 2303350, build 24353709; Windows AMD64; Unity 2021.3.45f2; IL2CPP metadata 31. Loader remains BepInEx 6 Unity.IL2CPP win-x64 6.0.0-be.788+5b766a3. The user's live test confirmed the previous main-menu speech. See FOUNDATION-TECHNICAL.md for historical analysis/tool provenance; its old scope and runtime limitations are superseded here.

Inspected local IL2CPP signatures and Unity serialized component data for SettingsScreen, MainMenuScreen, NewGameScreen, ScreenSwitcher, CanvasScreenManager, Popups, ControllerButtonCapture, ScrollOnEnable and TutorialTrigger. Dummy method bodies are not treated as recovered game implementations. Controls and callbacks were then exercised inside the installed game with a private integration build.

The New Game form has player/shop name fields (25 characters each), tutorial toggle, three day-speed choices, a validation label and a Create button. The mod invokes the actual Create button and normal validation. It does not construct or overwrite saves itself.

## Implementation

- Current ScreenSwitcher and active main-menu panels determine scope; popup canvas takes priority. Focus is remembered per panel. Navigation does not enumerate the main menu behind a submenu.
- Settings read labels, values, unavailable states, sliders, toggles and TMP dropdowns/fields. Offscreen controls in ScrollRects are scrolled into view. Actual value setters invoke existing game callbacks. Existing tab-like buttons/toggles use these same controls; no separate tab strip was found in the inspected settings layout.
- Expanded dropdowns own a temporary navigation context. Arrows browse a pending choice, Enter commits, Escape cancels. Screen changes supersede an open dropdown.
- Escape/Backspace invoke the active screen's existing Back/Cancel/No/Close button; message popups can use their OK action. Escape during editing restores the original field text; Backspace stays with text editing.
- Credits auto-scroll is stopped and full text is divided into ordered sections. Page Up/Down reads and scrolls them. F4 reads current text; F3 lists controls.
- Name entry uses the actual TMP field, focus and activation. Letter shortcuts are suspended while editing. F1/F2 remain available. Native UI navigation and the game's keyboard accept/cancel callbacks are guarded to prevent duplicate handling. Mouse/gamepad callbacks remain native.
- Opening tutorial text is announced with its Continue action. Gameplay editing/packing is not implemented. H/F1 help, R/F2 repeat, value-change tracking and speech deduplication are retained.

## Automated checks

The production .NET 6 plugin builds against the loader-generated interop with zero warnings/errors. Nine catalogue tests and seven navigation/text paging tests pass.

The private SMOKE_TEST integration build ran in the installed Unity player. It exercised actual game controls and the same keyboard-handler methods used by the release, rather than synthesizing Windows keystrokes. It verified settings discovery and reachability; slider increase/decrease; dropdown open/browse/cancel and day-speed commit; credits text; returning from settings, credits, New Game, nested streamer settings and Load Game; focus restoration; modal isolation and normal No callback; name editing, Tab completion, Escape cancellation and letter-shortcut isolation; New Game validation enabling Create; creation of a disposable new game; and arrival at the opening tutorial with readable instructions and Continue action.

Test isolation: a compile-time-only hook redirected Unity persistent storage into work/isolated-game-data, outside the distributable. The shell sandbox could not initialize the user's Steam session. A second compile-time-only hook supplied the normal non-beta result for SteamApps.GetCurrentBetaName, a metadata query used by language/version UI and game setup. It did not modify ownership, DLC, authentication, creation or save-validation logic. No existing user save was loaded, overwritten or deleted. This is an offline integration test, not a full Steam/NVDA desktop test.

The release build excludes the test harness, storage hook and Steam metadata hook. It uses normal user storage and Steam behavior. Generated game code, extracted assets, interop references and disposable saves are excluded from the package.

Updater fixture checks passed: DLL and receipt update, user catalogue preservation, rollback of DLL/receipt, refusal of a modified installed DLL, and refusal of a corrupt release payload. The updater replaces only the owned plugin DLL and its receipt, keeping a rollback copy. The loader and catalogue remain unchanged.

## Live NVDA testing still required

Use the short sequence in README.md. Confirm audible speech, real Windows key timing, no duplicate native key actions, normal character entry/backspacing, help/repeat, physical dropdown selection, focus restoration and the New Game transition under the user's Steam session. The automation assigned test strings to real fields; it did not prove physical typing or NVDA echo behavior. It did not exercise every language/resolution choice, account authentication, external links or destructive save deletion. No such actions are needed for the requested keyboard test.

The opening tutorial reached by the test introduces the desk and shop and offers Continue tutorial. The player controls whether to advance. Sticker editing is deliberately outside this release.
