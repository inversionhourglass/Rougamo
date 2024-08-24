using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class GenericNamePattern
    {
        public GenericNamePattern(string name, ITypePatterns genericPatterns)
        {
            Name = name;
            GenericPatterns = genericPatterns;
        }

        public string Name { get; }

        public ITypePatterns GenericPatterns { get; }

        public bool IsAny => Name == "*" && GenericPatterns.Count == CollectionCount.ANY;

        public string[]? ImplicitPrefixes { get; set; }

        public void ExtractGenerics(List<GenericParameterTypePattern> list)
        {
            if (GenericPatterns is TypePatterns patterns)
            {
                foreach (var pattern in patterns.Patterns)
                {
                    if (pattern is GenericParameterTypePattern gptp) list.Add(gptp);
                }
            }
        }

        public virtual bool IsMatch(GenericSignature method)
        {
            var name = method.Name;
            if (ImplicitPrefixes != null)
            {
                foreach (var implicitPrefix in ImplicitPrefixes)
                {
                    if (!method.Name.StartsWith(implicitPrefix)) continue;

                    name = name.Substring(implicitPrefix.Length);
                    break;
                }
            }
            return WildcardMatcher.IsMatch(Name, name) && GenericPatterns.IsMatch(method.Generics);
        }
    }
}
