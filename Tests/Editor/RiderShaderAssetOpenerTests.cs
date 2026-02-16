using Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge;
using NUnit.Framework;
using UnityEditor;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Tests
{
    public class RiderShaderAssetOpenerTests
    {
        [Test]
        public void IsBridgeEnabledForCurrentEditor_ReturnsFalse_WhenConfiguredEditorIsRider()
        {
            const string key = "kScriptsDefaultApp";
            var original = EditorPrefs.GetString(key);

            try
            {
                EditorPrefs.SetString(key, @"C:\Program Files\JetBrains\Rider\bin\rider64.exe");
                Assert.That(RiderShaderAssetOpener.IsBridgeEnabledForCurrentEditor(), Is.False);
            }
            finally
            {
                RestoreEditorPref(key, original);
            }
        }

        [Test]
        public void IsBridgeEnabledForCurrentEditor_ReturnsTrue_WhenConfiguredEditorIsNotRider()
        {
            const string key = "kScriptsDefaultApp";
            var original = EditorPrefs.GetString(key);

            try
            {
                EditorPrefs.SetString(key, @"C:\Program Files\Microsoft Visual Studio\2026\devenv.exe");
                Assert.That(RiderShaderAssetOpener.IsBridgeEnabledForCurrentEditor(), Is.True);
            }
            finally
            {
                RestoreEditorPref(key, original);
            }
        }

        private static void RestoreEditorPref(string key, string originalValue)
        {
            if (string.IsNullOrEmpty(originalValue))
            {
                EditorPrefs.DeleteKey(key);
            }
            else
            {
                EditorPrefs.SetString(key, originalValue);
            }
        }
    }
}
