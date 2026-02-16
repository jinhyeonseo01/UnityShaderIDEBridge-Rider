using System;
using System.Collections.Generic;
using System.IO;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge
{
    internal static class ShaderBridgeConstants
    {
        internal static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".shader",
            ".compute",
            ".cginc",
            ".glslinc",
            ".hlsl",
            ".cg"
        };

        internal static bool IsSupportedShaderAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            var extension = Path.GetExtension(assetPath);
            return SupportedExtensions.Contains(extension);
        }
    }
}
