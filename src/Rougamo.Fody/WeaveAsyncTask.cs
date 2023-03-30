using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void AsyncTaskMethodWeave(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_AsyncStateMachineAttribute);
            var moveNextMethodDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var moveNextMethodName = stateMachineTypeDef.DeclaringType.FullName;

            var fields = AsyncResolveFields(rouMethod, stateMachineTypeDef);
            var returnBoxTypeRef = AsyncResolveReturnBoxTypeRef(rouMethod.MethodDef.ReturnType, fields.Builder);
            var variables = AsyncResolveVariables(rouMethod.MethodDef, moveNextMethodDef, stateMachineTypeDef, returnBoxTypeRef);
            var anchors = AsyncCreateAnchors(rouMethod.MethodDef, moveNextMethodDef, variables);
            AsyncSetAnchors(rouMethod.MethodDef, moveNextMethodDef, anchors);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitMosStart, AsyncInitMos(rouMethod, fields, variables));
            rouMethod.MethodDef.Body.Instructions.InsertAfter(anchors.InitContextStart, AsyncInitMethodContext(rouMethod, fields, variables));

            var setResultMethodRef = variables.ReplacedReturn == null ? GetSetResult(fields.Builder.FieldType) : GetGenericSetResult(fields.Builder.FieldType);

            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.Insert(0, AsyncIfFirstTimeExecute(anchors.RetryStart, fields));
            instructions.InsertAfter(anchors.OnEntryStart, AsyncOnEntry(rouMethod, moveNextMethodDef, anchors.IfEntryReplacedStart, fields));
            instructions.InsertAfter(anchors.IfEntryReplacedStart, AsyncIfOnEntryReplacedReturn(rouMethod, moveNextMethodDef, returnBoxTypeRef, setResultMethodRef, anchors.RewriteArgStart, fields, variables));
            instructions.InsertAfter(anchors.RewriteArgStart, AsyncRewriteArguments(anchors.RetryStart, fields));

            instructions.InsertAfter(anchors.SaveException, AsyncSaveException(moveNextMethodName, anchors.CatchStart, fields));
            instructions.InsertAfter(anchors.OnExceptionStart, AsyncOnException(rouMethod, moveNextMethodDef, anchors.IfExceptionRetryStart, fields));
            instructions.InsertAfter(anchors.IfExceptionRetryStart, AsyncIfExceptionRetry(anchors.RetryStart, anchors.ExceptionContextStashStart, fields));
            instructions.InsertAfter(anchors.ExceptionContextStashStart, AsyncExceptionContextStash(fields, variables));
            instructions.InsertAfter(anchors.OnExitAfterExceptionHandledStart, AsyncOnExit(rouMethod, moveNextMethodDef, anchors.IfExceptionHandled, fields));
            instructions.InsertAfter(anchors.IfExceptionHandled, AsyncIfExceptionHandled(returnBoxTypeRef, setResultMethodRef, anchors.SetExceptionStart, anchors.LeaveCatch, fields, variables));

            if (anchors.BuilderSetResultStart != null)
            {
                var notVoid = anchors.LdlocReturn != null;
                if (notVoid) instructions.InsertAfter(anchors.SaveReturnValueStart, AsyncSaveReturnValue(returnBoxTypeRef, anchors.LdlocReturn!, fields));
                instructions.InsertAfter(anchors.OnSuccessStart, AsyncOnSuccess(rouMethod, moveNextMethodDef, anchors.IfSuccessRetryStart, fields));
                instructions.InsertAfter(anchors.IfSuccessRetryStart, AsyncIfSuccessRetry(anchors.RetryStart, anchors.IfSuccessReplacedStart, fields));
                if (notVoid) instructions.InsertAfter(anchors.IfSuccessReplacedStart, AsyncIfSuccessReplacedReturn(moveNextMethodName, returnBoxTypeRef, anchors.LdlocReturn!, anchors.OnExitAfterSuccessExecutedStart, fields));
                instructions.InsertAfter(anchors.OnExitAfterSuccessExecutedStart, AsyncOnExit(rouMethod, moveNextMethodDef, anchors.BuilderSetResultStart, fields));
            }

            moveNextMethodDef.Body.OptimizePlus();
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private AsyncFields AsyncResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var mosFieldDef = new FieldDefinition(Constants.FIELD_RougamoMos, FieldAttributes.Public, _typeIMoArrayRef);
            var contextFieldDef = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _typeMethodContextRef);
            var builderFieldDef = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_Builder);
            var stateFieldDef = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var parameterFieldDefs = StateMachineParameterFields(rouMethod);

            stateMachineTypeDef.Fields.Add(mosFieldDef);
            stateMachineTypeDef.Fields.Add(contextFieldDef);

            return new AsyncFields(
                        stateMachineTypeDef: stateMachineTypeDef,
                        mos: mosFieldDef, methodContext: contextFieldDef,
                        state: stateFieldDef, builder: builderFieldDef,
                        parameters: parameterFieldDefs);
        }

        private BoxTypeReference AsyncResolveReturnBoxTypeRef(TypeReference returnTypeRef, FieldReference builderField)
        {
            return new BoxTypeReference(returnTypeRef.IsTask() || returnTypeRef.IsValueTask() || returnTypeRef.IsVoid() ? _typeVoidRef : ((GenericInstanceType)builderField.FieldType).GenericArguments[0]);
        }

        private AsyncVariables AsyncResolveVariables(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, TypeDefinition stateMachineTypeDef, BoxTypeReference returnBoxTypeRef)
        {
            var stateMachineVariable = methodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var replacedReturnVariable = (TypeReference)returnBoxTypeRef == _typeVoidRef ? null : moveNextMethodDef.Body.CreateVariable(_typeObjectRef);
            var exceptionHandledVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);

            return new AsyncVariables(stateMachineVariable, replacedReturnVariable, exceptionHandledVariable);
        }

        private AsyncAnchors AsyncCreateAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, AsyncVariables variables)
        {
            var exceptionHandler = GetOuterExceptionHandler(moveNextMethodDef.Body);

            var builderCreateStart = AsyncGetBuilderCreateStartAnchor(methodDef.Body, out var builderTypeDef);
            var catchStart = exceptionHandler.HandlerStart;
            var setExceptionStart = AsyncGetSetExceptionStartAnchor(moveNextMethodDef, exceptionHandler);
            var leaveCatch = AsyncGetLeaveCatchAnchor(exceptionHandler);
            var setResult = moveNextMethodDef.Body.Instructions.SingleOrDefault(x => x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == builderTypeDef && methodRef.Name == Constants.METHOD_SetResult);
            Instruction? setResultStart, ldlocReturn;
            if (setResult == null)
            {
                setResultStart = null;
                ldlocReturn = null;
            }
            else if (variables.ReplacedReturn == null)
            {
                setResultStart = setResult.Previous.Previous;
                ldlocReturn = null;
            }
            else
            {
                setResultStart = setResult.Previous.Previous.Previous;
                ldlocReturn = setResult.Previous;
            }

            return new AsyncAnchors(builderCreateStart, catchStart, setExceptionStart, leaveCatch, setResultStart, ldlocReturn);
        }

        private Instruction AsyncGetBuilderCreateStartAnchor(MethodBody methodBody, out TypeDefinition builderTypeDef)
        {
            foreach (var instruction in methodBody.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference methodRef &&
                    methodRef.DeclaringType.FullName.StartsWithAny(Constants.TYPE_AsyncTaskMethodBuilder, Constants.TYPE_AsyncValueTaskMethodBuilder, Constants.TYPE_AsyncVoidMethodBuilder) &&
                    methodRef.Name == Constants.METHOD_Create)
                {
                    builderTypeDef = methodRef.DeclaringType.Resolve();

                    return instruction.Previous;
                }
            }

            throw new RougamoException("unable find call AsyncTaskMethodBuilder.Create instruction");
        }

        private Instruction AsyncGetSetExceptionStartAnchor(MethodDefinition moveNextMethodDef, ExceptionHandler exceptionHandler)
        {
            var instruction = exceptionHandler.HandlerStart;
            while (true)
            {
                if (instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference setException &&
                    setException.Name == Constants.METHOD_SetException &&
                    setException.DeclaringType.FullName.StartsWithAny(
                        Constants.TYPE_AsyncTaskMethodBuilder,
                        Constants.TYPE_AsyncValueTaskMethodBuilder,
                        Constants.TYPE_AsyncVoidMethodBuilder,
                        Constants.TYPE_ManualResetValueTaskSourceCore))
                {
                    var setExceptionStart = instruction.Previous.Previous.Previous;
                    if (setExceptionStart.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"Offset {setExceptionStart.Offset} of {moveNextMethodDef.FullName} is {setExceptionStart.OpCode.Code}, it should be Ldarg0 of SetResult which offset is {instruction.Offset}");

                    return setExceptionStart;
                }
                instruction = instruction.Next;
                if (instruction == null || instruction == exceptionHandler.HandlerEnd) throw new InvalidOperationException($"[{moveNextMethodDef.DeclaringType.FullName}] SetException instruction not found");
            }
        }

        private Instruction AsyncGetLeaveCatchAnchor(ExceptionHandler exceptionHandler)
        {
            var leaveCatch = exceptionHandler.HandlerEnd.Previous;
            while (leaveCatch.OpCode.Code != Code.Leave && leaveCatch.OpCode.Code != Code.Leave_S)
            {
                if (leaveCatch == exceptionHandler.HandlerStart) throw new RougamoException($"Cannot find leave or leave_s from exception handler range[{exceptionHandler.HandlerStart.Offset} - {exceptionHandler.HandlerEnd.Offset}]");

                leaveCatch = leaveCatch.Previous;
            }

            return leaveCatch;
        }

        private void AsyncSetAnchors(MethodDefinition methodDef, MethodDefinition moveNextMethodDef, AsyncAnchors anchors)
        {
            methodDef.Body.Instructions.InsertBefore(anchors.BuilderCreateStart, new[]
            {
                anchors.InitMosStart,
                anchors.InitContextStart
            });

            AsyncSetMoveNextAnchors(moveNextMethodDef, anchors);
        }

        private void AsyncSetMoveNextAnchors(MethodDefinition moveNextMethodDef, AsyncAnchors anchors)
        {
            var instructions = moveNextMethodDef.Body.Instructions;

            instructions.Insert(0, new[]
            {
                anchors.OnEntryStart,
                anchors.IfEntryReplacedStart,
                anchors.RewriteArgStart,
                anchors.RetryStart
            });

            instructions.InsertBefore(anchors.SetExceptionStart, new[]
            {
                anchors.SaveException,
                anchors.OnExceptionStart,
                anchors.IfExceptionRetryStart,
                anchors.ExceptionContextStashStart,
                anchors.OnExitAfterExceptionHandledStart,
                anchors.IfExceptionHandled
            });

            if (anchors.BuilderSetResultStart != null)
            {
                instructions.InsertBefore(anchors.BuilderSetResultStart, new[]
                {
                    anchors.SaveReturnValueStart,
                    anchors.OnSuccessStart,
                    anchors.IfSuccessRetryStart,
                    anchors.IfSuccessReplacedStart,
                    anchors.OnExitAfterSuccessExecutedStart
                });
            }
        }

        private IList<Instruction> AsyncInitMos(RouMethod rouMethod, AsyncFields fields, AsyncVariables variables)
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

        private IList<Instruction> AsyncInitMethodContext(RouMethod rouMethod, AsyncFields fields, AsyncVariables variables)
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

        private IList<Instruction> AsyncIfFirstTimeExecute(Instruction retryStart, AsyncFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.State),
                Create(OpCodes.Ldc_I4, -1),
                Create(OpCodes.Bne_Un, retryStart)
            };
        }

        private IList<Instruction> AsyncOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AsyncFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, false);
        }

        private IList<Instruction> AsyncIfOnEntryReplacedReturn(RouMethod rouMethod, MethodDefinition moveNextMethodDef, BoxTypeReference returnBoxTypeRef, MethodReference setResultMethodRef, Instruction endAnchor, AsyncFields fields, AsyncVariables variables)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor),
            };
            if (variables.ReplacedReturn != null)
            {
                instructions.AddRange(new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, fields.MethodContext),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                    Create(OpCodes.Stloc, variables.ReplacedReturn)
                });
            }
            instructions.AddRange(new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldc_I4, -2),
                Create(OpCodes.Stfld, fields.State)
            });
            var onExitEndAnchor = Create(OpCodes.Ldarg_0);
            instructions.AddRange(ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, onExitEndAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding()));
            instructions.Add(onExitEndAnchor);
            instructions.Add(Create(OpCodes.Ldflda, fields.Builder));
            if (variables.ReplacedReturn != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, variables.ReplacedReturn));
                if (returnBoxTypeRef)
                {
                    instructions.Add(Create(OpCodes.Unbox_Any, returnBoxTypeRef));
                }
            }
            instructions.Add(Create(OpCodes.Call, setResultMethodRef));
            instructions.Add(Create(OpCodes.Ret));

            return instructions;
        }

        private IList<Instruction> AsyncRewriteArguments(Instruction endAnchor, AsyncFields fields)
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

        private void AsyncRewriteArgument(int index, FieldReference contextFieldRef, FieldReference parameterFieldRef, Action<Instruction> append)
        {
            var parameterTypeRef = parameterFieldRef.FieldType.ImportInto(ModuleDefinition);
            Instruction? afterNullNop = null;
            if (parameterTypeRef.MetadataType == MetadataType.Class ||
                parameterTypeRef.MetadataType == MetadataType.Array ||
                parameterTypeRef.IsGenericParameter ||
                parameterTypeRef.IsString() || parameterTypeRef.IsNullable())
            {
                var notNullNop = Create(OpCodes.Nop);
                afterNullNop = Create(OpCodes.Nop);
                append(Create(OpCodes.Ldarg_0));
                append(Create(OpCodes.Ldfld, contextFieldRef));
                append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
                append(Create(OpCodes.Ldc_I4, index));
                append(Create(OpCodes.Ldelem_Ref));
                append(Create(OpCodes.Ldnull));
                append(Create(OpCodes.Ceq));
                append(Create(OpCodes.Brfalse_S, notNullNop));
                append(Create(OpCodes.Ldarg_0));
                if (parameterTypeRef.IsGenericParameter || parameterTypeRef.IsNullable())
                {
                    append(Create(OpCodes.Ldflda, parameterFieldRef));
                    append(Create(OpCodes.Initobj, parameterTypeRef));
                }
                else
                {
                    append(Create(OpCodes.Ldnull));
                    append(Create(OpCodes.Stfld, parameterFieldRef));
                }
                append(Create(OpCodes.Br_S, afterNullNop));
                append(notNullNop);
            }
            append(Create(OpCodes.Ldarg_0));
            append(Create(OpCodes.Ldarg_0));
            append(Create(OpCodes.Ldfld, contextFieldRef));
            append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
            append(Create(OpCodes.Ldc_I4, index));
            append(Create(OpCodes.Ldelem_Ref));
            if (parameterTypeRef.IsUnboxable())
            {
                append(Create(OpCodes.Unbox_Any, parameterTypeRef));
            }
            else if (!parameterTypeRef.Is(typeof(object).FullName))
            {
                append(Create(OpCodes.Castclass, parameterTypeRef));
            }
            append(Create(OpCodes.Stfld, parameterFieldRef));
            if (afterNullNop != null)
            {
                append(afterNullNop);
            }
        }

        private IList<Instruction> AsyncSaveException(string methodName, Instruction stlocException, AsyncFields fields)
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

        private IList<Instruction> AsyncOnException(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AsyncFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AsyncIfExceptionRetry(Instruction retryStart, Instruction endAnchor, AsyncFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Cgt),
                Create(OpCodes.Brfalse_S, endAnchor),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldc_I4_M1),
                Create(OpCodes.Stfld, fields.State),
                Create(OpCodes.Leave_S, retryStart)
            };
        }

        private IList<Instruction> AsyncExceptionContextStash(AsyncFields fields, AsyncVariables variables)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Stloc, variables.ExceptionHandled),
            };
            if (variables.ReplacedReturn != null)
            {
                instructions.AddRange(new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, fields.MethodContext),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                    Create(OpCodes.Stloc, variables.ReplacedReturn)
                });
            }

            return instructions;
        }

        private IList<Instruction> AsyncOnExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AsyncFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AsyncIfExceptionHandled(BoxTypeReference returnBoxTypeRef, MethodReference setResultMethodRef, Instruction ifUnhandledBrTo, Instruction ifHandledBrTo, AsyncFields fields, AsyncVariables variables)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.ExceptionHandled),
                Create(OpCodes.Brfalse_S, ifUnhandledBrTo),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldflda, fields.Builder)
            };
            if (variables.ReplacedReturn != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, variables.ReplacedReturn));
                if (returnBoxTypeRef)
                {
                    instructions.Add(Create(OpCodes.Unbox_Any, returnBoxTypeRef));
                }
            }
            instructions.Add(Create(OpCodes.Call, setResultMethodRef));
            instructions.Add(Create(OpCodes.Br_S, ifHandledBrTo));

            return instructions;
        }

        private IList<Instruction> AsyncSaveReturnValue(BoxTypeReference returnBoxTypeRef, Instruction ldlocReturn, AsyncFields fields)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                ldlocReturn.Copy(),
            };
            if (returnBoxTypeRef)
            {
                instructions.Add(Create(OpCodes.Box, returnBoxTypeRef));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));

            return instructions;
        }

        private IList<Instruction> AsyncOnSuccess(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, AsyncFields fields)
        {
            return ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, endAnchor, null, null, fields.Mos, fields.MethodContext, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> AsyncIfSuccessRetry(Instruction ifRetryGoTo, Instruction ifNotRetryGoTo, AsyncFields fields)
        {
            return new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Cgt),
                Create(OpCodes.Brfalse_S, ifNotRetryGoTo),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldc_I4_M1),
                Create(OpCodes.Stfld, fields.State),
                Create(OpCodes.Leave_S, ifRetryGoTo),
            };
        }

        private IList<Instruction> AsyncIfSuccessReplacedReturn(string methodName, BoxTypeReference returnBoxTypeRef, Instruction ldlocReturn, Instruction ifNotReplacedGoTo, AsyncFields fields)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, ifNotReplacedGoTo),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
            };
            if (returnBoxTypeRef)
            {
                instructions.Add(Create(OpCodes.Unbox_Any, returnBoxTypeRef));
            }
            instructions.Add(ldlocReturn.Ldloc2Stloc($"[{methodName}] offset: {ldlocReturn}, it should be ldloc"));

            return instructions;
        }

        private void StateMachineRewriteArguments(MethodDefinition moveNextMethodDef, FieldReference contextFieldRef, FieldReference?[] parameterFieldRefs, Instruction brFalseToIns)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertBefore(brFalseToIns, Create(OpCodes.Ldarg_0));
            instructions.InsertBefore(brFalseToIns, Create(OpCodes.Ldfld, contextFieldRef));
            instructions.InsertBefore(brFalseToIns, Create(OpCodes.Callvirt, _methodMethodContextGetRewriteArgumentsRef));
            instructions.InsertBefore(brFalseToIns, Create(OpCodes.Brfalse_S, brFalseToIns));
            for (var i = 0; i < parameterFieldRefs.Length; i++)
            {
                var parameterFieldRef = parameterFieldRefs[i];
                if (parameterFieldRef == null) continue;

                StateMachineRewriteArgument(i, contextFieldRef, parameterFieldRef, instruction => instructions.InsertBefore(brFalseToIns, instruction));
            }
        }

        private void StateMachineRewriteArgument(int index, FieldReference contextFieldRef, FieldReference parameterFieldRef, Action<Instruction> append)
        {
            var parameterTypeRef = parameterFieldRef.FieldType.ImportInto(ModuleDefinition);
            Instruction? afterNullNop = null;
            if (parameterTypeRef.MetadataType == MetadataType.Class ||
                parameterTypeRef.MetadataType == MetadataType.Array ||
                parameterTypeRef.IsGenericParameter ||
                parameterTypeRef.IsString() || parameterTypeRef.IsNullable())
            {
                var notNullNop = Create(OpCodes.Nop);
                afterNullNop = Create(OpCodes.Nop);
                append(Create(OpCodes.Ldarg_0));
                append(Create(OpCodes.Ldfld, contextFieldRef));
                append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
                append(Create(OpCodes.Ldc_I4, index));
                append(Create(OpCodes.Ldelem_Ref));
                append(Create(OpCodes.Ldnull));
                append(Create(OpCodes.Ceq));
                append(Create(OpCodes.Brfalse_S, notNullNop));
                append(Create(OpCodes.Ldarg_0));
                if (parameterTypeRef.IsGenericParameter || parameterTypeRef.IsNullable())
                {
                    append(Create(OpCodes.Ldflda, parameterFieldRef));
                    append(Create(OpCodes.Initobj, parameterTypeRef));
                }
                else
                {
                    append(Create(OpCodes.Ldnull));
                    append(Create(OpCodes.Stfld, parameterFieldRef));
                }
                append(Create(OpCodes.Br_S, afterNullNop));
                append(notNullNop);
            }
            append(Create(OpCodes.Ldarg_0));
            append(Create(OpCodes.Ldarg_0));
            append(Create(OpCodes.Ldfld, contextFieldRef));
            append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
            append(Create(OpCodes.Ldc_I4, index));
            append(Create(OpCodes.Ldelem_Ref));
            if (parameterTypeRef.IsUnboxable())
            {
                append(Create(OpCodes.Unbox_Any, parameterTypeRef));
            }
            else if (!parameterTypeRef.Is(typeof(object).FullName))
            {
                append(Create(OpCodes.Castclass, parameterTypeRef));
            }
            append(Create(OpCodes.Stfld, parameterFieldRef));
            if (afterNullNop != null)
            {
                append(afterNullNop);
            }
        }

        private Instruction AsyncOnException(RouMethod rouMethod, MethodDefinition moveNextMethodDef, ExceptionHandler exceptionHandler, FieldReference mosFieldRef, FieldReference contextFieldRef, out Instruction setExceptionFirst, out Instruction setExceptionLast)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var ldlocException = exceptionHandler.HandlerStart.Stloc2Ldloc($"[{rouMethod.MethodDef.FullName}] exception handler first instruction is not stloc.s exception");

            FindMoveNextSetExceptionFirstInstruction(moveNextMethodDef, exceptionHandler, out setExceptionFirst, out setExceptionLast);

            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldarg_0));
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldfld, contextFieldRef));
            instructions.InsertBefore(setExceptionFirst, ldlocException);
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));

            var afterOnExceptionNop = instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Nop));
            ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, afterOnExceptionNop, this.ConfigReverseCallEnding());

            return afterOnExceptionNop;
        }

        private FieldDefinition?[] StateMachineParameterFields(RouMethod rouMethod)
        {
            var isStaticMethod = rouMethod.MethodDef.IsStatic;
            var parameterFieldDefs = new FieldDefinition?[rouMethod.MethodDef.Parameters.Count];
            foreach (var instruction in rouMethod.MethodDef.Body.Instructions)
            {
                var index = -1;
                var code = instruction.OpCode.Code;
                if (isStaticMethod && code == Code.Ldarg_0 || !isStaticMethod && code == Code.Ldarg_1)
                {
                    index = 0;
                }
                else if (isStaticMethod && code == Code.Ldarg_1 || !isStaticMethod && code == Code.Ldarg_2)
                {
                    index = 1;
                }
                else if (isStaticMethod && code == Code.Ldarg_2 || !isStaticMethod && code == Code.Ldarg_3)
                {
                    index = 2;
                }
                else if (isStaticMethod && code == Code.Ldarg_3)
                {
                    index = 3;
                }
                else if (code == Code.Ldarg || code == Code.Ldarg_S)
                {
                    index = rouMethod.MethodDef.Parameters.IndexOf((ParameterDefinition)instruction.Operand);
                    if (index == -1) throw new RougamoException($"{rouMethod.MethodDef.FullName} can not locate the index of parameter {((ParameterDefinition)instruction.Operand).Name}");
                }
                if (index != -1)
                {
                    parameterFieldDefs[index] = ((FieldReference)instruction.Next.Operand).Resolve();
                }
            }

            return parameterFieldDefs;
        }

        private void StateMachineFieldDefinition(RouMethod rouMethod, string stateMachineName, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef)
        {
            stateTypeDef = rouMethod.MethodDef.ResolveStateMachine(stateMachineName);
            var fieldAttributes = FieldAttributes.Public;
            mosFieldDef = new FieldDefinition(Constants.FIELD_RougamoMos, fieldAttributes, _typeIMoArrayRef);
            contextFieldDef = new FieldDefinition(Constants.FIELD_RougamoContext, fieldAttributes, _typeMethodContextRef);
            stateTypeDef.Fields.Add(mosFieldDef);
            stateTypeDef.Fields.Add(contextFieldDef);
        }

        private void FindMoveNextSetExceptionFirstInstruction(MethodDefinition moveNextMethodDef, ExceptionHandler exceptionHandler, out Instruction setExceptionFirst, out Instruction setExceptionLast)
        {
            var instruction = exceptionHandler.HandlerStart;
            while (true)
            {
                if (instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference setException &&
                    setException.Name == Constants.METHOD_SetException &&
                    setException.DeclaringType.FullName.StartsWithAny(
                        Constants.TYPE_AsyncTaskMethodBuilder,
                        Constants.TYPE_AsyncValueTaskMethodBuilder,
                        Constants.TYPE_AsyncVoidMethodBuilder,
                        Constants.TYPE_ManualResetValueTaskSourceCore))
                {
                    setExceptionLast = instruction;
                    break;
                }
                instruction = instruction.Next;
                if (instruction == null || instruction == exceptionHandler.HandlerEnd) throw new InvalidOperationException($"[{moveNextMethodDef.DeclaringType.FullName}] SetException instruction not found");
            }
            setExceptionFirst = instruction.Previous.Previous.Previous;
            if (setExceptionFirst.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"Offset {setExceptionFirst.Offset} of {moveNextMethodDef.FullName} is {setExceptionFirst.OpCode.Code}, it should be Ldarg0 of SetResult which offset is {instruction.Offset}");
        }

        private ExceptionHandler GetOuterExceptionHandler(MethodBody methodBody)
        {
            ExceptionHandler? exceptionHandler = null;
            int offset = methodBody.Instructions.First().Offset;
            foreach (var handler in methodBody.ExceptionHandlers)
            {
                if (handler.HandlerType != ExceptionHandlerType.Catch) continue;
                if (handler.TryEnd.Offset > offset)
                {
                    exceptionHandler = handler;
                    offset = handler.TryEnd.Offset;
                }
            }
            return exceptionHandler ?? throw new RougamoException("can not find outer exception handler");
        }

        private MethodReference GetSetResult(TypeReference builderTypeRef)
        {
            return builderTypeRef.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 0 && x.IsPublic).ImportInto(ModuleDefinition);
        }

        private MethodReference GetGenericSetResult(TypeReference builderTypeRef)
        {
            return builderTypeRef.GenericTypeMethodReference(builderTypeRef.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 1 && x.IsPublic), ModuleDefinition);
        }
    }
}
