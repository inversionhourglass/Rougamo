using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Iterator;
using System.Collections.Generic;
using System.Linq;
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
            SetTryCatchFinally(moveNextMethodDef, anchors);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMos, StateMachineInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContext, StateMachineInitMethodContext(rouMethod, fields, variables));
            if (fields.RecordedReturn != null) rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitRecordedReturn, IteratorInitRecordedReturn(fields, variables));

            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertAfter(anchors.IfFirstTimeEntry, StateMachineIfFirstTimeEntry(0, anchors.TryStart, fields));
            instructions.InsertAfter(anchors.OnEntry, StateMachineOnEntry(rouMethod, moveNextMethodDef, anchors.RewriteArg, fields));
            if (rouMethod.MethodDef.Parameters.Count > 0) instructions.InsertAfter(anchors.RewriteArg, StateMachineRewriteArguments(anchors.TryStart, fields));

            instructions.InsertAfter(anchors.CatchStart, IteratorSaveException(fields, variables));
            instructions.InsertAfter(anchors.OnException, StateMachineOnException(rouMethod, moveNextMethodDef, anchors.OnExitAfterException, fields));
            instructions.InsertAfter(anchors.OnExitAfterException, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.Rethrow, fields));

            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.FinallyStart, IteratorSaveYeildReturn(fields));
            instructions.InsertAfter(anchors.IfLastYeild, IteratorIfLastYeild(anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.IfHasException, IteratorIfHasException(anchors.OnExitAfterSuccess, fields));
            if (fields.RecordedReturn != null) instructions.InsertAfter(anchors.SaveReturnValue, IteratorSaveReturnValue(fields));
            instructions.InsertAfter(anchors.OnSuccess, StateMachineOnSuccess(rouMethod, moveNextMethodDef, anchors.OnExitAfterSuccess, fields));
            instructions.InsertAfter(anchors.OnExitAfterSuccess, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.EndFinally, fields));

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private IteratorFields IteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.METHOD_GetEnumerator_Prefix) && x.Name.EndsWith(Constants.METHOD_GetEnumerator_Suffix));

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

            return new IteratorFields(stateMachineTypeDef, moArray, mos, methodContext, state, current, recordedReturn, parameters);
        }

        private IteratorVariables IteratorResolveVariables(RouMethod rouMethod, MethodDefinition moveNextMethodDef, TypeDefinition stateMachineTypeDef)
        {
            // Can not replace to stateMachineTypeDef.ImportInto(ModuleDefinition)
            var stateMachineTypeRef = ((MethodReference)rouMethod.MethodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Newobj && x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == stateMachineTypeDef).Operand).DeclaringType;

            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);

            var exceptionVariable = moveNextMethodDef.Body.CreateVariable(_typeExceptionRef);
            var moveNextReturnVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);

            return new IteratorVariables(stateMachineVariable, exceptionVariable, moveNextReturnVariable);
        }

        private IteratorAnchors IteratorCreateAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, IteratorVariables variables)
        {
            var hostsReturn = methodDef.Body.Instructions.Single(x => x.OpCode.Code == Code.Ret);
            var tryStart = moveNextMethodDef.Body.Instructions.First();
            var finallyEnd = Create(OpCodes.Ldloc, variables.MoveNextReturn);

            return new IteratorAnchors(variables, hostsReturn, tryStart, finallyEnd);
        }

        private void IteratorSetAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, IteratorVariables variables, IteratorAnchors anchors)
        {
            methodDef.Body.Instructions.InsertBefore(anchors.HostsReturn, new[]
            {
                Create(OpCodes.Stloc, variables.StateMachine),
                anchors.InitMos,
                anchors.InitContext,
                anchors.InitRecordedReturn,
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
                anchors.IfFirstTimeEntry,
                anchors.OnEntry,
                anchors.RewriteArg
            });

            instructions.Add(new[]
            {
                anchors.CatchStart,
                anchors.OnException,
                anchors.OnExitAfterException,
                anchors.Rethrow,
                anchors.FinallyStart,
                anchors.IfLastYeild,
                anchors.IfHasException,
                anchors.SaveReturnValue,
                anchors.OnSuccess,
                anchors.OnExitAfterSuccess,
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

        private IList<Instruction> IteratorInitRecordedReturn(IIteratorFields fields, IIteratorVariables variables)
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

        private IList<Instruction> IteratorSaveException(IteratorFields fields, IteratorVariables variables)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldloc, variables.Exception),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            };
        }

        private IList<Instruction> IteratorSaveYeildReturn(IIteratorFields fields)
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

        private IList<Instruction> IteratorSaveReturnValue(IIteratorFields fields)
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

        private FieldDefinition?[] IteratorGetPrivateParameterFields(MethodDefinition getEnumeratorMethodDef, FieldDefinition?[] parameterFieldDefs)
        {
            if (parameterFieldDefs.Length == 0) return parameterFieldDefs;

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
