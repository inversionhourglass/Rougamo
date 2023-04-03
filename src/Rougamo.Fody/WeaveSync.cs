using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var returnBoxTypeRef = new BoxTypeReference(rouMethod.MethodDef.ReturnType.ImportInto(ModuleDefinition));

            var variables = SyncCreateVariables(rouMethod);
            var anchors = SyncCreateAnchors(rouMethod.MethodDef, variables);
            SyncSetAnchors(rouMethod.MethodDef, anchors, variables);
            SetTryCatchFinally(rouMethod.MethodDef, anchors);

            instructions.InsertAfter(anchors.InitMos, SyncInitMos(rouMethod.Mos, variables));
            instructions.InsertAfter(anchors.InitContext, SyncInitMethodContext(rouMethod.MethodDef, variables));
            instructions.InsertAfter(anchors.OnEntry, SyncOnEntry(rouMethod, anchors.IfEntryReplaced, variables));
            instructions.InsertAfter(anchors.IfEntryReplaced, SyncIfOnEntryReplacedReturn(rouMethod, returnBoxTypeRef, anchors.RewriteArg, variables));
            instructions.InsertAfter(anchors.RewriteArg, SyncRewriteArguments(rouMethod, anchors.Retry, variables));
            instructions.InsertAfter(anchors.Retry, SyncResetRetryVariable(rouMethod, variables));

            instructions.InsertAfter(anchors.CatchStart, SyncSaveException(variables));
            instructions.InsertAfter(anchors.OnException, SyncOnException(rouMethod, anchors.IfExceptionRetry, variables));
            instructions.InsertAfter(anchors.IfExceptionRetry, SyncIfExceptionRetry(rouMethod, anchors.Retry, anchors.IfExceptionHandled, variables));
            instructions.InsertAfter(anchors.IfExceptionHandled, SyncIfExceptionHandled(rouMethod, returnBoxTypeRef, anchors.FinallyEnd, anchors.Rethrow, variables));

            instructions.InsertAfter(anchors.FinallyStart, SyncHasExceptionCheck(rouMethod, anchors.OnExit, variables));
            instructions.InsertAfter(anchors.SaveReturnValue, SyncSaveReturnValue(returnBoxTypeRef, variables));
            instructions.InsertAfter(anchors.OnSuccess, SyncOnSuccess(rouMethod, anchors.IfSuccessRetry, variables));
            instructions.InsertAfter(anchors.IfSuccessRetry, SyncIfOnSuccessRetry(rouMethod, anchors.IfSuccessReplaced, anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.IfSuccessReplaced, SyncIfOnSuccessReplacedReturn(rouMethod, returnBoxTypeRef, anchors.OnExit, variables));
            instructions.InsertAfter(anchors.OnExit, SyncOnExit(rouMethod, anchors.EndFinally, variables));

            instructions.InsertAfter(anchors.FinallyEnd, SyncRetryFork(rouMethod, anchors.Retry, anchors.Ret, variables));

            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private SyncVariables SyncCreateVariables(RouMethod rouMethod)
        {
            VariableDefinition? moArray = null;
            var mos = new VariableDefinition[0];
            if (rouMethod.Mos.Length >= _config.MoArrayThreshold)
            {
                moArray = rouMethod.MethodDef.Body.CreateVariable(_typeIMoArrayRef);
            }
            else
            {
                mos = rouMethod.Mos.Select(x => rouMethod.MethodDef.Body.CreateVariable(_typeIMoRef)).ToArray();
            }
            var methodContext = rouMethod.MethodDef.Body.CreateVariable(_typeMethodContextRef);
            var isRetry = rouMethod.MethodDef.Body.CreateVariable(_typeBoolRef);
            var exception = rouMethod.MethodDef.Body.CreateVariable(_typeExceptionRef);

            return new SyncVariables(moArray, mos, methodContext, isRetry, exception);
        }

        private SyncAnchors SyncCreateAnchors(MethodDefinition methodDef, SyncVariables variables)
        {
            return new SyncAnchors(variables, methodDef.Body.Instructions.First());
        }

        private void SyncSetAnchors(MethodDefinition methodDef, SyncAnchors anchors, SyncVariables variables)
        {
            var instructions = methodDef.Body.Instructions;

            variables.Return = methodDef.ReturnType.IsVoid() ? null : methodDef.Body.CreateVariable(Import(methodDef.ReturnType));

            instructions.InsertBefore(anchors.TryStart, new[]
            {
                anchors.InitMos,
                anchors.InitContext,
                anchors.OnEntry,
                anchors.IfEntryReplaced,
                anchors.RewriteArg,
                anchors.Retry
            });

            instructions.Add(new[]
            {
                anchors.CatchStart,
                anchors.OnException,
                anchors.IfExceptionRetry,
                anchors.IfExceptionHandled,
                anchors.Rethrow,
                anchors.FinallyStart,
                anchors.SaveReturnValue,
                anchors.OnSuccess,
                anchors.IfSuccessRetry,
                anchors.IfSuccessReplaced,
                anchors.OnExit,
                anchors.EndFinally,
                anchors.FinallyEnd
            });

            var returns = instructions.Where(ins => ins.IsRet()).ToArray();

            if (variables.Return == null)
            {
                instructions.Add(anchors.Ret);
            }
            else
            {
                var ret = anchors.Ret;
                instructions.Add(anchors.Ret = variables.Return.Ldloc());
                instructions.Add(ret);
            }

            if (returns.Length != 0)
            {
                foreach (var @return in returns)
                {
                    if (variables.Return == null)
                    {
                        @return.OpCode = OpCodes.Leave;
                        @return.Operand = anchors.FinallyEnd;
                    }
                    else
                    {
                        @return.OpCode = OpCodes.Stloc;
                        @return.Operand = variables.Return;
                        instructions.InsertAfter(@return, Create(OpCodes.Leave, anchors.FinallyEnd));
                    }
                }
            }
        }

        private IList<Instruction> SyncInitMos(Mo[] mos, SyncVariables variables)
        {
            if (variables.MoArray != null)
            {
                var instructions = InitMoArray(mos);
                instructions.Add(Create(OpCodes.Stloc, variables.MoArray));

                return instructions;
            }

            return SyncInitMoVariables(mos, variables.Mos);
        }

        private IList<Instruction> SyncInitMoVariables(Mo[] mos, VariableDefinition[] moVariables)
        {
            var instructions = new List<Instruction>();

            var i = 0;
            foreach (var mo in mos)
            {
                instructions.AddRange(InitMo(mo));
                instructions.Add(Create(OpCodes.Stloc, moVariables[i]));

                i++;
            }

            return instructions;
        }

        private IList<Instruction> SyncInitMethodContext(MethodDefinition methodDef, SyncVariables variables)
        {
            var instructions = new List<Instruction>();
            VariableDefinition moArray;
            if (variables.MoArray != null)
            {
                moArray = variables.MoArray;
            }
            else
            {
                moArray = methodDef.Body.CreateVariable(_typeIMoArrayRef);
                instructions.AddRange(CreateTempMoArray(variables.Mos));
                instructions.Add(Create(OpCodes.Stloc, moArray));
            }
            instructions.AddRange(InitMethodContext(methodDef, false, false, moArray, null, null));
            instructions.Add(Create(OpCodes.Stloc, variables.MethodContext));

            return instructions;
        }

        private IList<Instruction> SyncOnEntry(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.OnEntry.IsMatch(rouMethod.Features)) return new Instruction[0];

            if (variables.MoArray != null)
            {
                return ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Length, endAnchor, variables.MoArray, variables.MethodContext, null, null, false);
            }
            return ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.Mos.Length, variables.Mos, variables.MethodContext, null, null, false);
        }

        private IList<Instruction> SyncIfOnEntryReplacedReturn(RouMethod rouMethod, BoxTypeReference returnBoxTypeRef, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.EntryReplace.IsMatch(rouMethod.Features)) return new Instruction[0];

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            var ret = Create(OpCodes.Ret);
            var onExitEndAnchor = ret;
            if (variables.Return != null)
            {
                onExitEndAnchor = Create(OpCodes.Ldloc, variables.Return);
                instructions.AddRange(ReplaceReturnValue(variables.MethodContext, variables.Return, returnBoxTypeRef));
            }

            instructions.AddRange(SyncOnExit(rouMethod, onExitEndAnchor, variables));
            if (variables.Return != null)
            {
                instructions.Add(onExitEndAnchor);
            }
            instructions.Add(ret);

            return instructions;
        }

        private IList<Instruction> SyncRewriteArguments(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            if (rouMethod.MethodDef.Parameters.Count == 0 || !Feature.RewriteArgs.IsMatch(rouMethod.Features)) return new Instruction[0];

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRewriteArgumentsRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            for (var i = 0; i < rouMethod.MethodDef.Parameters.Count; i++)
            {
                SyncRewriteArgument(i, variables.MethodContext, rouMethod.MethodDef.Parameters[i], instructions.Add);
            }

            return instructions.Count == 3 ? new Instruction[0] : instructions;
        }

        private void SyncRewriteArgument(int index, VariableDefinition contextVariable, ParameterDefinition parameterDef, Action<Instruction> append)
        {
            if (parameterDef.IsOut) return;

            var parameterTypeRef = parameterDef.ParameterType.ImportInto(ModuleDefinition);
            var isByReference = parameterDef.ParameterType.IsByReference;
            if (isByReference)
            {
                parameterTypeRef = ((ByReferenceType)parameterDef.ParameterType).ElementType.ImportInto(ModuleDefinition);
            }
            Instruction? afterNullNop = null;
            if (parameterTypeRef.MetadataType == MetadataType.Class ||
                parameterTypeRef.MetadataType == MetadataType.Array ||
                parameterTypeRef.IsGenericParameter ||
                parameterTypeRef.IsString() || parameterTypeRef.IsNullable())
            {
                var notNullNop = Create(OpCodes.Nop);
                afterNullNop = Create(OpCodes.Nop);
                append(Create(OpCodes.Ldloc, contextVariable));
                append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
                append(Create(OpCodes.Ldc_I4, index));
                append(Create(OpCodes.Ldelem_Ref));
                append(Create(OpCodes.Ldnull));
                append(Create(OpCodes.Ceq));
                append(Create(OpCodes.Brfalse_S, notNullNop));
                if (parameterTypeRef.IsGenericParameter || parameterTypeRef.IsNullable())
                {
                    if (isByReference)
                    {
                        append(Create(OpCodes.Ldarg_S, parameterDef));
                    }
                    else
                    {
                        append(Create(OpCodes.Ldarga_S, parameterDef));
                    }
                    append(Create(OpCodes.Initobj, parameterTypeRef));
                }
                else if (isByReference)
                {
                    append(Create(OpCodes.Ldarg_S, parameterDef));
                    append(Create(OpCodes.Ldnull));
                    append(parameterTypeRef.Stind());
                }
                else
                {
                    append(Create(OpCodes.Ldnull));
                    append(Create(OpCodes.Starg_S, parameterDef));
                }
                append(Create(OpCodes.Br_S, afterNullNop));
                append(notNullNop);
            }
            if (isByReference)
            {
                append(Create(OpCodes.Ldarg_S, parameterDef));
            }
            append(Create(OpCodes.Ldloc, contextVariable));
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
            if (isByReference)
            {
                append(parameterTypeRef.Stind());
            }
            else
            {
                append(Create(OpCodes.Starg_S, parameterDef));
            }
            if (afterNullNop != null)
            {
                append(afterNullNop);
            }
        }

        private IList<Instruction> SyncResetRetryVariable(RouMethod rouMethod, SyncVariables variables)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!Feature.RetryAny.IsMatch(rouMethod.Features)) return new Instruction[0];
