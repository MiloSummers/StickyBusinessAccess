# Sticky Business Accessibility Mod

Sticky Business Access is an unofficial accessibility mod for **Sticky Business**, designed for keyboard play with screen-reader speech through **Prism 0.18.3**. Current mod version: **0.8.2**. Current player package: **0.8.2-prism**, with the existing Setup.bat installation process.

Sticky Business is a cosy sticker-shop game. Combine artwork into designs, arrange and print sticker sheets, receive customer orders, pack and send their stickers, and buy upgrades as customer stories progress.

You must own and install the game separately; the base game is not included. See the [official Sticky Business Steam store page](https://store.steampowered.com/app/2303350/Sticky_Business/).

## Accessibility Features

- Prism announcements for focus, screen changes, actions, contextual help, and information. Repeat announcements or list current controls, including unavailable controls.
- Keyboard access to supported buttons, toggles, dropdowns, sliders, and text fields. Supported lists scroll into view; returning from screens restores remembered focus where possible.
- Text editing with caret and deletion announcements. Password and authentication fields are excluded from spoken text. Finishing editing is separate from submitting a screen.
- Creator navigation through categories, available elements, and placed elements, with localized names and selection feedback. Native options include outlines and border width. Move, rotate, resize, layer, and remove placed elements.
- Keyboard placement of printed stickers on sheets and in packing boxes. Read placed-copy coordinates. Invalid sheet placement and overlaps are rejected; packing permits stacking. Removing packed copies returns stock through native handling.
- Add, select, move, and remove goodies from the selected packing category, with quantities and placement feedback.
- Production inventory navigation with available and ordered quantities. A spoken information view lists owned designs, including zero printed stock, and purchased goodies, including depleted stock.
- Live coins, hearts, working time, and displayed order details: customer, requested stickers, remaining quantities, reward, deadline, message, and selected packing materials.
- Shop, upgrade, and message announcements: prices, ownership, availability, progression requirements where exposed, new messages, story state, and history. Colour and pattern controls use native selections.
- Supported tutorial controls, pause menus, end-of-day results, and credits. Tutorials retain native progression requirements; credits are read in sections with automatic scrolling paused.
- Configurable keys and persistent day length: Normal, 1.25 times, 1.5 times, 2 times, or Unlimited working time. This changes time charged for supported work actions, not animations or customer deadlines. Native Limitless mode remains limitless.
- Diagnostic output. Native mouse interaction and game actions are retained.
- Section navigation for Creative Corner, sheet design and order packing. Sticker choices are restored after positioning, using game item identifiers even when the catalogue copy is replaced.
- Native game-save completion announcements. The game saves when creating a game and at end-of-day/next-day transitions; it has no ordinary manual-save button in its pause menu. Unsaved design editing and saving a sheet template are separate from saving game progress.

## Controls

These are **default bindings**. Context determines whether a key navigates, adjusts a control, or moves an item. Ordinary text entry takes priority; modified NVDA and Windows commands are left to those systems.

### Navigation and announcements

| Key | Action |
| --- | --- |
| Tab / Shift+Tab | In Creative Corner, sheet design, packing, shop, customization and upgrades: next / previous main section (categories, items, placement, controls), skipping empty sections. Elsewhere: next / previous control. Tab finishes text editing; Enter or Tab after sticker positioning returns to its item choice when available. |
| Enter / Space | Activate, confirm a dropdown, or finish item adjustment. While editing text, Enter finishes without submitting the screen. |
| Escape / Backspace / 0 | Back or cancel in supported navigation. Escape restores original text while editing. On the hub, opens pause when permitted. On end-of-day results, focuses Next Day, usable after native animations finish. |
| Up / Down | Browse within the current section on creator, sheet, packing, shop, customization and upgrades screens. Elsewhere: previous / next control, dropdown option, or text. Move selected items vertically during positioning. |
| Left / Right | Adjust sliders, supported toggles/dropdowns, navigate where supported, or move items horizontally. Hold for supported repeated adjustments; Shift makes larger steps. |
| Home / End | First / last item within the current section on creator, sheet, packing, shop, customization and upgrades screens. Elsewhere: first / last control or dropdown option; first / last row in text/information views. |
| Page Up / Page Down | Previous / next text section outside text editing. |
| F1 / H | Contextual help. |
| F2 / R | Repeat the last announcement. |
| F3 | List current controls. |
| F4 | Read screen information; on ordinary screens, finish positioning and switch to text reading. Tab returns to controls. |
| F5 | Open or close the spoken game-information view. |
| F8 | Write accessibility diagnostics. |
| B | Read current coins and hearts. |
| T | Read working-time information. |
| O | Read the currently displayed packing order; does not browse all orders from the hub. |

In the **game-information view**, Left selects the previous section; Right or Tab selects the next. Shift+Tab also goes forward here. Sections: Shop status, Current order, Printed inventory, Purchased goodies. Up/Down or Page Up/Page Down browse rows; Home/End select first/last row. Enter, Space, or F4 refresh the row. Back commands or F5 close the view and restore previous focus. Help, repeat, balance, and time remain available.

### Creating, printing, and packing

| Key | Action |
| --- | --- |
| F6 | Jump to creator categories or packing category tabs where available. |
| F7 | Jump to creator element choices or printed sticker choices where available. |
| F9 | Jump to placed elements, printed copies, or goodies where available. Sections remember their last focused item while the screen remains open. |
| Enter / Space | Add a focused available choice through its native action, or finish adjusting a placed item. |
| Arrow keys | Move selected creator elements, printed copies, or goodies. Hold to repeat and accelerate; Shift makes larger steps. |
| C | Read focused/selected placed **printed-copy** coordinates relative to sheet or box. Does not read creator-element or goodie coordinates. |
| Q / E | Rotate creator elements left / right. For printed copies, both invoke the same native quarter-turn rotation. |
| Minus / Numpad Minus | Resize selected creator element smaller. |
| Equals/Plus key / Numpad Plus | Resize selected creator element larger. The main binding is the Equals/Plus key itself. |
| [ / ] | Move selected creator element backward / forward in layer order. |
| Delete | Remove focused/selected placed element, printed copy, or goodie through native removal. |

Goodies support movement and removal, not creator rotation/resizing/layering. The first printed copy is centred on an empty sheet or box; later copies use relevant placement rules. Creator elements can deliberately overlap; print-sheet copies are checked for overlap.

### Shop hub shortcuts

These apply on the hub when available.

| Key | Action |
| --- | --- |
| 1 | Shop |
| 2 | Creative Corner |
| 3 | Upgrades |
| 4 | Production |
| 5 | Order packing |
| 6 | Messages |
| 7 | Send packed orders |
| 8 | Sleep |

### Changing controls

Open **Settings > Accessibility controls**. Actions appear in pages of eight. Activate an action and press a replacement single key. Duplicate/reserved keys are rejected. Possible native-control conflicts produce a warning; Enter confirms and Escape cancels.

**Restore defaults** resets bindings only. Bindings persist independently of saves in `BepInEx/config/local.stickybusiness.access.cfg`, under `AccessibilityKeys`. Modifier combinations are not configurable. Invalid/conflicting configuration values restore defaults.

Updates preserve valid custom bindings. If an older assignment occupies the new information-view default, another free key is assigned to that new action. Settings shows actual assignments.

Day length is also in the mod's Settings panel and persists under `Accessibility.DayLength` in the same file. Changing it does not restore minutes already spent.

The player ZIP opens to **Setup.bat**, **Uninstall.bat**, **START-HERE.txt**, and one **support** folder. Keep support intact; it holds the runtime, installer scripts, notices, and corresponding dependency sources. Gameplay DLL version is 0.8.2.

## Installation

### Requirements

- Your own Steam installation on **64-bit Windows**.
- A supported screen reader running for speech; Windows 10 or later.
- The entire extracted **StickyBusinessAccess-0.8.2-prism.zip**, including scripts and payload.
- An internet connection for first-launch generation of Unity interop files.

The player release bundles the exact required **BepInEx 6.0.0-be.788+5b766a3, Unity.IL2CPP win-x64**, Unity Doorstop 4.5.0, the loader's .NET runtime, and Prism 0.18.3 Windows x64 runtime. No separate BepInEx download, developer SDK, or configuration editing is needed. A source checkout is for development and is not an installer package.

Inspected game: Steam build **24353709**, Unity **2021.3.45f2**, IL2CPP metadata **31**. Future updates and every DLC combination are not guaranteed compatible.

### Fresh installation

1. Close the game completely.
2. Extract the whole ZIP. Run scripts from the extracted folder, not inside the archive.
3. Select **Setup.bat** and press Enter. It finds the game through Steam's registered libraries, including additional libraries, and installs the bundled dependencies and mod. This is the single entry point for installation and updates.
4. Read the result. If detection fails, setup asks for the actual game-folder address. Use Steam's **Manage > Browse local files**, copy the folder address, paste it into the text prompt, and press Enter. No drag-and-drop is needed. Multiple detected installations produce a numbered choice. Advanced users can also select a folder explicitly:

   ```powershell
   .\support\setup.ps1 -GameDir 'YOUR ACTUAL STICKY BUSINESS FOLDER'
   ```

5. Start your supported screen reader and launch normally through Steam. Keep an internet connection on first launch while BepInEx obtains Unity libraries and generates interop files. Allow several minutes; no manual dependency setup is required.

The installer validates the game, copies packaged loader/plugin/Prism/supporting files, and records ownership in `StickyBusinessAccess-install.json`. It does not replace original game assemblies or delete saves.

Setup validates the payload checksums before installation. If Windows denies write access, your Steam folder may require running Setup.bat as administrator. Error text remains visible until you press a key to close the setup window.

A completely unmodded game is supported: the existing-loader check does not require BepInEx to be installed first. Setup updates a recognized installed mod automatically. When an existing loader is found, setup first checks its version, dependency files, and configuration. A compatible installation offers a keyboard prompt: type Y to accept using it, or N/Enter to decline without changes. Accepting installs only Sticky Access; existing loader files, settings, and other mods remain untouched. Uninstall then removes this mod only and leaves the external loader in place. Updates preserve it too.

Only the pinned Unity.IL2CPP win-x64 be.788 bundle is supported for reuse; BepInEx 5, Mono builds, other versions, missing/modified dependencies, or disabled/incompatible Doorstop configurations produce an explanation and stop without overwriting. Acceptance cannot bypass compatibility checks. Compatibility with the loader does not certify interactions with every other installed mod.

### Updating

1. Close the game and extract the new release.
2. Run **Setup.bat** again; it recognizes the installed mod and updates it. For explicit selection, use `setup.ps1 -GameDir` with your actual game folder.
3. Read the backup location and retain the extracted folder.

The updater verifies ownership and checksums, replaces the mod and Prism together, removes an unchanged receipt-owned old controller, and preserves saves, loader, bindings and description catalogue. Modified or unowned speech binaries cause a refusal before changes.

Timestamped rollback copies of the mod, speech dependencies and receipt are under `support/backups` in the extracted release folder. Restore using the support folder's update.ps1 and actual paths reported by the updater:

```powershell
.\update.ps1 -GameDir 'YOUR ACTUAL STICKY BUSINESS FOLDER' -RestoreBackup 'YOUR ACTUAL BACKUP FOLDER'
```

### Uninstalling

Close the game and run **Uninstall.bat**. It removes unchanged receipt-owned files and retains modified files. Saves, custom settings, and generated logs/interop may remain. A small receipt records the completed uninstall so **Setup.bat can reinstall while preserving preferences**. Keep that receipt; it distinguishes this mod's leftover files from an unrelated loader. Modified executable files or another mod's files still require review and are not overwritten.

The setup scripts use ordinary text prompts and keyboard activation, with no graphical dependency wizard or drag-and-drop steps. Their physical-keyboard and spoken-NVDA usability still benefits from human testing; automated script checks do not certify audible accessibility.

## Using the Mod

1. Start NVDA, then the game. Listen for main-menu/focus announcements. See Screen Reader Information if speech is missing.
2. Navigate using the Controls section and choose the native new-game or continue action. Tutorials may restrict actions. Use contextual help on unfamiliar screens.
3. Explore hub destinations; information commands provide balances, time, inventory, and the displayed order.
4. Choose and arrange creator elements, then save your design. **Export** writes an image and is separate from saving a shop design.
5. Arrange copies in Production and activate **Print**. Printing spends coins and creates stock; saving a design does not create printed inventory.
6. Read the packing order and add required stickers, packaging, and optional goodies. Activate **Pack**, then **Send packed orders** from the hub. These are separate steps. Order switching may be unavailable while stickers remain in the box; pack or remove them first.
7. Read messages, buy available upgrades, and sleep to advance the day.

The mod gives access to your choices; it does not create designs, complete orders, or advance tutorials automatically.

## Screen Reader Information

Speech uses the bundled **Prism 0.18.3** native runtime through one central speech service. The old standalone NVDA controller DLL is no longer needed. Players need no reader add-on, Visual Studio, developer SDK or manual DLL download. Keep the complete release together and use **Setup.bat** as before. Existing installations also use Setup.bat: it updates the mod and Prism together, backs up owned speech binaries and removes the old controller only when its recorded checksum matches. Settings, keybindings and the description catalogue are preserved. Rollback restores matching speech binaries with the mod.

Prism's Windows backends include **NVDA, JAWS, Zhengdu Screen Reader (ZDSR), ZoomText, BoyPCReader, PCTalker and Sense Reader**, plus **Windows Narrator through UI Automation (UIA)**. Official release builds also include the legacy **System Access and Window-Eyes** backends, subject to compatible installed reader components. See Prism's [backend identifiers](https://github.com/ethindp/prism/blob/v0.18.3/doc/src/api/backend-identifiers.md) and [backend notes](https://github.com/ethindp/prism/blob/v0.18.3/doc/src/api/backend-notes.md). A registered backend does not prove that a reader is running or that this mod has been audibly tested with it. Prism also supports VoiceOver, Orca and Android screen-reader services on their platforms; **this BepInEx package remains Windows x64 only**, on Windows 10 or later.

Only one available reader backend receives each announcement, selected in Prism's priority order. UIA is an alternative path for readers consuming notification events. Detection uses Prism's runtime availability flags. Navigation and existing interrupting announcements replace speech; existing appended validation and paged-text messages retain their append behavior. The original 500 ms duplicate filter, intentional forced announcements and F2 repeat are retained. No application speech queue is added.

Use a correctly installed screen reader. JAWS, ZoomText, Sense Reader and Window-Eyes need their vendor-installed COM interfaces. PCTalker, ZDSR and BoyPCReader need their vendor client libraries to be resolvable by Prism, as detailed in the upstream backend notes. These proprietary reader components are supplied by their vendors, not redistributed as Prism development files in this mod. Missing components are reported as backend unavailability rather than crashing the game.

With no available reader, the game continues with keyboard accessibility and no mod speech. Availability is checked every second, and failed connections are recreated after reader exits/restarts. There is no system-TTS fallback. Prism's UIA availability check is advisory: it cannot guarantee a listening reader will speak the notification.

**Testing status:** on 4 October 2026, **MiloSummers** reported testing the 0.8.2 Prism build with **NVDA** and that everything was running smoothly. This is human testing of the current build; it does not establish exhaustive testing of every feature, DLC or tutorial branch. Other readers are supported through Prism but have not been reported tested in this mod.

For missing speech:

1. Start your reader on the same Windows logon session and desktop as the game. Start Steam first, then launch from Steam. After starting/restarting the reader during gameplay, allow a few seconds and press F2.
2. Re-extract the complete player ZIP and run Setup.bat with the game closed. `prism.dll` and `StickyBusinessAccess.dll` must be together in `BepInEx/plugins/StickyBusinessAccess`. Do not copy SDK headers, `.lib` files, debug builds or Prism compatibility shims.
3. Check first-launch BepInEx interop setup and `BepInEx/LogOutput.log`. Look for `Prism 0.18.3 loaded`, the selected backend, missing-DLL errors or backend initialization errors. Repeated identical warnings are suppressed. Per-announcement text uses debug logging; logged requests do not prove audible output.
4. Narrator/UIA needs the game's visible, non-minimized window and a reader consuming UIA notifications. Failed backends are recreated on a later availability check. If only the window title is spoken, check the loader timestamp using the Steam troubleshooting below.
5. Include mod version, reader/version, selected backend and keyboard sequence when reporting a problem. A successful build or log entry does not establish reader testing.


If the game opens and NVDA only announces its window title, check whether `BepInEx/LogOutput.log` has a new launch timestamp. If it has not changed, completely exit Steam using its Exit command, start Steam again, then launch Sticky Business from your Steam library with NVDA running. Closing the Steam window alone may leave the client running. A direct game launch that starts Steam can leave stale Doorstop loader flags in Steam's environment, causing later launches to skip the mod. Restarting Steam clears them; reinstalling or deleting saves is not needed. Start Steam before launching the game rather than opening the game executable while Steam is closed.

## AI Development Disclosure

The code has been developed with AI assistance. This is not an untested AI-generated project: a human screen-reader user has manually tested accessibility during development, identifying bugs through real gameplay.

AI assisted with code writing, revisions, and technical checks. Human accessibility testing and feedback came from the screen-reader user. Automated and isolated runtime checks do not replace physical keyboard use and listening to NVDA in gameplay.

## Human Accessibility Testing

**Milo Summers**, project creator and accessibility tester, has tested actual gameplay as a blind/screen-reader user using NVDA. Development involves repeated testing, bug reporting, fixes, and retesting of interactions and announcements.

This describes development, not certification of every release, screen, or DLC. Historical verification is recorded in `docs/ACCESSIBILITY-0.8.md`; migration checks and the manual NVDA checklist are in `docs/PRISM-MIGRATION.md`. MiloSummers reported successful NVDA testing of 0.8.2 on 4 October 2026; exhaustive coverage is not claimed.

## Known Issues / Work in Progress

- Announcements do not fully describe every sticker image or visual variant. The editable description catalogue does not replace all creator labels or describe every appearance.
- Every DLC unlock, large-inventory scenario, and tutorial branch has not been exhaustively tested.
- Twitch account connection is unverified, though supported settings controls are accessible.
- Installation on another physical computer and future game versions still need verification. Clean-copy installation, updates, uninstall, and reinstall are checked separately from manual NVDA gameplay testing.
- Other readers and exhaustive feature/DLC coverage still need human testing; NVDA testing of 0.8.2 has been reported successful.

## Feedback and Bug Reports

Report problems, confusing announcements, missing information, and suggestions through the project's **GitHub Issues** when using its published repository, or contact **milosummers** on Discord.

This checkout has no GitHub remote, so no verified repository or Issues URL is available. Use the repository you obtained the project from; direct links can be added when its actual publication URL is known.

Please include:

- What you were doing and which screen you were on.
- Expected behavior and what actually happened.
- What the screen reader announced, including confusing wording or silence.
- Screen reader and version.
- Reproduction steps, key sequence, and custom bindings.
- Mod version, game build, and relevant diagnostics.

Remove personal information, account details, and save contents before sharing logs.

## Credits

- **Milo Summers** — project creator, human accessibility testing, gameplay feedback, and bug reports.
- **Spellgarden Games** — original game developer.
- **Assemble Entertainment** — publisher.
- **AI-assisted development** — code and technical verification assistance; human testing is credited to the human tester.
- Third-party components — see `THIRD-PARTY.md` and bundled license notices. The mod's license is in `LICENSE`.

## Disclaimer

This is an **unofficial accessibility modification**, not the original game. It is not affiliated with or endorsed by its developers or publisher. The game and content belong to their respective owners; your own installation is required.

## README Maintenance

Update this README when controls, major features, installation requirements, compatibility, or limitations change. Check `src/AccessBindings.cs`, contextual access handlers, and release scripts. Distinguish human testing from AI coding and automated checks. Add verified GitHub links when a remote exists, and ship the matching README in the release folder and ZIP.


Shop sections: upgrades use Categories, Items and Controls; the sticker storefront uses Stickers and Controls; Customize uses Colours, Backgrounds and Controls. F6 jumps to upgrade categories, F7 to items/stickers/colours, and F9 to shop controls or customization backgrounds. Empty sections are skipped. Arrow navigation and native purchases/selections are retained.

