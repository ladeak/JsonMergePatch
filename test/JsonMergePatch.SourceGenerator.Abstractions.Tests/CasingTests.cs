using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Abstractions.Tests
{
    public class CasingTests
    {
        [Theory]
        [InlineData("testCase", "_testCase")]
        [InlineData("TestCase", "_testCase")]
        [InlineData("_testCase", "__testCase")]
        [InlineData("_TestCase", "__TestCase")]
        [InlineData("testCaSe", "_testCaSe")]
        [InlineData("TestCasE", "_testCasE")]
        [InlineData("-testCase", "_-testCase")]
        public void PrefixUnderscoreCamelCase_TestCases(string input, string expected)
        {
            Assert.Equal(expected, Casing.PrefixUnderscoreCamelCase(input));
        }

    }
}