#pragma warning restore CS0618 // Type or member is obsolete

            return new[]
            {
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Stloc, variables.IsRetry)
            };
        }

        private IList<Instruction> SyncSaveException(SyncVariables variables)
        {
            return new[]
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Ldloc, variables.Exception),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            };
        }

        private IList<Instruction> SyncOnException(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.OnException.IsMatch(rouMethod.Features)) return new Instruction[0];

            if (variables.MoArray != null)
            {
                return ExecuteMoMethod(Constants.METHOD_OnException, rouMethod.MethodDef, rouMethod.Mos.Length, endAnchor, variables.MoArray, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
            }
            return ExecuteMoMethod(Constants.METHOD_OnException, rouMethod.Mos.Length, variables.Mos, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
        }

        private IList<Instruction> SyncIfExceptionRetry(RouMethod rouMethod, Instruction retryAnchor, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.ExceptionRetry.IsMatch(rouMethod.Features)) return new Instruction[0];

            return new[]
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Cgt),
                Create(OpCodes.Brfalse_S, endAnchor),
                Create(OpCodes.Leave_S, retryAnchor)
            };
        }

        private IList<Instruction> SyncIfExceptionHandled(RouMethod rouMethod, BoxTypeReference returnBoxTypeRef, Instruction finallyEndAnchor, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.ExceptionHandle.IsMatch(rouMethod.Features)) return new Instruction[0];

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            if (variables.Return != null)
            {
                instructions.AddRange(ReplaceReturnValue(variables.MethodContext, variables.Return, returnBoxTypeRef));
            }
            instructions.Add(Create(OpCodes.Leave, finallyEndAnchor));

            return instructions;
        }

        private IList<Instruction> SyncHasExceptionCheck(RouMethod rouMethod, Instruction onExitStart, SyncVariables variables)
        {
            if ((rouMethod.Features & (int)(Feature.ExceptionHandle | Feature.OnSuccess | Feature.SuccessRetry | Feature.SuccessReplace | Feature.OnExit)) == 0) return new Instruction[0];

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                Create(OpCodes.Brtrue_S, onExitStart)
            };

            if (Feature.ExceptionHandle.IsMatch(rouMethod.Features))
            {
                instructions.Add(Create(OpCodes.Ldloc, variables.MethodContext));
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef));
                instructions.Add(Create(OpCodes.Brtrue_S, onExitStart));
            }

            return instructions;
        }

        private IList<Instruction>? SyncSaveReturnValue(BoxTypeReference returnBoxTypeRef, SyncVariables variables)
        {
            if (variables.Return == null) return null;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Ldloc, variables.Return)
            };
            if (returnBoxTypeRef)
            {
                instructions.Add(Create(OpCodes.Box, returnBoxTypeRef));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));

            return instructions;
        }

        private IList<Instruction> SyncOnSuccess(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.OnSuccess.IsMatch(rouMethod.Features)) return new Instruction[0];

            if (variables.MoArray != null)
            {
                return ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.MethodDef, rouMethod.Mos.Length, endAnchor, variables.MoArray, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
            }
            return ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.Mos.Length, variables.Mos, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
        }

        private IList<Instruction> SyncIfOnSuccessRetry(RouMethod rouMethod, Instruction endAnchor, Instruction endFinally, SyncVariables variables)
        {
            if (!Feature.SuccessRetry.IsMatch(rouMethod.Features)) return new Instruction[0];

            return new[]
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Cgt),
                Create(OpCodes.Brfalse_S, endAnchor),
                Create(OpCodes.Ldc_I4_1),
                Create(OpCodes.Stloc, variables.IsRetry),
                Create(OpCodes.Leave_S, endFinally)
            };
        }

        private IList<Instruction> SyncIfOnSuccessReplacedReturn(RouMethod rouMethod, BoxTypeReference returnBoxTypeRef, Instruction endAnchor, SyncVariables variables)
        {
            if (variables.Return == null || !Feature.SuccessReplace.IsMatch(rouMethod.Features)) return new Instruction[0];

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            instructions.AddRange(ReplaceReturnValue(variables.MethodContext, variables.Return, returnBoxTypeRef));

            return instructions;
        }

        private IList<Instruction> SyncOnExit(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            if (!Feature.OnExit.IsMatch(rouMethod.Features)) return new Instruction[0];

            if (variables.MoArray != null)
            {
                return ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Length, endAnchor, variables.MoArray, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
            }
            return ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.Mos.Length, variables.Mos, variables.MethodContext, null, null, _config.ReverseCallNonEntry);
        }

        private IList<Instruction> SyncRetryFork(RouMethod rouMethod, Instruction retry, Instruction notRetry, SyncVariables variables)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!Feature.RetryAny.IsMatch(rouMethod.Features)) return new Instruction[0];
#pragma warning restore CS0618 // Type or member is obsolete

            return new[]
            {
                Create(OpCodes.Ldloc, variables.IsRetry),
                Create(OpCodes.Brfalse, notRetry),
                Create(OpCodes.Br_S, retry)
            };
        }

        private List<Instruction> ReplaceReturnValue(VariableDefinition contextVariable, VariableDefinition returnVariable, BoxTypeReference returnBoxTypeRef)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, contextVariable),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef)
            };
            if (returnBoxTypeRef)
            {
                instructions.Add(Create(OpCodes.Unbox_Any, returnBoxTypeRef));
            }
            instructions.Add(Create(OpCodes.Stloc, returnVariable));
            return instructions;
        }
    }
}
