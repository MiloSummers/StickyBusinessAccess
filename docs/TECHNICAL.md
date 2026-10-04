# Version 0.5.0 technical verification — 1 October 2026

This update visually inspected tutorial Inventory, the paper sheet, hub orders/balance and the packing phone/box. Native metadata/disassembly confirmed inventory toggling, sheet/container geometry, packing bounds and paper/filling selection state. Isolated runtime checks verified real centred copies at X 0/Y 0, arrow movement, optional coordinate speech, native printed inventory decrement and restoration on deletion, selected-state changes, B balance, O order requirements and automatic customer announcements after Next Order. Escape restored hub focus. Build passes with zero warnings/errors and the existing 24 checks pass. Physical keyboard timing and audible NVDA still need live testing. Mouse handlers were left unchanged; a mouse click automation attempt did not provide a reliable runtime placement result, so mouse regression remains a live check.

Settings adjustment/repeat and text begin/review/finish methods were compared against 0.4 and remain identical. CreatorAccess is unchanged; category selection and scrolling branches are preserved. Only printed sticker copies have the new centred positioning; optional goodie movement still needs work. No user save was loaded or overwritten during these tests: private builds redirected storage to isolated fixtures and are excluded from the installed release. See PLACEMENT-0.5.md and GIT-PUBLICATION.md.

The following is the historical 0.4 report and its coverage limits.

# Version 0.4.0 technical verification — 1 October 2026

Compatibility remains Steam app 2303350 build 24353709, Windows x64, Unity 2021.3.45f2, IL2CPP metadata 31 and BepInEx 6.0.0-be.788+5b766a3. FOUNDATION-TECHNICAL.md and MENUS-0.2-TECHNICAL.md are historical reports.

## Actual UI investigation

Inspected rendered game windows, serialized controls and persistent callbacks, localization, IL2CPP signatures and selected native disassembly. Generated interop, extracted code/assets and disposable saves remain in the private work directory and are excluded from the distribution.

Outline swatches invoke BorderColorChanger.ChangeColor on the complete composited sticker's border. Separate controls change border width. The inspected starting palette provides black/white. Additional outline choices depend on upgrades. The fresh tutorial's SaveScreen material dropdown contained only White Paper; it filters native unlock state. No additional starting material was invented.

The clock is a semicircular day-progress gauge rather than a numeric clock face. T reports Game.Day and Game.MinutesLeft. Pause's actual controls are Continue, Settings and Exit. No separate Save button was found. Sleep results show Game saved, and isolated save timestamps advanced after the day ended. This report does not claim autosaving on every menu transition.

Shop customization has six native assets: Yellow, Purple, Blue, Salmon, Pink and Green. Four background assets are None, Stars, Bubbles and Hearts. Swatches apply immediately. Sticker preview, sale listing, rename, export and delete are separate actions. Empty/locked slots are informational and disabled component proxies are omitted from keyboard actions.

Upgrade cards read native localization titles, ownership/blocking, visible cost/currency, statistics level, displayed progress and hover text where supplied. Enter invokes the game's purchase immediately; no extra confirmation or descriptions are invented. Verified examples include Sleeping Cat owned, Blob Cat costing 25 hearts, 5 shop slots owned and 10 shop slots costing 150 coins.

Message Board entries include customer names/nicknames, latest text or hints and native new-message/action-required/story-complete indicators. Revealed customers open a history with message text and Back/Close. Unknown entries can remain locked. No reply control was found in the inspected UI and native control metadata.

Production uses a paper choice, sheet designer, saved-sheet actions and printed inventory. The designer's internally named Print Button exports an image: it is labelled Export sticker sheet image. Save creates the template. SheetPrintPopup.Print spends coins and adds native inventory. Packing uses finite inventory, customer/message, required quantities, order value/XP/deadline, paper/filling and actual Pack action. Pack creates a package; hub Send takes packed packages to the post office. Printed-copy placement/movement uses the game's controller/drag APIs and synchronizes the native 2D body before validity checks. It does not generate extra inventory or mark an order complete artificially.

## Focus and keyboard behavior

Native CanvasScreenManager.CurrentSwitcher, modal controls and active TutorialTextBox determine navigation. A blocking tutorial with its own Continue action confines focus to that instruction. A tutorial that requests an external action includes the currently available native controls. Keyboard callbacks for creator/production/packing are guarded while the mod owns navigation; mouse/controller behavior is retained. The release never advances tutorial steps or creates designs without player activation.

