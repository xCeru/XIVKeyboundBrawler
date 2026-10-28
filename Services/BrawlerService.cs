using Dalamud.Game.Gui;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace XIVKeyboundBrawler.Services;

public sealed class BrawlerService : IDisposable
{
    private readonly Plugin plugin;
    private readonly Random random = new();
    private readonly HashSet<string> processedWords = new(StringComparer.OrdinalIgnoreCase);
    private string lastAddonName = "";
    private DateTime lastProcessTime = DateTime.MinValue;
    private readonly object processLock = new();

    public BrawlerService(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void OnFrameworkUpdate(IFramework framework)
    {
        if (!plugin.Configuration.Enabled)
            return;

        if (plugin.Configuration.RequireGameFocus && !IsGameFocused())
            return;

        TryProcessAddon();
    }

    private bool IsGameFocused()
    {
        try
        {
            var foregroundWindow = GetForegroundWindow();
            var currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(foregroundWindow, out uint processId);
            return processId == currentProcessId;
        }
        catch
        {
            return true; // Default to true if check fails
        }
    }

    private void TryProcessAddon()
    {
        lock (processLock)
        {
            var config = plugin.Configuration;
            var addonName = config.AddonName;

            if (string.IsNullOrEmpty(addonName))
                return;

            var addon = Plugin.GameGui.GetAddonByName(addonName);
            if (addon.Address == nint.Zero)
            {
                if (lastAddonName != "")
                {
                    Plugin.Log.Verbose($"Addon '{addonName}' not found");
                    lastAddonName = "";
                    processedWords.Clear();
                }
                return;
            }

            if (!addon.IsVisible)
                return;

            lastAddonName = addonName;
            ProcessAddonText(addon.Address);
        }
    }

    private void ProcessAddonText(nint addonPtr)
    {
        var config = plugin.Configuration;
        var words = ExtractWordsFromAddon(addonPtr);

        if (words.Count == 0)
            return;

        var now = DateTime.Now;
        var minInterval = TimeSpan.FromMilliseconds(50); // Minimum time between processing cycles
        if (now - lastProcessTime < minInterval)
            return;

        foreach (var word in words)
        {
            if (ShouldProcessWord(word))
            {
                if (config.LogDetectedWords)
                    Plugin.Log.Information($"[KBB] Detected word: '{word}'");

                TypeWord(word);
                processedWords.Add(word);
                lastProcessTime = DateTime.Now;
                break; // Process one word per cycle to avoid overwhelming
            }
        }

        // Clean up old processed words periodically
        if (processedWords.Count > 100)
        {
            processedWords.Clear();
        }
    }

    private List<string> ExtractWordsFromAddon(nint addonPtr)
    {
        var words = new List<string>();
        var config = plugin.Configuration;

        try
        {
            // Traverse the addon's node tree to find text nodes
            TraverseNodesForText(addonPtr, words, config.MinWordLength, config.MaxWordLength);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[KBB] Error extracting text from addon: {ex}");
        }

        return words;
    }

    private void TraverseNodesForText(nint nodePtr, List<string> words, int minLen, int maxLen)
    {
        if (nodePtr == nint.Zero)
            return;

        try
        {
            unsafe
            {
                var node = (AtkResNode*)nodePtr;
                var nodeType = (NodeType)node->Type;

                // Check if this is a text node (Type == 3 for AtkTextNode)
                if (nodeType == NodeType.Text)
                {
                    var textNode = (AtkTextNode*)nodePtr;
                    if (textNode->NodeText.StringPtr != nint.Zero)
                    {
                        var text = Marshal.PtrToStringUTF8(textNode->NodeText.StringPtr) ?? "";
                        text = text.Trim();

                        if (IsValidWord(text, minLen, maxLen))
                        {
                            words.Add(text);
                        }
                    }
                }

                // Recursively check children
                var childCount = node->ChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = node->GetChildNode(i);
                    if (child != null)
                    {
                        TraverseNodesForText((nint)child, words, minLen, maxLen);
                    }
                }
            }
        }
        catch
        {
            // Ignore individual node errors
        }
    }

    private bool IsValidWord(string text, int minLen, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Length < minLen || text.Length > maxLen)
            return false;

        // Filter out UI text that's not words (numbers only, symbols, etc.)
        if (Regex.IsMatch(text, @"^[\d\s\W]+$"))
            return false;

