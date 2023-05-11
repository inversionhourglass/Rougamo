using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody.Signature
{
    /// <summary>
    /// Generic type
    /// </summary>
    public class GenericTypeSignature : TypeSignature
    {
        public GenericTypeSignature(string name, TypeSignature? declaringType, TypeSignature[] parameters, TypeReference reference) : base(name, declaringType, TypeCategory.Generic, reference)
        {
            GenericParameters = parameters;
        }

        /// <summary>
        /// Generic parameters
        /// </summary>
        public TypeSignature[] GenericParameters { get; set; }

        protected override string FormatName => $"{Name}<{string.Join(",", GenericParameters.AsEnumerable())}>";
    }
}