Categories announce selected. ScrollRect items remain in navigation when clipped and are revealed as focus moves. F6/F7/F9 jump to categories, palette and placed objects as appropriate. Short spoken element names use the game's localization; detailed image/variant descriptions are still incomplete. The editable stable-ID catalogue remains validated but does not yet override creator labels.

End-of-day Up/Down reads individual text/items. Escape focuses the real Next Day button once available; it does not force continuation. Until then it reports that the results are appearing. After native continuation, normal hub/pause focus resumes. Other areas use their actual Back/Close, with remembered focus. Hub keys 1–8 invoke actual buttons; 0 uses Back. These shortcuts are inactive while editing and on other screens.

Text fields retain native typing/deletion/caret behavior. Deleted graphemes and cursor characters are announced; password/authentication text is excluded from character feedback. Enter finishes editing without form submission, Tab finishes/moves, and Escape restores the original field value while staying on that screen. H/R/T/number shortcuts cannot consume typed text. NVDA modifier chords are ignored. Credits behavior and Settings slider adjustment/repeat logic are retained from 0.3.

## Verification

Release build: zero warnings/errors. Automated tests: 9 catalogue, 7 navigation/text paging and 8 editing feedback checks, 24 total. Editing tests cover middle/end/selection deletion, emoji, spaces/punctuation and text-end review.

Private integration tests use workspace save redirection and a metadata-only Steam beta-query fixture. They do not bypass ownership, authentication or DLC. Tests invoke the actual game controls plus mod handlers, rather than physical keyboard events. Normal game actions can still have normal Steam effects, including achievements.

Verified in isolated runtime: native sticker saving; tutorial sleep/production transition; sheet paper popup; instruction focus containing only Continue; explicit sticker placement on a sheet; saving a 12-copy template; Print adding inventory and spending 10 coins per sheet; order remaining quantity reaching zero; Pack producing a package; hub Send clearing it; tutorial completion instruction; shop swatches/patterns; upgrade cost/ownership/tab labels; customer history text and return; Sleep → Next Day → Pause. Tests preserved actual tutorial requirements and native progression.

Final candidate runtime verified native shop asset names, available packing copy counts, delayed inside-box placement speech, F4 leaving placement mode and Up/Down reading the customer instead of moving the sticker. End-of-day XP/progress/bonus values were read; the affected element image did not match an available name, so that row currently says Element upgrade. A private all-visible-controls snapshot encountered a not-yet-initialized goodie identifier during a screen transition; its label now tolerates missing metadata. That final null guard is build-checked. No menu navigator access-error was logged in the final candidate run. Installation/release hash results are recorded in RELEASE-VERIFICATION.json.

Physical key timing, audible NVDA output, duplicate-speech experience, cursor review and deletion with NVDA's own typing echo require the user's live testing. Twitch connection, all purchased upgrades/materials/DLC, every tutorial/save branch, complex sticker variants and full visual canvas exploration remain unverified. Optional goodie movement/removal is not yet equivalent to the implemented printed-sticker positioning controls. Packing access in this version covers the verified basic sticker/paper/filling workflow; no claim of complete creative parity is made.

The release excludes private automation, Steam metadata mocking, save redirection, generated interop and extracted game content. The updater changes only the owned plugin/receipt and retains a rollback copy. Original game files and existing saves are not replaced.

The installed normal 0.4.0 release was launched through Steam after removal of the fixture. It reached the rendered main menu, logged the load announcement and available controls, and logged no access-error. No existing game was loaded in this normal startup check.

## Version 0.6 update

See [controls and placement verification](CONTROLS-PLACEMENT-0.6.md). Settings slider adjustment code is unchanged. Rebinding maps physical keys to existing accessibility commands; native typing uses physical text-editing keys. The new Settings page stores persistent bindings separately from game saves.

The final normal 0.6.0 release reached the rendered main menu through Steam and logged the initial Continue focus after controls became available. No access-error was logged; no existing game was loaded. The release DLL, installed DLL and ZIP plugin hashes match. Five recorded originals and 229 other owned files remain unchanged.


## Version 0.7 update

See [accessibility investigation and verification](ACCESSIBILITY-0.7.md). Earlier reports above are historical.
