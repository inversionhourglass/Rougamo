using Mono.Cecil;
using Cecil.AspectN;
using System.Collections.Generic;
using System.Linq;
using Rougamo.Fody.Models;

namespace Rougamo.Fody
{
    internal static class ModelExtension
    {
        public static void Initialize(this RouType rouType, MethodDefinition methdDef,
            ConfiguredMo[] configuredMos, CustomAttribute[] assemblyAttributes, TypeReference[] assemblyGenerics,
            RepulsionMo[] typeImplements, CustomAttribute[] typeAttributes, TypeReference[] typeGenerics, TypeReference[] typeProxies,
            CustomAttribute[] methodAttributes, TypeReference[] methodGenerics, TypeReference[] methodProxies,
            string[] assemblyIgnores, string[] typeIgnores, string[] methodIgnores, bool compositeAccessibility, bool skipRefStruct)
        {
            var ignores = new HashSet<string>(assemblyIgnores);
            ignores.AddRange(typeIgnores);
            ignores.AddRange(methodIgnores);

            var rouMethod = new RouMethod(rouType, methdDef, skipRefStruct);

            rouMethod.AddMo(methodAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Method, compositeAccessibility);
            rouMethod.AddMo(methodGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Method, compositeAccessibility);
            rouMethod.AddMo(methodProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Method, compositeAccessibility);

            rouMethod.AddMo(typeAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Class, compositeAccessibility);
            rouMethod.AddMo(typeGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Class, compositeAccessibility);
            rouMethod.AddMo(typeProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Class, compositeAccessibility);
            foreach (var implement in typeImplements.Where(x => !ignores.Contains(x.Mo.FullName)))
            {
                if (!rouMethod.Any(implement.Repulsions))
                {
                    rouMethod.AddMo(implement.Mo, MoFrom.Class, compositeAccessibility);
                    ignores.AddRange(implement.Repulsions.Select(x => x.FullName));
                }
            }

            rouMethod.AddMo(assemblyAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Assembly, compositeAccessibility);
            rouMethod.AddMo(assemblyGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Assembly, compositeAccessibility);
            rouMethod.AddMo(configuredMos.Where(x => MatchMo(rouMethod, x, MoFrom.Assembly, compositeAccessibility)));

            if (rouMethod.MosAny())
            {
                rouType.Methods.Add(rouMethod);
            }
        }

        public static void AddMo(this RouMethod method, TypeReference typeRef, MoFrom from, bool compositeAccessibility) => AddMo(method, new[] { typeRef }, from, compositeAccessibility);

        public static void AddMo(this RouMethod method, IEnumerable<CustomAttribute> attributes, MoFrom from, bool compositeAccessibility)
        {
            var mos = attributes.Select(x => new CustomAttributeMo(x, from)).Where(x => MatchMo(method, x, from, compositeAccessibility));
            method.AddMo(mos);
        }

        public static void AddMo(this RouMethod method, IEnumerable<TypeReference> typeRefs, MoFrom from, bool compositeAccessibility)
        {
            var mos = typeRefs.Select(x => new TypeReferenceMo(x, from)).Where(x => MatchMo(method, x, from, compositeAccessibility));
            method.AddMo(mos);
        }

        private static bool MatchMo(RouMethod method, Mo mo, MoFrom from, bool compositeAccessibility)
        {
            if (from == MoFrom.Method) return true;

            return mo.Pattern == null ? MatchMoFlags(method, mo, from) : MatchMoPattern(method, mo, from, compositeAccessibility);
        }

        private static bool MatchMoFlags(RouMethod method, Mo mo, MoFrom from)
        {
            var methodFlags = method.Flags(from);
            var targetAccessable = methodFlags & AccessFlags.All;
            var targetCategory = methodFlags & (AccessFlags.Method | AccessFlags.Property | AccessFlags.Constructor);
            var declaredAccessable = mo.Flags & AccessFlags.All;
            var declaredCategories = mo.Flags & (AccessFlags.Method | AccessFlags.Property | AccessFlags.Constructor);
            if (declaredCategories == 0) declaredCategories = AccessFlags.Method | AccessFlags.Property;
            var categoryMatch = (declaredCategories & targetCategory) != 0;
            var accessableMatch = (declaredAccessable & targetAccessable) != 0;
            return categoryMatch && accessableMatch;
        }

        private static bool MatchMoPattern(RouMethod method, Mo mo, MoFrom from, bool compositeAccessibility)
        {
            compositeAccessibility = from == MoFrom.Assembly && compositeAccessibility;
            var signature = SignatureParser.ParseMethod(method.MethodDef, compositeAccessibility);
            var pattern = PatternParser.Parse(mo.Pattern!);
            return pattern.IsMatch(signature);
        }

        public static bool Any(this RouMethod method, TypeReference typeRef)
        {
            return method.MosAny(mo => mo.FullName == typeRef.FullName);
        }

        public static bool Any(this RouMethod method, TypeReference[] typeRefs)
        {
            return typeRefs.Any(method.Any);
        }
    }
}
