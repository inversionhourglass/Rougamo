using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void AiteratorMethodWeave(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_AsyncIteratorStateMachineAttribute);
            var moveNextMethodDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var moveNextMethodName = stateMachineTypeDef.DeclaringType.FullName;

            var fields = AiteratorResolveFields(rouMethod, stateMachineTypeDef);
            var variables = AiteratorResolveVariables(rouMethod, moveNextMethodDef, stateMachineTypeDef);
            var anchors = AiteratorCreateAnchors(rouMethod.MethodDef, moveNextMethodDef);
            AiteratorSetAnchors(rouMethod.MethodDef, moveNextMethodDef, variables, anchors);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMosStart, AiteratorInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContextStart, AiteratorInitMethodContext(rouMethod, fields, variables));
            if (fields.RecordedReturn != null) rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitRecordedReturnStart, AiteratorInitRecordedReturn(fields, variables));

            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.Insert(0, AiteratorIfFirstTimeExecute(anchors.OriginStart, fields));
            instructions.InsertAfter(anchors.OnEntryStart, AiteratorOnEntry(rouMethod, moveNextMethodDef, anchors.RewriteArgStart, fields));
            instructions.InsertAfter(anchors.RewriteArgStart, AiteratorRewriteArguments(anchors.OriginStart, fields));

            instructions.InsertAfter(anchors.SaveException, AiteratorSaveException(moveNextMethodName, anchors.CatchStart, fields));
            instructions.InsertAfter(anchors.OnExceptionStart, AiteratorOnException(rouMethod, moveNextMethodDef, anchors.OnExitAfterExceptionStart, fields));
            instructions.InsertAfter(anchors.OnExitAfterExceptionStart, AiteratorOnExit(rouMethod, moveNextMethodDef, anchors.SetExceptionStart, fields));

            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.SaveReturnValueStart, AiteratorSaveReturnValue(fields));
            instructions.InsertAfter(anchors.OnSuccessStart, AiteratorOnSuccess(rouMethod, moveNextMethodDef, anchors.OnExitAfterSuccessStart, fields));
            instructions.InsertAfter(anchors.OnExitAfterSuccessStart, AiteratorOnExit(rouMethod, moveNextMethodDef, anchors.FinishSetResultStart, fields));

            if (fields.RecordedReturn != null && anchors.YieldReturnStart != null) instructions.InsertAfter(anchors.SaveYieldReturnValueStart, AiteratorSaveYeildReturn(fields));

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private IList<Instruction> AiteratorInitMos(RouMethod rouMethod, AiteratorFields fields, AiteratorVariables variables)
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

        private IList<Instruction> AiteratorInitMethodContext(RouMethod rouMethod, AiteratorFields fields, AiteratorVariables variables)
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

        private IList<Instruction> AiteratorInitRecordedReturn(AiteratorFields fields, AiteratorVariables variables)
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

        private AiteratorFields AiteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.METHOD_GetAsyncEnumerator_Prefix) && x.Name.EndsWith(Constants.METHOD_GetAsyncEnumerator_Suffix));

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

            return new AiteratorFields(stateMachineTypeDef, mosFieldDef, contextFieldDef, stateFieldDef, currentFieldDef, recordedReturnFieldDef, parameterFieldDefs);
        }

        private AiteratorVariables AiteratorResolveVariables(RouMethod rouMethod, MethodDefinition moveNextMethodDef, TypeDefinition stateMachineTypeDef)
        {
            // maybe can simply replace to stateMachineTypeDef.ImportInto(ModuleDefinition)
            var stateMachineTypeRef = ((MethodReference)rouMethod.MethodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Newobj && x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == stateMachineTypeDef).Operand).DeclaringType;

            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);

            var moveNextReturnVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);

            return new AiteratorVariables(stateMachineVariable, moveNextReturnVariable);
        }

        private AiteratorAnchors AiteratorCreateAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef)
        {
            var @return = methodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Ret);

            var exceptionHandler = GetOuterExceptionHandler(moveNextMethodDef.Body);

            var originStart = moveNextMethodDef.Body.Instructions.First();
            var catchStart = exceptionHandler.HandlerStart;
            var setExceptionStart = AsyncGetSetExceptionStartAnchor(moveNextMethodDef, exceptionHandler);
            var leaveCatch = AsyncGetLeaveCatchAnchor(exceptionHandler);
            var setResults = moveNextMethodDef.Body.Instructions.Where(x => (x.OpCode.Code == Code.Call || x.OpCode.Code == Code.Callvirt) && x.Operand is MethodReference methodRef && methodRef.DeclaringType.FullName == Constants.TYPE_ManualResetValueTaskSourceCore_Bool && methodRef.Name == Constants.METHOD_SetResult).ToArray();
            if (setResults.Length > 2 || setResults.Length < 1) throw new RougamoException($"[{moveNextMethodDef.FullName}] find {setResults.Length} call SetResult instruction(s), except [1, 2].");

            var finishReturnStart = setResults.Single(x => x.Previous.OpCode.Code == Code.Ldc_I4_0).Previous.Previous.Previous;
            var yieldReturnStart = setResults.SingleOrDefault(x => x.Previous.OpCode.Code == Code.Ldc_I4_1)?.Previous.Previous.Previous;
            if (yieldReturnStart != null && yieldReturnStart.OpCode.Code != Code.Ldarg_0 || finishReturnStart.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"[{moveNextMethodDef.FullName}] SetResult start is not ldarg0");

            finishReturnStart.OpCode = OpCodes.Nop;
            if (yieldReturnStart != null) yieldReturnStart.OpCode = OpCodes.Nop;

            return new AiteratorAnchors(@return, originStart, catchStart, setExceptionStart, finishReturnStart, yieldReturnStart);
        }

        private void AiteratorSetAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, AiteratorVariables variables, AiteratorAnchors anchors)
        {
            methodDef.Body.Instructions.InsertBefore(anchors.Return, new[]
            {
                Create(OpCodes.Stloc, variables.StateMachine),
                anchors.InitMosStart,
                anchors.InitContextStart,
                anchors.InitRecordedReturnStart,
                Create(OpCodes.Ldloc, variables.StateMachine),
            });

            AiteratorSetMoveNextAnchors(moveNextMethodDef, anchors);
        }

        private void AiteratorSetMoveNextAnchors(MethodDefinition moveNextMethodDef, AiteratorAnchors anchors)
        {
            var instructions = moveNextMethodDef.Body.Instructions;

            instructions.Insert(0, new[]
            {
                anchors.OnEntryStart,
                anchors.RewriteArgStart
            });

            instructions.InsertBefore(anchors.SetExceptionStart, new[]
            {
                anchors.SaveException,
                anchors.OnExceptionStart,
                anchors.OnExitAfterExceptionStart
            });

            instructions.InsertAfter(anchors.FinishReturnStart, new[]
            {
                anchors.SaveReturnValueStart,
                anchors.OnSuccessStart,
                anchors.OnExitAfterSuccessStart,
                anchors.FinishSetResultStart
            });

            if (anchors.YieldReturnStart != null)
            {
                instructions.InsertAfter(anchors.YieldReturnStart, new[]
                {
                    anchors.SaveYieldReturnValueStart,
                    anchors.YieldSetResultStart
                });
            }
        }

        private IList<Instruction> AiteratorIfFirstTimeExecute(Instruction retryStart, AiteratorFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.State),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Bne_Un, retryStart)
            };
        }

        private IList<Instruction> AiteratorOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AiteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, false);
        }

        private IList<Instruction> AiteratorRewriteArguments(Instruction endAnchor, AiteratorFields fields)
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

                AsyncRewriteArgument(i, fields.MethodContext, parameterFieldRef, instruction => instructions.Add(instruction));
            }

            return instructions;
        }

        private IList<Instruction> AiteratorSaveException(string methodName, Instruction stlocException, AiteratorFields fields)
        {
            var ldlocException = stlocException.Stloc2Ldloc($"{methodName} exception handler first instruction is not stloc.* exception");

            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                ldlocException,
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            };
        }

        private IList<Instruction> AiteratorOnException(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AiteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AiteratorOnExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AiteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AiteratorSaveReturnValue(AiteratorFields fields)
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

        private IList<Instruction> AiteratorOnSuccess(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AiteratorFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AiteratorSaveYeildReturn(AiteratorFields fields)
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
    }
}
