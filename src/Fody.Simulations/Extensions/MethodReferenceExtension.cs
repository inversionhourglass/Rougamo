using Fody;
using Mono.Cecil.Cil;
using System.Collections;
using System.Linq;
using BindingFlags = System.Reflection.BindingFlags;

namespace Mono.Cecil
{
    public static class MethodReferenceExtension
    {
        public static VariableDefinition CreateVariable(this MethodBody body, TypeReference variableTypeReference)
        {
            var variable = new VariableDefinition(variableTypeReference);
            body.Variables.Add(variable);
            return variable;
        }

        public static MethodReference WithGenericDeclaringType(this MethodReference methodRef, TypeReference typeRef)
        {
            methodRef = methodRef.ImportInto(typeRef.Module);
            var typeDef = typeRef.Resolve();
            if (typeDef != null && typeDef != methodRef.DeclaringType.Resolve() && typeDef.IsInterface) return methodRef;

            var genericMethodRef = new MethodReference(methodRef.Name, methodRef.ReturnType, typeRef)
            {
                HasThis = methodRef.HasThis,
                ExplicitThis = methodRef.ExplicitThis,
                CallingConvention = methodRef.CallingConvention
            };
            foreach (var parameter in methodRef.Parameters)
            {
                genericMethodRef.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }
            foreach (var parameter in methodRef.GenericParameters)
            {
                genericMethodRef.GenericParameters.Add(new GenericParameter(parameter.Name, genericMethodRef));
            }

            return genericMethodRef;
        }

        public static MethodReference WithGenerics(this MethodReference methodRef, params TypeReference[] genericTypeRefs)
        {
            if (genericTypeRefs.Length == 0) return methodRef;

            var genericInstanceMethod = new GenericInstanceMethod(methodRef.GetElementMethod());
            genericInstanceMethod.GenericArguments.Add(genericTypeRefs);

            return genericInstanceMethod;
        }

        public static TypeDefinition ResolveStateMachine(this MethodDefinition methodDef, string stateMachineAttributeName)
        {
            var stateMachineAttr = methodDef.CustomAttributes.Single(attr => attr.Is(stateMachineAttributeName));
            var obj = stateMachineAttr.ConstructorArguments[0].Value;
            return obj as TypeDefinition ?? ((TypeReference)obj).Resolve();
        }

        public static bool TryResolveStateMachine(this MethodDefinition methodDef, string stateMachineAttributeName, out TypeDefinition stateMachineTypeDef)
        {
            stateMachineTypeDef = null!;
            var stateMachineAttr = methodDef.CustomAttributes.SingleOrDefault(attr => attr.Is(stateMachineAttributeName));
            if (stateMachineAttr == null) return false;
            var obj = stateMachineAttr.ConstructorArguments[0].Value;
            stateMachineTypeDef = obj as TypeDefinition ?? ((TypeReference)obj).Resolve();
            return true;
        }

        public static void Clear(this MethodDefinition methodDef)
        {
            methodDef.Body.Instructions.Clear();
            methodDef.Body.Variables.Clear();
            methodDef.Body.ExceptionHandlers.Clear();
            methodDef.HardClearCustomDebugInformation();
            methodDef.DebugInformation.Clear();
        }

        public static void Clear(this MethodDebugInformation debugInformation)
        {
            debugInformation.CustomDebugInformations.Clear();
            debugInformation.SequencePoints.Clear();
            if (debugInformation.Scope != null)
            {
                debugInformation.Scope.Constants.Clear();
                debugInformation.Scope.Variables.Clear();
                debugInformation.Scope.Scopes.Clear();
                debugInformation.Scope.CustomDebugInformations.Clear();
            }
        }

        public static void HardClearCustomDebugInformation(this MethodDefinition methodDef)
        {
            methodDef.CustomDebugInformations.Clear();

            var module = methodDef.Module;
            var metadata = module.GetType().GetField("MetadataSystem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(module);
            if (metadata == null) return;

            var customDebugInformations = (IDictionary)metadata.GetType().GetField("CustomDebugInformations", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
            if (customDebugInformations == null) return;

            customDebugInformations.Remove(methodDef.MetadataToken);
        }

        #region Import
        public static MethodReference ImportInto(this MethodReference methodRef, BaseModuleWeaver moduleWeaver)
        {
            return moduleWeaver.Import(methodRef);
        }

        public static MethodReference ImportInto(this MethodReference methodRef, ModuleDefinition moduleDef)
        {
            return moduleDef.ImportReference(methodRef);
        }
        #endregion Import
    }
}