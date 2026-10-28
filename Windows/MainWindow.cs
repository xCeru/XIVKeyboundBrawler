using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Numerics;

namespace XIVKeyboundBrawler.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly string iconPath;

    public MainWindow(Plugin plugin, string iconPath) : base("XIVKeyboundBrawler###XIVKeyboundBrawlerMain")
    {
        this.plugin = plugin;
        this.iconPath = iconPath;
        Size = new Vector2(400, 350);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var config = plugin.Configuration;

        ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), "XIVKeyboundBrawler");
        ImGui.Text("Keybound Brawler Auto-Typer");
        ImGui.Separator();

        // Status
        var statusColor = config.Enabled ? new Vector4(0.2f, 0.9f, 0.2f, 1.0f) : new Vector4(0.9f, 0.2f, 0.2f, 1.0f);
        var statusText = config.Enabled ? "ENABLED" : "DISABLED";
        ImGui.TextColored(statusColor, $"Status: {statusText}");

        if (ImGui.Button(config.Enabled ? "Disable" : "Enable"))
        {
            config.Enabled = !config.Enabled;
            config.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("Open Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Separator();

        // Current configuration summary
        ImGui.Text("Current Settings:");
        ImGui.Indent();
        ImGui.BulletText($"Addon: {config.AddonName}");
        ImGui.BulletText($"Typing Delay: {config.TypingDelayMs}ms" + (config.UseRandomDelays ? $" (+{config.TypingRandomMinMs}-{config.TypingRandomMaxMs}ms)" : ""));
        ImGui.BulletText($"Submit Delay: {config.SubmitDelayMs}ms" + (config.UseRandomDelays ? $" (+{config.SubmitRandomMinMs}-{config.SubmitRandomMaxMs}ms)" : ""));
        ImGui.BulletText($"Word Length: {config.MinWordLength}-{config.MaxWordLength} chars");
        ImGui.BulletText($"Case Sensitive: {config.CaseSensitive}");
        ImGui.BulletText($"Require Focus: {config.RequireGameFocus}");
        ImGui.Unindent();

        ImGui.Separator();

        // Addon detection status
        ImGui.Text("Addon Detection:");
        ImGui.Indent();
        var addon = Plugin.GameGui.GetAddonByName(config.AddonName);
        if (addon.Address != nint.Zero)
        {
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.2f, 1.0f), $"Found: {config.AddonName}");
            ImGui.Text($"Visible: {addon.IsVisible}");
            ImGui.Text($"Address: 0x{addon.Address:X}");
        }
        else
        {
            ImGui.TextColored(new Vector4(0.9f, 0.3f, 0.3f, 1.0f), $"Not Found: {config.AddonName}");
            ImGui.Text("Start Keybound Brawler to detect the addon");
        }
        ImGui.Unindent();

        ImGui.Separator();

        // Quick links
        ImGui.Text("Resources:");
        if (ImGui.Button("Open Dalamud Debug UI (/xldebug)"))
        {
            Plugin.CommandManager.ProcessCommand("/xldebug");
        }

        ImGui.SameLine();
        if (ImGui.Button("Plugin Log (/xllog)"))
        {
            Plugin.CommandManager.ProcessCommand("/xllog");
        }
    }
}