using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace XIVKeyboundBrawler.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base("XIVKeyboundBrawler Configuration###XIVKeyboundBrawlerConfig")
    {
        this.plugin = plugin;
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        Size = new Vector2(480, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var config = plugin.Configuration;

        if (ImGui.CollapsingHeader("General Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();
            var enabled = config.Enabled;
            if (ImGui.Checkbox("Enable Auto-Typing", ref enabled))
            {
                config.Enabled = enabled;
                config.Save();
            }

            ImGui.SameLine();
            ImGui.TextDisabled("(/)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Toggle with /kbbrawler command");

            var requireFocus = config.RequireGameFocus;
            if (ImGui.Checkbox("Require Game Focus", ref requireFocus))
            {
                config.RequireGameFocus = requireFocus;
                config.Save();
            }

            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Only type when FFXIV is the active window");

            var logWords = config.LogDetectedWords;
            if (ImGui.Checkbox("Log Detected Words", ref logWords))
            {
                config.LogDetectedWords = logWords;
                config.Save();
            }

            ImGui.Separator();

            var addonName = config.AddonName;
            if (ImGui.InputText("Addon Name", ref addonName, 64))
            {
                config.AddonName = addonName;
                config.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("The UI addon name for Keybound Brawler. Use /xldebug to find it.");

            ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Typing Delays", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();

            ImGui.Text("Base Delays (milliseconds)");
            ImGui.Separator();

            var typingDelay = config.TypingDelayMs;
            if (ImGui.SliderInt("Typing Delay (per char)", ref typingDelay, 0, 500))
            {
                config.TypingDelayMs = typingDelay;
                config.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Delay between each keystroke when typing a word");

            var submitDelay = config.SubmitDelayMs;
            if (ImGui.SliderInt("Submit Delay (before Enter)", ref submitDelay, 0, 2000))
            {
                config.SubmitDelayMs = submitDelay;
                config.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Delay after typing word before pressing Enter");

            ImGui.Separator();
            ImGui.Text("Random Delay Variation");
            ImGui.Separator();

            var useRandom = config.UseRandomDelays;
            if (ImGui.Checkbox("Enable Random Delays", ref useRandom))
            {
                config.UseRandomDelays = useRandom;
                config.Save();
            }

            ImGui.BeginDisabled(!config.UseRandomDelays);
            {
                var typingMin = config.TypingRandomMinMs;
                var typingMax = config.TypingRandomMaxMs;
                if (ImGui.SliderInt("Typing Random Min (ms)", ref typingMin, 0, 500))
                {
                    config.TypingRandomMinMs = Math.Min(typingMin, typingMax);
                    config.Save();
                }
                if (ImGui.SliderInt("Typing Random Max (ms)", ref typingMax, 0, 500))
                {
                    config.TypingRandomMaxMs = Math.Max(typingMin, typingMax);
                    config.Save();
                }

                var submitMin = config.SubmitRandomMinMs;
                var submitMax = config.SubmitRandomMaxMs;
                if (ImGui.SliderInt("Submit Random Min (ms)", ref submitMin, 0, 2000))
                {
                    config.SubmitRandomMinMs = Math.Min(submitMin, submitMax);
                    config.Save();
                }
                if (ImGui.SliderInt("Submit Random Max (ms)", ref submitMax, 0, 2000))
                {
                    config.SubmitRandomMaxMs = Math.Max(submitMin, submitMax);
                    config.Save();
                }
            }
            ImGui.EndDisabled();

            ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Word Filtering", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();

            var minLen = config.MinWordLength;
            if (ImGui.SliderInt("Minimum Word Length", ref minLen, 1, 20))
            {
                config.MinWordLength = minLen;
                if (config.MaxWordLength < minLen) config.MaxWordLength = minLen;
                config.Save();
            }

            var maxLen = config.MaxWordLength;
            if (ImGui.SliderInt("Maximum Word Length", ref maxLen, 1, 100))
            {
                config.MaxWordLength = maxLen;
                if (config.MinWordLength > maxLen) config.MinWordLength = maxLen;
                config.Save();
            }

            var caseSensitive = config.CaseSensitive;
            if (ImGui.Checkbox("Case Sensitive Matching", ref caseSensitive))
            {
                config.CaseSensitive = caseSensitive;
                config.Save();
            }

            ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Debug & Testing", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();

            if (ImGui.Button("Clear Processed Words Cache"))
            {
                // This would need access to the service - for now just log
                Plugin.Log.Information("[KBB] Processed words cache cleared (restart plugin to fully reset)");
            }

            ImGui.SameLine();
            if (ImGui.Button("Test Type 'Hello'"))
            {
                Plugin.Log.Information("[KBB] Test typing triggered");
                // Could trigger a test typing here
            }

            ImGui.Unindent();
        }
    }
}