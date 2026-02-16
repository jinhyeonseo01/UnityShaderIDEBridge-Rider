using UnityEditor;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Settings
{
    [FilePath("ProjectSettings/ShaderIdeBridgeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class ShaderIdeBridgeSettings : ScriptableSingleton<ShaderIdeBridgeSettings>
    {
        public bool EnableOnOpenAsset = true;
        public bool PreferCodeEditorApi = true;
        public bool EnableDiagnostics = true;

        public void SaveSettings()
        {
            Save(true);
        }
    }
}
