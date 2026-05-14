using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace RainbowCommitNag.Editor
{
    [InitializeOnLoad]
    internal static class RainbowCommitHeaderButton
    {
        private const string RootElementName = "rainbow-commit-nag-root";
        private const string ButtonElementName = "rainbow-commit-nag-button";
        private const double ReminderIntervalSeconds = 600d;
        private const double FlashDurationSeconds = 8d;
        private const double MessageSwapSeconds = 0.8d;

        private static readonly string[] FlashMessages =
        {
            "SAVE. COMMIT. ASCEND.",
            "UNSAVED CHAOS DETECTED",
            "GIT HUNGERS",
            "COMMIT BEFORE REGRET",
            "CTRL+S IS NOT A PERSONALITY",
            "HISTORY WANTS A NEW ENTRY"
        };

        private static readonly string[] CalmMessages =
        {
            "Commit Goblin",
            "Version Your Sins",
            "History Awaits",
            "Save Before Chaos"
        };

        private static VisualElement s_Root;
        private static Button s_Button;
        private static double s_NextReminderTime;
        private static double s_FlashUntilTime;
        private static double s_NextMessageSwapTime;
        private static int s_MessageIndex;

        static RainbowCommitHeaderButton()
        {
            s_NextReminderTime = EditorApplication.timeSinceStartup + ReminderIntervalSeconds;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
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
            UpdateVisuals(forceMessageRefresh: true);
        }

        private static void Update()
        {
            EnsureAttached();
            UpdateReminderState();
            UpdateVisuals(forceMessageRefresh: false);
        }

        private static void EnsureAttached()
        {
            if (s_Root != null && s_Root.panel != null)
            {
                return;
            }

            var toolbarRoot = FindToolbarRoot();
            if (toolbarRoot == null)
            {
                return;
            }

            var rightZone = toolbarRoot.Q("ToolbarZoneRightAlign") ??
                            toolbarRoot.Q("ToolbarZonePlayMode") ??
                            toolbarRoot;
            var existingRoot = rightZone.Q<VisualElement>(RootElementName);
            if (existingRoot != null)
            {
                s_Root = existingRoot;
                s_Button = existingRoot.Q<Button>(ButtonElementName);
                return;
            }

            s_Root = new VisualElement
            {
                name = RootElementName
            };
            s_Root.style.flexDirection = FlexDirection.Row;
            s_Root.style.alignItems = Align.Center;
            s_Root.style.justifyContent = Justify.Center;
            s_Root.style.marginLeft = 6;
            s_Root.style.marginRight = 6;
            s_Root.style.paddingLeft = 0;
            s_Root.style.paddingRight = 0;

            s_Button = new Button(OnButtonClicked)
            {
                name = ButtonElementName
            };
            s_Button.style.minWidth = 162;
            s_Button.style.height = 24;
            s_Button.style.paddingLeft = 10;
            s_Button.style.paddingRight = 10;
            s_Button.style.marginTop = 0;
            s_Button.style.marginBottom = 0;
            s_Button.style.borderTopLeftRadius = 12;
            s_Button.style.borderTopRightRadius = 12;
            s_Button.style.borderBottomLeftRadius = 12;
            s_Button.style.borderBottomRightRadius = 12;
            s_Button.style.borderTopWidth = 2;
            s_Button.style.borderBottomWidth = 2;
            s_Button.style.borderLeftWidth = 2;
            s_Button.style.borderRightWidth = 2;
            s_Button.style.unityFontStyleAndWeight = FontStyle.Bold;
            s_Button.style.unityTextAlign = TextAnchor.MiddleCenter;

            s_Root.Add(s_Button);
            rightZone.Add(s_Root);
            UpdateVisuals(forceMessageRefresh: true);
        }

        private static VisualElement FindToolbarRoot()
        {
            var toolbarType = Type.GetType("UnityEditor.Toolbar, UnityEditor");
            if (toolbarType == null)
            {
                return null;
            }

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars == null || toolbars.Length == 0)
            {
                return null;
            }

            var toolbar = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);
            return rootField?.GetValue(toolbar) as VisualElement;
        }

        private static void UpdateReminderState()
        {
            var now = EditorApplication.timeSinceStartup;
            if (now < s_NextReminderTime)
            {
                return;
            }

            StartFlash();
        }

        private static void StartFlash()
        {
            var now = EditorApplication.timeSinceStartup;
            s_FlashUntilTime = now + FlashDurationSeconds;
            s_NextReminderTime = now + ReminderIntervalSeconds;
            s_NextMessageSwapTime = now;
            s_MessageIndex++;
            UpdateVisuals(forceMessageRefresh: true);
        }

        private static void UpdateVisuals(bool forceMessageRefresh)
        {
            if (s_Button == null)
            {
                return;
            }

            var now = EditorApplication.timeSinceStartup;
            var isFlashing = now < s_FlashUntilTime;

            if (isFlashing)
            {
                if (forceMessageRefresh || now >= s_NextMessageSwapTime)
                {
                    s_MessageIndex++;
                    s_NextMessageSwapTime = now + MessageSwapSeconds;
                }

                var hue = Mathf.Repeat((float)(now * 0.18d), 1f);
                var accent = Color.HSVToRGB(hue, 0.82f, 1f);
                var accentDark = Color.HSVToRGB(Mathf.Repeat(hue + 0.08f, 1f), 0.95f, 0.35f);
                var glow = 1f + Mathf.Sin((float)(now * 7d)) * 0.08f;

                s_Button.text = FlashMessages[s_MessageIndex % FlashMessages.Length];
                s_Button.style.backgroundColor = accent;
                s_Button.style.borderTopColor = Color.white;
                s_Button.style.borderBottomColor = accentDark;
                s_Button.style.borderLeftColor = Color.white;
                s_Button.style.borderRightColor = accentDark;
                s_Button.style.color = accentDark.grayscale > 0.45f ? Color.black : Color.white;
                s_Button.style.scale = new Scale(new Vector3(glow, glow, 1f));
            }
            else
            {
                var calmIndex = Mathf.Abs((int)(now / 30d)) % CalmMessages.Length;
                s_Button.text = CalmMessages[calmIndex];
                s_Button.style.backgroundColor = new Color(0.16f, 0.18f, 0.22f, 0.92f);
                s_Button.style.borderTopColor = new Color(0.45f, 0.49f, 0.56f, 1f);
                s_Button.style.borderBottomColor = new Color(0.08f, 0.09f, 0.11f, 1f);
                s_Button.style.borderLeftColor = new Color(0.45f, 0.49f, 0.56f, 1f);
                s_Button.style.borderRightColor = new Color(0.08f, 0.09f, 0.11f, 1f);
                s_Button.style.color = new Color(0.93f, 0.95f, 0.98f, 1f);
                s_Button.style.scale = new Scale(Vector3.one);
            }

            s_Button.tooltip = BuildTooltip(isFlashing);
        }

        private static string BuildTooltip(bool isFlashing)
        {
            var repoRoot = TryFindGitRoot();
            var gitState = repoRoot == null ? "git: not found" : ReadGitStatus(repoRoot);
            var scenesDirty = AnySceneDirty() ? "unsaved scene changes: yes" : "unsaved scene changes: no";
            var nextAt = DateTime.Now.AddSeconds(Math.Max(0d, s_NextReminderTime - EditorApplication.timeSinceStartup));

            return
                "10-minute rainbow save/commit nuisance\n" +
                $"{scenesDirty}\n" +
                $"{gitState}\n" +
                $"flashing: {(isFlashing ? "yes" : "no")}\n" +
                $"next nag: {nextAt:HH:mm:ss}\n" +
                "click: save now and copy suggested git commands";
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
            if (s_Root != null)
            {
                s_Root.RemoveFromHierarchy();
            }

            s_Root = null;
            s_Button = null;
        }
    }
}
