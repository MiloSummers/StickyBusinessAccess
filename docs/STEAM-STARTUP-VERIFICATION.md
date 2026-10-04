# Actual Steam launch investigation — 2 October 2026

Reported symptom: game opened, only its title was spoken, accessibility keys did not work. The actual Unity Player.log showed recent normal game launches, while the installed BepInEx log still contained old fixture output. The installed gameplay DLL matched the release checksum.

A normal Steam launch reproduced the problem. Its process loaded the game's Doorstop winhttp proxy, but no CoreCLR runtime or new BepInEx log appeared. Read-only inspection of the game and Steam process environments found DOORSTOP_DISABLE=TRUE and DOORSTOP_INITIALIZED=TRUE in both, with loader paths pointing to this game. Steam had inherited stale flags from a prior loader/game launch; its child game processes inherited them again.

The upstream Doorstop source skips initialization when these flags are set. The corresponding Steam restart case is documented at https://github.com/NeighTools/UnityDoorstop/issues/34 . The earlier file-copy installer checks and direct clean-copy boot did not cover this state of the real Steam client.

Closed the diagnostic game through its normal window close, requested Steam's graceful shutdown, and restarted Steam from an environment without Doorstop flags. Read-only inspection confirmed the restarted client had no Doorstop environment entries. A subsequent normal Steam launch created a fresh BepInEx log, loaded the unchanged Sticky Business Access 0.7.0 DLL, completed chainloader startup, and logged startup and main-menu speech. No fixture code, Steam app-id file, save edits, or loader replacement was used for this repair.

README and START-HERE now document starting Steam first and fully restarting it if this symptom occurs. The game was left on its main menu for an audible NVDA and physical navigation check. The user then confirmed: “Yes, speech and navigation work.” This confirms the reported startup problem was resolved on this computer; it does not establish testing on every other player's computer.
