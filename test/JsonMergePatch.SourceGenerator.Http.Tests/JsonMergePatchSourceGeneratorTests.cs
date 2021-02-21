using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Http.Tests
{
    public class JsonMergePatchSourceGeneratorTests
    {
        [Fact]
        public void CreatingGenerator_DoesNotThrow()
        {
            _ = new JsonMergePatchSourceGenerator();
        }
    }
}
