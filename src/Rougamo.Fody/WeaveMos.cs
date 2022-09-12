using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

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
                        EmptyMethodWeave(rouMethod);
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

        private void EmptyMethodWeave(RouMethod rouMethod)
        {
            var bodyInstructions = rouMethod.MethodDef.Body.Instructions;
            var ret = bodyInstructions.Last();
            Instruction afterOnSuccessNop;
            if (bodyInstructions.Count > 1)
            {
                afterOnSuccessNop = ret.Previous;
            }
            else
            {
                afterOnSuccessNop = Create(OpCodes.Nop);
                bodyInstructions.Insert(bodyInstructions.Count - 1, afterOnSuccessNop);
            }

            var instructions = InitMosArrayVariable(rouMethod, out var mosVariable);
            var contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, mosVariable, false, false, instructions);

            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, false);

            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
            instructions.Add(Create(OpCodes.Brtrue_S, afterOnSuccessNop));

            ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());

            rouMethod.MethodDef.Body.Instructions.InsertBefore(afterOnSuccessNop, instructions);

            instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());
            rouMethod.MethodDef.Body.Instructions.InsertAfter(afterOnSuccessNop, instructions);
            rouMethod.MethodDef.Body.OptimizePlus();
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
        private void GenerateTryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction? finallyEnd, out Instruction rethrow, out VariableDefinition exceptionVariable, out VariableDefinition? returnVariable)
        {
            var instructions = methodDef.Body.Instructions;
            var isVoid = methodDef.ReturnType.IsVoid();
            tryStart = instructions.First();
            finallyEnd = null;
            returnVariable = isVoid ? null : methodDef.Body.CreateVariable(Import(methodDef.ReturnType));

            var returns = instructions.Where(ins => ins.IsRet()).ToArray();
            if (returns.Length != 0)
            {
                finallyEnd = Create(OpCodes.Ret);
                instructions.Add(finallyEnd);
                if (returnVariable != null)
                {
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
                        instructions.InsertAfter(@return, Create(OpCodes.Leave, finallyEnd));
                    }
                    else
                    {
                        @return.OpCode = OpCodes.Leave;
                        @return.Operand = finallyEnd;
                    }
                }
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            catchStart = Create(OpCodes.Stloc, exceptionVariable);
            finallyStart = Create(OpCodes.Nop);
            rethrow = Create(OpCodes.Rethrow);
            instructions.InsertBeforeOrAppend(finallyEnd, new[]
            {
                catchStart,
                rethrow,
                finallyStart,
                Create(OpCodes.Endfinally)
            });
        }

        private void SetTryCatchFinally(MethodDefinition methodDef, Instruction tryStart, Instruction catchStart, Instruction finallyStart, Instruction? finallyEnd)
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
            ins.Add(Create(OpCodes.Ldarg_0));
            ins.Add(Create(OpCodes.Ldfld, contextFieldRef));
            ins.Add(Create(OpCodes.Ldloc, exceptionVariable));
            ins.Add(Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, methodDef, mosCount, moFieldRef, contextFieldRef, catchStart.Next, this.ConfigReverseCallEnding());
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
                instructions.Add(Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                instructions.Add(Create(OpCodes.Stloc, variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable));
                }
            }
            else
            {
                variable = methodBody.CreateVariable(Import(mo.TypeDef!));
                instructions.Add(Create(OpCodes.Newobj, Import(mo.TypeDef!.GetZeroArgsCtor())));
                instructions.Add(Create(OpCodes.Stloc, variable));
            }
            return variable;
        }

        private List<Instruction> InitMosArrayVariable(RouMethod rouMethod, out VariableDefinition mosVariable)
        {
            mosVariable = rouMethod.MethodDef.Body.CreateVariable(_typeIMoArrayRef);
            var instructions = InitMosArray(rouMethod);
            instructions.Add(Create(OpCodes.Stloc, mosVariable));

            return instructions;
        }

        private void StateMachineInitMosField(RouMethod rouMethod, FieldDefinition mosFieldDef, VariableDefinition stateMachineVariable, Instruction insertBeforeThisInstruction)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(insertBeforeThisInstruction, stateMachineVariable.LdlocOrA());
            instructions.InsertBefore(insertBeforeThisInstruction, InitMosArray(rouMethod));
            var mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateMachineVariable.VariableType);
            instructions.InsertBefore(insertBeforeThisInstruction, Create(OpCodes.Stfld, mosFieldRef));
        }

        private List<Instruction> InitMosArray(RouMethod rouMethod)
        {
            var instructions = new List<Instruction>();

            instructions.Add(Create(OpCodes.Ldc_I4, rouMethod.Mos.Count));
            instructions.Add(Create(OpCodes.Newarr, _typeIMoRef));
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                if (mo.Attribute != null)
                {
                    instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                    instructions.Add(Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                    if (mo.Attribute.HasProperties)
                    {
                        instructions.AddRange(LoadAttributePropertyDup(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties));
                    }
                }
                else
                {
                    instructions.Add(Create(OpCodes.Newobj, Import(mo.TypeDef!.GetZeroArgsCtor())));
                }
                instructions.Add(Create(OpCodes.Stelem_Ref));
                i++;
            }

            return instructions;
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
                ins.Add(Create(OpCodes.Ldloc, attributeDef));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyDup(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Create(OpCodes.Dup));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition CreateMethodContextVariable(MethodDefinition methodDef, VariableDefinition mosVariable, bool isAsync, bool isIterator, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            InitMethodContext(methodDef, isAsync, isIterator, mosVariable, null, null, instructions);
            instructions.Add(Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void StateMachineInitMethodContextField(RouMethod rouMethod, FieldDefinition mosFieldDef, FieldDefinition contextFieldDef, VariableDefinition stateMachineVariable, Instruction insertBeforeThis, bool isAsync, bool isIterator)
        {
            var mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateMachineVariable.VariableType);
            var contextFieldRef = new FieldReference(contextFieldDef.Name, contextFieldDef.FieldType, stateMachineVariable.VariableType);
            var bodyInstructions = rouMethod.MethodDef.Body.Instructions;
            var instructions = new List<Instruction>();
            bodyInstructions.InsertBefore(insertBeforeThis, stateMachineVariable.LdlocOrA());
            InitMethodContext(rouMethod.MethodDef, isAsync, isIterator, null, stateMachineVariable, mosFieldRef, instructions);
            bodyInstructions.InsertBefore(insertBeforeThis, instructions);
            bodyInstructions.InsertBefore(insertBeforeThis, Create(OpCodes.Stfld, contextFieldRef));
        }

        private void InitMethodContext(MethodDefinition methodDef, bool isAsync, bool isIterator, VariableDefinition? mosVariable, VariableDefinition? stateMachineVariable, FieldReference? mosFieldRef, List<Instruction> instructions)
        {
            var isAsyncCode = isAsync ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var isIteratorCode = isIterator ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var mosNonEntryFIFO = this.ConfigReverseCallEnding() ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1;
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.Add(Create(isAsyncCode));
            instructions.Add(Create(isIteratorCode));
            instructions.Add(Create(mosNonEntryFIFO));
            if (stateMachineVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldloc, mosVariable));
            }
            else
            {
                instructions.Add(stateMachineVariable.LdlocOrA());
                instructions.Add(Create(OpCodes.Ldfld, mosFieldRef));
            }
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef));
            instructions.Add(Create(OpCodes.Newobj, _methodMethodContextCtorRef));
        }

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, FieldReference mosField, FieldReference contextField, Instruction beforeThisInstruction, bool reverseCall)
        {
            var instructions = methodDef.Body.Instructions;
            instructions.InsertBefore(beforeThisInstruction, ExecuteMoMethod(methodName, methodDef, mosCount, beforeThisInstruction, null, null, mosField, contextField, reverseCall));
        }

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, VariableDefinition mosVariable, VariableDefinition contextVariable, List<Instruction> instructions, bool reverseCall)
        {
            var loopExit = Create(OpCodes.Nop);

            instructions.AddRange(ExecuteMoMethod(methodName, methodDef, mosCount, loopExit, mosVariable, contextVariable, null, null, reverseCall));

            instructions.Add(loopExit);
        }

        private List<Instruction> ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, Instruction loopExit, VariableDefinition? mosVariable, VariableDefinition? contextVariable, FieldReference? mosField, FieldReference? contextField, bool reverseCall)
        {
            var instructions = new List<Instruction>();
            var flagVariable = methodDef.Body.CreateVariable(_typeIntRef);

            Instruction loopFirst;

            if (reverseCall)
            {
                instructions.Add(Create(OpCodes.Ldc_I4, mosCount));
                instructions.Add(Create(OpCodes.Ldc_I4_1));
                instructions.Add(Create(OpCodes.Sub));
                instructions.Add(Create(OpCodes.Stloc, flagVariable));
                loopFirst = Create(OpCodes.Ldloc, flagVariable);
                instructions.Add(loopFirst);
                instructions.Add(Create(OpCodes.Ldc_I4_0));
                instructions.Add(Create(OpCodes.Clt));
                instructions.Add(Create(OpCodes.Brtrue, loopExit));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldc_I4_0));
                instructions.Add(Create(OpCodes.Stloc, flagVariable));
                loopFirst = Create(OpCodes.Ldloc, flagVariable);
                instructions.Add(loopFirst);
                instructions.Add(Create(OpCodes.Ldc_I4, mosCount));
                instructions.Add(Create(OpCodes.Clt));
                instructions.Add(Create(OpCodes.Brfalse_S, loopExit));
            }

            if (mosVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, mosField));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldloc, mosVariable));
            }
            instructions.Add(Create(OpCodes.Ldloc, flagVariable));
            instructions.Add(Create(OpCodes.Ldelem_Ref));
            if (contextVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, contextField));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
            instructions.Add(Create(OpCodes.Ldloc, flagVariable));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            if (reverseCall)
            {
                instructions.Add(Create(OpCodes.Sub));
            }
            else
            {
                instructions.Add(Create(OpCodes.Add));
            }
            instructions.Add(Create(OpCodes.Stloc, flagVariable));
            instructions.Add(Create(OpCodes.Br_S, loopFirst));

            return instructions;
        }
    }
}
