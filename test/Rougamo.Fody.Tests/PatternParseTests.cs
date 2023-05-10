using Rougamo.Fody.Signature;
using Rougamo.Fody.Signature.Matchers;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class PatternParseTests
    {
        [Fact]
        public void UnionTest()
        {
            var pattern1 = "regex(public )";
            var pattern2 = "execution(* *..*(..))";
            var pattern3 = "property(* *..*)";
            var pattern4 = "getter(* *..*Value)";
            var pattern5 = "setter(* *.._*)";
            var pattern6 = "property(* *..Open.*)";
            var pattern7 = "property(* *..Wide.*)";
            var pattern = $"{pattern1} && {pattern2} || {pattern3} && !{pattern4} && !{pattern5} || {pattern6} || {pattern7}";

            var matcher = PatternParser.Parse(pattern);
            var orMatcher = Assert.IsType<OrMatcher>(matcher);
            var p1p2Matcher = Assert.IsType<AndMatcher>(orMatcher.Left);
            var p1Matcher = Assert.IsType<RegexMatcher>(p1p2Matcher.Left);
            var p2Matcher = Assert.IsType<ExecutionMatcher>(p1p2Matcher.Right);
            var p3p4p5p6p7Matcher = Assert.IsType<OrMatcher>(orMatcher.Right);
            var p3p4p5Matcher = Assert.IsType<AndMatcher>(p3p4p5p6p7Matcher.Left);
            var p3Matcher = Assert.IsType<PropertyMatcher>(p3p4p5Matcher.Left);
            var p4p5Matcher = Assert.IsType<AndMatcher>(p3p4p5Matcher.Right);
            var p4NotMatcher = Assert.IsType<NotMatcher>(p4p5Matcher.Left);
            var p4Matcher = Assert.IsType<GetterMatcher>(p4NotMatcher.InnerMatcher);
            var p5NotMatcher = Assert.IsType<NotMatcher>(p4p5Matcher.Right);
            var p5Matcher = Assert.IsType<SetterMatcher>(p5NotMatcher.InnerMatcher);
            var p6p7Matcher = Assert.IsType<OrMatcher>(p3p4p5p6p7Matcher.Right);
            var p6Matcher = Assert.IsType<PropertyMatcher>(p6p7Matcher.Left);
            var p7Matcher = Assert.IsType<PropertyMatcher>(p6p7Matcher.Right);

            Assert.Equal(pattern1, p1Matcher.ToString());
            Assert.Equal(pattern2, p2Matcher.ToString());
            Assert.Equal(pattern3, p3Matcher.ToString());
            Assert.Equal(pattern4, p4Matcher.ToString());
            Assert.Equal(pattern5, p5Matcher.ToString());
            Assert.Equal(pattern6, p6Matcher.ToString());
            Assert.Equal(pattern7, p7Matcher.ToString());
        }

        [Fact]
        public void RegexPatternTest()
        {
            var regex = @"public [^ ]+? [^ \(]+?\.Get[^\(]+\(";
            var matcher = PatternParser.Parse($"regex({regex})");
            var regexMatcher = Assert.IsType<RegexMatcher>(matcher);
            Assert.Equal(regex, regexMatcher.Pattern);
        }

        [Fact]
        public void RegexWithGroupTest()
        {
            var regex = @"(public |protected |private |internal |static )+ \S+ [^<(]+(<[^>]+>)*\(([^,)]+,){2}[^\)]+\)";
            var matcher = PatternParser.Parse($"regex({regex})");
            var regexMatcher = Assert.IsType<RegexMatcher>(matcher);
            Assert.Equal(regex, regexMatcher.Pattern);
        }
    }
}
