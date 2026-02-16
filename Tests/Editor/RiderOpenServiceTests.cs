using Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge;
using NUnit.Framework;
using UnityEditor;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Tests
{
    public class RiderOpenServiceTests
    {
        [TestCase("C:/Program Files/JetBrains/Rider/bin/rider64.exe", true)]
        [TestCase("/Applications/Rider.app", true)]
        [TestCase("C:/Program Files/Visual Studio/Code.exe", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsRiderEditorPath_WorksAsExpected(string path, bool expected)
        {
            var result = RiderOpenService.IsRiderEditorPath(path);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void BuildRiderArguments_ContainsLineAndPath()
        {
            const string filePath = "C:/repo/Test.hlsl";
            var args = RiderOpenService.BuildRiderArguments(filePath, 17);

            Assert.That(args, Does.Contain("--line 17"));
            Assert.That(args, Does.Contain($"\"{filePath}\""));
        }

        [Test]
        public void IsConfiguredEditorRider_DependsOnEditorPref()
        {
            const string key = "kScriptsDefaultApp";
            var original = EditorPrefs.GetString(key);

            try
            {
                EditorPrefs.SetString(key, @"C:\Program Files\Microsoft VS Code\Code.exe");
                Assert.That(RiderOpenService.IsConfiguredEditorRider(), Is.False);

                EditorPrefs.SetString(key, @"C:\Program Files\JetBrains\Rider\bin\rider64.exe");
                Assert.That(RiderOpenService.IsConfiguredEditorRider(), Is.True);
            }
            finally
            {
                if (string.IsNullOrEmpty(original))
                {
                    EditorPrefs.DeleteKey(key);
                }
                else
                {
                    EditorPrefs.SetString(key, original);
                }
            }
        }
    }
}
