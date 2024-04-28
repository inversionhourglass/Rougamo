using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Enhances.AsyncIterator;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictAiteratorTaskMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var actualStateMachineTypeDef = StrictStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = StrictStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_AsyncIteratorStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;

            var moveNextDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            moveNextDef.Clear();
            var context = StrictAiteratorProxyCall(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
            StrictAiteratorWeave(rouMethod, moveNextDef, context);

            moveNextDef.DebuggerStepThrough(_methodDebuggerStepThroughCtorRef);
            moveNextDef.Body.OptimizePlus(EmptyInstructions);
        }

        private StrictAiteratorContext StrictAiteratorProxyCall(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var proxyStateMachineTypeRef = proxyStateMachineTypeDef.MakeReference();
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var returnTypeRef = actualMethodDef.ReturnType.ReplaceGenericArgs(genericMap);
            var getAsyncEnumeratorMethodRef = returnTypeRef.Resolve().Methods.Single(x => x.Name == Constants.METHOD_GetAsyncEnumerator).WithGenericDeclaringType(returnTypeRef);
            var enumeratorTypeDef = getAsyncEnumeratorMethodRef.ReturnType.Resolve();
            var enumeratorTypeRef = enumeratorTypeDef.ImportInto(ModuleDefinition).MakeGenericInstanceType(((GenericInstanceType)returnTypeRef).GenericArguments.ToArray());
            var moveNextAsyncMethodDef = enumeratorTypeDef.Methods.Single(x => x.Name == Constants.METHOD_MoveNextAsync);
            var moveNextAsyncMethodRef = moveNextAsyncMethodDef.WithGenericDeclaringType(enumeratorTypeRef);
            var getCurrentMethodRef = enumeratorTypeDef.Methods.Single(x => x.Name == Constants.Getter(Constants.PROP_Current)).WithGenericDeclaringType(enumeratorTypeRef);
            var valueTaskTypeDef = moveNextAsyncMethodDef.ReturnType.Resolve();
            var valueTaskTypeRef = valueTaskTypeDef.ImportInto(ModuleDefinition).MakeGenericInstanceType(_typeBoolRef);
            var getAwaiterMethodDef = valueTaskTypeDef.Methods.Single(x => x.Name == Constants.METHOD_GetAwaiter);
            var getAwaiterMethodRef = getAwaiterMethodDef.WithGenericDeclaringType(valueTaskTypeRef);
            var awaiterTypeDef = getAwaiterMethodDef.ReturnType.Resolve();
            var awaiterTypeRef = awaiterTypeDef.ImportInto(ModuleDefinition).MakeGenericInstanceType(_typeBoolRef);
            var isCompletedMethodRef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.Getter(Constants.PROP_IsCompleted)).WithGenericDeclaringType(awaiterTypeRef);
            var getResultMethodRef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.METHOD_GetResult).WithGenericDeclaringType(awaiterTypeRef);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AiteratorResolveFields(rouMethod, proxyStateMachineTypeDef);
            StrictIteratorSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields, Constants.GenericPrefix(Constants.TYPE_IAsyncEnumerable), Constants.GenericSuffix(Constants.METHOD_GetAsyncEnumerator));
            StrictFieldCleanup(proxyStateMachineTypeDef, fields);

            var builderTypeRef = fields.Builder.FieldType;
            var builderTypeDef = builderTypeRef.Resolve();
            var awaitUnsafeOnCompletedMethodDef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_AwaitUnsafeOnCompleted && x.IsPublic);
            var awaitUnsafeOnCompletedMethodRef = awaitUnsafeOnCompletedMethodDef.WithGenericDeclaringType(builderTypeRef).WithGenerics(awaiterTypeRef, proxyStateMachineTypeRef);
            var completeMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_Complete).WithGenericDeclaringType(builderTypeRef);
            var promiseTypeRef = fields.Promise.FieldType;
            var promiseTypeDef = promiseTypeRef.Resolve();
            var setExceptionMethodRef = promiseTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetException).WithGenericDeclaringType(promiseTypeRef);
            var setResultMethodRef = promiseTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetResult).WithGenericDeclaringType(promiseTypeRef);

            var fIterator = new FieldDefinition(Constants.FIELD_Iterator, FieldAttributes.Private, enumeratorTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fIterator);
            fields.Iterator = fIterator;
            var fAwaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fAwaiter);
            fields.Awaiter = fAwaiter;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);
            var vValueTask = proxyMoveNextDef.Body.CreateVariable(valueTaskTypeRef);
            var vAwaiter = proxyMoveNextDef.Body.CreateVariable(awaiterTypeRef);
            var vToken = proxyMoveNextDef.Body.CreateVariable(_typeCancellationToken);
            var vThis = proxyMoveNextDef.Body.CreateVariable(proxyStateMachineTypeRef);
            var vHasNext = proxyMoveNextDef.Body.CreateVariable(_typeBoolRef);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);

            var instructions = proxyMoveNextDef.Body.Instructions;

            var anchors = new StrictAiteratorAnchors();
            var nopDone = Create(OpCodes.Nop);
            var nopYield = Create(OpCodes.Nop);
            var nopNotDisposed = Create(OpCodes.Nop);
            var nopStateNotZero = Create(OpCodes.Nop);
            var nopGetResult = Create(OpCodes.Nop);
            var nopTryStart = Create(OpCodes.Nop);
            var nopCatchStart = Create(OpCodes.Nop);
            var ret = Create(OpCodes.Ret);

            // var state = _state;
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));

            // -try
            {
                instructions.Add(nopTryStart.Set(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Brfalse, anchors.CheckStateIs0));
                // -if (state != 0)
                {
                    // if (_disposed) goto Done;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Disposed));
                    instructions.Add(Create(OpCodes.Brfalse, nopNotDisposed));
                    instructions.Add(Create(OpCodes.Leave, nopDone));

                    // _state = -1;
                    instructions.Add(nopNotDisposed.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4_M1));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));

                    instructions.Add(Create(OpCodes.Ldloc, vState));
                    instructions.Add(Create(OpCodes.Ldc_I4, -4));
                    instructions.Add(Create(OpCodes.Beq, anchors.CheckStateIs0));
                    // -if (state != -4)
                    {
                        // _iterator = ActualMethod(x, y, z).GetAsyncEnumerator();
                        instructions.Add(anchors.ReadyProxyCall.Set(OpCodes.Ldarg_0));
                        if (fields.DeclaringThis != null)
                        {
                            var ldfld = fields.DeclaringThis.FieldType.IsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
                            instructions.Add(Create(OpCodes.Ldarg_0));
                            instructions.Add(Create(ldfld, fields.DeclaringThis));
                        }
                        foreach (var parameter in fields.Parameters)
                        {
                            instructions.Add(Create(OpCodes.Ldarg_0));
                            instructions.Add(Create(OpCodes.Ldfld, parameter));
                        }
                        instructions.Add(Create(OpCodes.Call, actualMethodRef));
                        instructions.Add(Create(OpCodes.Ldloca, vToken));
                        instructions.Add(Create(OpCodes.Initobj, _typeCancellationToken));
                        instructions.Add(Create(OpCodes.Ldloc, vToken));
                        instructions.Add(Create(OpCodes.Callvirt, getAsyncEnumeratorMethodRef));
                        instructions.Add(Create(OpCodes.Stfld, fields.Iterator));
                    }
                }

                instructions.Add(anchors.CheckStateIs0.Set(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Brtrue, nopStateNotZero));
                // -if (state == 0)
                {
                    // _state = -1;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4_M1));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));

                    // awaiter = _awaiter;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Awaiter));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    // _awaiter = default;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldflda, fields.Awaiter));
                    instructions.Add(Create(OpCodes.Initobj, awaiterTypeRef));

                    instructions.Add(Create(OpCodes.Br, nopGetResult));
                }
                // -else (state != 0)
                {
                    // awaiter = _iterator.MoveNextAsync().GetAwaiter();
                    instructions.Add(nopStateNotZero.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Iterator));
                    instructions.Add(Create(OpCodes.Callvirt, moveNextAsyncMethodRef));
                    instructions.Add(Create(OpCodes.Stloc, vValueTask));
                    instructions.Add(Create(OpCodes.Ldloca, vValueTask));
                    instructions.Add(Create(OpCodes.Call, getAwaiterMethodRef));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    instructions.Add(Create(OpCodes.Ldloca, vAwaiter));
                    instructions.Add(Create(OpCodes.Call, isCompletedMethodRef));
                    instructions.Add(Create(OpCodes.Brtrue, nopGetResult));
                    // -if (!isCompleted)
                    {
                        // _state = 0;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldc_I4_0));
                        instructions.Add(Create(OpCodes.Stfld, fields.State));

                        // _awaiter = awaiter;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldloc, vAwaiter));
                        instructions.Add(Create(OpCodes.Stfld, fields.Awaiter));

                        // @this = this;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Stloc, vThis));

                        // _builder.AwaitUnsafeOnCompleted(ref awaiter, ref @this);
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldflda, fields.Builder));
                        instructions.Add(Create(OpCodes.Ldloca, vAwaiter));
                        instructions.Add(Create(OpCodes.Ldloca, vThis));
                        instructions.Add(Create(OpCodes.Call, awaitUnsafeOnCompletedMethodRef));

                        instructions.Add(Create(OpCodes.Leave, ret));
                    }
                }

                // hasNext = awaiter.GetResult();
                instructions.Add(nopGetResult.Set(OpCodes.Ldloca, vAwaiter));
                instructions.Add(Create(OpCodes.Call, getResultMethodRef));
                instructions.Add(Create(OpCodes.Stloc, vHasNext));

                instructions.Add(anchors.CheckHasNext.Set(OpCodes.Ldloc, vHasNext));
                instructions.Add(Create(OpCodes.Brfalse, anchors.SetDisposedToTrue));
                // -if (hasNext)
                {
                    // _current = _iterator.Current;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Iterator));
                    instructions.Add(Create(OpCodes.Callvirt, getCurrentMethodRef));
                    instructions.Add(Create(OpCodes.Stfld, fields.Current));

                    // _state = -4;
                    instructions.Add(anchors.SetStateToM4.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4, -4));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));

                    instructions.Add(Create(OpCodes.Leave, nopYield));
                }

                // _disposed = true;
                instructions.Add(anchors.SetDisposedToTrue);
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4_1));
                instructions.Add(Create(OpCodes.Stfld, fields.Disposed));

                instructions.Add(Create(OpCodes.Leave, nopDone));
            }
            // catch (Exception ex)
            {
                instructions.Add(nopCatchStart.Set(OpCodes.Stloc, vException));

                // _state = -2;
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4, -2));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                // _current = defalut;
                instructions.Add(Create(OpCodes.Ldarg_0));
                if (fields.Current.FieldType.ShouldInitobj(out var ops1))
                {
                    instructions.Add(Create(OpCodes.Ldflda, fields.Current));
                    instructions.Add(ops1);
                }
                else
                {
                    instructions.Add(ops1);
                    instructions.Add(Create(OpCodes.Stfld, fields.Current));
                }

                // _builder.Complete();
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldflda, fields.Builder));
                instructions.Add(Create(OpCodes.Call, completeMethodRef));

                // _promise.SetException(ex);
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldflda, fields.Promise));
                instructions.Add(Create(OpCodes.Ldloc, vException));
                instructions.Add(Create(OpCodes.Call, setExceptionMethodRef));

                instructions.Add(Create(OpCodes.Leave, ret));
            }

            // Done:
            // _state = -2;
            instructions.Add(nopDone.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4, -2));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            // _current = defalut;
            instructions.Add(Create(OpCodes.Ldarg_0));
            if (fields.Current.FieldType.ShouldInitobj(out var ops2))
            {
                instructions.Add(Create(OpCodes.Ldflda, fields.Current));
                instructions.Add(ops2);
            }
            else
            {
                instructions.Add(ops2);
                instructions.Add(Create(OpCodes.Stfld, fields.Current));
            }

            // _builder.Complete();
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldflda, fields.Builder));
            instructions.Add(Create(OpCodes.Call, completeMethodRef));

            // _promise.SetResult(false);
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldflda, fields.Promise));
            instructions.Add(Create(OpCodes.Ldc_I4_0));
            instructions.Add(Create(OpCodes.Call, setResultMethodRef));

            // return;
            instructions.Add(Create(OpCodes.Ret));

            // _promise.SetResult(true);
            instructions.Add(nopYield.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldflda, fields.Promise));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Call, setResultMethodRef));

            instructions.Add(ret);

            proxyMoveNextDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = nopTryStart,
                TryEnd = nopCatchStart,
                HandlerStart = nopCatchStart,
                HandlerEnd = nopDone,
                CatchType = _typeExceptionRef
            });

            return new(fields, anchors);
        }

        private void StrictAiteratorWeave(RouMethod rouMethod, MethodDefinition proxyMoveNextDef, StrictAiteratorContext context)
        {
            var instructions = proxyMoveNextDef.Body.Instructions;

            var vInnerException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);

            var catchStart = Create(OpCodes.Stloc, vInnerException);

            /**
             * _items = new List<>(); // record return value only
             * _mo1 = new Mo1Attribute();
             * _context = new MethodContext(...);
             * if (_context.RewriteArguments)
             * {
             *     arg1 = (int)_context.Argument[0];
             * }
             */
            var anchorReadyProxyCall = context.Anchors.ReadyProxyCall;
            instructions.InsertBefore(anchorReadyProxyCall, StrictIteratorInitRecordedReturn(rouMethod, context.Fields));
            instructions.InsertBefore(anchorReadyProxyCall, StrictStateMachineInitMos(proxyMoveNextDef, rouMethod.Mos, context.Fields));
            instructions.InsertBefore(anchorReadyProxyCall, StrictStateMachineInitMethodContext(rouMethod, proxyMoveNextDef, context.Fields));
            instructions.InsertBefore(anchorReadyProxyCall, StateMachineOnEntry(rouMethod, proxyMoveNextDef, null, context.Fields));
            instructions.InsertBefore(anchorReadyProxyCall, StateMachineRewriteArguments(rouMethod, anchorReadyProxyCall, context.Fields));

            /**
             * try
             * {
             *     ...
             * }
             * catch (Exception e)
             * {
             *     _context.Exception = e;
             *     _mo1.OnException(_context);
             *     _mo1.OnExit(_context);
             *     throw;
             * }
             */
            var anchorCheckHasNext = context.Anchors.CheckHasNext;
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                instructions.InsertBefore(anchorCheckHasNext, Create(OpCodes.Leave, anchorCheckHasNext));
                instructions.InsertBefore(anchorCheckHasNext, catchStart);
                instructions.InsertBefore(anchorCheckHasNext, StrictStateMachineSaveException(rouMethod, context.Fields, vInnerException));
                instructions.InsertBefore(anchorCheckHasNext, StateMachineOnException(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(anchorCheckHasNext, StateMachineOnExit(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(anchorCheckHasNext, Create(OpCodes.Rethrow));
            }

            /**
			 * _items.Add(_current);
			 */
            var anchorSetStateToM4 = context.Anchors.SetStateToM4;
            instructions.InsertBefore(anchorSetStateToM4, IteratorSaveYeildReturn(rouMethod, context.Fields));

            /**
             * _context.ReturnValue = _items;
             * _mo1.OnSuccess(_context);
             * _mo1.OnExit(_context);
             */
            var anchorSetDisposedToTrue = context.Anchors.SetDisposedToTrue.Next;
            instructions.InsertBefore(anchorSetDisposedToTrue, IteratorSaveReturnValue(rouMethod, context.Fields));
            instructions.InsertBefore(anchorSetDisposedToTrue, StateMachineOnSuccess(rouMethod, proxyMoveNextDef, null, context.Fields));
            instructions.InsertBefore(anchorSetDisposedToTrue, StateMachineOnExit(rouMethod, proxyMoveNextDef, anchorSetDisposedToTrue, context.Fields));

            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                proxyMoveNextDef.Body.ExceptionHandlers.Insert(0, new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = context.Anchors.CheckStateIs0,
                    TryEnd = catchStart,
                    HandlerStart = catchStart,
                    HandlerEnd = anchorCheckHasNext,
                    CatchType = _typeExceptionRef
                });
            }
        }
    }
}
