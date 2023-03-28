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
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var returnBoxTypeRef = new BoxTypeReference(rouMethod.MethodDef.ReturnType.ImportInto(ModuleDefinition));

            var variables = SyncCreateVariables(rouMethod.MethodDef);
            var anchors = SyncCreateAnchors(rouMethod.MethodDef, variables);

            SyncGenerateAnchors(rouMethod.MethodDef, anchors, variables);
            SyncSetTryCatchFinally(rouMethod.MethodDef, anchors);

            instructions.Insert(0, SyncInitMoArray(rouMethod.Mos, variables));
            instructions.InsertAfter(anchors.InitContextStart, SyncInitMethodContext(rouMethod.MethodDef, variables));
            instructions.InsertAfter(anchors.OnEntryStart, SyncOnEntry(rouMethod, anchors.IfOnEntryReplacedStart, variables));
            instructions.InsertAfter(anchors.IfOnEntryReplacedStart, SyncIfOnEntryReplacedReturn(rouMethod, returnBoxTypeRef, anchors.RewriteArgStart, variables));
            instructions.InsertAfter(anchors.RewriteArgStart, SyncRewriteArguments(rouMethod.MethodDef, anchors.RetryStart, variables));
            instructions.InsertAfter(anchors.RetryStart, SyncResetRetryVariable(variables));
            instructions.InsertAfter(anchors.CatchStart, SyncSaveException(variables));
            instructions.InsertAfter(anchors.OnExceptionStart, SyncOnException(rouMethod, anchors.IfOnExceptionRetryStart, variables));
            instructions.InsertAfter(anchors.IfOnExceptionRetryStart, SyncExceptionIfRetry(anchors.RetryStart, anchors.IfOnExceptionHandledStart, variables));
            instructions.InsertAfter(anchors.IfOnExceptionHandledStart, SyncIfExceptionHandled(returnBoxTypeRef, anchors.FinallyEnd, anchors.Rethrow, variables));
            instructions.InsertAfter(anchors.FinallyStart, SyncHasExceptionCheck(anchors.OnExitStart, variables));
            instructions.InsertAfter(anchors.SetBackReturnStart, SyncSetBackReturnValue(returnBoxTypeRef, variables));
            instructions.InsertAfter(anchors.OnSuccessStart, SyncOnSuccess(rouMethod, anchors.IfOnSuccessRetryStart, variables));
            instructions.InsertAfter(anchors.IfOnSuccessRetryStart, SyncIfOnSuccessRetry(anchors.IfOnSuccessReplacedStart, anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.IfOnSuccessReplacedStart, SyncIfOnSuccessReplacedReturn(returnBoxTypeRef, anchors.OnExitStart, variables));
            instructions.InsertAfter(anchors.OnExitStart, SyncOnExit(rouMethod, anchors.EndFinally, variables));
            instructions.InsertAfter(anchors.FinallyEnd, SyncRetryFork(anchors.RetryStart, anchors.ReturnStart, variables));

            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private SyncVariables SyncCreateVariables(MethodDefinition methodDef)
        {
            return new SyncVariables(
                mos: methodDef.Body.CreateVariable(_typeIMoArrayRef),
                methodContext: methodDef.Body.CreateVariable(_typeMethodContextRef),
                isRetry: methodDef.Body.CreateVariable(_typeBoolRef),
                exception: methodDef.Body.CreateVariable(_typeExceptionRef));
        }

        private SyncAnchors SyncCreateAnchors(MethodDefinition methodDef, SyncVariables variables)
        {
            return new SyncAnchors(variables, methodDef.Body.Instructions.First());
        }

        private void SyncGenerateAnchors(MethodDefinition methodDef, SyncAnchors anchors, SyncVariables variables)
        {
            var instructions = methodDef.Body.Instructions;

            variables.Return = methodDef.ReturnType.IsVoid() ? null : methodDef.Body.CreateVariable(Import(methodDef.ReturnType));

            instructions.InsertBefore(anchors.TryStart, new[]
            {
                anchors.InitContextStart,
                anchors.OnEntryStart,
                anchors.IfOnEntryReplacedStart,
                anchors.RewriteArgStart,
                anchors.RetryStart
            });

            instructions.Add(new[]
            {
                anchors.CatchStart,
                anchors.OnExceptionStart,
                anchors.IfOnExceptionRetryStart,
                anchors.IfOnExceptionHandledStart,
                anchors.Rethrow,
                anchors.FinallyStart,
                anchors.SetBackReturnStart,
                anchors.OnSuccessStart,
                anchors.IfOnSuccessRetryStart,
                anchors.IfOnSuccessReplacedStart,
                anchors.OnExitStart,
                anchors.EndFinally,
                anchors.FinallyEnd
            });

            var returns = instructions.Where(ins => ins.IsRet()).ToArray();

            if (variables.Return == null)
            {
                instructions.Add(anchors.ReturnStart);
            }
            else
            {
                var ret = anchors.ReturnStart;
                instructions.Add(anchors.ReturnStart = variables.Return.Ldloc());
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

        private void SyncSetTryCatchFinally(MethodDefinition methodDef, SyncAnchors anchors)
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

        private IList<Instruction> SyncInitMoArray(HashSet<Mo> mos, SyncVariables variables)
        {
            var instructions = InitMosArray(mos);
            instructions.Add(Create(OpCodes.Stloc, variables.Mos));

            return instructions;
        }

        private IList<Instruction> SyncInitMethodContext(MethodDefinition methodDef, SyncVariables variables)
        {
            var instructions = InitMethodContext(methodDef, false, false, variables.Mos, null, null);
            instructions.Add(Create(OpCodes.Stloc, variables.MethodContext));

            return instructions;
        }

        private IList<Instruction> SyncOnEntry(RouMethod rouMethod, Instruction endAnchor, SyncVariables variables)
        {
            return ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, endAnchor, variables.Mos, variables.MethodContext, null, null, false);
        }

        private IList<Instruction> SyncIfOnEntryReplacedReturn(RouMethod rouMethod, BoxTypeReference returnBoxTypeRef, Instruction endAnchor, SyncVariables variables)
        {
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
            instructions.AddRange(ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Count, onExitEndAnchor, variables.Mos, variables.MethodContext, null, null, this.ConfigReverseCallEnding()));
            if (variables.Return != null)
            {
                instructions.Add(onExitEndAnchor);
            }
            instructions.Add(ret);

            return instructions;
        }

        private IList<Instruction> SyncRewriteArguments(MethodDefinition methodDef, Instruction endAnchor, SyncVariables variables)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRewriteArgumentsRef),
                Create(OpCodes.Brfalse_S, endAnchor)
            };
            for (var i = 0; i < methodDef.Parameters.Count; i++)
            {
                SyncRewriteArgument(i, variables.MethodContext, methodDef.Parameters[i], instructions.Add);
            }

            return instructions;
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

        private IList<Instruction> SyncResetRetryVariable(SyncVariables variables)
        {
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
            return ExecuteMoMethod(Constants.METHOD_OnException, rouMethod.MethodDef, rouMethod.Mos.Count, endAnchor, variables.Mos, variables.MethodContext, null, null, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> SyncExceptionIfRetry(Instruction retryAnchor, Instruction endAnchor, SyncVariables variables)
        {
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

        private IList<Instruction> SyncIfExceptionHandled(BoxTypeReference returnBoxTypeRef, Instruction finallyEndAnchor, Instruction endAnchor, SyncVariables variables)
        {
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

        private IList<Instruction> SyncHasExceptionCheck(Instruction onExitStart, SyncVariables variables)
        {
            return new[]
            {
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                Create(OpCodes.Brtrue_S, onExitStart),
                Create(OpCodes.Ldloc, variables.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brtrue_S, onExitStart)
            };
        }

        private IList<Instruction>? SyncSetBackReturnValue(BoxTypeReference returnBoxTypeRef, SyncVariables variables)
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
            return ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.MethodDef, rouMethod.Mos.Count, endAnchor, variables.Mos, variables.MethodContext, null, null, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> SyncIfOnSuccessRetry(Instruction endAnchor, Instruction endFinally, SyncVariables variables)
        {
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

        private IList<Instruction>? SyncIfOnSuccessReplacedReturn(BoxTypeReference returnBoxTypeRef, Instruction endAnchor, SyncVariables variables)
        {
            if (variables.Return == null) return null;

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
            return ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Count, endAnchor, variables.Mos, variables.MethodContext, null, null, this.ConfigReverseCallEnding());
        }

        private IList<Instruction> SyncRetryFork(Instruction retry, Instruction notRetry, SyncVariables variables)
        {
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
