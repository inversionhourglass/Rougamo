using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class MonoCloneExtension
    {
        private static readonly System.Reflection.ConstructorInfo _CtorMethodDebugInformation = typeof(MethodDebugInformation).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Single();
        private static readonly System.Reflection.FieldInfo _FieldVariableIndex = typeof(VariableDebugInformation).GetField("index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private static readonly System.Reflection.FieldInfo _FieldIndexVariable = typeof(VariableIndex).GetField("variable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static MethodDefinition Clone(this MethodDefinition methodDef, string methodName)
        {
            var clonedMethodDef = new MethodDefinition(methodName, methodDef.Attributes, methodDef.ReturnType);

            if (methodDef.HasCustomAttributes)
            {
                clonedMethodDef.CustomAttributes.Add(methodDef.CustomAttributes);
            }

            var map = new Dictionary<object, object>();

            if (methodDef.HasGenericParameters)
            {
                foreach (var generic in methodDef.GenericParameters)
                {
                    var clonedGeneric = new GenericParameter(generic.Name, clonedMethodDef);
                    clonedMethodDef.GenericParameters.Add(clonedGeneric);

                    map[generic] = clonedGeneric;
                }
            }

            foreach (var parameterDef in methodDef.Parameters)
            {
                var parameterTypeRef = parameterDef.ParameterType;
                var isByReference = parameterTypeRef.IsByReference;
                if (isByReference)
                {
                    parameterTypeRef = ((ByReferenceType)parameterTypeRef).ElementType;
                }
                if (parameterTypeRef.IsGenericParameter && map.TryGetValue(parameterTypeRef, out var mapTo))
                {
                    parameterTypeRef = (GenericParameter)mapTo;
                }
                if (isByReference)
                {
                    parameterTypeRef = new ByReferenceType(parameterTypeRef);
                    map[parameterDef.ParameterType] = parameterTypeRef;
                }
                var clonedParameterDef = new ParameterDefinition(parameterDef.Name, parameterDef.Attributes, parameterTypeRef);
                clonedMethodDef.Parameters.Add(clonedParameterDef);

                map[parameterDef] = clonedParameterDef;
            }

            foreach (var variable in methodDef.Body.Variables)
            {
                var clonedVariable = new VariableDefinition(variable.VariableType);
                clonedMethodDef.Body.Variables.Add(clonedVariable);

                map[variable] = clonedVariable;
            }

            var instructionMap = new Dictionary<Instruction, Instruction>();
            var offsetMap = new Dictionary<int, Instruction>();
            var brs = new List<Instruction>();
            foreach (var instruction in methodDef.Body.Instructions)
            {
                var cloned = instruction.Clone();
                clonedMethodDef.Body.Instructions.Add(cloned);

                instructionMap[instruction] = cloned;
                offsetMap[instruction.Offset] = cloned;
                if (cloned.Operand != null)
                {
                    if (map.TryGetValue(cloned.Operand, out var value))
                    {
                        cloned.Operand = value;
                    }
                    else if (cloned.Operand is Instruction)
                    {
                        brs.Add(cloned);
                    }
                }
            }
            foreach (var br in brs)
            {
                br.Operand = instructionMap[(Instruction)br.Operand];
            }

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                clonedMethodDef.Body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType)
                {
                    TryStart = instructionMap[handler.TryStart],
                    TryEnd = instructionMap[handler.TryEnd],
                    HandlerStart = instructionMap[handler.HandlerStart],
                    HandlerEnd = instructionMap[handler.HandlerEnd]
                });
            }

            clonedMethodDef.Body.InitLocals = methodDef.Body.InitLocals;
            clonedMethodDef.Body.MaxStackSize = methodDef.Body.MaxStackSize;

            clonedMethodDef.DebugInformation = (MethodDebugInformation)_CtorMethodDebugInformation.Invoke(new object[] { clonedMethodDef });
            foreach (var point in methodDef.DebugInformation.SequencePoints)
            {
                var clonedPoint = new SequencePoint(offsetMap[point.Offset], point.Document)
                {
                    StartLine = point.StartLine,
                    StartColumn = point.StartColumn,
                    EndLine = point.EndLine,
                    EndColumn = point.EndColumn
                };
                clonedMethodDef.DebugInformation.SequencePoints.Add(clonedPoint);
            }
            clonedMethodDef.DebugInformation.Scope = methodDef.DebugInformation.Scope.Clone(offsetMap, map);

            return clonedMethodDef;
        }

        public static ScopeDebugInformation Clone(this ScopeDebugInformation scope, Dictionary<int, Instruction> offsetMap, Dictionary<object, object> variableMap)
        {
            var start = scope.Start.IsEndOfMethod ? null : offsetMap[scope.Start.Offset];
            var end = scope.End.IsEndOfMethod ? null : offsetMap[scope.End.Offset];
            var clonedScope = new ScopeDebugInformation(start, end);
            foreach (var constant in scope.Constants)
            {
                clonedScope.Constants.Add(constant);
            }
            foreach (var variable in scope.Variables)
            {
                var index = _FieldVariableIndex.GetValue(variable);
                var variableDef = _FieldIndexVariable.GetValue(index);
                clonedScope.Variables.Add(new VariableDebugInformation((VariableDefinition)variableMap[variableDef], variable.Name));
            }
            foreach (var childScope in scope.Scopes)
            {
                clonedScope.Scopes.Add(childScope.Clone(offsetMap, variableMap));
            }

            return clonedScope;
        }
    }
}
