using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody.Signature
{
    public class MethodSignature
    {
        public Modifier Modifiers { get; set; }

        public TypeSignature ReturnType { get; set; }

        public TypeSignature DeclareType { get; set; }

        public string MethodName { get; set; }

        public string[]? MethodGenericParameters { get; set; }

        public TypeSignature[] MethodParameters { get; set; }

        public override string ToString()
        {
            return $"{Modifiers.ToDefinitionString()} {ReturnType} ";
        }
    }
}
