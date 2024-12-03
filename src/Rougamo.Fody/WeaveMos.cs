using Fody;
using Fody.Simulations;
using Fody.Simulations.PlainValues;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Models;
using Rougamo.Fody.Simulations.Types;
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

        private IList<Instruction> NewMo(RouMethod rouMethod, Mo mo, TsMo tMo, MethodSimulation executingMethod, List<IParameterSimulation> pooledItems)
        {
            switch (mo.Lifetime)
            {
                case Lifetime.Transient:
                    return [
                        .. tMo.New(executingMethod, mo),
                        .. SetMoProperties(mo, tMo, executingMethod)
                    ];
                case Lifetime.Singleton:
                    if (mo.HasProperties) throw new FodyWeavingException($"{mo.MoTypeDef} is applied to {rouMethod.MethodDef} with properties, but {mo.MoTypeDef} has a singleton lifetime that cannot has any property and constructor argument.");

                    return tMo.M_Singleton.Call(executingMethod);
                case Lifetime.Pooled:
                    pooledItems.Add(tMo.Host);
                    var tPool = _tPoolRef.MakeGenericInstanceType(tMo.Type).Simulate(this);
                    var mGet = _mPoolGetRef.Simulate(tPool);
                    return [
                        .. mGet.Call(executingMethod),
                        .. SetMoProperties(mo, tMo, executingMethod)
                    ];
                default:
                    throw new FodyWeavingException($"Unknow lifetime {mo.Lifetime} of {mo.MoTypeDef}");
            }
        }

        private IList<Instruction> SetMoProperties(Mo mo, TsMo tMo, MethodSimulation executingMethod)
        {
            if (mo is not CustomAttributeMo caMo || !caMo.HasProperties) return [];

            var instructions = new List<Instruction>();

            foreach (var property in caMo.Attribute.Properties)
            {
                var propSimulation = tMo.OptionalPropertySimulate(property.Name, true);
                if (propSimulation?.Setter != null)
                {
                    var propValue = new ObjectValue(property.Argument.Value, property.Argument.Type, tMo.ModuleWeaver);
                    instructions.Add(propSimulation.Setter.DupCall(executingMethod, propValue));
                }
            }

            return instructions;
        }

        private IList<Instruction> AssignByPool(VariableSimulation variable)
        {
            return AssignByPool(variable, variable.DeclaringMethod);
        }

        private IList<Instruction> AssignByPool(IAssignable assignable, MethodSimulation executingMethod)
        {
            return assignable.Assign(target => GetFromPool(assignable, executingMethod));
        }

        private IList<Instruction> GetFromPool(IAnalysable analysable, MethodSimulation executingMethod)
        {
            var tPool = _tPoolRef.MakeGenericInstanceType(analysable.Type).Simulate(this);
            var mGet = _mPoolGetRef.Simulate(tPool);
            return mGet.Call(executingMethod);
        }

        private IList<Instruction> ReturnToPool(IParameterSimulation poolItem, MethodSimulation executingMethod)
        {
            var tPool = _tPoolRef.MakeGenericInstanceType(poolItem.Type).Simulate(this);
            var mReturn = _mPoolReturnRef.Simulate(tPool);
            return mReturn.Call(executingMethod, poolItem);
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

        private void SetTryFinally(MethodDefinition methodDef, Instruction? tryStart, Instruction? finallyStart, Instruction? finallyEnd)
        {
            if (tryStart == null || finallyStart == null || finallyEnd == null) return;

            methodDef.Body.ExceptionHandlers.Add(new(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = finallyEnd
            });
        }

        private void SetTryFault(MethodDefinition methodDef, Instruction? tryStart, Instruction? faultStart, Instruction? faultEnd)
        {
            if (tryStart == null || faultStart == null || faultEnd == null) return;

            methodDef.Body.ExceptionHandlers.Add(new(ExceptionHandlerType.Fault)
            {
                TryStart = tryStart,
                TryEnd = faultStart,
                HandlerStart = faultStart,
                HandlerEnd = faultEnd
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
