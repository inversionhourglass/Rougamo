using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    if (rouMethod.MethodDef.IsEmpty())
                    {
                        EmptyMethodWeave();
                    }
                    else if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncIterator)
                    {
                        AsyncIteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncTaskOrValueTask)
                    {
                        AsyncTaskMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void EmptyMethodWeave()
        {

        }

        /// <summary>
        /// generate try..catch..finally for <paramref name="methodDef"/>
        /// </summary>
        /// <param name="methodDef">user code MethodDefinition</param>
        /// <param name="tryStart">first of method body's instructions</param>
        /// <param name="catchStart">stloc exception variable</param>
        /// <param name="finallyStart">nop placeholder</param>
        /// <param name="finallyEnd">
        /// null if method no return just throw exception<br/>
        /// ret if method return void<br/>
        /// ldloc/ldarg/ldfld etc if has return value
        /// </param>
        /// <param name="exceptionVariable">catch generate exception variable</param>
        /// <param name="returnVariable">method return value variable, null if void</param>
        private void GenerateTryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction finallyEnd, out VariableDefinition exceptionVariable, out VariableDefinition returnVariable)
        {
            var instructions = methodDef.Body.Instructions;
            tryStart = instructions.First();
            finallyEnd = null;
            returnVariable = null;

            var returns = instructions.Where(ins => ins.OpCode == OpCodes.Ret).ToArray();
            if (returns.Length != 0)
            {
                var isVoid = methodDef.ReturnType.Is(Constants.TYPE_Void);
                finallyEnd = Instruction.Create(OpCodes.Ret);
                instructions.Add(finallyEnd);
                if (!isVoid)
                {
                    returnVariable = methodDef.Body.CreateVariable(Import(methodDef.ReturnType));
                    var loadReturnValue = returnVariable.Ldloc();
                    instructions.InsertBefore(finallyEnd, loadReturnValue);
                    finallyEnd = loadReturnValue;
                }
                foreach (var @return in returns)
                {
                    if (!isVoid)
                    {
                        @return.OpCode = OpCodes.Stloc;
                        @return.Operand = returnVariable;
                        instructions.InsertAfter(@return, Instruction.Create(OpCodes.Leave, finallyEnd));
                    }
                    else
                    {
                        @return.OpCode = OpCodes.Leave;
                        @return.Operand = finallyEnd;
                    }
                }
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            catchStart = Instruction.Create(OpCodes.Stloc, exceptionVariable);
            finallyStart = Instruction.Create(OpCodes.Nop);
            instructions.InsertBeforeOrAppend(finallyEnd, new[]
            {
                catchStart,
                Instruction.Create(OpCodes.Rethrow),
                finallyStart,
                Instruction.Create(OpCodes.Endfinally)
            });
        }

        private void SetTryCatchFinally(MethodDefinition methodDef, Instruction tryStart, Instruction catchStart, Instruction finallyStart, Instruction finallyEnd)
        {
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = _typeExceptionRef,
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = finallyStart
            };
            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = finallyEnd
            };

            methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            methodDef.Body.ExceptionHandlers.Add(finallyHandler);
        }

        private void OnExceptionFromField(MethodDefinition methodDef, int mosCount, FieldReference moFieldRef, FieldReference contextFieldRef, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldarg_0));
            ins.Add(Instruction.Create(OpCodes.Ldfld, contextFieldRef));
            ins.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, methodDef, mosCount, null, moFieldRef, contextFieldRef, catchStart.Next);
            methodDef.Body.Instructions.InsertAfter(catchStart, ins);
        }

        #region LoadMosOnStack

        private VariableDefinition[] LoadMosOnStack(RouMethod rouMethod, List<Instruction> instructions)
        {
            var mos = new VariableDefinition[rouMethod.Mos.Count];
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                mos[i++] = LoadMoOnStack(mo, rouMethod.MethodDef.Body, instructions);
            }
            return mos;
        }

        private VariableDefinition LoadMoOnStack(Mo mo, MethodBody methodBody, List<Instruction> instructions)
        {
            VariableDefinition variable;
            if (mo.Attribute != null)
            {
                variable = methodBody.CreateVariable(Import(mo.Attribute.AttributeType));
                instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                instructions.Add(Instruction.Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable));
                }
            }
            else
            {
                variable = methodBody.CreateVariable(Import(mo.TypeDef));
                instructions.Add(Instruction.Create(OpCodes.Newobj, Import(mo.TypeDef.GetZeroArgsCtor())));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
            }
            return variable;
        }

        private void InitMosField(RouMethod rouMethod, FieldReference mosFieldRef, VariableDefinition variable, Instruction insertBeforeThisInstruction)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(insertBeforeThisInstruction, variable.LdlocOrA());
            instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Ldc_I4, rouMethod.Mos.Count));
            instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Newarr, _typeIMoRef));
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Dup));
                instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Ldc_I4, i));
                if (mo.Attribute != null)
                {
                    instructions.InsertBefore(insertBeforeThisInstruction, LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                    instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                    if (mo.Attribute.HasProperties)
                    {
                        instructions.InsertBefore(insertBeforeThisInstruction, LoadAttributePropertyDup(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties));
                    }
                }
                else
                {
                    instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Newobj, Import(mo.TypeDef.GetZeroArgsCtor())));
                }
                instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Stelem_Ref));
                i++;
            }
            instructions.InsertBefore(insertBeforeThisInstruction, Instruction.Create(OpCodes.Stfld, mosFieldRef));
        }

        private Collection<Instruction> LoadAttributeArgumentIns(Collection<CustomAttributeArgument> arguments)
        {
            var instructions = new Collection<Instruction>();
            foreach (var arg in arguments)
            {
                instructions.Add(LoadValueOnStack(arg.Type, arg.Value));
            }
            return instructions;
        }

        private Collection<Instruction> LoadAttributePropertyIns(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties, VariableDefinition attributeDef)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, attributeDef));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyDup(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Dup));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition CreateMethodContextVariable(MethodDefinition methodDef, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            InitMethodContext(methodDef, instructions);
            instructions.Add(Instruction.Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void InitMethodContextField(RouMethod rouMethod, FieldReference contextFieldRef, VariableDefinition variable, Instruction insertBeforeThis)
        {
            var bodyInstructions = rouMethod.MethodDef.Body.Instructions;
            var instructions = new List<Instruction>();
            bodyInstructions.InsertBefore(insertBeforeThis, variable.LdlocOrA());
            InitMethodContext(rouMethod.MethodDef, instructions);
            bodyInstructions.InsertBefore(insertBeforeThis, instructions);
            bodyInstructions.InsertBefore(insertBeforeThis, Instruction.Create(OpCodes.Stfld, contextFieldRef));
        }

        private void InitMethodContext(MethodDefinition methodDef, List<Instruction> instructions)
        {
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef));
            instructions.Add(Instruction.Create(OpCodes.Newobj, _methodMethodContextCtorRef));
        }

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, VariableDefinition stateMachineVariable, FieldReference mosField, FieldReference contextField, Instruction beforeThisInstruction)
        {
            var instructions = methodDef.Body.Instructions;
            var flagVariable = methodDef.Body.CreateVariable(_typeIntRef);
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Stloc, flagVariable));
            var loopFirst = Instruction.Create(OpCodes.Ldloc, flagVariable);
            instructions.InsertBefore(beforeThisInstruction, loopFirst);
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldc_I4, mosCount));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Clt));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Brfalse_S, beforeThisInstruction));
            if (stateMachineVariable == null)
            {
                instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldarg_0));
            }
            else
            {
                instructions.InsertBefore(beforeThisInstruction, stateMachineVariable.LdlocOrA());
            }
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldfld, mosField));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldloc, flagVariable));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldelem_Ref));
            if (stateMachineVariable == null)
            {
                instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldarg_0));
            }
            else
            {
                instructions.InsertBefore(beforeThisInstruction, stateMachineVariable.LdlocOrA());
            }
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldfld, contextField));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
            // maybe nop
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldloc, flagVariable));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Add));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Stloc, flagVariable));
            instructions.InsertBefore(beforeThisInstruction, Instruction.Create(OpCodes.Br_S, loopFirst));
        }

        private void ExecuteMoMethod(string methodName, VariableDefinition[] mos, VariableDefinition methodContext, List<Instruction> instructions)
        {
            foreach (var mo in mos)
            {
                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldloc, mo),
                    Instruction.Create(OpCodes.Ldloc, methodContext),
                    Instruction.Create(OpCodes.Callvirt, _methodIMosRef[methodName])
                });
            }
        }
    }
}
