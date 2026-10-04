# Navigation and DLC update 0.8 — 3 October 2026

## Controls

Creative Corner, sheet design and packing now have four navigation sections: categories, items, placement, and controls. Tab moves forward and Shift+Tab backward, skipping empty sections. Arrows browse within the current section; Home/End reach its first/last item. Sliders and toggles retain their existing Left/Right adjustment behavior. During positioning, arrows move the selected object.

F6 reaches creator categories or packing tabs, F7 reaches item choices, and F9 reaches placed objects. Each section remembers its last focused object while the screen remains open. These use existing configurable commands. No additional key binding is needed. Outside these three screens, Tab retains ordinary control navigation, including the production inventory.

Enter or Tab finishes sticker positioning and restores the same catalogue choice. Stable upgrade/design IDs handle replacement catalogue objects and duplicate display names. If stock is exhausted or a choice disappears, navigation falls back to available choices. Rejected printed placements restore choice focus without suppressing the native validity reason. Existing native quantity, physics, collision, printing and packing rules remain in force.

## Investigation

The 0.7 navigator gathered, filtered, raycast-tested and sorted controls every 150 milliseconds. Creator and printed-element deduplication also searched the accumulating list for every element. Focusing a creator item revealed both its button and parent element, and reveal forced all canvases to update before and after every browse step.

0.8 uses hash-set deduplication, remembers section membership, removes the duplicate reveal and avoids unconditional canvas flushes while browsing creator/printed item catalogues. Scrolling still updates the canvas when its content moves. Other controls, including inventory rows, retain their initial flush. Heavy screens gather at most once per second while unchanged. Screen resolution still runs every 150 milliseconds for modal ownership; category selection, activation, deletion, returning to choices and screen changes refresh promptly. A native selected-category change also invalidates the gathered list. Other screens keep the previous scan schedule.

These findings establish avoidable mod overhead. They do not establish that every reported delay originates in the mod. Native category activation still changes active panels, checks item availability and performs layout work. No base-game category/animation timing was patched. Speech is still interrupted for intentional navigation; placement completion now uses one focus announcement instead of announcing controls and immediately replacing it.

The old navigator also blocked input for 250 milliseconds after every category button click. Category/tab selection now refreshes immediately and uses a 50-millisecond input guard. Other screen transitions retain their previous guard. This removes a definite 200-millisecond mod-imposed wait without changing native category callbacks.

Paired microbenchmarks in the same fully unlocked test game, averaging 20 calls per version: Animals gathering 4.94 ms in 0.7 versus 4.45 ms in 0.8 (90 controls); Fonts gathering 4.20 versus 4.04 ms (65 controls). Fonts reveal averaged 0.132 versus 0.050 ms; Animals reveal 0.085 versus 0.060 ms. These are small synchronous samples, not end-to-end input or NVDA latency measurements. The previous duplicate creator reveal made two calls per focus. The reduction in scan frequency and category input guard are additional changes beyond these per-call figures.

## DLC UI and saving

The installed game reports ownership of PlanWithMe, CampZinnias, Witchy and SeasideTales. The inspected creator category strip is icon based. “DLC” is derived from its native `DLC Items` group, rather than proof that sighted players see a text heading reading “DLC”. It holds Plan With Me elements: tape, stationery, trackers, lettering and decorations. Camp, Witchy DLC and ST DLC are separate existing native groups. Their availability depends on owned/unlocked upgrades. The mod retains these existing group names rather than inventing a new category hierarchy.

The fully unlocked private fixture exposes all 23 native creator categories, including these four DLC groups. Localized element titles come from the game's upgrade metadata. Clipped catalogue entries remain accessible and scroll into view. Locked or unowned elements retain native availability rules.

Native `Game.Save` calls synchronous file writes and then records the saved game. Direct native callers inspected in this build are new-game creation and end-of-day/next-day coroutines. The pause implementation has Continue, Settings, BackToMenu and ExitGame, with no ordinary manual-save action. The end-of-day UI has `Saved Icon/Text (TMP)` reading “Game saved”. 0.8 observes successful return from `Game.Save` and queues “Game saved” after action feedback, without initiating a save. Saving a sticker or sheet design is not represented as a manual game-save function.

## Verification limits

Observed integration results:

| Workflow | Result |
| --- | --- |
| Plan With Me creator | Colorful Washitape selected, placed, finished, same choice restored, placed again |
| Camp creator | Butterfly Wings selected, placed, finished, same choice restored, placed again |
| Witchy creator | Crystal Ball selected, placed, finished, same choice restored, placed again |
| Seaside creator | Amazon Dolphin selected, placed, finished, same choice restored, placed again |
| Base-game sheet | Test Cat placed and same choice restored; repeated placement |
| DLC sheet | Saved composite of all four DLCs placed twice, same choice restored after each; both copies native-valid |
| DLC printing and packing | Two composite copies printed through native Print, both packed; same choice restored while stock remained; exhausted choice fell back to Test Cat |
| Base-game packing | Test Cat placed and same choice restored |
| Section navigation | F6 packing tabs, arrows between tabs, activation, F7 choices; creator End/Down wraps within items, Tab to controls, F6 categories, Down remains in categories |
| Inventory | Native production sidebar remained open, quantities and zero-stock design rows accessible; native print and packing stock accounting retained |
| Save | End-of-day native “Game saved” indicator observed and corresponding mod completion announcement logged |
| Fast category navigation | Arrow input 60 ms after selecting Food moved category focus to Gaming; item list remained Food (Apple), confirming refresh and shorter guard |
| Rejected placement | Forced a DLC copy onto an existing sheet copy; native overlap rejected it, same DLC choice restored; F3 and arrows worked afterwards without stale-control or fixture errors |

Build and automated catalogue/navigation/editing/day-length checks pass. `InventoryAccess.cs` is unchanged from 0.7.

Rejected-copy controls are removed immediately and a fresh gather runs on the following frame, after Unity's deferred destruction. Keyboard input also refreshes stale disabled/destroyed controls before using them. This avoids retaining removed objects during the longer idle refresh interval.

Tests use the installed Windows IL2CPP build in a private game copy and redirected persistent storage. Real saves and DLC ownership were not modified. Native upgrade unlocks were granted only to the private fixture to exercise all DLC categories. No DLC-ownership patch was used.

These are AI-operated integration checks and automated code checks, not human keyboard and audible NVDA gameplay certification. Physical-keyboard and audible-NVDA confirmation of responsiveness remains necessary. Future builds and all possible stock/unlock/variant combinations are not certified.

## 0.8.1 shop section extension

Upgrades: categories (native tabs and category icons), items (native upgrade cards), controls. Storefront: sticker actions and controls. Customize: colours, backgrounds and controls. Tab/Shift+Tab cycle nonempty sections; arrows and Home/End remain within the current section, retaining its remembered focus. F6 upgrade categories, F7 items/stickers/colours; F9 controls on shop/upgrades, backgrounds on Customize. Category-tab activation refreshes immediately with the same shorter input guard as creator categories. Modal preview/rename/purchase dialogs retain their existing control navigation. InventoryAccess.cs and native purchase handlers are unchanged.

An isolated all-DLC fixture exercised section cycling, arrows and section shortcuts in all three shop contexts with no logged mod/fixture errors. Build and 32 pure checks pass. This revision does not claim new human NVDA or physical-keyboard testing.
