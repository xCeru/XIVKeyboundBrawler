# XIVKeyboundBrawler

A Dalamud plugin for FFXIV that automatically types words in the Keybound Brawler (Gold Saucer) mini-game.

## Features

- **Auto-detects words** above enemies in the Keybound Brawler UI
- **Configurable typing speed** - set delay between keystrokes
- **Configurable submit delay** - set delay before pressing Enter
- **Random delay variation** - add human-like randomization to delays
- **Word filtering** - configure minimum/maximum word length
- **Case sensitivity option** - choose exact or case-insensitive matching
- **Game focus requirement** - only types when FFXIV is the active window

## Installation

### Via XIVLauncher (Recommended)

1. Open XIVLauncher settings (`/xlsettings` in-game)
2. Go to "Experimental" tab
3. Add custom plugin repository: `https://raw.githubusercontent.com/xCeru/XIVKeyboundBrawler/main/pluginmaster.json`
4. Install "XIVKeyboundBrawler" from `/xlplugins`

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/xCeru/XIVKeyboundBrawler/releases)
2. Extract the ZIP to your Dalamud plugins folder: `%APPDATA%\XIVLauncher\addon\Hooks\dev\`
3. Restart XIVLauncher or reload plugins (`/xlreload`)

## Usage

1. Open the plugin UI with `/kbbrawler` command
2. Enable "Enable Auto-Typing"
3. Start the Keybound Brawler mini-game in Gold Saucer
4. The plugin will automatically detect words above enemies and type them

## Configuration

| Setting | Description |
|---------|-------------|
| **Enable Auto-Typing** | Toggle the auto-typing feature on/off |
| **Addon Name** | The UI addon name for Keybound Brawler (default: "KeyboundBrawler") |
| **Typing Delay (per char)** | Base delay between keystrokes in milliseconds |
| **Submit Delay (before Enter)** | Delay after typing word before pressing Enter |
| **Enable Random Delays** | Add random variation to delays |
| **Typing Random Range** | Min/max additional ms per keystroke |
| **Submit Random Range** | Min/max additional ms before Enter |
| **Minimum Word Length** | Ignore words shorter than this |
| **Maximum Word Length** | Ignore words longer than this |
| **Case Sensitive Matching** | Match word case exactly |
| **Require Game Focus** | Only type when FFXIV is focused |
| **Log Detected Words** | Log detected words to plugin log |

## Finding the Addon Name

If the plugin doesn't detect the Keybound Brawler UI:

1. Open Dalamud Debug UI: `/xldebug`
2. Search for "Keybound" or "Brawler" in the addon list
3. Note the exact addon name (e.g., "KeyboundBrawler", "KeyboundBrawlerMain", etc.)
4. Enter it in the plugin configuration

## Building from Source

Requirements:
- .NET 10.0 SDK
- FFXIV with XIVLauncher/Dalamud installed

```bash
dotnet build -c Release
```

The compiled plugin will be in `bin/Release/`.

## License

AGPL-3.0-or-later