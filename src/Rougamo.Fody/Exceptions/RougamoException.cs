using Mono.Cecil;
using System;

namespace Rougamo.Fody
{
    internal class RougamoException : Exception
    {
        public RougamoException() : base() { }

        public RougamoException(string message) : base(message) { }

        public RougamoException(string message, MethodDefinition methodDef) : this(message)
        {
            MethodDef = methodDef;
        }

        public MethodDefinition? MethodDef { get; set; }
    }
}
