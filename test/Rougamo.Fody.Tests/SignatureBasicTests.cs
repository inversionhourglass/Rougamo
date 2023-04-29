using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Signature;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(SignatureBasicTests))]
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

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"public System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protected internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"protected internal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"private protected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void InternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"internal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"private protected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void DefaultClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"internal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"private protected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void PublicNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"public System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protected internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"protected internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protected internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPrivateClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PrivateICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/InternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"private protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"private protected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void GenericTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal("public System.Collections.Generic.List<System.Collections.Generic.List<T2>> SignatureUsage.GenericCls<T1>.Public<T2,T3>(T1,System.Nullable<T3>)", publicSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal("internal static System.Collections.Generic.List<T1> SignatureUsage.GenericCls<T1>.Internal<T2,T3>(T2,T3)", internalSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal("private T SignatureUsage.GenericCls<T>.Private(T)", privateSignature.ToString());

            typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1/ProtectedICls`2");
            methods = ResolveMethods(typeDef);
            publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal("protected T1 SignatureUsage.GenericCls<T1>/ProtectedICls<T2,T3>.Public<T4>(T2,T3,T4)", publicSignature.ToString());

            typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1/ProtectedICls`2/PrivateICls`2");
            methods = ResolveMethods(typeDef);
            internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal("private System.ValueTuple<T5,T4,T1> SignatureUsage.GenericCls<T1>/ProtectedICls<T2,T3>/PrivateICls<T4,T5>.Internal<T6,T7>(T2,T3,T6,T7)", internalSignature.ToString());
        }

        private Methods ResolveMethods(TypeDefinition typeDef)
        {
            var methods = new Methods();
            foreach(var methodDef in typeDef.GetMethods())
            {
                switch(methodDef.Name)
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
