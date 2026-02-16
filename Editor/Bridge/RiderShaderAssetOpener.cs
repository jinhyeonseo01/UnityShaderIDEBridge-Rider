using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge
{
    internal static class RiderShaderAssetOpener
    {
        // Run earlier than common IDE open handlers so shader assets are routed to Rider first.
        [OnOpenAsset(-1000)]
        internal static bool OpenShaderAssetInRider(int instanceId, int line)
        {
            if (!Settings.ShaderIdeBridgeSettings.instance.EnableOnOpenAsset)
            {
                return false;
            }

            var assetObject = EditorUtility.InstanceIDToObject(instanceId);
            if (assetObject == null)
            {
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(assetObject);
            if (!ShouldHandleAssetPath(assetPath))
            {
                return false;
            }

            var absolutePath = ToAbsoluteAssetPath(assetPath);
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            var opened = RiderOpenService.TryOpen(absolutePath, Math.Max(1, line), 0);
            if (!opened && Settings.ShaderIdeBridgeSettings.instance.EnableDiagnostics)
            {
                Debug.LogWarning($"[ShaderIDEBridge] Failed to open in Rider: {absolutePath}");
            }

            return opened;
        }

        internal static bool ShouldHandleAssetPath(string assetPath)
        {
            return ShaderBridgeConstants.IsSupportedShaderAssetPath(assetPath);
        }

        internal static string ToAbsoluteAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null)
            {
                return string.Empty;
            }

            return Path.GetFullPath(Path.Combine(projectRoot.FullName, assetPath));
        }
    }
}
