using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void IteratorMethodWeave(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_IteratorStateMachineAttribute);
            var moveNextMethodDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var moveNextMethodName = stateMachineTypeDef.DeclaringType.FullName;

            var fields = IteratorResolveFields(rouMethod, stateMachineTypeDef);
            var variables = IteratorResolveVariables(rouMethod, moveNextMethodDef, stateMachineTypeDef);
            var anchors = IteratorCreateAnchors(rouMethod.MethodDef, moveNextMethodDef, variables);
            IteratorSetAnchors(rouMethod.MethodDef, moveNextMethodDef, variables, anchors);
            IteratorSetTryCatchFinally(moveNextMethodDef, anchors);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMosStart, IteratorInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContextStart, IteratorInitMethodContext(rouMethod, fields, variables));
            if (fields.RecordedReturn != null) rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitRecordedReturnStart, IteratorInitRecordedReturn(fields, variables));

            var instructions = moveNextMethodDef.Body.Instructions;

            instructions.Insert(0, IteratorIfFirstTimeExecute(anchors.TryStart, fields));
            instructions.InsertAfter(anchors.OnEntryStart, IteratorOnEntry(rouMethod, moveNextMethodDef, anchors.RewriteArgStart, fields));
            instructions.InsertAfter(anchors.RewriteArgStart, IteratorRewriteArguments(anchors.TryStart, fields));

            instructions.InsertAfter(anchors.CatchStart, IteratorSaveException(anchors.OnExceptionStart, fields, variables));
            instructions.InsertAfter(anchors.OnExceptionStart, IteratorOnException(rouMethod, moveNextMethodDef, anchors.Rethrow, fields));

            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.FinallyStart, IteratorSaveYeildReturn(fields));
            instructions.InsertAfter(anchors.IfLastYeildStart, IteratorIfLastYeild(anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.IfHasExceptionStart, IteratorIfHasException(anchors.OnExitStart, fields));
            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.SaveReturnValueStart, IteratorSaveReturnValue(fields));
            instructions.InsertAfter(anchors.OnSuccessStart, IteratorOnSuccess(rouMethod, moveNextMethodDef, anchors.OnExitStart, fields));
            instructions.InsertAfter(anchors.OnExitStart, IteratorOnExit(rouMethod, moveNextMethodDef, anchors.EndFinally, fields));

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private IteratorFields IteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.METHOD_GetEnumerator_Prefix) && x.Name.EndsWith(Constants.METHOD_GetEnumerator_Suffix));

            var mosFieldDef = new FieldDefinition(Constants.FIELD_RougamoMos, FieldAttributes.Public, _typeIMoArrayRef);
            var contextFieldDef = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _typeMethodContextRef);
            var stateFieldDef = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var currentFieldDef = stateMachineTypeDef.Fields.Single(m => m.Name.EndsWith(Constants.FIELD_Current_Suffix));
            FieldDefinition? recordedReturnFieldDef = null;
            var parameterFieldDefs = StateMachineParameterFields(rouMethod);
            parameterFieldDefs = IteratorGetPrivateParameterFields(getEnumeratorMethodDef, parameterFieldDefs);
            if (this.ConfigRecordingIteratorReturnValue())
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                recordedReturnFieldDef = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateMachineTypeDef.Fields.Add(recordedReturnFieldDef);
            }

            stateMachineTypeDef.Fields.Add(mosFieldDef);
            stateMachineTypeDef.Fields.Add(contextFieldDef);

            return new IteratorFields(stateMachineTypeDef, mosFieldDef, contextFieldDef, stateFieldDef, currentFieldDef, recordedReturnFieldDef, parameterFieldDefs);
        }

        private IteratorVariables IteratorResolveVariables(RouMethod rouMethod, MethodDefinition moveNextMethodDef, TypeDefinition stateMachineTypeDef)
        {
            // maybe can simply replace to stateMachineTypeDef.ImportInto(ModuleDefinition)
            var stateMachineTypeRef = ((MethodReference)rouMethod.MethodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Newobj && x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == stateMachineTypeDef).Operand).DeclaringType;

            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);

            var exceptionVariable = moveNextMethodDef.Body.CreateVariable(_typeExceptionRef);
            var moveNextReturnVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);

            return new IteratorVariables(stateMachineVariable, exceptionVariable, moveNextReturnVariable);
        }

        private IteratorAnchors IteratorCreateAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, IteratorVariables variables)
        {
            var @return = methodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Ret);
            var tryStart = moveNextMethodDef.Body.Instructions.First();
            var finallyEnd = Create(OpCodes.Ldloc, variables.MoveNextReturn);

            return new IteratorAnchors(variables, @return, tryStart, finallyEnd);
        }

        private void IteratorSetAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, IteratorVariables variables, IteratorAnchors anchors)
        {
            methodDef.Body.Instructions.InsertBefore(anchors.Return, new[]
            {
                Create(OpCodes.Stloc, variables.StateMachine),
                anchors.InitMosStart,
                anchors.InitContextStart,
                anchors.InitRecordedReturnStart,
                Create(OpCodes.Ldloc, variables.StateMachine),
            });

            IteratorSetMoveNextAnchors(moveNextMethodDef, variables, anchors);
        }

        private void IteratorSetMoveNextAnchors(MethodDefinition moveNextMethodDef, IteratorVariables variables, IteratorAnchors anchors)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var returns = instructions.Where(ins => ins.IsRet()).ToArray();

            instructions.Insert(0, new[]
            {
                anchors.OnEntryStart,
                anchors.RewriteArgStart
            });

            instructions.Add(new[]
            {
                anchors.CatchStart,
                anchors.OnExceptionStart,
                anchors.Rethrow,
                anchors.FinallyStart,
                anchors.IfLastYeildStart,
                anchors.IfHasExceptionStart,
                anchors.SaveReturnValueStart,
                anchors.OnSuccessStart,
                anchors.OnExitStart,
                anchors.EndFinally,
                anchors.FinallyEnd,
                Create(OpCodes.Ret)
            });

            if (returns.Length != 0)
            {
                foreach (var @return in returns)
                {
                    @return.OpCode = OpCodes.Stloc;
                    @return.Operand = variables.MoveNextReturn;
                    instructions.InsertAfter(@return, Create(OpCodes.Leave, anchors.FinallyEnd));
                }
            }
        }

        private void IteratorSetTryCatchFinally(MethodDefinition methodDef, IteratorAnchors anchors)
        {
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = _typeExceptionRef,
                TryStart = anchors.TryStart,
                TryEnd = anchors.CatchStart,
                HandlerStart = anchors.CatchStart,
                HandlerEnd = anchors.FinallyStart
            };
            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = anchors.TryStart,
                TryEnd = anchors.FinallyStart,
                HandlerStart = anchors.FinallyStart,
                HandlerEnd = anchors.FinallyEnd
            };

            methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            methodDef.Body.ExceptionHandlers.Add(finallyHandler);
        }

        private IList<Instruction> IteratorInitMos(RouMethod rouMethod, IteratorFields fields, IteratorVariables variables)
        {
            var mosFieldRef = new FieldReference(fields.Mos.Name, fields.Mos.FieldType, variables.StateMachine.VariableType);

            var instructions = new List<Instruction>
            {
                variables.StateMachine.LdlocOrA()
            };
            instructions.AddRange(InitMosArray(rouMethod.Mos));
            instructions.Add(Create(OpCodes.Stfld, mosFieldRef));

            return instructions;
        }

        private IList<Instruction> IteratorInitMethodContext(RouMethod rouMethod, IteratorFields fields, IteratorVariables variables)
        {
            var mosFieldRef = new FieldReference(fields.Mos.Name, fields.Mos.FieldType, variables.StateMachine.VariableType);
            var contextFieldRef = new FieldReference(fields.MethodContext.Name, fields.MethodContext.FieldType, variables.StateMachine.VariableType);

            var instructions = new List<Instruction>
            {
                variables.StateMachine.LdlocOrA()
            };
            instructions.AddRange(InitMethodContext(rouMethod.MethodDef, true, false, null, variables.StateMachine, mosFieldRef));
            instructions.Add(Create(OpCodes.Stfld, contextFieldRef));

            return instructions;
        }

        private IList<Instruction> IteratorInitRecordedReturn(IteratorFields fields, IteratorVariables variables)
        {
            var returnsFieldRef = new FieldReference(fields.RecordedReturn!.Name, fields.RecordedReturn!.FieldType, variables.StateMachine.VariableType);
            var returnsTypeCtorDef = _typeListDef.GetZeroArgsCtor();
            var returnsTypeCtorRef = fields.RecordedReturn.FieldType.GenericTypeMethodReference(returnsTypeCtorDef, ModuleDefinition);

            return new[]
            {
                variables.StateMachine.LdlocOrA(),
                Create(OpCodes.Newobj, returnsTypeCtorRef),
                Create(OpCodes.Stfld, returnsFieldRef)
            };
        }

        private IList<Instruction> IteratorIfFirstTimeExecute(Instruction ifNotFirstTimeGoTo, IteratorFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.State),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Bne_Un, ifNotFirstTimeGoTo)
            };
        }

        private IList<Instruction> IteratorOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, IteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, false);
        }

        private IList<Instruction> IteratorRewriteArguments(Instruction endAnchor, IteratorFields fields)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRewriteArgumentsRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            for (var i = 0; i < fields.Parameters.Length; i++)
            {
                var parameterFieldRef = fields.Parameters[i];
                if (parameterFieldRef == null) continue;

                AsyncRewriteArgument(i, fields.MethodContext, parameterFieldRef, instructions.Add);
            }

            return instructions;
        }

        private IList<Instruction> IteratorSaveException(Instruction endAnchor, IteratorFields fields, IteratorVariables variables)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldloc, variables.Exception),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            };
        }

        private IList<Instruction> IteratorOnException(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, IteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> IteratorSaveYeildReturn(IteratorFields fields)
        {
            var listAddMethodRef = fields.RecordedReturn!.FieldType.GenericTypeMethodReference(_methodListAddRef, ModuleDefinition);

            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.RecordedReturn),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.Current),
                Create(OpCodes.Callvirt, listAddMethodRef)
            };
        }

        private IList<Instruction> IteratorIfLastYeild(Instruction ifNotLastYeildGoto, IteratorVariables variables)
        {
            return new[]
            {
                Create(OpCodes.Ldloc, variables.MoveNextReturn),
                Create(OpCodes.Brtrue_S, ifNotLastYeildGoto)
            };
        }

        private IList<Instruction> IteratorIfHasException(Instruction ifHasExceptionGoto, IteratorFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                Create(OpCodes.Brtrue_S, ifHasExceptionGoto)
            };
        }

        private IList<Instruction> IteratorSaveReturnValue(IteratorFields fields)
        {
            var listToArrayMethodRef = fields.RecordedReturn!.FieldType.GenericTypeMethodReference(_methodListToArrayRef, ModuleDefinition);
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.RecordedReturn),
                Create(OpCodes.Callvirt, listToArrayMethodRef),
                Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
            };
        }

        private IList<Instruction> IteratorOnSuccess(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, IteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> IteratorOnExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, IteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private FieldDefinition?[] IteratorGetPrivateParameterFields(MethodDefinition getEnumeratorMethodDef, FieldDefinition?[] parameterFieldDefs)
        {
            var map = new Dictionary<FieldDefinition, int>();
            for (var i = 0; i < parameterFieldDefs.Length; i++)
            {
                var parameterFieldDef = parameterFieldDefs[i];
                if (parameterFieldDef == null) continue;

                map.Add(parameterFieldDef, i);
            }

            foreach (var instruction in getEnumeratorMethodDef.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldfld && map.TryGetValue(((FieldReference)instruction.Operand).Resolve(), out var index))
                {
                    parameterFieldDefs[index] = ((FieldReference)instruction.Next.Operand).Resolve();
                }
            }

            return parameterFieldDefs;
        }
    }
}
