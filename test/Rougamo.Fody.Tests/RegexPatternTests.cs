using Rougamo.Fody.Signature.Tokens;
using System.Text.RegularExpressions;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class RegexPatternTests
    {
        [Fact]
        public void SimpleTokenTest()
        {
            var regex = @"public [^ ]+? [^ \(]+?\.Get[^\(]+\(";
            var tokenSource = TokenSourceBuilder.Build($"regex({regex})");
            Assert.Equal(4, tokenSource.Tokens.Length);
            Assert.Equal("regex", tokenSource.Tokens[0].Value);
            Assert.Equal('(', (char)tokenSource.Tokens[1].Value);
            Assert.Equal(regex, tokenSource.Tokens[2].Value);
            Assert.Equal(')', (char)tokenSource.Tokens[3].Value);
        }

        [Fact]
        public void WithGroupTokenTest()
        {
            var regex = @"(public |protected |private |internal |static )+ \S+ [^<(]+(<[^>]+>)*\(([^,)]+,){2}[^\)]+\)";
            var tokenSource = TokenSourceBuilder.Build($"regex({regex})");
            Assert.Equal(4, tokenSource.Tokens.Length);
            Assert.Equal("regex", tokenSource.Tokens[0].Value);
            Assert.Equal('(', (char)tokenSource.Tokens[1].Value);
            Assert.Equal(regex, tokenSource.Tokens[2].Value);
            Assert.Equal(')', (char)tokenSource.Tokens[3].Value);
        }
    }
}
