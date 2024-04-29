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

            if (_config.ProxyCalling)
            {
                ProxyIteratorMethodWeave(rouMethod, stateMachineTypeDef);
                return;
            }

            var moveNextMethodDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var moveNextMethodName = stateMachineTypeDef.DeclaringType.FullName;

            var fields = IteratorResolveFields(rouMethod, stateMachineTypeDef);
            var variables = IteratorResolveVariables(rouMethod, moveNextMethodDef, stateMachineTypeDef);
            var anchors = IteratorCreateAnchors(rouMethod.MethodDef, moveNextMethodDef, variables);
            IteratorSetAnchors(rouMethod, moveNextMethodDef, variables, anchors);
            SetTryCatchFinally(rouMethod.Features, moveNextMethodDef, anchors);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMos, StateMachineInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContext, StateMachineInitMethodContext(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitRecordedReturn, IteratorInitRecordedReturn(fields, variables));

            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertAfter(anchors.IfFirstTimeEntry, StateMachineIfFirstTimeEntry(rouMethod, 0, anchors.TryStart, fields));
            instructions.InsertAfter(anchors.OnEntry, StateMachineOnEntry(rouMethod, moveNextMethodDef, anchors.RewriteArg, fields));
            instructions.InsertAfter(anchors.RewriteArg, StateMachineRewriteArguments(rouMethod, anchors.TryStart, fields));

            instructions.InsertAfter(anchors.CatchStart, IteratorSaveException(rouMethod, fields, variables));
            instructions.InsertAfter(anchors.OnExceptionRefreshArgs, StateMachineOnExceptionRefreshArgs(rouMethod, fields));
            instructions.InsertAfter(anchors.OnException, StateMachineOnException(rouMethod, moveNextMethodDef, anchors.OnExitAfterException, fields));
            instructions.InsertAfter(anchors.OnExitAfterException, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.Rethrow, fields));

            instructions.InsertAfter(anchors.FinallyStart, IteratorSaveYeildReturn(rouMethod, fields));
            instructions.InsertAfter(anchors.IfLastYeild, IteratorIfLastYeild(rouMethod, anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.IfHasException, IteratorIfHasException(rouMethod, anchors.EndFinally, fields));
            instructions.InsertAfter(anchors.SaveReturnValue, IteratorSaveReturnValue(rouMethod, fields));
            instructions.InsertAfter(anchors.OnSuccessRefreshArgs, StateMachineOnSuccessRefreshArgs(rouMethod, fields));
            instructions.InsertAfter(anchors.OnSuccess, StateMachineOnSuccess(rouMethod, moveNextMethodDef, anchors.OnExitAfterSuccess, fields));
            instructions.InsertAfter(anchors.OnExitAfterSuccess, StateMachineOnExit(rouMethod, moveNextMethodDef, anchors.EndFinally, fields));

            rouMethod.MethodDef.Body.InitLocals = true;
            moveNextMethodDef.Body.InitLocals = true;

            rouMethod.MethodDef.Body.OptimizePlus(EmptyInstructions);
            moveNextMethodDef.Body.OptimizePlus(anchors.GetNops());
        }

        private IteratorFields IteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.GenericPrefix(Constants.TYPE_IEnumerable)) && x.Name.EndsWith(Constants.GenericSuffix(Constants.METHOD_GetEnumerator)));

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
                    mos[i] = new FieldDefinition(Constants.FIELD_RougamoMo_Prefix + i, FieldAttributes.Public, rouMethod.Mos[i].MoTypeDef.ImportInto(ModuleDefinition));
                    stateMachineTypeDef.Fields.Add(mos[i]);
                }
            }
            var methodContext = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _typeMethodContextRef);
            var state = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var current = stateMachineTypeDef.Fields.Single(m => m.Name.EndsWith(Constants.FIELD_Current_Suffix));
            var initialThreadId = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_InitialThreadId);
            var declaringThis = stateMachineTypeDef.Fields.SingleOrDefault(x => x.FieldType.Resolve() == stateMachineTypeDef.DeclaringType);
            FieldDefinition? recordedReturn = null;
            var transitParameters = StateMachineParameterFields(rouMethod);
            var parameters = IteratorGetPrivateParameterFields(getEnumeratorMethodDef, transitParameters);
            if (_config.RecordingIteratorReturns && (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) != 0 && (rouMethod.MethodContextOmits & Omit.ReturnValue) == 0)
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                recordedReturn = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateMachineTypeDef.Fields.Add(recordedReturn);
            }

            stateMachineTypeDef.Fields.Add(methodContext);

            return new IteratorFields(stateMachineTypeDef, moArray, mos, methodContext, state, current, initialThreadId, recordedReturn, declaringThis, transitParameters, parameters);
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

        private void IteratorSetAnchors(RouMethod rouMethod, MethodDefinition moveNextMethodDef, IteratorVariables variables, IteratorAnchors anchors)
        {
            rouMethod.MethodDef.Body.Instructions.InsertBefore(anchors.HostsReturn, new[]
            {
                Create(OpCodes.Stloc, variables.StateMachine),
                anchors.InitMos,
                anchors.InitContext,
                anchors.InitRecordedReturn,
                Create(OpCodes.Ldloc, variables.StateMachine),
            });

            IteratorSetMoveNextAnchors(rouMethod, moveNextMethodDef, variables, anchors);
        }

        private void IteratorSetMoveNextAnchors(RouMethod rouMethod, MethodDefinition moveNextMethodDef, IteratorVariables variables, IteratorAnchors anchors)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var returns = instructions.Where(ins => ins.IsRet()).ToArray();

            instructions.Insert(0, new[]
            {
                anchors.IfFirstTimeEntry,
                anchors.OnEntry,
                anchors.RewriteArg
            });

            var brCode = OpCodes.Br;

            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnSuccess | Feature.OnExit)) != 0)
            {
                brCode = OpCodes.Leave;
                instructions.Add(new[]
                {
                    anchors.CatchStart,
                    anchors.OnExceptionRefreshArgs,
                    anchors.OnException,
                    anchors.OnExitAfterException,
                    anchors.Rethrow
                });
            }
            instructions.Add(anchors.FinallyStart);
            if ((rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) != 0)
            {
                brCode = OpCodes.Leave;
                instructions.Add(new[]
                {
                    anchors.IfLastYeild,
                    anchors.IfHasException,
                    anchors.SaveReturnValue,
                    anchors.OnSuccessRefreshArgs,
                    anchors.OnSuccess,
                    anchors.OnExitAfterSuccess,
                    anchors.EndFinally,
                });
            }
            instructions.Add(anchors.FinallyEnd);
            instructions.Add(Create(OpCodes.Ret));

            if (returns.Length != 0)
            {
                foreach (var @return in returns)
                {
                    @return.OpCode = OpCodes.Stloc;
                    @return.Operand = variables.MoveNextReturn;
                    instructions.InsertAfter(@return, Create(brCode, anchors.FinallyEnd));
                }
            }
        }

        private IList<Instruction> IteratorInitRecordedReturn(IIteratorFields fields, IIteratorVariables variables)
        {
            if (fields.RecordedReturn == null) return EmptyInstructions;

            var returnsFieldRef = new FieldReference(fields.RecordedReturn!.Name, fields.RecordedReturn!.FieldType, variables.StateMachine.VariableType);
            var returnsTypeCtorDef = _typeListDef.GetZeroArgsCtor();
            var returnsTypeCtorRef = returnsTypeCtorDef.WithGenericDeclaringType(fields.RecordedReturn.FieldType);

            return new[]
            {
                variables.StateMachine.LdlocOrA(),
                Create(OpCodes.Newobj, returnsTypeCtorRef),
                Create(OpCodes.Stfld, returnsFieldRef)
            };
        }

        private IList<Instruction> IteratorSaveException(RouMethod rouMethod, IteratorFields fields, IteratorVariables variables)
        {
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnSuccess | Feature.OnExit)) == 0) return EmptyInstructions;

            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldloc, variables.Exception),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            };
        }

        private IList<Instruction>? IteratorSaveYeildReturn(RouMethod rouMethod, IIteratorFields fields)
        {
            if (fields.RecordedReturn == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var listAddMethodRef = _methodListAddRef.WithGenericDeclaringType(fields.RecordedReturn!.FieldType);

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.RecordedReturn),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.Current),
                Create(OpCodes.Callvirt, listAddMethodRef)
            ];
        }

        private IList<Instruction> IteratorIfLastYeild(RouMethod rouMethod, Instruction ifNotLastYeildGoto, IteratorVariables variables)
        {
            if ((rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0) return EmptyInstructions;

            return new[]
            {
                Create(OpCodes.Ldloc, variables.MoveNextReturn),
                Create(OpCodes.Brtrue_S, ifNotLastYeildGoto)
            };
        }

        private IList<Instruction> IteratorIfHasException(RouMethod rouMethod, Instruction ifHasExceptionGoto, IteratorFields fields)
        {
            if ((rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0) return EmptyInstructions;

            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                Create(OpCodes.Brtrue_S, ifHasExceptionGoto)
            };
        }

        private IList<Instruction>? IteratorSaveReturnValue(RouMethod rouMethod, IIteratorFields fields)
        {
            if (fields.RecordedReturn == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var listToArrayMethodRef = _methodListToArrayRef.WithGenericDeclaringType(fields.RecordedReturn!.FieldType);

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.RecordedReturn),
                Create(OpCodes.Callvirt, listToArrayMethodRef),
                Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
            ];
        }

        private FieldDefinition?[] IteratorGetPrivateParameterFields(MethodDefinition getEnumeratorMethodDef, FieldDefinition?[] transitParameterFieldDefs)
        {
            if (transitParameterFieldDefs.Length == 0) return [];

            var parameters = new FieldDefinition?[transitParameterFieldDefs.Length];
            var map = new Dictionary<FieldDefinition, int>();
            for (var i = 0; i < transitParameterFieldDefs.Length; i++)
            {
                var parameterFieldDef = transitParameterFieldDefs[i];
                if (parameterFieldDef == null) continue;

                map.Add(parameterFieldDef, i);
            }

            foreach (var instruction in getEnumeratorMethodDef.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldfld && map.TryGetValue(((FieldReference)instruction.Operand).Resolve(), out var index))
                {
                    parameters[index] = ((FieldReference)instruction.Next.Operand).Resolve();
                }
            }

            return parameters;
        }
    }
}
