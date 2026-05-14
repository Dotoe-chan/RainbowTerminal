using System;
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
            var icon = CreateIcon(s_CurrentColor);
            var content = new MainToolbarContent(
                "terminal",
                icon,
                BuildTooltip());

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

        private static string BuildTooltip()
        {
            var repoRoot = TryFindGitRoot();
            var gitState = repoRoot == null ? "git: not found" : ReadGitStatus(repoRoot);

            return
                "terminal shortcut\n" +
                $"{gitState}\n" +
                "click: open terminal in this Unity project\n" +
                "click side effect: randomize icon color";
        }

        private static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction("Open Terminal", _ => OnButtonClicked());
            menu.AppendAction("Randomize Color", _ => RandomizeColor());
            menu.AppendAction("Reset Color", _ => ResetColor());
        }

        private static void OnButtonClicked()
        {
            RandomizeColor();
            OpenTerminal();
        }

        private static void RandomizeColor()
        {
            s_CurrentColor = Color.HSVToRGB(UnityEngine.Random.value, 0.8f, 1f);
            MainToolbar.Refresh(ToolbarPath);
        }

        private static void ResetColor()
        {
            s_CurrentColor = new Color(0.28f, 0.82f, 0.45f, 1f);
            MainToolbar.Refresh(ToolbarPath);
        }

        private static void OpenTerminal()
        {
            var workingDirectory = GetProjectRootPath();

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

        private static string GetProjectRootPath()
        {
            return Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        }

        private static string TryFindGitRoot()
        {
            var directory = new DirectoryInfo(GetProjectRootPath());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string ReadGitStatus(string workingDirectory)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --short --branch",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                if (!process.WaitForExit(1500))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }

                    return "git: status timed out";
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                var error = process.StandardError.ReadToEnd().Trim();
                if (!string.IsNullOrWhiteSpace(output))
                {
                    return output.Replace(Environment.NewLine, " | ");
                }

                return string.IsNullOrWhiteSpace(error) ? "git: clean enough, apparently" : $"git: {error}";
            }
            catch (Exception exception)
            {
                return $"git: {exception.GetType().Name}";
            }
        }

        private static void Cleanup()
        {
            foreach (var pair in IconCache)
            {
                if (pair.Value != null)
                {
                    UnityEngine.Object.DestroyImmediate(pair.Value);
                }
            }

            IconCache.Clear();
        }
    }
}
