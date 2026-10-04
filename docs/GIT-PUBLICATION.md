# GitHub publication — 3 October 2026

Use the clean StickyBusinessAccess-GitHub-source snapshot for a fresh repository. Gameplay version is 0.8.1; the simplified player package is setup revision 1. Use the 0.8.1 source snapshot. No GitHub remote, commit or upload was created during this audit.

The original local repository is retained for recovery. Its four historical commits include a removed private test harness with local paths. Do not push that history or upload the parent workspace. Deleting the harness from the current tree does not remove it from historical commits.

The publication snapshot uses an explicit source allowlist: current source, tests, build/install/package scripts, editable catalogue, configuration defaults, documentation and license notices. It excludes .git, private fixtures, build outputs, game binaries/assets, generated interop, runtime payloads, saves, logs, backups and source-archive binaries. Contributors supply their own game installation and local interop; see CONTRIBUTING.md. Corresponding third-party sources are fetched and hash-verified before packaging.

Before creating the initial commit, choose your Git author identity (a GitHub no-reply address is suitable), inspect the staged files and use a fresh repository. Upload the player ZIP separately as a release asset. Keep its support folder and license/source notices intact. Automated and isolated game checks are documented in RELEASE-VERIFICATION.json; audible NVDA and physical-keyboard testing remain distinct.
