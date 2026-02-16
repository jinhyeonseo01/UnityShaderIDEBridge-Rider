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
        private const string LastSuccessPathKey = "Clerin.ShaderIdeBridge.Rider.LastSuccessPath";
        private const string LastSuccessUtcKey = "Clerin.ShaderIdeBridge.Rider.LastSuccessUtc";
        private static readonly string[] RiderPathEnvKeys =
        {
            "RIDER_PATH",
            "JETBRAINS_RIDER",
            "JETBRAINS_RIDER_PATH"
        };
        private static readonly TimeSpan CandidateCacheTtl = TimeSpan.FromMinutes(5);
        private static List<string> s_cachedCandidates = new List<string>();
        private static DateTime s_candidateCacheTimestampUtc = DateTime.MinValue;
        private static string s_candidateCacheKey = string.Empty;

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
            var configuredEditorIsRider = IsConfiguredEditorRider();

            // Use CodeEditor API only when Unity's configured editor is already Rider.
            if (settings.PreferCodeEditorApi && configuredEditorIsRider && TryOpenViaCodeEditor(normalizedPath, safeLine, safeColumn))
            {
                return true;
            }

            if (TryOpenViaExternalEditor(normalizedPath, safeLine))
            {
                return true;
            }

            // Avoid silently routing shader files to a non-Rider default editor.
            if (configuredEditorIsRider && TryOpenViaUnityFallback(normalizedPath, safeLine))
            {
                return true;
            }

            if (settings.EnableDiagnostics)
            {
                var configuredEditor = EditorPrefs.GetString(ScriptsDefaultAppKey);
                var candidates = GetRiderExecutableCandidatesSnapshot();
                UnityEngine.Debug.LogWarning(
                    $"[ShaderIDEBridge] Rider open failed. file='{normalizedPath}', configuredEditor='{configuredEditor}', riderCandidates={candidates.Count}");
            }

            InvalidateCandidateCache();
            return false;
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
            var cacheKey = BuildCandidateCacheKey();
            var now = DateTime.UtcNow;
            if (s_cachedCandidates.Count == 0 || now - s_candidateCacheTimestampUtc > CandidateCacheTtl || !string.Equals(cacheKey, s_candidateCacheKey, StringComparison.Ordinal))
            {
                s_cachedCandidates = BuildRiderExecutableCandidates().ToList();
                s_candidateCacheTimestampUtc = now;
                s_candidateCacheKey = cacheKey;
            }

            var lastSuccess = GetValidatedLastSuccessPath();
            if (string.IsNullOrWhiteSpace(lastSuccess))
            {
                return s_cachedCandidates;
            }

            var orderedCandidates = new List<string>(s_cachedCandidates.Count + 1) { lastSuccess };
            foreach (var candidate in s_cachedCandidates)
            {
                if (!string.Equals(candidate, lastSuccess, StringComparison.OrdinalIgnoreCase))
                {
                    orderedCandidates.Add(candidate);
                }
            }

            return orderedCandidates;
        }

        private static IEnumerable<string> BuildRiderExecutableCandidates()
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
            AddWindowsToolboxScriptCandidates(candidates, seen);
            AddWindowsStandardInstallCandidates(candidates, seen);
            AddWindowsToolboxAppCandidates(candidates, seen);
            AddWindowsLocalProgramsCandidates(candidates, seen);
            AddPathCandidates(candidates, seen, "rider64.exe", "rider.exe", "rider.cmd", "rider.bat");
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
                var startInfo = BuildStartInfo(editorPath, absolutePath, line);

                Process.Start(startInfo);
                SaveLastSuccessPath(editorPath);
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
            var executableExtensions = new[] { ".exe", ".cmd", ".bat" };
            foreach (var executableExtension in executableExtensions)
            {
                var extensionIndex = trimmed.IndexOf(executableExtension, StringComparison.OrdinalIgnoreCase);
                if (extensionIndex <= 0)
                {
                    continue;
                }

                var executablePath = trimmed.Substring(0, extensionIndex + executableExtension.Length).Trim('"');
                if (File.Exists(executablePath))
                {
                    return executablePath;
                }
            }
#endif
            return string.Empty;
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
            foreach (var pathEntry in pathEntries.Select(entry => entry.Trim().Trim('"')).Where(entry => !string.IsNullOrWhiteSpace(entry)))
            {
                foreach (var executableName in executableNames)
                {
                    AddCandidate(candidates, seen, Path.Combine(pathEntry, executableName));
                }
            }
        }

