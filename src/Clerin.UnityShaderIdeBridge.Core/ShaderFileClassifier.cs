using System;
using System.Collections.Generic;
using System.IO;

namespace Clerin.UnityShaderIdeBridge.Core
{
    public static class ShaderFileClassifier
    {
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".shader",
            ".compute",
            ".cginc",
            ".glslinc",
            ".hlsl",
            ".cg"
        };

        public static bool IsShaderRelatedFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return SupportedExtensions.Contains(Path.GetExtension(path));
        }
    }
}
