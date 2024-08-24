using Mono.Cecil;
using System;

namespace Cecil.AspectN
{
    public class GenericParameterTypeSignature : TypeSignature
    {
        public GenericParameterTypeSignature(string name, TypeReference reference) : base(string.Empty, GenericSignature.EmptyArray, Array.Empty<int>(), reference)
        {
            Name = name;
        }

        public string Name { get; }

        public string? SortName { get; set; }

        public string? VirtualName { get; set; }

        public override string ToString()
        {
            return SortName ?? Name;
        }
    }
}
