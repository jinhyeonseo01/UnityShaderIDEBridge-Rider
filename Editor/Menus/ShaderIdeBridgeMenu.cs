using Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge;
using Clerin.UnityShaderIdeBridge.Rider.Editor.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Menus
{
    internal static class ShaderIdeBridgeMenu
    {
        [MenuItem("Tools/Clerin/Shader IDE Bridge/Open In Rider", false, 110)]
        private static void OpenSelectedInRider()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Shader IDE Bridge", "Select a shader-related asset first.", "OK");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(selected);
            if (!RiderShaderAssetOpener.ShouldHandleAssetPath(assetPath))
            {
                EditorUtility.DisplayDialog("Shader IDE Bridge", $"Unsupported file type: {assetPath}", "OK");
                return;
            }

            var absolutePath = RiderShaderAssetOpener.ToAbsoluteAssetPath(assetPath);
            if (!RiderOpenService.TryOpen(absolutePath, 1, 0))
            {
                EditorUtility.DisplayDialog("Shader IDE Bridge", $"Failed to open in Rider:\n{absolutePath}", "OK");
            }
        }

        [MenuItem("Tools/Clerin/Shader IDE Bridge/Open In Rider", true)]
        private static bool ValidateOpenSelectedInRider()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(selected);
            return RiderShaderAssetOpener.ShouldHandleAssetPath(assetPath);
        }

        [MenuItem("Tools/Clerin/Shader IDE Bridge/Validate Rider Shader Setup", false, 111)]
        private static void ValidateRiderShaderSetup()
        {
            var report = ShaderIdeBridgeDiagnostics.BuildReport();
            Debug.Log(report);
            EditorUtility.DisplayDialog("Shader IDE Bridge Diagnostics", report, "OK");
        }
    }
}
