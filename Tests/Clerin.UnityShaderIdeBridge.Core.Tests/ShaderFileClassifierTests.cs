using Clerin.UnityShaderIdeBridge.Core;
using Xunit;

namespace Clerin.UnityShaderIdeBridge.Core.Tests
{
    public class ShaderFileClassifierTests
    {
        [Theory]
        [InlineData("A.shader", true)]
        [InlineData("A.compute", true)]
        [InlineData("A.cginc", true)]
        [InlineData("A.glslinc", true)]
        [InlineData("A.hlsl", true)]
        [InlineData("A.cg", true)]
        [InlineData("A.cs", false)]
        [InlineData("A.txt", false)]
        [InlineData("", false)]
        public void IsShaderRelatedFile_WorksAsExpected(string path, bool expected)
        {
            Assert.Equal(expected, ShaderFileClassifier.IsShaderRelatedFile(path));
        }
    }
}
