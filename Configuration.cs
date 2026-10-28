using Dalamud.Configuration;
using System;

namespace XIVKeyboundBrawler;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // Enable/disable the auto-typing feature
    public bool Enabled { get; set; } = false;

    // Addon name to watch for Keybound Brawler UI
    public string AddonName { get; set; } = "KeyboundBrawler";

    // Delay between keystrokes in milliseconds (base typing speed)
    public int TypingDelayMs { get; set; } = 50;

    // Delay before pressing Enter after typing the word in milliseconds
    public int SubmitDelayMs { get; set; } = 100;

    // Enable random delay variation
    public bool UseRandomDelays { get; set; } = true;

    // Random delay range for typing (min/max additional ms per keystroke)
    public int TypingRandomMinMs { get; set; } = 0;
    public int TypingRandomMaxMs { get; set; } = 100;

    // Random delay range for submit (min/max additional ms before Enter)
    public int SubmitRandomMinMs { get; set; } = 0;
    public int SubmitRandomMaxMs { get; set; } = 200;

    // Only type when the game window is focused
    public bool RequireGameFocus { get; set; } = true;

    // Minimum word length to process (filter out short noise)
    public int MinWordLength { get; set; } = 2;

    // Maximum word length to process
    public int MaxWordLength { get; set; } = 50;

    // Case sensitivity for matching
    public bool CaseSensitive { get; set; } = false;

    // Log detected words to plugin log
    public bool LogDetectedWords { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}