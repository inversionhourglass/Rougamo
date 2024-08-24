using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cecil.AspectN;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests.Signatures
{
    [Collection("SignatureUsage")]
    public class PropertyMatchTests
    {
        private readonly ModuleDefinition _moduleDef;
        private readonly (MethodDefinition method, PropertyDefinition property)[] _defs;

        public PropertyMatchTests()
        {
            var parameters = new ReaderParameters
            {
                AssemblyResolver = new TestAssemblyResolver()
            };
            _moduleDef = ModuleDefinition.ReadModule("SignatureUsage.dll", parameters);
            var types = _moduleDef.GetTypes();
            var methodDefs = types.SelectMany(x => x.GetMethods()).ToArray();
            var propertyDefs = types.SelectMany(x => x.Properties).ToArray();
            _defs = methodDefs.Select(x => (x, propertyDefs.FirstOrDefault(y => y.GetMethod == x || y.SetMethod == x))).ToArray();
        }

        protected virtual string PatternType => "property";

        protected virtual bool Filter((MethodDefinition method, PropertyDefinition property) def)
        {
            return def.property != null;
        }

        [Fact]
        public void AnyMatchTest()
        {
            var matcher1 = PatternParser.Parse($"{PatternType}(* *.*)");
            var matcher2 = PatternParser.Parse($"{PatternType}(* *..*.*)");
            var matcher3 = PatternParser.Parse($"{PatternType}(* *)");

            var expected1 = _defs.Where(Filter).Select(x => x.method).ToArray();
            var expected2 = _defs.Where(Filter).Select(x => x.method).ToArray();
            var expected3 = _defs.Where(Filter).Select(x => x.method).ToArray();

            var actual1 = new List<MethodDefinition>();
            var actual2 = new List<MethodDefinition>();
            var actual3 = new List<MethodDefinition>();
            foreach (var def in _defs)
            {
                var signature = SignatureParser.ParseMethod(def.method, true);
                if (matcher1.IsMatch(signature)) actual1.Add(def.method);
                if (matcher2.IsMatch(signature)) actual2.Add(def.method);
                if (matcher3.IsMatch(signature)) actual3.Add(def.method);
            }

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
        }

        [Fact]
        public void MidifierMatchTest()
        {
            var publicMatcher = PatternParser.Parse($"{PatternType}(public * *)");
            var privateStaticMatcher = PatternParser.Parse($"{PatternType}(private static * *)");
            var staticMatcher = PatternParser.Parse($"{PatternType}(static * *.*)");
            var privateProtectedMatcher = PatternParser.Parse($"{PatternType}(privateprotected * *..*.*)");
            var nonInternalMatcher = PatternParser.Parse($"{PatternType}(!internal * *.*)");

            var methods = _defs.Where(Filter).Select(x => x.method).Select<MethodDefinition, (MethodDefinition def, Modifier modifier)>(x => (x, SignatureParser.ParseMethod(x, true).Modifiers)).ToArray();
            var expectedPublics = methods.Where(x => x.modifier == Modifier.Public || x.modifier == Modifier.PublicStatic).Select(x => x.def).ToArray();
            var expectedPrivateStatics = methods.Where(x => x.modifier == Modifier.PrivateStatic).Select(x => x.def).ToArray();
            var expectedStatics = methods.Where(x => (x.modifier & Modifier.Static) != 0).Select(x => x.def).ToArray();
            var expectedPrivateProtecteds = methods.Where(x => x.modifier == Modifier.PrivateProtected || x.modifier == Modifier.PrivateProtectedStatic).Select(x => x.def).ToArray();
            var expectedNonInternals = methods.Where(x => x.modifier != Modifier.Internal && x.modifier != Modifier.InternalStatic).Select(x => x.def).ToArray();

            var actualPublics = new List<MethodDefinition>();
            var actualPrivateStatics = new List<MethodDefinition>();
            var actualStatics = new List<MethodDefinition>();
            var actualPrivateProtecteds = new List<MethodDefinition>();
            var actualNonInternals = new List<MethodDefinition>();
            foreach (var def in _defs)
            {
                var methodDef = def.method;
                var signature = SignatureParser.ParseMethod(methodDef, true);
                if (publicMatcher.IsMatch(signature)) actualPublics.Add(methodDef);
                if (privateStaticMatcher.IsMatch(signature)) actualPrivateStatics.Add(methodDef);
                if (staticMatcher.IsMatch(signature)) actualStatics.Add(methodDef);
                if (privateProtectedMatcher.IsMatch(signature)) actualPrivateProtecteds.Add(methodDef);
                if (nonInternalMatcher.IsMatch(signature)) actualNonInternals.Add(methodDef);
            }

            Assert.Equal(expectedPublics, actualPublics);
            Assert.Equal(expectedPrivateStatics, actualPrivateStatics);
            Assert.Equal(expectedStatics, actualStatics);
            Assert.Equal(expectedPrivateProtecteds, actualPrivateProtecteds);
            Assert.Equal(expectedNonInternals, actualNonInternals);
        }

        [Fact]
        public void WildNameMatchTest()
        {
            var matcher1 = PatternParser.Parse($"{PatternType}(* *.*.Pr*)");
            var matcher2 = PatternParser.Parse($"{PatternType}(double *.*Prop)");
            var matcher3 = PatternParser.Parse($"{PatternType}(*Class *)");
            
            var expected1 = _defs.Where(Filter).Where(x => x.property.DeclaringType.Namespace.Count(y => y == '.') == 0 && x.property.Name.StartsWith("Pr")).Select(x => x.method).ToArray();
            var expected2 = _defs.Where(Filter).Where(x => x.property.PropertyType.ToString() == "System.Double" && x.property.Name.EndsWith("Prop")).Select(x => x.method).ToArray();
            var expected3 = _defs.Where(Filter).Where(x => x.property.PropertyType.Name.EndsWith("Class")).Select(x => x.method).ToArray();

            Assert.NotEmpty(expected1);
            Assert.NotEmpty(expected2);
            Assert.NotEmpty(expected3);

            var actual1 = new List<MethodDefinition>();
            var actual2 = new List<MethodDefinition>();
            var actual3 = new List<MethodDefinition>();
            foreach (var def in _defs)
            {
                var methodDef = def.method;
                var signature = SignatureParser.ParseMethod(methodDef, true);
                if (matcher1.IsMatch(signature)) actual1.Add(methodDef);
                if (matcher2.IsMatch(signature)) actual2.Add(methodDef);
                if (matcher3.IsMatch(signature)) actual3.Add(methodDef);
            }

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
        }

        [Fact]
        public void AssignableMatchTest()
        {
            var matcher1 = PatternParser.Parse($"{PatternType}(SignatureUsage.Assignables.Interface+ *)");
            var matcher2 = PatternParser.Parse($"{PatternType}(* SignatureUsage.Assignables.BaseClass+.*)");
            var matcher3 = PatternParser.Parse($"{PatternType}(*..*Class+ *)");
            var matcher4 = PatternParser.Parse($"{PatternType}(* *Interface*+.*)");
            var matcher5 = PatternParser.Parse($"{PatternType}(Interface+ Interface+.*Class)");

            var expected1 = _defs.Where(Filter).Where(x => x.property.PropertyType.GetInterfaceBaseTypes().Any(y => y == "SignatureUsage.Assignables.Interface")).Select(x => x.method).ToArray();
            var expected2 = _defs.Where(Filter).Where(x => x.property.DeclaringType.GetInterfaceBaseTypes().Any(y => y == "SignatureUsage.Assignables.BaseClass")).Select(x => x.method).ToArray();
            var expected3 = _defs.Where(Filter).Where(x => x.property.PropertyType.GetInterfaceBaseTypes().Any(y => y.EndsWith("Class"))).Select(x => x.method).ToArray();
            var expected4 = _defs.Where(Filter).Where(x => x.property.DeclaringType.GetInterfaceBaseTypes().Any(y => y.Split(new[] { '.' }).Last().Contains("Interface"))).Select(x => x.method).ToArray();
            var expected5 = _defs.Where(Filter).Where(x => x.property.PropertyType.GetInterfaceBaseTypes().Any(y => y.EndsWith(".Interface")) && x.property.DeclaringType.GetInterfaceBaseTypes().Any(y => y.EndsWith(".Interface")) && x.property.Name.EndsWith("Class")).Select(x => x.method).ToArray();

            var actual1 = new List<MethodDefinition>();
            var actual2 = new List<MethodDefinition>();
            var actual3 = new List<MethodDefinition>();
            var actual4 = new List<MethodDefinition>();
            var actual5 = new List<MethodDefinition>();
            foreach (var def in _defs)
            {
                var methodDef = def.method;
                var signature = SignatureParser.ParseMethod(methodDef, true);
                if (matcher1.IsMatch(signature)) actual1.Add(methodDef);
                if (matcher2.IsMatch(signature)) actual2.Add(methodDef);
                if (matcher3.IsMatch(signature)) actual3.Add(methodDef);
                if (matcher4.IsMatch(signature)) actual4.Add(methodDef);
                if (matcher5.IsMatch(signature)) actual5.Add(methodDef);
            }

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
            Assert.Equal(expected4, actual4);
            Assert.Equal(expected5, actual5);

        }
    }
}
