using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void ProxySyncMethodWeave(RouMethod rouMethod)
        {
            var actualMethodDef = rouMethod.MethodDef.Clone($"$Rougamo_{rouMethod.MethodDef.Name}");
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);
            actualMethodDef.CustomAttributes.Clear();

            rouMethod.MethodDef.DebuggerStepThrough(_methodDebuggerStepThroughCtorRef);
            rouMethod.MethodDef.Clear();

            ProxyCallSync(rouMethod, actualMethodDef);
        }

        private void ProxyCallSync(RouMethod rouMethod, MethodDefinition methodDef)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;

            if (!methodDef.IsStatic) instructions.Add(Create(OpCodes.Ldarg_0));
            foreach(var parameter in rouMethod.MethodDef.Parameters)
            {
                instructions.Add(Create(OpCodes.Ldarg, parameter));
            }
            MethodReference methodRef = methodDef;
            if (rouMethod.MethodDef.DeclaringType.HasGenericParameters)
            {
                var git = rouMethod.MethodDef.DeclaringType.MakeGenericInstanceType(rouMethod.MethodDef.DeclaringType.GenericParameters.Cast<TypeReference>().ToArray());
                methodRef = methodRef.WithGenericDeclaringType(git);
            }
            if (rouMethod.MethodDef.HasGenericParameters)
            {
                methodRef = methodRef.WithGenerics(rouMethod.MethodDef.GenericParameters.Cast<TypeReference>().ToArray());
            }
            instructions.Add(Create(OpCodes.Call, methodRef));
            instructions.Add(Create(OpCodes.Ret));
        }
    }
}
