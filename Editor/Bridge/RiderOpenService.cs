using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditorInternal;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge
{
    internal static class RiderOpenService
    {
        private const string ScriptsDefaultAppKey = "kScriptsDefaultApp";
        private static readonly string[] RiderPathEnvKeys =
        {
            "RIDER_PATH",
            "JETBRAINS_RIDER",
            "JETBRAINS_RIDER_PATH"
        };

        internal static bool TryOpen(string absolutePath, int line, int column)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            var normalizedPath = Path.GetFullPath(absolutePath);
            if (!File.Exists(normalizedPath))
            {
                return false;
            }

            var safeLine = Math.Max(1, line);
            var safeColumn = Math.Max(0, column);
            var settings = Settings.ShaderIdeBridgeSettings.instance;

            // Use CodeEditor API only when Unity's configured editor is already Rider.
            if (settings.PreferCodeEditorApi && IsConfiguredEditorRider() && TryOpenViaCodeEditor(normalizedPath, safeLine, safeColumn))
            {
                return true;
            }

            if (TryOpenViaExternalEditor(normalizedPath, safeLine))
            {
                return true;
            }

            return TryOpenViaUnityFallback(normalizedPath, safeLine);
        }

        internal static bool IsRiderEditorPath(string editorPath)
        {
            if (string.IsNullOrWhiteSpace(editorPath))
            {
                return false;
            }

            var lower = editorPath.ToLowerInvariant();
            return lower.Contains("rider");
        }

        internal static bool IsConfiguredEditorRider()
        {
            var configuredPath = EditorPrefs.GetString(ScriptsDefaultAppKey);
            return IsRiderEditorPath(configuredPath);
        }

        internal static IReadOnlyList<string> GetRiderExecutableCandidatesSnapshot()
        {
            return GetRiderExecutableCandidates().ToList();
        }

        internal static string BuildRiderArguments(string filePath, int line)
        {
            return $"--line {Math.Max(1, line)} \"{filePath}\"";
        }

        private static bool TryOpenViaCodeEditor(string absolutePath, int line, int column)
        {
            try
            {
                var editor = CodeEditor.CurrentEditor;
                if (editor == null)
                {
                    return false;
                }

                return editor.OpenProject(absolutePath, line, column);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryOpenViaExternalEditor(string absolutePath, int line)
        {
            foreach (var editorPath in GetRiderExecutableCandidates())
            {
                if (TryStartRiderProcess(editorPath, absolutePath, line))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GetRiderExecutableCandidates()
        {
            var candidates = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var configuredPath = EditorPrefs.GetString(ScriptsDefaultAppKey);
            if (IsRiderEditorPath(configuredPath))
            {
                AddCandidate(candidates, seen, configuredPath);
            }

            foreach (var envKey in RiderPathEnvKeys)
            {
                AddCandidate(candidates, seen, Environment.GetEnvironmentVariable(envKey));
            }

#if UNITY_EDITOR_WIN
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            AddCandidate(candidates, seen, Path.Combine(programFiles ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
            AddCandidate(candidates, seen, Path.Combine(programFilesX86 ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
            AddWindowsToolboxCandidates(candidates, seen);
            AddPathCandidates(candidates, seen, "rider64.exe", "rider.exe");
#elif UNITY_EDITOR_OSX
            AddCandidate(candidates, seen, "/Applications/Rider.app/Contents/MacOS/rider");
            AddCandidate(candidates, seen, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "Rider.app", "Contents", "MacOS", "rider"));
            AddMacToolboxCandidates(candidates, seen);
            AddPathCandidates(candidates, seen, "rider");
#else
            AddCandidate(candidates, seen, "/usr/bin/rider");
            AddCandidate(candidates, seen, "/snap/bin/rider");
            AddCandidate(candidates, seen, "/opt/rider/bin/rider.sh");
            AddLinuxToolboxCandidates(candidates, seen);
            AddPathCandidates(candidates, seen, "rider", "rider.sh");
#endif

            return candidates;
        }

        private static bool TryStartRiderProcess(string editorPath, string absolutePath, int line)
        {
            if (string.IsNullOrWhiteSpace(editorPath) || !File.Exists(editorPath))
            {
                return false;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = editorPath,
                    Arguments = BuildRiderArguments(absolutePath, line),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void AddCandidate(List<string> candidates, HashSet<string> seen, string rawPath)
        {
            var normalizedPath = NormalizeExecutablePath(rawPath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return;
            }

            if (seen.Add(normalizedPath))
            {
                candidates.Add(normalizedPath);
            }
        }

        private static string NormalizeExecutablePath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return string.Empty;
            }

            var trimmed = rawPath.Trim().Trim('"');
            if (File.Exists(trimmed))
            {
                return trimmed;
            }

#if UNITY_EDITOR_WIN
            var exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex > 0)
            {
                var exePath = trimmed.Substring(0, exeIndex + 4).Trim('"');
                if (File.Exists(exePath))
                {
                    return exePath;
                }
            }
#endif
            return trimmed;
        }

        private static void AddPathCandidates(List<string> candidates, HashSet<string> seen, params string[] executableNames)
        {
            var pathValue = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                return;
            }

#if UNITY_EDITOR_WIN
            var separator = ';';
#else
            var separator = ':';
#endif
            var pathEntries = pathValue.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pathEntry in pathEntries.Select(entry => entry.Trim()).Where(entry => !string.IsNullOrWhiteSpace(entry)))
            {
                foreach (var executableName in executableNames)
                {
                    AddCandidate(candidates, seen, Path.Combine(pathEntry, executableName));
                }
            }
        }

#if UNITY_EDITOR_WIN
        private static void AddWindowsToolboxCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var toolboxRoot = Path.Combine(localAppData, "JetBrains", "Toolbox", "apps", "Rider");
                if (!Directory.Exists(toolboxRoot))
                {
                    return;
                }

                foreach (var channelDir in Directory.GetDirectories(toolboxRoot, "ch-*"))
                {
                    foreach (var buildDir in Directory.GetDirectories(channelDir))
                    {
                        AddCandidate(candidates, seen, Path.Combine(buildDir, "bin", "rider64.exe"));
                    }
                }
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }
#endif

#if UNITY_EDITOR_OSX
        private static void AddMacToolboxCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var toolboxRoot = Path.Combine(home, "Library", "Application Support", "JetBrains", "Toolbox", "apps", "Rider");
                if (!Directory.Exists(toolboxRoot))
                {
                    return;
                }

                foreach (var channelDir in Directory.GetDirectories(toolboxRoot, "ch-*"))
                {
                    foreach (var buildDir in Directory.GetDirectories(channelDir))
                    {
                        AddCandidate(candidates, seen, Path.Combine(buildDir, "Rider.app", "Contents", "MacOS", "rider"));
                    }
                }
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }
#endif

#if UNITY_EDITOR_LINUX
        private static void AddLinuxToolboxCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var toolboxRoot = Path.Combine(home, ".local", "share", "JetBrains", "Toolbox", "apps", "Rider");
                if (!Directory.Exists(toolboxRoot))
                {
                    return;
                }

                foreach (var channelDir in Directory.GetDirectories(toolboxRoot, "ch-*"))
                {
                    foreach (var buildDir in Directory.GetDirectories(channelDir))
                    {
                        AddCandidate(candidates, seen, Path.Combine(buildDir, "bin", "rider.sh"));
                    }
                }
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }
#endif

        private static bool TryOpenViaUnityFallback(string absolutePath, int line)
        {
            try
            {
                InternalEditorUtility.OpenFileAtLineExternal(absolutePath, line);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
