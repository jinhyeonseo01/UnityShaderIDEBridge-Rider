using System;
using System.IO;
using System.Linq;
using System.Text;
using Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge;
using UnityEditor;
using UnityEditor.PackageManager;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Diagnostics
{
    internal static class ShaderIdeBridgeDiagnostics
    {
        internal static string BuildReport()
        {
            var settings = Settings.ShaderIdeBridgeSettings.instance;
            var builder = new StringBuilder();

            var editorPath = EditorPrefs.GetString("kScriptsDefaultApp");
            var isRiderSelected = RiderOpenService.IsRiderEditorPath(editorPath);
            var riderPackageInstalled = IsRiderPackageInstalled();
            var lockFileContainsRider = PackagesLockContainsRider();

            builder.AppendLine("Shader IDE Bridge Diagnostics");
            builder.AppendLine("-----------------------------");
            builder.AppendLine($"EnableOnOpenAsset: {settings.EnableOnOpenAsset}");
            builder.AppendLine($"PreferCodeEditorApi: {settings.PreferCodeEditorApi}");
            builder.AppendLine($"EnableDiagnostics: {settings.EnableDiagnostics}");
            builder.AppendLine();
            builder.AppendLine($"External Script Editor: {(string.IsNullOrWhiteSpace(editorPath) ? "<empty>" : editorPath)}");
            builder.AppendLine($"Rider selected: {isRiderSelected}");
            builder.AppendLine($"com.unity.ide.rider installed: {riderPackageInstalled}");
            builder.AppendLine($"packages-lock contains Rider: {lockFileContainsRider}");
            builder.AppendLine($"Supported extensions: {string.Join(", ", ShaderBridgeConstants.SupportedExtensions.OrderBy(e => e))}");
            builder.AppendLine();

            var selectedAssetPath = GetSelectedAssetPath();
            if (string.IsNullOrWhiteSpace(selectedAssetPath))
            {
                builder.AppendLine("Selected asset: <none>");
            }
            else
            {
                builder.AppendLine($"Selected asset: {selectedAssetPath}");
                builder.AppendLine($"Selected asset supported: {ShaderBridgeConstants.IsSupportedShaderAssetPath(selectedAssetPath)}");
            }

            builder.AppendLine();
            builder.AppendLine("Expected Rider include behavior:");
            builder.AppendLine("- Shader/HLSL navigation and include handling are delegated to Rider.");
            builder.AppendLine("- This package focuses on Unity->Rider open reliability and setup diagnostics.");

            return builder.ToString();
        }

        internal static string GetSelectedAssetPath()
        {
            if (Selection.activeObject == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(Selection.activeObject);
        }

        private static bool IsRiderPackageInstalled()
        {
            try
            {
                var packages = PackageInfo.GetAllRegisteredPackages();
                return packages.Any(p => string.Equals(p.name, "com.unity.ide.rider", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool PackagesLockContainsRider()
        {
            try
            {
                var projectRoot = Directory.GetParent(UnityEngine.Application.dataPath);
                if (projectRoot == null)
                {
                    return false;
                }

                var lockPath = Path.Combine(projectRoot.FullName, "Packages", "packages-lock.json");
                if (!File.Exists(lockPath))
                {
                    return false;
                }

                var json = File.ReadAllText(lockPath);
                return json.IndexOf("\"com.unity.ide.rider\"", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
