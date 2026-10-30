# StickyBusinessAccess 0.8.2 — installer revision 2

For installation, download **StickyBusinessAccess-0.8.2-setup2-player.zip** from **Assets**, extract it with **Extract All**, close the game, and run **Setup.bat**.

**Do not download “Source code (zip)” or “Source code (tar.gz)” to install the mod.** GitHub generates those source downloads automatically for tags; they are not removable uploaded player assets. Any separately uploaded DEVELOPER-source or prism-source archive is also for developers only.

This revision identifies source downloads before game selection, gives exact paths for missing or corrupt player files, handles a missing installer script clearly, and validates the completed player ZIP after extraction. Player instructions name the exact asset for screen-reader navigation.

The original v0.8.2 player ZIP contains all 231 expected payload files. This release addresses source/player confusion and diagnostics, not an omitted payload in that player ZIP. The reported error can be reproduced from the source archive; the tester's exact downloaded filename has not been confirmed.

The accessibility DLL stays at 0.8.2. Installer revision is 2; release tag is v0.8.2-setup2. All bundled dependencies, runtime files, licences and corresponding-source archives are retained. No game files or generated game assemblies are distributed. Uninstall behaviour is unchanged.
