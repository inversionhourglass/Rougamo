using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeaveMos()
        {
            CheckRefStruct();

            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    try
                    {
                        if (rouMethod.IsIterator)
                        {
                            WeavingIteratorMethod(rouMethod);
                        }
                        else if (rouMethod.IsAsyncIterator)
                        {
                            WeavingAsyncIteratorMethod(rouMethod);
                        }
                        else if (rouMethod.IsAsyncTaskOrValueTask)
                        {
                            WeavingAsyncTaskMethod(rouMethod);
                        }
                        else if (!rouMethod.MethodDef.IsConstructor)
                        {
                            WeavingSyncMethod(rouMethod);
                        }
                        else
                        {
                            WeavingConstructor(rouMethod);
                        }
                    }
                    catch (FodyWeavingException ex)
                    {
                        ex.MethodDef ??= rouMethod.MethodDef;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new FodyWeavingException(ex.ToString(), rouMethod.MethodDef);
                    }
                }
            }
        }

        private void SetTryCatch(MethodDefinition methodDef, Instruction? tryStart, Instruction? catchStart, Instruction? catchEnd, TypeReference? catchType = null)
        {
            if (tryStart == null || catchStart == null || catchEnd == null) return;

            catchType ??= _tExceptionRef;

            methodDef.Body.ExceptionHandlers.Add(new(ExceptionHandlerType.Catch)
            {
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = catchEnd,
                CatchType = catchType
            });
        }

        private void CheckRefStruct()
        {
            var exceptions = new List<FodyWeavingException>();
            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    CheckRefStruct(rouMethod, exceptions);
                }
            }

            if (exceptions.Count == 0) return;
            if (exceptions.Count == 1) throw exceptions[0];
            throw new FodyAggregateWeavingException(exceptions);
        }

        private void CheckRefStruct(RouMethod rouMethod, List<FodyWeavingException> exceptions)
        {
            TypeDefinition typeDef;
            if (!rouMethod.SkipRefStruct && !rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) && (typeDef = rouMethod.MethodDef.ReturnType.Resolve()) != null && typeDef.CustomAttributes.Any(x => x.Is(Constants.TYPE_IsByRefLikeAttribute)))
            {
                var builder = new StringBuilder("Cannot save a ref struct value as an object. Change the return value type or set the MethodContextOmits property for the listed types to Omit.ReturnValue: [");
                foreach (var mo in rouMethod.Mos)
                {
                    if ((rouMethod.MethodContextOmits & Omit.ReturnValue) == 0)
                    {
                        builder.Append(mo.MoTypeDef.FullName);
                        builder.Append(", ");
                    }
                }
                builder.Length -= 2;
                builder.Append("]. For more information: https://github.com/inversionhourglass/Rougamo/issues/61");

                exceptions.Add(new FodyWeavingException(builder.ToString(), rouMethod.MethodDef));
            }

            if (!rouMethod.SkipRefStruct && !rouMethod.MethodContextOmits.Contains(Omit.Arguments) && rouMethod.MethodDef.Parameters.Any(x => (typeDef = x.ParameterType.Resolve()) != null && typeDef.CustomAttributes.Any(y => y.Is(Constants.TYPE_IsByRefLikeAttribute))))
            {
                var builder = new StringBuilder("Cannot save a ref struct value as an object. Change the parameter type or set the MethodContextOmits property for the listed types to Omit.Arguments: [");
                foreach (var mo in rouMethod.Mos)
                {
                    if ((rouMethod.MethodContextOmits & Omit.Arguments) == 0)
                    {
                        builder.Append(mo.MoTypeDef.FullName);
                        builder.Append(", ");
                    }
                }
                builder.Length -= 2;
                builder.Append("]. For more information: https://github.com/inversionhourglass/Rougamo/issues/61");

                exceptions.Add(new FodyWeavingException(builder.ToString(), rouMethod.MethodDef));
            }
        }

        private void DebuggerStepThrough(MethodDefinition methodDef)
        {
            if (methodDef.CustomAttributes.Any(x => x.Is(typeof(DebuggerStepThroughAttribute).FullName))) return;

            methodDef.CustomAttributes.Add(new CustomAttribute(_ctorDebuggerStepThroughRef));
        }

    }
}
