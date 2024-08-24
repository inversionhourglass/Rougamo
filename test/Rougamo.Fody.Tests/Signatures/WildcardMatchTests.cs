using Cecil.AspectN.Patterns;
using Cecil.AspectN.Tokens;
using System;
using Xunit;

namespace Rougamo.Fody.Tests.Signatures
{
    public class WildcardMatchTests
    {
        [Fact]
        public void NonWildcardTest()
        {
            var pattern = "Abb.Cdd.Eff.Ghh";
            var nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("Abb.Cdd.Eff.Ghh"));
            Assert.True(nsPattern.IsMatch("Abb.Cdd.Eff.Ghh."));
            Assert.False(nsPattern.IsMatch("Abb.Cdd.Ef.Ghh"));
            Assert.False(nsPattern.IsMatch("Abbb.Cdd.Eff.Ghh"));
            Assert.False(nsPattern.IsMatch("ABB.Cdd.Eff.Ghh"));
        }

        [Fact]
        public void StarTest()
        {
            var pattern = "Abb.C*d.Eff*.Gh*";
            var nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("Abb.Cxyzd.Effuuv.Gh996"));
            Assert.True(nsPattern.IsMatch("Abb.Cd.Eff.Gh"));
            Assert.True(nsPattern.IsMatch("Abb.Cd.Eff999.Gh"));
            Assert.False(nsPattern.IsMatch("Abb.C.Effd.Eff.Gh"));
            Assert.False(nsPattern.IsMatch("Abb.Cdb.Eff.Gh"));
            Assert.False(nsPattern.IsMatch("Abb.Cd.Eff.Ggh"));
        }

        [Fact]
        public void EillipsisTest()
        {
            var pattern = "Abb..Eff..";
            var nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("Abb.Eff"));
            Assert.True(nsPattern.IsMatch("Abb.Xyz.Abc.Eff"));
            Assert.True(nsPattern.IsMatch("Abb.Eff.Opq.Rst"));
            Assert.True(nsPattern.IsMatch("Abb.Xyz.Eff.Oo0"));
            Assert.False(nsPattern.IsMatch("Abb.Xyz.Ccc"));
            Assert.False(nsPattern.IsMatch("Uvw.Eff.Abb"));

            pattern = "Abb..Eff";
            nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("Abb.Eff"));
            Assert.True(nsPattern.IsMatch("Abb.Qwe.ASD.Eff"));
            Assert.True(nsPattern.IsMatch("Abb.Lmn.Eff"));
            Assert.False(nsPattern.IsMatch("Abb.Eff.Right"));
            Assert.False(nsPattern.IsMatch("Abb.Wrong.Eff.OK"));
        }

        [Fact]
        public void StarEllipsisTest()
        {
            var pattern = "*..";
            var nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("A.B.C.D.E.F"));
            Assert.True(nsPattern.IsMatch(Guid.NewGuid().ToString("N")));

            pattern = "*.Abc..*z";
            nsPattern = new NamespacePattern(TokenSourceBuilder.Build(pattern));
            Assert.True(nsPattern.IsMatch("Zzz.Abc.Zz"));
            Assert.True(nsPattern.IsMatch("K.Abc.Yy.Xyz"));
            Assert.True(nsPattern.IsMatch("Pt.Abc.N.M.z"));
            Assert.False(nsPattern.IsMatch("Abc.Xyz"));
            Assert.False(nsPattern.IsMatch("V.Abc.Xyz.Cba"));
        }
    }
}