        // Must contain at least one letter
        if (!Regex.IsMatch(text, @"[a-zA-Z]"))
            return false;

        return true;
    }

    private bool ShouldProcessWord(string word)
    {
        var config = plugin.Configuration;

        if (config.CaseSensitive)
        {
            return !processedWords.Contains(word);
        }
        else
        {
            return !processedWords.Contains(word, StringComparer.OrdinalIgnoreCase);
        }
    }

    private void TypeWord(string word)
    {
        var config = plugin.Configuration;

        try
        {
            // Type each character
            foreach (char c in word)
            {
                if (!plugin.Configuration.Enabled)
                    break;

                SendKeyChar(c);

                var delay = config.TypingDelayMs;
                if (config.UseRandomDelays && config.TypingRandomMaxMs > config.TypingRandomMinMs)
                {
                    delay += random.Next(config.TypingRandomMinMs, config.TypingRandomMaxMs + 1);
                }

                System.Threading.Thread.Sleep(delay);
            }

            // Submit with Enter
            if (plugin.Configuration.Enabled)
            {
                var submitDelay = config.SubmitDelayMs;
                if (config.UseRandomDelays && config.SubmitRandomMaxMs > config.SubmitRandomMinMs)
                {
                    submitDelay += random.Next(config.SubmitRandomMinMs, config.SubmitRandomMaxMs + 1);
                }

                System.Threading.Thread.Sleep(submitDelay);
                SendKey((ushort)VirtualKeyCode.RETURN);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[KBB] Error typing word: {ex}");
        }
    }

    private void SendKeyChar(char c)
    {
        // Convert char to virtual key code and send
        var vk = VkKeyScan(c);
        var keyCode = (ushort)(vk & 0xFF);
        var shiftState = (vk >> 8) & 0xFF;

        bool needsShift = (shiftState & 1) != 0;
        bool needsCtrl = (shiftState & 2) != 0;
        bool needsAlt = (shiftState & 4) != 0;

        if (needsShift) KeyDown((ushort)VirtualKeyCode.SHIFT);
        if (needsCtrl) KeyDown((ushort)VirtualKeyCode.CONTROL);
        if (needsAlt) KeyDown((ushort)VirtualKeyCode.MENU);

        KeyPress(keyCode);

        if (needsAlt) KeyUp((ushort)VirtualKeyCode.MENU);
        if (needsCtrl) KeyUp((ushort)VirtualKeyCode.CONTROL);
        if (needsShift) KeyUp((ushort)VirtualKeyCode.SHIFT);
    }

    private void SendKey(ushort key)
    {
        KeyPress(key);
    }

    private void KeyDown(ushort key)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = 0,
                time = 0,
                dwExtraInfo = nint.Zero
            }
        };
        SendInput(1, ref input, INPUT.Size);
    }

    private void KeyUp(ushort key)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = KEYEVENTF_KEYUP,
                time = 0,
                dwExtraInfo = nint.Zero
            }
        };
        SendInput(1, ref input, INPUT.Size);
    }

    private void KeyPress(ushort key)
    {
        KeyDown(key);
        KeyUp(key);
    }

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

    // Windows INPUT structures
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public KEYBDINPUT ki;
        public static int Size => Marshal.SizeOf<INPUT>();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    // Virtual key codes
    private enum VirtualKeyCode : ushort
    {
        SHIFT = 0x10,
        CONTROL = 0x11,
        MENU = 0x12, // Alt
        RETURN = 0x0D,
    }

    // Node types from FFXIVClientStructs
    private enum NodeType : byte
    {
        Text = 3,
    }

    // Minimal structs for UI traversal
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct AtkResNode
    {
        public byte Type;
        public byte ChildCount;
        public AtkResNode* ParentNode;
        public AtkResNode* PrevSiblingNode;
        public AtkResNode* NextSiblingNode;
        public AtkResNode* ChildNode;

        public AtkResNode* GetChildNode(int index)
        {
            var child = ChildNode;
            for (int i = 0; i < index && child != null; i++)
            {
                child = child->NextSiblingNode;
            }
            return child;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct AtkTextNode
    {
        public AtkResNode Base;
        public AtkUldAsset UldAsset;
        public AtkTextNodeText NodeText;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AtkUldAsset
    {
        public nint Data;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AtkTextNodeText
    {
        public nint StringPtr;
        public uint BufSize;
        public uint StringLen;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}