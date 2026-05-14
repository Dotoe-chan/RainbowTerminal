using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace RainbowCommitNag.Editor
{
    [InitializeOnLoad]
    internal static class RainbowCommitHeaderButton
    {
        private const string ToolbarPath = "Rainbow Commit Nag/Commit Goblin";
        private const double ReminderIntervalSeconds = 600d;
        private const double FlashDurationSeconds = 8d;
        private const double FlashRefreshSeconds = 0.12d;

        private static readonly string[] FlashMessages =
        {
            "SAVE NOW",
            "COMMIT NOW",
            "GIT HUNGERS",
            "VERSION YOUR SINS",
            "CHECKPOINT OR CHAOS",
            "CTRL+S THEN GIT"
        };

        private static readonly Dictionary<int, Texture2D> IconCache = new();

        private static double s_NextReminderTime;
        private static double s_FlashUntilTime;
        private static double s_NextRefreshTime;
        private static bool s_WasFlashing;

        static RainbowCommitHeaderButton()
        {
            s_NextReminderTime = EditorApplication.timeSinceStartup + ReminderIntervalSeconds;
            EditorApplication.update += Update;
            EditorApplication.quitting += Cleanup;
        }

        [MainToolbarElement(
            ToolbarPath,
            defaultDockPosition = MainToolbarDockPosition.Right,
            defaultDockIndex = 0)]
        private static MainToolbarElement CreateToolbarElement()
        {
            var flashing = IsFlashing();
            var icon = CreateIcon(flashing ? EvaluateFlashColor() : new Color(0.28f, 0.82f, 0.45f, 1f));
            var content = new MainToolbarContent(
                flashing ? CurrentFlashMessage() : "Commit Goblin",
                icon,
                BuildTooltip(flashing));

            return new MainToolbarButton(content, OnButtonClicked)
            {
                populateContextMenu = PopulateContextMenu,
                enabled = true,
                displayed = true
            };
        }

        [MenuItem("Tools/Rainbow Commit Nag/Flash Now")]
        private static void FlashNow()
        {
            StartFlash();
        }

        [MenuItem("Tools/Rainbow Commit Nag/Snooze 10 Minutes")]
        private static void Snooze()
        {
            s_FlashUntilTime = 0d;
            s_NextReminderTime = EditorApplication.timeSinceStartup + ReminderIntervalSeconds;
            MainToolbar.Refresh(ToolbarPath);
        }

        private static void Update()
        {
            var now = EditorApplication.timeSinceStartup;
            if (now >= s_NextReminderTime && !IsFlashing())
            {
                StartFlash();
            }

            var flashing = IsFlashing();
            if (flashing)
            {
                if (now >= s_NextRefreshTime)
                {
                    s_NextRefreshTime = now + FlashRefreshSeconds;
                    MainToolbar.Refresh(ToolbarPath);
                }
            }
            else if (s_WasFlashing)
            {
                MainToolbar.Refresh(ToolbarPath);
            }

            s_WasFlashing = flashing;
        }

        private static void StartFlash()
        {
            var now = EditorApplication.timeSinceStartup;
            s_FlashUntilTime = now + FlashDurationSeconds;
            s_NextReminderTime = now + ReminderIntervalSeconds;
            s_NextRefreshTime = now;
            MainToolbar.Refresh(ToolbarPath);
        }

        private static bool IsFlashing()
        {
            return EditorApplication.timeSinceStartup < s_FlashUntilTime;
        }

        private static string CurrentFlashMessage()
        {
            var index = (int)(EditorApplication.timeSinceStartup / 0.8d) % FlashMessages.Length;
            return FlashMessages[index];
        }

        private static Color EvaluateFlashColor()
        {
            var hue = Mathf.Repeat((float)(EditorApplication.timeSinceStartup * 0.35d), 1f);
            return Color.HSVToRGB(hue, 0.85f, 1f);
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

        private static string BuildTooltip(bool flashing)
        {
            var repoRoot = TryFindGitRoot();
            var gitState = repoRoot == null ? "git: not found" : ReadGitStatus(repoRoot);
            var nextAt = DateTime.Now.AddSeconds(Math.Max(0d, s_NextReminderTime - EditorApplication.timeSinceStartup));

            return
                "10-minute save/commit goblin\n" +
                $"unsaved scene changes: {(AnySceneDirty() ? "yes" : "no")}\n" +
                $"{gitState}\n" +
                $"flashing: {(flashing ? "yes" : "no")}\n" +
                $"next nag: {nextAt:HH:mm:ss}\n" +
                "click: save now and copy suggested git commands";
        }

        private static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction("Flash Now", _ => FlashNow());
            menu.AppendAction("Snooze 10 Minutes", _ => Snooze());
        }

        private static void OnButtonClicked()
        {
            SaveEverything();

            var repoRoot = TryFindGitRoot();
            var suggestedCommit = SuggestCommitMessage();
            var commandText = $"git add .{Environment.NewLine}git commit -m \"{suggestedCommit}\"";
            EditorGUIUtility.systemCopyBuffer = commandText;

            var gitState = repoRoot == null
                ? "git repository was not detected from the Unity project root upward."
                : ReadGitStatus(repoRoot);

            EditorUtility.DisplayDialog(
                "Commit Goblin",
                "Saved open scenes and assets.\n\n" +
                "Copied to clipboard:\n" +
                commandText + "\n\n" +
                gitState,
                "I Will Version My Sins");

            Snooze();
        }

        private static void SaveEverything()
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool AnySceneDirty()
        {
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isDirty)
                {
                    return true;
                }
            }

            return false;
        }

        private static string TryFindGitRoot()
        {
            var directory = new DirectoryInfo(Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath);
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

        private static string SuggestCommitMessage()
        {
            var messages = new[]
            {
                "Save progress before the goblin unionizes",
                "Checkpoint before reality forks",
                "Record editor progress",
                "Save and checkpoint changes"
            };

            var index = DateTime.Now.Minute % messages.Length;
            return messages[index];
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
