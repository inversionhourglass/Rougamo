using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Signature;
using SignatureUsage;
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
            Assert.Equal("public System.String SignatureUsage.PublicCls.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal("protected System.String SignatureUsage.PublicCls.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal("private System.String SignatureUsage.PublicCls.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal("internal System.String SignatureUsage.PublicCls.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal("protected internal System.String SignatureUsage.PublicCls.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal("private protected System.String SignatureUsage.PublicCls.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal("private System.String SignatureUsage.PublicCls.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal("private static System.String SignatureUsage.PublicCls.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal("protected internal static System.String SignatureUsage.PublicCls.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal("private protected static System.String SignatureUsage.PublicCls.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
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