#if UNITY_EDITOR_WIN
        private static void AddWindowsStandardInstallCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                AddCandidate(candidates, seen, Path.Combine(programFiles ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
                AddCandidate(candidates, seen, Path.Combine(programFilesX86 ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
                AddStandardInstallCandidatesFromRoot(candidates, seen, programFiles);
                AddStandardInstallCandidatesFromRoot(candidates, seen, programFilesX86);
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }

        private static void AddStandardInstallCandidatesFromRoot(List<string> candidates, HashSet<string> seen, string programFilesRoot)
        {
            if (string.IsNullOrWhiteSpace(programFilesRoot))
            {
                return;
            }

            try
            {
                var jetBrainsRoot = Path.Combine(programFilesRoot, "JetBrains");
                if (!Directory.Exists(jetBrainsRoot))
                {
                    return;
                }

                foreach (var riderDir in Directory.GetDirectories(jetBrainsRoot, "*Rider*"))
                {
                    AddCandidate(candidates, seen, Path.Combine(riderDir, "bin", "rider64.exe"));
                    AddCandidate(candidates, seen, Path.Combine(riderDir, "bin", "rider.exe"));
                }
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }

        private static void AddWindowsToolboxScriptCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var toolboxScripts = Path.Combine(localAppData, "JetBrains", "Toolbox", "scripts");
                AddCandidate(candidates, seen, Path.Combine(toolboxScripts, "rider.cmd"));
                AddCandidate(candidates, seen, Path.Combine(toolboxScripts, "rider.bat"));
            }
            catch (Exception)
            {
                // Ignore probing issues and continue with other candidates.
            }
        }

        private static void AddWindowsToolboxAppCandidates(List<string> candidates, HashSet<string> seen)
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

        private static void AddWindowsLocalProgramsCandidates(List<string> candidates, HashSet<string> seen)
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrWhiteSpace(localAppData))
                {
                    return;
                }

                var riderBin = Path.Combine(localAppData, "Programs", "Rider", "bin");
                AddCandidate(candidates, seen, Path.Combine(riderBin, "rider64.exe"));
                AddCandidate(candidates, seen, Path.Combine(riderBin, "rider.exe"));
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

        private static ProcessStartInfo BuildStartInfo(string editorPath, string absolutePath, int line)
        {
            var extension = Path.GetExtension(editorPath)?.ToLowerInvariant();
            var lineArgs = BuildRiderArguments(absolutePath, line);

#if UNITY_EDITOR_WIN
            if (extension == ".cmd" || extension == ".bat")
            {
                return new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{editorPath}\" {lineArgs}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
#endif

            return new ProcessStartInfo
            {
                FileName = editorPath,
                Arguments = lineArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

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

        private static void InvalidateCandidateCache()
        {
            s_cachedCandidates.Clear();
            s_candidateCacheTimestampUtc = DateTime.MinValue;
            s_candidateCacheKey = string.Empty;
        }

        internal static void ClearCandidateCache()
        {
            InvalidateCandidateCache();
            ClearLastSuccessPath();
        }

        internal static string GetLastSuccessPath()
        {
            return EditorPrefs.GetString(LastSuccessPathKey);
        }

        internal static bool IsLastSuccessPathValid()
        {
            return !string.IsNullOrWhiteSpace(GetValidatedLastSuccessPath());
        }

        private static void SaveLastSuccessPath(string editorPath)
        {
            if (!IsValidRiderExecutable(editorPath, out var normalizedPath))
            {
                return;
            }

            EditorPrefs.SetString(LastSuccessPathKey, normalizedPath);
            EditorPrefs.SetString(LastSuccessUtcKey, DateTime.UtcNow.ToString("o"));
        }

        private static void ClearLastSuccessPath()
        {
            EditorPrefs.DeleteKey(LastSuccessPathKey);
            EditorPrefs.DeleteKey(LastSuccessUtcKey);
        }

        private static string GetValidatedLastSuccessPath()
        {
            var rawPath = EditorPrefs.GetString(LastSuccessPathKey);
            if (!IsValidRiderExecutable(rawPath, out var normalizedPath))
            {
                return string.Empty;
            }

            return normalizedPath;
        }

        private static bool IsValidRiderExecutable(string editorPath, out string normalizedPath)
        {
            normalizedPath = NormalizeExecutablePath(editorPath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return false;
            }

            if (!File.Exists(normalizedPath))
            {
                return false;
            }

            if (!IsRiderEditorPath(normalizedPath))
            {
                return false;
            }

#if UNITY_EDITOR_WIN
            var extension = Path.GetExtension(normalizedPath);
            if (!string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, ".cmd", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, ".bat", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
#endif

            return true;
        }

        private static string BuildCandidateCacheKey()
        {
            var configuredPath = EditorPrefs.GetString(ScriptsDefaultAppKey);
            var riderPath = Environment.GetEnvironmentVariable("RIDER_PATH");
            var jetBrainsRider = Environment.GetEnvironmentVariable("JETBRAINS_RIDER");
            var jetBrainsRiderPath = Environment.GetEnvironmentVariable("JETBRAINS_RIDER_PATH");
            var pathValue = Environment.GetEnvironmentVariable("PATH");

            return string.Join("|", configuredPath, riderPath, jetBrainsRider, jetBrainsRiderPath, pathValue);
        }
    }
}
