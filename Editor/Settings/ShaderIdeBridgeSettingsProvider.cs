using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Settings
{
    internal static class ShaderIdeBridgeSettingsProvider
    {
        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Clerin/Shader IDE Bridge", SettingsScope.Project)
            {
                label = "Shader IDE Bridge",
                guiHandler = OnGui,
                keywords = new HashSet<string>(new[] { "Shader", "Rider", "HLSL", "Bridge", "Diagnostics" })
            };
        }

        private static void OnGui(string searchContext)
        {
            var settings = ShaderIdeBridgeSettings.instance;

            EditorGUI.BeginChangeCheck();
            settings.EnableOnOpenAsset = EditorGUILayout.ToggleLeft("Enable OnOpenAsset Bridge", settings.EnableOnOpenAsset);
            settings.PreferCodeEditorApi = EditorGUILayout.ToggleLeft("Prefer Unity CodeEditor API", settings.PreferCodeEditorApi);
            settings.EnableDiagnostics = EditorGUILayout.ToggleLeft("Enable Diagnostics Warnings", settings.EnableDiagnostics);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This package handles Unity->Rider open bridge for shader-related files. " +
                "Advanced include behavior is delegated to Rider.",
                MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                settings.SaveSettings();
            }
        }
    }
}
