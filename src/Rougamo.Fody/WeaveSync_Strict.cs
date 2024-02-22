using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictSyncMethodWeave(RouMethod rouMethod)
        {
            var actualMethodDef = rouMethod.MethodDef.Clone($"$Rougamo_{rouMethod.MethodDef.Name}");
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            //var swap = rouMethod.MethodDef.DebugInformation;
            //actualMethodDef.DebugInformation = rouMethod.MethodDef.DebugInformation;
            rouMethod.MethodDef.DebugInformation = null;

            DebuggerStepThrough(rouMethod.MethodDef);

            StrictSyncAdaptedCall(rouMethod, actualMethodDef);
        }

        private void StrictSyncAdaptedCall(RouMethod rouMethod, MethodDefinition methodDef)
        {
            var body = rouMethod.MethodDef.Body;
            var instructions = body.Instructions;

            instructions.Clear();
            body.Variables.Clear();
            body.ExceptionHandlers.Clear();

            if (!methodDef.IsStatic) instructions.Add(Create(OpCodes.Ldarg_0));
            foreach(var parameter in rouMethod.MethodDef.Parameters)
            {
                instructions.Add(Create(OpCodes.Ldarg, parameter));
            }
            MethodReference methodRef = methodDef;
            if (rouMethod.MethodDef.DeclaringType.HasGenericParameters)
            {
                var git = rouMethod.MethodDef.DeclaringType.MakeGenericInstanceType(rouMethod.MethodDef.DeclaringType.GenericParameters.Cast<TypeReference>().ToArray());
                methodRef = git.GenericTypeMethodReference(methodRef, ModuleDefinition);
            }
            if (rouMethod.MethodDef.HasGenericParameters)
            {
                methodRef = methodRef.GenericMethodReference(rouMethod.MethodDef.GenericParameters.Cast<TypeReference>().ToArray());
            }
            instructions.Add(Create(OpCodes.Call, methodRef));
            instructions.Add(Create(OpCodes.Ret));
        }

        private void DebuggerStepThrough(MethodDefinition methodDef)
        {
            methodDef.CustomAttributes.Clear();
            methodDef.CustomAttributes.Add(new CustomAttribute(_methodDebuggerStepThroughCtorRef));
        }
    }
}
