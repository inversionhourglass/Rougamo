﻿using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Signature;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests
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

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"public System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protectedinternal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"protectedinternal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"privateprotected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());

            var voidMethod = typeDef.GetMethods().Single(x => x.Name == "Void");
            var voidSignature = SignatureParser.ParseMethod(voidMethod);
            Assert.Equal($"public System.Void {typeDef.FullName}.Void()", voidSignature.ToString());
        }

        [Fact]
        public void InternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"internal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"privateprotected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
        }

        [Fact]
        public void DefaultClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());

            var defaultSignature = SignatureParser.ParseMethod(methods.Default);
            Assert.Equal($"private System.String {typeDef.FullName}.Default()", defaultSignature.ToString());

            var defaultStaticSignature = SignatureParser.ParseMethod(methods.DefaultStatic);
            Assert.Equal($"private static System.String {typeDef.FullName}.DefaultStatic()", defaultStaticSignature.ToString());

            var protectedInternalStaticSignature = SignatureParser.ParseMethod(methods.ProtectedInternalStatic);
            Assert.Equal($"internal static System.String {typeDef.FullName}.ProtectedInternalStatic()", protectedInternalStaticSignature.ToString());

            var privateProtectedStaticSignature = SignatureParser.ParseMethod(methods.PrivateProtectedStatic);
            Assert.Equal($"privateprotected static System.String {typeDef.FullName}.PrivateProtectedStatic()", privateProtectedStaticSignature.ToString());
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
            Assert.Equal($"protectedinternal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
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
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
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
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"protectedinternal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"protected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"protectedinternal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void PublicNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.PublicCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
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
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void InternalNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.InternalCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPublicClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PublicICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
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
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedProtectedInternalClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/ProtectedInternalICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"internal System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"internal System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"internal System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void DefaultNestedPrivateProtectedClassModifierTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.DefaultCls/PrivateProtectedICls");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Public()", publicSignature.ToString());

            var protectSignature = SignatureParser.ParseMethod(methods.Protected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Protected()", protectSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal($"private System.String {typeDef.FullName}.Private()", privateSignature.ToString());

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.Internal()", internalSignature.ToString());

            var protectedInternalSignature = SignatureParser.ParseMethod(methods.ProtectedInternal);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.ProtectedInternal()", protectedInternalSignature.ToString());

            var privateProtectedSignature = SignatureParser.ParseMethod(methods.PrivateProtected);
            Assert.Equal($"privateprotected System.String {typeDef.FullName}.PrivateProtected()", privateProtectedSignature.ToString());
        }

        [Fact]
        public void GenericTest()
        {
            var typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1");
            var methods = ResolveMethods(typeDef);

            var publicSignature = SignatureParser.ParseMethod(methods.Public);
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

            var internalSignature = SignatureParser.ParseMethod(methods.Internal);
            Assert.Equal("internal static System.Collections.Generic.List<T1> SignatureUsage.GenericCls<T1>.Internal<T2,T3>(T2,T3)", internalSignature.ToString());

            var privateSignature = SignatureParser.ParseMethod(methods.Private);
            Assert.Equal("private T SignatureUsage.GenericCls<T>.Private(T)", privateSignature.ToString());
            var t = Assert.IsType<GenericParameterTypeSignature>(privateSignature.ReturnType);
            Assert.Equal("T", t.SortName);
            Assert.Equal("T11", t.VirtualName);

            typeDef = _moduleDef.GetType("SignatureUsage.GenericCls`1/ProtectedICls`2");
            methods = ResolveMethods(typeDef);
            publicSignature = SignatureParser.ParseMethod(methods.Public);
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
            internalSignature = SignatureParser.ParseMethod(methods.Internal);
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
