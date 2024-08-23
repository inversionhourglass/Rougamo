using Mono.Cecil;
using System;

namespace Fody
{
    public class FodyWeavingException : Exception
    {
        public FodyWeavingException() : base() { }

        public FodyWeavingException(string message) : base(message) { }

        public FodyWeavingException(string message, MethodDefinition methodDef) : this(message)
        {
            MethodDef = methodDef;
        }

        public virtual MethodDefinition? MethodDef { get; set; }
    }
}
