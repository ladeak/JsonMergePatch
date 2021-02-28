using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LaDeak.JsonMergePatch.SourceGenerator.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void Null_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<int> source = null;
            Assert.True(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void Empty_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<int> source = Enumerable.Empty<int>();
            Assert.True(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void FalsePredicate_AnyOrNull_ReturnsFalse()
        {
            IEnumerable<int> source = new[] { 1, 2, 3 };
            Assert.False(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void TruePredicateFirst_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<int> source = new[] { 0, 1, 2 };
            Assert.True(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void TruePredicateLast_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<int> source = new[] { 1, 2, 0 };
            Assert.True(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void TruePredicateMiddle_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<int> source = new[] { 1, 0, 2 };
            Assert.True(source.AnyOrNull(x => x == 0));
        }

        [Fact]
        public void WithNull_AnyOrNull_ReturnsTrue()
        {
            IEnumerable<string> source = new[] { null, "hello" };
            Assert.True(source.AnyOrNull(x => x == "hello"));
        }

    }
}
