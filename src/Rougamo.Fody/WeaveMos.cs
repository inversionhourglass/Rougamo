using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Rougamo.Fody.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeaveMos()
        {
            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsync)
                    {
                        AsyncMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void IteratorMethodWeave(RouMethod rouMethod)
        {

        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {
        }

        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var variables = CreateVariables(rouMethod);
            var methodBody = rouMethod.MethodDef.Body;
            
        }

        #region CreateVariables

        private MoVariables CreateVariables(RouMethod rouMethod)
        {
            var methodBody = rouMethod.MethodDef.Body;
            var entryContext = methodBody.CreateVariable(_entryContextTypeRef);
            var successContext = methodBody.CreateVariable(_successContextTypeRef);
            var exceptionContext = methodBody.CreateVariable(_exceptionContextTypeRef);
            var exitContext = methodBody.CreateVariable(_exitContextTypeRef);
            var arguments = methodBody.CreateVariable(_objectArrayTypeRef);
            var mos = new VariableWithInitialize[rouMethod.Mos.Count];
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                mos[i++] = CreateVariable(mo, methodBody);
            }
            return new MoVariables
            {
                EntryContext = entryContext,
                SuccessContext = successContext,
                ExceptionContext = exceptionContext,
                ExitContext = exitContext,
                Mos = mos,
                Arguments = arguments
            };
        }

        private VariableWithInitialize CreateVariable(Mo mo, MethodBody methodBody)
        {
            var variable = new VariableWithInitialize();
            if (mo.Attribute != null)
            {
                variable.Variable = methodBody.CreateVariable(mo.Attribute.AttributeType);
                var instructions = LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments);
                instructions.Add(Instruction.Create(OpCodes.Newobj, mo.Attribute.Constructor));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable.Variable));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, variable.Variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable.Variable));
                }

                variable.Instructions = instructions.ToArray();
            }
            else
            {
                variable.Variable = methodBody.CreateVariable(mo.TypeDef);
            }
            return variable;
        }

        private Collection<Instruction> LoadAttributeArgumentIns(Collection<CustomAttributeArgument> arguments)
        {
            var ins = new Collection<Instruction>();
            foreach (var arg in arguments)
            {
                ins.AddRange(LoadValueOnStack(arg.Type, arg.Value));
            }
            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyIns(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties, VariableDefinition attributeDef)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, attributeDef));
                ins.AddRange(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
                ins.Add(Instruction.Create(OpCodes.Nop));
            }

            return ins;
        }

        #endregion CreateVariables

        #region InitMoContexts

        private List<Instruction> InitEntryContext(MethodDefinition methodDef)
        {
            return null;
        }

        #endregion InitMoContexts
    }
}
