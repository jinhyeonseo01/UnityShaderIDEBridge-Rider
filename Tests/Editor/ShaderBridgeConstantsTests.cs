using Clerin.UnityShaderIdeBridge.Rider.Editor.Bridge;
using NUnit.Framework;

namespace Clerin.UnityShaderIdeBridge.Rider.Editor.Tests
{
    public class ShaderBridgeConstantsTests
    {
        [TestCase("Assets/Shaders/Test.shader", true)]
        [TestCase("Assets/Shaders/Test.compute", true)]
        [TestCase("Assets/Shaders/Test.cginc", true)]
        [TestCase("Assets/Shaders/Test.glslinc", true)]
        [TestCase("Assets/Shaders/Test.hlsl", true)]
        [TestCase("Assets/Shaders/Test.cg", true)]
        [TestCase("Assets/Shaders/Test.cs", false)]
        [TestCase("Assets/Shaders/Test.png", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsSupportedShaderAssetPath_WorksAsExpected(string path, bool expected)
        {
            var result = ShaderBridgeConstants.IsSupportedShaderAssetPath(path);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
