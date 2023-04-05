using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances.AsyncIterator;
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

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMos, StateMachineInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContext, StateMachineInitMethodContext(rouMethod, fields, variables));
            if (fields.RecordedReturn != null) rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitRecordedReturn, IteratorInitRecordedReturn(fields, variables));

            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertAfter(anchors.IfFirstTimeEntry, StateMachineIfFirstTimeEntry(rouMethod, 0, anchors.HostsStart, fields));
            instructions.InsertAfter(anchors.OnEntry, StateMachineOnEntry(rouMethod, moveNextMethodDef, anchors.RewriteArg, fields));
            instructions.InsertAfter(anchors.RewriteArg, StateMachineRewriteArguments(rouMethod, anchors.HostsStart, fields));

            instructions.InsertAfter(anchors.SaveException, StateMachineSaveException(rouMethod, moveNextMethodName, anchors.HostsCatchStart, fields));
            instructions.InsertAfter(anchors.OnException, StateMachineOnException(rouMethod, moveNextMethodDef, anchors.OnExitAfterException, fields));
            instructions.InsertAfter(anchors.OnExitAfterException, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.HostsSetException, fields));

            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.SaveReturnValue, IteratorSaveReturnValue(fields));
            instructions.InsertAfter(anchors.OnSuccess, StateMachineOnSuccess(rouMethod, moveNextMethodDef, anchors.OnExitAfterSuccess, fields));
            instructions.InsertAfter(anchors.OnExitAfterSuccess, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.FinishSetResultLdarg0, fields));

            if (fields.RecordedReturn != null && anchors.YieldReturn != null) instructions.InsertAfter(anchors.SaveYieldReturnValue, IteratorSaveYeildReturn(fields));

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private AiteratorFields AiteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.METHOD_GetAsyncEnumerator_Prefix) && x.Name.EndsWith(Constants.METHOD_GetAsyncEnumerator_Suffix));

            FieldDefinition? moArray = null;
            var mos = new FieldDefinition[0];
            if (rouMethod.Mos.Length >= _config.MoArrayThreshold)
            {
                moArray = new FieldDefinition(Constants.FIELD_RougamoMos, FieldAttributes.Public, _typeIMoArrayRef);
                stateMachineTypeDef.Fields.Add(moArray);
            }
            else
            {
                mos = new FieldDefinition[rouMethod.Mos.Length];
                for (int i = 0; i < rouMethod.Mos.Length; i++)
                {
                    mos[i] = new FieldDefinition(Constants.FIELD_RougamoMo_Prefix + i, FieldAttributes.Public, _typeIMoRef);
                    stateMachineTypeDef.Fields.Add(mos[i]);
                }
            }
            var methodContext = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _typeMethodContextRef);
            var state = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var current = stateMachineTypeDef.Fields.Single(m => m.Name.EndsWith(Constants.FIELD_Current_Suffix));
            FieldDefinition? recordedReturn = null;
            var parameters = StateMachineParameterFields(rouMethod);
            parameters = IteratorGetPrivateParameterFields(getEnumeratorMethodDef, parameters);
            if (_config.RecordingIteratorReturns)
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                recordedReturn = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateMachineTypeDef.Fields.Add(recordedReturn);
            }

            stateMachineTypeDef.Fields.Add(methodContext);

            return new AiteratorFields(stateMachineTypeDef, moArray, mos, methodContext, state, current, recordedReturn, parameters);
        }

        private AiteratorVariables AiteratorResolveVariables(RouMethod rouMethod, MethodDefinition moveNextMethodDef, TypeDefinition stateMachineTypeDef)
        {
            // Can not replace to stateMachineTypeDef.ImportInto(ModuleDefinition)
            var stateMachineTypeRef = ((MethodReference)rouMethod.MethodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Newobj && x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == stateMachineTypeDef).Operand).DeclaringType;

            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);

            var moveNextReturnVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);

            return new AiteratorVariables(stateMachineVariable, moveNextReturnVariable);
        }

        private AiteratorAnchors AiteratorCreateAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef)
        {
            var hostsReturn = methodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Ret);

            var exceptionHandler = GetOuterExceptionHandler(moveNextMethodDef);

            var hostsStart = moveNextMethodDef.Body.Instructions.First();
            var hostsCatchStart = exceptionHandler.HandlerStart;
            var hostsSetException = AsyncGetSetExceptionStartAnchor(moveNextMethodDef, exceptionHandler);
            var leaveCatch = AsyncGetLeaveCatchAnchor(exceptionHandler);
            var setResults = moveNextMethodDef.Body.Instructions.Where(x => (x.OpCode.Code == Code.Call || x.OpCode.Code == Code.Callvirt) && x.Operand is MethodReference methodRef && methodRef.DeclaringType.FullName == Constants.TYPE_ManualResetValueTaskSourceCore_Bool && methodRef.Name == Constants.METHOD_SetResult).ToArray();
            if (setResults.Length > 2 || setResults.Length < 1) throw new RougamoException($"[{moveNextMethodDef.FullName}] find {setResults.Length} call SetResult instruction(s), except [1, 2].");

            var finishReturn = setResults.Single(x => x.Previous.OpCode.Code == Code.Ldc_I4_0).Previous.Previous.Previous;
            var yieldReturn = setResults.SingleOrDefault(x => x.Previous.OpCode.Code == Code.Ldc_I4_1)?.Previous.Previous.Previous;
            if (yieldReturn != null && yieldReturn.OpCode.Code != Code.Ldarg_0 || finishReturn.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"[{moveNextMethodDef.FullName}] SetResult start is not ldarg0");

            finishReturn.OpCode = OpCodes.Nop;
            if (yieldReturn != null) yieldReturn.OpCode = OpCodes.Nop;

            return new AiteratorAnchors(hostsReturn, hostsStart, hostsCatchStart, hostsSetException, finishReturn, yieldReturn);
        }

        private void AiteratorSetAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, AiteratorVariables variables, AiteratorAnchors anchors)
        {
            methodDef.Body.Instructions.InsertBefore(anchors.HostsReturn, new[]
            {
                Create(OpCodes.Stloc, variables.StateMachine),
                anchors.InitMos,
                anchors.InitContext,
                anchors.InitRecordedReturn,
                Create(OpCodes.Ldloc, variables.StateMachine),
            });

            AiteratorSetMoveNextAnchors(moveNextMethodDef, anchors);
        }

        private void AiteratorSetMoveNextAnchors(MethodDefinition moveNextMethodDef, AiteratorAnchors anchors)
        {
            var instructions = moveNextMethodDef.Body.Instructions;

            instructions.Insert(0, new[]
            {
                anchors.IfFirstTimeEntry,
                anchors.OnEntry,
                anchors.RewriteArg
            });

            instructions.InsertBefore(anchors.HostsSetException, new[]
            {
                anchors.SaveException,
                anchors.OnException,
                anchors.OnExitAfterException
            });

            instructions.InsertAfter(anchors.FinishReturn, new[]
            {
                anchors.SaveReturnValue,
                anchors.OnSuccess,
                anchors.OnExitAfterSuccess,
                anchors.FinishSetResultLdarg0
            });

            if (anchors.YieldReturn != null)
            {
                instructions.InsertAfter(anchors.YieldReturn, new[]
                {
                    anchors.SaveYieldReturnValue,
                    anchors.YieldSetResultLdarg0
                });
            }
        }
    }
}
