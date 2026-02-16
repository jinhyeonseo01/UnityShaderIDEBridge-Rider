using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditorInternal;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge
{
    internal static class RiderOpenService
    {
        private const string ScriptsDefaultAppKey = "kScriptsDefaultApp";

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

            if (settings.PreferCodeEditorApi && TryOpenViaCodeEditor(normalizedPath, safeLine, safeColumn))
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
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var configuredPath = EditorPrefs.GetString(ScriptsDefaultAppKey);
            if (IsRiderEditorPath(configuredPath) && !string.IsNullOrWhiteSpace(configuredPath))
            {
                unique.Add(configuredPath);
            }

#if UNITY_EDITOR_WIN
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            AddIfNotEmpty(unique, Path.Combine(programFiles ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
            AddIfNotEmpty(unique, Path.Combine(programFilesX86 ?? string.Empty, "JetBrains", "Rider", "bin", "rider64.exe"));
#elif UNITY_EDITOR_OSX
            AddIfNotEmpty(unique, "/Applications/Rider.app/Contents/MacOS/rider");
#else
            AddIfNotEmpty(unique, "/usr/bin/rider");
            AddIfNotEmpty(unique, "/snap/bin/rider");
#endif
            return unique;
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

        private static void AddIfNotEmpty(HashSet<string> values, string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                values.Add(path);
            }
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
    }
}
