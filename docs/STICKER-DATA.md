# Sticker metadata and the next accessibility stages

## Verified storage

The bundled `data.unity3d` contains serialized data. Reconstructed `UpgradeCategory` assets contain **1,271 upgrade records with 1,271 unique string IDs** in **30 categories**. These counts include colours, paper, goodies and shop upgrades as well as sticker elements; they are not a count of usable sticker pictures. DLC availability and unlock state must be obtained from the game at runtime.

`UpgradeInfo.ID` is the catalogue key. `UpgradesList` is a dictionary keyed by string; `UpgradeInfo.FromID` resolves it. Placed `StickerElement` objects hold `upgradeID` and expose `Upgrade`. `StickerItemLayerData.Sticker` uses `UpgradeInfoJsonConverter`, so the upgrade identity is associated with saved layer data. These are strong within-build persistent identifiers, not scene instance IDs. Their stability across future developer updates is not guaranteed: preserve unknown entries and compare ID inventories when updating.

Verified fields include tags, category asset membership, localization category, unlock/blocked state, price, currency, DLC, colour-change capability, sprite references and variant references. There are **65 StickerVariantList assets**, keyed by upgradeID; each variant has its own variantID and preview colour/sprite. Do not use asset path IDs, sprite names, object instance IDs or localized titles as permanent catalogue keys.

Actual examples from localization data (names, not newly authored visual descriptions):

| Upgrade ID | Existing English title | Category | Tags |
| --- | --- | --- | --- |
| st_apple | Apple | Food | food, fruit |
| st_cat | Blob Cat | Animals | animal, cat |
| st_cat_02 | Sleeping Cat | Animals | animal, cat |

The game has `GetLocalizationTitleKey`, `SpellLoca.HasKey` and `SpellLoca.Translate`; sampled title keys are `st_apple_Title` and `st_cat_02_Title`. Resolve titles through the game rather than assuming every key is ID + `_Title`. Localization containers can have translator description fields; those are not automatically accessible visual descriptions. No reliable, dedicated visual-description field was found in UpgradeInfo. Tags and names do not describe colour, shape, pose, markings or text placement in sufficient detail.

## Editable catalogue

`catalogue/descriptions.en.json` is an empty, valid starter. `descriptions.schema.json` describes the format; `DescriptionCatalogue.cs` parses it, rejects duplicate keys and invalid names/versions, and resolves names without Unity dependencies. It is validated on plugin load but is not yet connected to a sticker-selection UI.

The `entries` object is keyed by exact, case-sensitive UpgradeInfo.ID. Each entry requires `shortName` and may have `description`. Optional `variants` is keyed by exact variantID with the same short-name/detail structure. Keep language in separate files and declare `locale`. Preserve a user's file when updating; do not replace it with a regenerated file.

Resolution order: exact variant override, base upgrade override, game's localized title, then “Unlabelled sticker” and the exact ID. Missing detail stays absent; future speech should say that no detailed description is available if asked. Short names are spoken during navigation; details only on request. Unknown IDs should be logged once, without fabricating image content. Runtime unlock/DLC/price status belongs in the adapter, not in static descriptions.

Before authoring a visual description, inspect the corresponding sprite and variant. Record subject, pose/orientation, colours, shape, notable details and any lettering as observable facts. Keep user edits separate from the game's assets. This stage includes no image descriptions and no redistributed image inventory.

## Creative controls needed later

1. **Selection:** a searchable keyboard list with category/filter navigation, names, optional detailed descriptions, unlock/DLC state, cost and available colour variants. Keep distinct designs distinguishable even when tags match. Use `UpgradeInfo` and existing selection/spawn actions; do not unlock or buy automatically.
2. **Placement:** separate browsing and editing modes. Let the player select any placed layer, choose its position with coarse/fine steps, and hear coordinates relative to the canvas, edges, overlap and nearby items. The game exposes `SpawnInWorld`, `MoveStickerWithWASD`, `CheckIfNewPositionIsInBounds` and `IsOutOfBounds`. Validate coordinate units and bounds live. Snapping/alignment should be optional, explicit player commands.
3. **Resizing:** report current dimensions or scale and min/max limits; support predictable small and large increments. Existing `ScaleBigger`/`ScaleSmaller` and stored layer `Scale` are candidates. Preserve aspect and mirroring unless the player requests a change. Test how scale affects the game's bounds and undo.
4. **Rotation and mirroring:** announce an absolute angle and the change, with adjustable increments. Use `Rotate`, `SetRotation`, `MirrorHorizontal` and `MirrorVertical`. Verify angle direction/units live; do not silently straighten a design.
5. **Layering:** speak layer index, name and overlap relationships; expose forward/backward/front/back, lock/unlock and duplicate. Existing methods include `MoveLayerForward`, `MoveLayerBackwards`, `MoveToFrontLayer`, `MoveToBackLayer`, `ToggleLock` and `DoubleSticker`. Use the game's `UndoManager` and verify every operation is undoable. Do not reorder automatically for perceived aesthetics.
6. **Packing:** read customer/order requirements and quantities; distinguish designed stickers by player-assigned name/ID; announce packed and remaining items, available extras, letter placement, available space, costs and final confirmation. Investigate `PackingControllerManager`, `PrintedStickerElement`, `PackingExtrasManager`, `LetterPlacer` and `StickerPrint`. Preserve the player's choice of arrangement and decoration; offer optional placement assistance instead of auto-completing the order.

These method names and data fields were verified in metadata. Their behavioral contracts, numeric steps, undo semantics and speech timing require focused runtime work. No sticker selection, transformation, packing or economic action is implemented in this prototype.
