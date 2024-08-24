using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cecil.AspectN;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests.Signatures
{
    [Collection("SignatureUsage")]
    public class SignatureBasicTests
    {
        private readonly ModuleDefinition _moduleDef;

        public SignatureBasicTests()
        {
            var parameters = new ReaderParameters
            {
                AssemblyResolver = new TestAssemblyResolver()
            };
            _moduleDef = ModuleDefinition.ReadModule("SignatureUsage.dll", parameters);
        }

        [Fact]
        public void PublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $"public System.String {typeDef.FullName}.Public()";
            Assert.Equal(expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal(expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $"protected System.String {typeDef.FullName}.Protected()";
            Assert.Equal(expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal(expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $"private System.String {typeDef.FullName}.Private()";
            Assert.Equal(expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal(expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $"internal System.String {typeDef.FullName}.Internal()";
            Assert.Equal(expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal(expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $"protectedinternal System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal(expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal(expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $"privateprotected System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal(expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal(expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());

            var compositeDefaultSignature = SignatureParser.ParseMethod(methods.Default, true);
            var singleDefaultSignature = SignatureParser.ParseMethod(methods.Default, false);
            var expectedDefaultSignature = $"private System.String {typeDef.FullName}.Default()";
            Assert.Equal(expectedDefaultSignature, compositeDefaultSignature.ToString());
            Assert.Equal(expectedDefaultSignature, singleDefaultSignature.ToString());

            var compositeDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, true);
            var singleDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, false);
            var expectedDefaultStaticSignature = $"private static System.String {typeDef.FullName}.DefaultStatic()";
            Assert.Equal(expectedDefaultStaticSignature, compositeDefaultStaticSignature.ToString());
            Assert.Equal(expectedDefaultStaticSignature, singleDefaultStaticSignature.ToString());

            var compositeProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, true);
            var singleProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, false);
            var expectedProtectedInternalStaticSignature = $"protectedinternal static System.String {typeDef.FullName}.ProtectedInternalStatic()";
            Assert.Equal(expectedProtectedInternalStaticSignature, compositeProtectedInternalStaticSignature.ToString());
            Assert.Equal(expectedProtectedInternalStaticSignature, singleProtectedInternalStaticSignature.ToString());

            var compositePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, true);
            var singlePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, false);
            var expectedPrivateProtectedStaticSignature = $"privateprotected static System.String {typeDef.FullName}.PrivateProtectedStatic()";
            Assert.Equal(expectedPrivateProtectedStaticSignature, compositePrivateProtectedStaticSignature.ToString());
            Assert.Equal(expectedPrivateProtectedStaticSignature, singlePrivateProtectedStaticSignature.ToString());

            var voidMethod = typeDef.GetMethods().Single(x => x.Name == "Void");
            var compositeVoidSignature = SignatureParser.ParseMethod(voidMethod, true);
            var singleVoidSignature = SignatureParser.ParseMethod(voidMethod, false);
            var expectedVoidSignature = $"public System.Void {typeDef.FullName}.Void()";
            Assert.Equal(expectedVoidSignature, compositeVoidSignature.ToString());
            Assert.Equal(expectedVoidSignature, singleVoidSignature.ToString());
        }

        [Fact]
        public void InternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());

            var compositeDefaultSignature = SignatureParser.ParseMethod(methods.Default, true);
            var singleDefaultSignature = SignatureParser.ParseMethod(methods.Default, false);
            var expectedDefaultSignature = $" System.String {typeDef.FullName}.Default()";
            Assert.Equal("private" + expectedDefaultSignature, compositeDefaultSignature.ToString());
            Assert.Equal("private" + expectedDefaultSignature, singleDefaultSignature.ToString());

            var compositeDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, true);
            var singleDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, false);
            var expectedDefaultStaticSignature = $" static System.String {typeDef.FullName}.DefaultStatic()";
            Assert.Equal("private" + expectedDefaultStaticSignature, compositeDefaultStaticSignature.ToString());
            Assert.Equal("private" + expectedDefaultStaticSignature, singleDefaultStaticSignature.ToString());

            var compositeProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, true);
            var singleProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, false);
            var expectedProtectedInternalStaticSignature = $" static System.String {typeDef.FullName}.ProtectedInternalStatic()";
            Assert.Equal("internal" + expectedProtectedInternalStaticSignature, compositeProtectedInternalStaticSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalStaticSignature, singleProtectedInternalStaticSignature.ToString());

            var compositePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, true);
            var singlePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, false);
            var expectedPrivateProtectedStaticSignature = $" static System.String {typeDef.FullName}.PrivateProtectedStatic()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedStaticSignature, compositePrivateProtectedStaticSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedStaticSignature, singlePrivateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void DefaultClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());

            var compositeDefaultSignature = SignatureParser.ParseMethod(methods.Default, true);
            var singleDefaultSignature = SignatureParser.ParseMethod(methods.Default, false);
            var expectedDefaultSignature = $" System.String {typeDef.FullName}.Default()";
            Assert.Equal("private" + expectedDefaultSignature, compositeDefaultSignature.ToString());
            Assert.Equal("private" + expectedDefaultSignature, singleDefaultSignature.ToString());

            var compositeDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, true);
            var singleDefaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic, false);
            var expectedDefaultStaticSignature = $" static System.String {typeDef.FullName}.DefaultStatic()";
            Assert.Equal("private" + expectedDefaultStaticSignature, compositeDefaultStaticSignature.ToString());
            Assert.Equal("private" + expectedDefaultStaticSignature, singleDefaultStaticSignature.ToString());

            var compositeProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, true);
            var singleProtectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic, false);
            var expectedProtectedInternalStaticSignature = $" static System.String {typeDef.FullName}.ProtectedInternalStatic()";
            Assert.Equal("internal" + expectedProtectedInternalStaticSignature, compositeProtectedInternalStaticSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalStaticSignature, singleProtectedInternalStaticSignature.ToString());

            var compositePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, true);
            var singlePrivateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic, false);
            var expectedProtectedStaticSignature = $" static System.String {typeDef.FullName}.PrivateProtectedStatic()";
            Assert.Equal("privateprotected" + expectedProtectedStaticSignature, compositePrivateProtectedStaticSignature.ToString());
            Assert.Equal("privateprotected" + expectedProtectedStaticSignature, singlePrivateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void PublicNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $"public System.String {typeDef.FullName}.Public()";
            Assert.Equal(expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal(expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $"protected System.String {typeDef.FullName}.Protected()";
            Assert.Equal(expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal(expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $"private System.String {typeDef.FullName}.Private()";
            Assert.Equal(expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal(expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $"internal System.String {typeDef.FullName}.Internal()";
            Assert.Equal(expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal(expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $"protectedinternal System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal(expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal(expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $"privateprotected System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal(expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal(expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("protected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("protected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("protected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("private" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("private" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("private" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("private" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("private" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("protectedinternal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("protected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("privateprotected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("privateprotected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("privateprotected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("privateprotected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("private" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("private" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("private" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("private" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("private" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("privateprotected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("privateprotected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("privateprotected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("privateprotected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("private" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("private" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("private" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("private" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("private" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("internal" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("internal" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("internal" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var compositePublicSignature = SignatureParser.ParseMethod(methods.Public, true);
            var singlePublicSignature = SignatureParser.ParseMethod(methods.Public, false);
            var expectedPublicSignature = $" System.String {typeDef.FullName}.Public()";
            Assert.Equal("privateprotected" + expectedPublicSignature, compositePublicSignature.ToString());
            Assert.Equal("public" + expectedPublicSignature, singlePublicSignature.ToString());

            var compositeProtectSignature = SignatureParser.ParseMethod(methods.Protected, true);
            var singleProtectSignature = SignatureParser.ParseMethod(methods.Protected, false);
            var expectedProtectSignature = $" System.String {typeDef.FullName}.Protected()";
            Assert.Equal("privateprotected" + expectedProtectSignature, compositeProtectSignature.ToString());
            Assert.Equal("protected" + expectedProtectSignature, singleProtectSignature.ToString());

            var compositePrivateSignature = SignatureParser.ParseMethod(methods.Private, true);
            var singlePrivateSignature = SignatureParser.ParseMethod(methods.Private, false);
            var expectedPrivateSignature = $" System.String {typeDef.FullName}.Private()";
            Assert.Equal("private" + expectedPrivateSignature, compositePrivateSignature.ToString());
            Assert.Equal("private" + expectedPrivateSignature, singlePrivateSignature.ToString());

            var compositeInternalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            var singleInternalSignature = SignatureParser.ParseMethod(methods.Internal, false);
            var expectedInternalSignature = $" System.String {typeDef.FullName}.Internal()";
            Assert.Equal("privateprotected" + expectedInternalSignature, compositeInternalSignature.ToString());
            Assert.Equal("internal" + expectedInternalSignature, singleInternalSignature.ToString());

            var compositeProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, true);
            var singleProtectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal, false);
            var expectedProtectedInternalSignature = $" System.String {typeDef.FullName}.ProtectedInternal()";
            Assert.Equal("privateprotected" + expectedProtectedInternalSignature, compositeProtectedInternalSignature.ToString());
            Assert.Equal("protectedinternal" + expectedProtectedInternalSignature, singleProtectedInternalSignature.ToString());

            var compositePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, true);
            var singlePrivateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected, false);
            var expectedPrivateProtectedSignature = $" System.String {typeDef.FullName}.PrivateProtected()";
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, compositePrivateProtectedSignature.ToString());
            Assert.Equal("privateprotected" + expectedPrivateProtectedSignature, singlePrivateProtectedSignature.ToString());
        }

        [Fact]
        public void GenericTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public, true);
            Assert.Equal("public System.Collections.Generic.List<System.Collections.Generic.List<T2>> SignatureUsage.GenericCls<T1>.Public<T2,T3>(T1,System.Nullable<T3>)", publicSignature.ToString());
            var nestedType = Assert.Single(publicSignature.ReturnType.NestedTypes);
            var generic = Assert.Single(nestedType.Generics);
            nestedType = Assert.Single(generic.NestedTypes);
            generic = Assert.Single(nestedType.Generics);
            var t2 = Assert.IsType<GenericParameterTypeSignature>(generic);
            Assert.Equal("T2", t2.SortName);
            Assert.Equal("TM1", t2.VirtualName);
            nestedType = Assert.Single(publicSignature.DeclareType.NestedTypes);
            generic = Assert.Single(nestedType.Generics);
            var t1 = Assert.IsType<GenericParameterTypeSignature>(generic);
            Assert.Equal("T1", t1.SortName);
            Assert.Equal("T11", t1.VirtualName);
            Assert.Equal(2, publicSignature.Method.Generics.Length);
            t2 = Assert.IsType<GenericParameterTypeSignature>(publicSignature.Method.Generics[0]);
            var t3 = Assert.IsType<GenericParameterTypeSignature>(publicSignature.Method.Generics[1]);
            Assert.Equal("T2", t2.SortName);
            Assert.Equal("TM1", t2.VirtualName);
            Assert.Equal("T3", t3.SortName);
            Assert.Equal("TM2", t3.VirtualName);

            var internalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            Assert.Equal("internal static System.Collections.Generic.List<T1> SignatureUsage.GenericCls<T1>.Internal<T2,T3>(T2,T3)", internalSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private, true);
            Assert.Equal("private T SignatureUsage.GenericCls<T>.Private(T)", privateSignature.ToString());
            var t = Assert.IsType<GenericParameterTypeSignature>(privateSignature.ReturnType);
            Assert.Equal("T", t.SortName);
            Assert.Equal("T11", t.VirtualName);

            typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1/ProtectedICls`2");
            methods = ResolveMethods(typeDef);
            publicSignature = SignatureParser.ParseMethod(methods.Public, true);
            Assert.Equal("protected T1 SignatureUsage.GenericCls<T1>/ProtectedICls<T2,T3>.Public<T4>(T2,T3,T4)", publicSignature.ToString());
            t1 = Assert.IsType<GenericParameterTypeSignature>(publicSignature.ReturnType);
            Assert.Equal("T1", t1.SortName);
            Assert.Equal("T21", t1.VirtualName);
            Assert.Equal(2, publicSignature.DeclareType.NestedTypes.Length);
            generic = Assert.Single(publicSignature.DeclareType.NestedTypes[0].Generics);
            t1 = Assert.IsType<GenericParameterTypeSignature>(generic);
            Assert.Equal("T1", t1.SortName);
            Assert.Equal("T21", t1.VirtualName);
            Assert.Equal(2, publicSignature.DeclareType.NestedTypes[1].Generics.Length);
            t2 = Assert.IsType<GenericParameterTypeSignature>(publicSignature.DeclareType.NestedTypes[1].Generics[0]);
            Assert.Equal("T2", t2.SortName);
            Assert.Equal("T11", t2.VirtualName);
            t3 = Assert.IsType<GenericParameterTypeSignature>(publicSignature.DeclareType.NestedTypes[1].Generics[1]);
            Assert.Equal("T3", t3.SortName);
            Assert.Equal("T12", t3.VirtualName);
            generic = Assert.Single(publicSignature.Method.Generics);
            var t4 = Assert.IsType<GenericParameterTypeSignature>(generic);
            Assert.Equal("T4", t4.SortName);
            Assert.Equal("TM1", t4.VirtualName);

            typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1/ProtectedICls`2/PrivateICls`2");
            methods = ResolveMethods(typeDef);
            internalSignature = SignatureParser.ParseMethod(methods.Internal, true);
            Assert.Equal("private System.ValueTuple<T5,T4,T1> SignatureUsage.GenericCls<T1>/ProtectedICls<T2,T3>/PrivateICls<T4,T5>.Internal<T6,T7>(T2,T3,T6,T7)", internalSignature.ToString());
            nestedType = Assert.Single(internalSignature.ReturnType.NestedTypes);
            Assert.Equal(3, nestedType.Generics.Length);
            var t5 = Assert.IsType<GenericParameterTypeSignature>(nestedType.Generics[0]);
            Assert.Equal("T5", t5.SortName);
            Assert.Equal("T12", t5.VirtualName);
            t4 = Assert.IsType<GenericParameterTypeSignature>(nestedType.Generics[1]);
            Assert.Equal("T4", t4.SortName);
            Assert.Equal("T11", t4.VirtualName);
            t1 = Assert.IsType<GenericParameterTypeSignature>(nestedType.Generics[2]);
            Assert.Equal("T1", t1.SortName);
            Assert.Equal("T31", t1.VirtualName);
            Assert.Equal(3, internalSignature.DeclareType.NestedTypes.Length);
            generic = Assert.Single(internalSignature.DeclareType.NestedTypes[0].Generics);
            t1 = Assert.IsType<GenericParameterTypeSignature>(generic);
            Assert.Equal("T1", t1.SortName);
            Assert.Equal("T31", t1.VirtualName);
            Assert.Equal(2, internalSignature.DeclareType.NestedTypes[1].Generics.Length);
            t2 = Assert.IsType<GenericParameterTypeSignature>(internalSignature.DeclareType.NestedTypes[1].Generics[0]);
            Assert.Equal("T2", t2.SortName);
            Assert.Equal("T21", t2.VirtualName);
            t3 = Assert.IsType<GenericParameterTypeSignature>(internalSignature.DeclareType.NestedTypes[1].Generics[1]);
            Assert.Equal("T3", t3.SortName);
            Assert.Equal("T22", t3.VirtualName);
            Assert.Equal(2, internalSignature.DeclareType.NestedTypes[2].Generics.Length);
            t4 = Assert.IsType<GenericParameterTypeSignature>(internalSignature.DeclareType.NestedTypes[2].Generics[0]);
            Assert.Equal("T4", t4.SortName);
            Assert.Equal("T11", t4.VirtualName);
            t5 = Assert.IsType<GenericParameterTypeSignature>(internalSignature.DeclareType.NestedTypes[2].Generics[1]);
            Assert.Equal("T5", t5.SortName);
            Assert.Equal("T12", t5.VirtualName);
        }

        private Methods ResolveMethods(TypeDefinition typeDef)
        {
            var methods = new Methods();
            foreach (var methodDef in typeDef.GetMethods())
            {
                switch (methodDef.Name)
                {
                    case nameof(Methods.Public):
                        methods.Public = methodDef;
                        break;
                    case nameof(Methods.Protected):
                        methods.Protected = methodDef;
                        break;
                    case nameof(Methods.Private):
                        methods.Private = methodDef;
                        break;
                    case nameof(Methods.Internal):
                        methods.Internal = methodDef;
                        break;
                    case nameof(Methods.ProtectedInternal):
                        methods.ProtectedInternal = methodDef;
                        break;
                    case nameof(Methods.PrivateProtected):
                        methods.PrivateProtected = methodDef;
                        break;
                    case nameof(Methods.Default):
                        methods.Default = methodDef;
                        break;
                    case nameof(Methods.DefaultStatic):
                        methods.DefaultStatic = methodDef;
                        break;
                    case nameof(Methods.ProtectedInternalStatic):
                        methods.ProtectedInternalStatic = methodDef;
                        break;
                    case nameof(Methods.PrivateProtectedStatic):
                        methods.PrivateProtectedStatic = methodDef;
                        break;
                }
            }

            return methods;
        }

        class Methods
        {
            public MethodDefinition Public { get; set; }

            public MethodDefinition Protected { get; set; }

            public MethodDefinition Private { get; set; }

            public MethodDefinition Internal { get; set; }

            public MethodDefinition ProtectedInternal { get; set; }

            public MethodDefinition PrivateProtected { get; set; }

            public MethodDefinition Default { get; set; }

            public MethodDefinition DefaultStatic { get; set; }

            public MethodDefinition ProtectedInternalStatic { get; set; }

            public MethodDefinition PrivateProtectedStatic { get; set; }
        }
    }
}
