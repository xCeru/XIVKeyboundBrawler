# Installation

Add the following custom repository in Dalamud Settings → Experimental tab → Custom Plugin Repositories:

```
https://raw.githubusercontent.com/xCeru/XIVKeyboundBrawler/main/pluginmaster.json
```

Click `+`, then `Save`. Open `/xlplugins`, switch to **All Plugins**, and install **XIVKeyboundBrawler**.

## Supported plugins

- **XIVKeyboundBrawler** — Auto-types words in the Keybound Brawler (Gold Saucer) mini-game.
  - Detects words above enemies in the Keybound Brawler UI
  - Configurable typing speed and submit delay
  - Random delay variation for human-like behavior
  - Word filtering by length and case sensitivity

## Cutting a new release (run locally)

You do not need the GitHub Actions `PAT` secret. Everything builds and releases from this repo directly:

1. **Build the plugin**
   ```bash
   dotnet build XIVKeyboundBrawler.csproj -c Release
   ```
   Output lands in `bin/Release/`.

2. **Package the zip**
   Zip the contents of `bin/Release/` (the DLL, deps, manifest, and dependency DLLs) into
   `plugins/XIVKeyboundBrawler/latest.zip`.

3. **Update the plugin manifest**
   Edit `plugins/XIVKeyboundBrawler/XIVKeyboundBrawler.json` — bump `AssemblyVersion` and add a changelog line.

4. **Regenerate the Dalamud repo manifest**
   ```bash
   python generate_pluginmaster.py
   ```
   This rewrites `pluginmaster.json` with the new version and download links.

5. **Commit and push**
   ```bash
   git add plugins/XIVKeyboundBrawler/XIVKeyboundBrawler.json plugins/XIVKeyboundBrawler/latest.zip pluginmaster.json
   git commit -m "Release XIVKeyboundBrawler vX.Y.Z.W"
   git push origin main
   ```

The Dalamud installer picks up the new version from `pluginmaster.json` automatically on its next refresh.

## Cutting a new release via GitHub Actions

You can also trigger a release from the GitHub web UI — no local build needed:

1. Go to `https://github.com/xCeru/XIVKeyboundBrawler/actions`
2. Select the **"Release plugin"** workflow
3. Click **Run workflow**, pick a version bump (patch/minor/major/revision), and run it

The workflow builds the plugin, bumps `XIVKeyboundBrawler.csproj`, packages `latest.zip`, regenerates `pluginmaster.json`, commits everything to this repo, and creates a GitHub release tagged `vX.Y.Z.W` with the zip as an asset. It uses the built-in `github.token`, so no PAT secret is required.

## Dalamud repo URL

```
https://raw.githubusercontent.com/xCeru/XIVKeyboundBrawler/main/pluginmaster.json
```