using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Rougamo.Fody.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeaveMos()
        {
            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    try
                    {
                        CheckRefStruct(rouMethod);

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
                    catch (Exception ex) when (ex is not RougamoException)
                    {
                        throw new RougamoException($"[{rouMethod.MethodDef.FullName}] {ex}");
                    }
                }
            }
        }

        private void SetTryCatch(MethodDefinition methodDef, Instruction? tryStart, Instruction? catchStart, Instruction? catchEnd, TypeReference? catchType = null)
        {
            if (tryStart == null || catchStart == null || catchEnd == null) return;

            catchType ??= _typeExceptionRef;

            methodDef.Body.ExceptionHandlers.Add(new(ExceptionHandlerType.Catch)
            {
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = catchEnd,
                CatchType = catchType
            });
        }

        private void CheckRefStruct(RouMethod rouMethod)
        {
            TypeDefinition typeDef;
            if ((rouMethod.MethodContextOmits & Omit.ReturnValue) == 0 && (typeDef = rouMethod.MethodDef.ReturnType.Resolve()) != null && typeDef.CustomAttributes.Any(x => x.Is(Constants.TYPE_IsByRefLikeAttribute)))
            {
                var builder = new StringBuilder($"Cannot save ref struct value as an object. Change the return value type of the {rouMethod.MethodDef.FullName} method or set the MethodContextOmits property for the listed types to Omit.ReturnValue: [");
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

                throw new RougamoException(builder.ToString(), rouMethod.MethodDef);
            }

            if ((rouMethod.MethodContextOmits & Omit.Arguments) == 0 && rouMethod.MethodDef.Parameters.Any(x => (typeDef = x.ParameterType.Resolve()) != null && typeDef.CustomAttributes.Any(y => y.Is(Constants.TYPE_IsByRefLikeAttribute))))
            {
                var builder = new StringBuilder($"Cannot save ref struct value as an object. Change the parameter type of the {rouMethod.MethodDef.FullName} method or set the MethodContextOmits property for the listed types to Omit.Arguments: [");
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

                throw new RougamoException(builder.ToString(), rouMethod.MethodDef);
            }
        }

        private void DebuggerStepThrough(MethodDefinition methodDef)
        {
            methodDef.CustomAttributes.Add(new CustomAttribute(_methodDebuggerStepThroughCtorRef));
        }

    }
}
