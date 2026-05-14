using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace RainbowTerminal.Editor
{
    [InitializeOnLoad]
    internal static class RainbowCommitHeaderButton
    {
        private const string ToolbarPath = "Rainbow Terminal/terminal";

        private static readonly Dictionary<int, Texture2D> IconCache = new();
        private static Color s_CurrentColor = new(0.28f, 0.82f, 0.45f, 1f);

        static RainbowCommitHeaderButton()
        {
            EditorApplication.quitting += Cleanup;
        }

        [MainToolbarElement(
            ToolbarPath,
            defaultDockPosition = MainToolbarDockPosition.Right,
            defaultDockIndex = 0)]
        private static MainToolbarElement CreateToolbarElement()
        {
            var content = new MainToolbarContent(
                L10n.Tr("terminal"),
                CreateIcon(s_CurrentColor),
                string.Empty);

            return new MainToolbarButton(content, OnButtonClicked)
            {
                populateContextMenu = PopulateContextMenu,
                enabled = true,
                displayed = true
            };
        }

        private static Texture2D CreateIcon(Color color)
        {
            var key = QuantizeColor(color);
            if (IconCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[16 * 16];
            var border = Color.Lerp(color, Color.white, 0.45f);
            var dark = Color.Lerp(color, Color.black, 0.35f);
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var isBorder = x == 0 || y == 0 || x == 15 || y == 15;
                    var isGlow = x is > 2 and < 13 && y is > 2 and < 13;
                    pixels[(y * 16) + x] = isBorder ? border : (isGlow ? color : dark);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            IconCache[key] = texture;
            return texture;
        }

        private static int QuantizeColor(Color color)
        {
            var r = Mathf.Clamp(Mathf.RoundToInt(color.r * 15f), 0, 15);
            var g = Mathf.Clamp(Mathf.RoundToInt(color.g * 15f), 0, 15);
            var b = Mathf.Clamp(Mathf.RoundToInt(color.b * 15f), 0, 15);
            return (r << 8) | (g << 4) | b;
        }

        private static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction(L10n.Tr("Open Terminal"), _ => OnButtonClicked());
        }

        private static void OnButtonClicked()
        {
            RandomizeColor();
            OpenTerminal();
        }

        private static void RandomizeColor()
        {
            s_CurrentColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
            MainToolbar.Refresh(ToolbarPath);
        }

        private static void OpenTerminal()
        {
            var workingDirectory = GetProjectRootPath();

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                TryOpenMacTerminal(workingDirectory);
                return;
            }

            if (TryOpenWindowsTerminal(workingDirectory))
            {
                return;
            }

            TryOpenPowerShell(workingDirectory);
        }

        private static bool TryOpenWindowsTerminal(string workingDirectory)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "wt.exe",
                    Arguments = $"-d \"{workingDirectory}\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true
                };

                return process.Start();
            }
            catch
            {
                return false;
            }
        }

        private static bool TryOpenMacTerminal(string workingDirectory)
        {
            try
            {
                using var process = new Process();
                var appleScriptPath = EscapeAppleScriptString(workingDirectory);
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments =
                        $"-e \"tell application \\\"Terminal\\\" to do script \\\"cd \\\" & quoted form of \\\"{appleScriptPath}\\\"\" " +
                        "-e \"tell application \\\"Terminal\\\" to activate\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false
                };

                return process.Start();
            }
            catch
            {
                return false;
            }
        }

        private static bool TryOpenPowerShell(string workingDirectory)
        {
            try
            {
                using var process = new Process();
                var escapedPath = workingDirectory.Replace("'", "''");
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -Command \"Set-Location -LiteralPath '{escapedPath}'\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true
                };

                return process.Start();
            }
            catch
            {
                return false;
            }
        }

        private static string EscapeAppleScriptString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string GetProjectRootPath()
        {
            return Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        }

        private static void Cleanup()
        {
            foreach (var pair in IconCache)
            {
                if (pair.Value != null)
                {
                    Object.DestroyImmediate(pair.Value);
                }
            }

            IconCache.Clear();
        }
    }
}
