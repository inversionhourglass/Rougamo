using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

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
                    if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsync)
                    {
                        AsyncMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void IteratorMethodWeave(RouMethod rouMethod)
        {

        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {
        }

        private void SyncMethodWeave(RouMethod rouMethod)
        {
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            OnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
            OnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            OnSuccess(rouMethod.MethodDef, moVariables, contextVariable, finallyStart, finallyEnd);
            OnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyEnd);
        }

        private void GenerateTryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction finallyEnd, out VariableDefinition exceptionVariable)
        {
            var skipRedirects = new List<Instruction>();
            var bodyIns = methodDef.Body.Instructions;
            tryStart = bodyIns.First();
            while (tryStart.OpCode != OpCodes.Nop || tryStart.Next.OpCode == OpCodes.Ret)
            {
                tryStart = Instruction.Create(OpCodes.Nop);
                bodyIns.Insert(0, tryStart);
            }

            var returns = methodDef.Body.Instructions.Where(ins => ins.OpCode == OpCodes.Ret);
            if (returns.Count() != 1)
            {
                throw new RougamoException($"[{methodDef.FullName}] multiple ret found");
            }
            var returnIns = returns.Single();
            finallyEnd = returnIns.Previous;
            if (methodDef.ReturnType.Is(Constants.TYPE_Void))
            {
                if (finallyEnd == null || finallyEnd.OpCode != OpCodes.Nop)
                {
                    finallyEnd = Instruction.Create(OpCodes.Nop);
                    methodDef.Body.Instructions.InsertBefore(returnIns, finallyEnd);
                }
            }
            else
            {
                while (true)
                {
                    if (finallyEnd == null) throw new RougamoException($"[{methodDef.FullName}] not found Ldloc before ret");
                    if (finallyEnd.OpCode == OpCodes.Ldloc_0 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_1 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_2 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_3 ||
                        finallyEnd.OpCode == OpCodes.Ldloc ||
                        finallyEnd.OpCode == OpCodes.Ldloc_S) break;
                    finallyEnd = finallyEnd.Previous;
                }
            }
            if (finallyEnd.Previous != null && (finallyEnd.Previous.OpCode == OpCodes.Br || finallyEnd.Previous.OpCode == OpCodes.Br_S))
            {
                finallyEnd.Previous.OpCode = OpCodes.Nop;
                finallyEnd.Previous.Operand = null;
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            bodyIns.InsertBefore(finallyEnd, skipRedirects.AddAndGet(Instruction.Create(OpCodes.Leave_S, finallyEnd)));
            catchStart = Instruction.Create(OpCodes.Stloc, exceptionVariable);
            bodyIns.InsertBefore(finallyEnd, catchStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Rethrow));
            bodyIns.InsertBefore(finallyEnd, skipRedirects.AddAndGet(Instruction.Create(OpCodes.Leave_S, finallyEnd)));
            finallyStart = Instruction.Create(OpCodes.Nop);
            bodyIns.InsertBefore(finallyEnd, finallyStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Endfinally));

            AdjustRedirects(methodDef, skipRedirects, catchStart, finallyEnd);
        }

        private HashSet<OpCode> _redirectOps = new HashSet<OpCode>
        {
            OpCodes.Leave, OpCodes.Leave_S, OpCodes.Br, OpCodes.Br_S,
            OpCodes.Brtrue, OpCodes.Brtrue_S, OpCodes.Brfalse, OpCodes.Brfalse_S,
        };
        private void AdjustRedirects(MethodDefinition methodDef, List<Instruction> skipRedirects, Instruction catchStart, Instruction finallyEnd)
        {
            var redirectToEnds = methodDef.Body.Instructions.Where(x => _redirectOps.Contains(x.OpCode) && x.Operand == finallyEnd && !skipRedirects.Contains(x));
            if (redirectToEnds.Any())
            {
                foreach (var item in redirectToEnds)
                {
                    item.Operand = catchStart.Previous;
                }
            }

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                if (handler.HandlerEnd == finallyEnd)
                {
                    handler.HandlerEnd = catchStart.Previous;
                }
            }
        }

        private void SetTryCatchFinally(MethodDefinition methodDef, Instruction tryStart, Instruction catchStart, Instruction finallyStart, Instruction finallyEnd)
        {
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = _typeExceptionRef,
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = finallyStart
            };
            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = finallyEnd
            };

            methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            methodDef.Body.ExceptionHandlers.Add(finallyHandler);
        }

        private void OnEntry(RouMethod rouMethod, Instruction tryStart, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var ins = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, ins);
            contextVariable = InitMethodContext(rouMethod.MethodDef, ins);
            ExecuteMoMethod(Constants.METHOD_OnEntry, moVariables, contextVariable, ins);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(tryStart, ins);
        }

        private void OnException(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            ins.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertAfter(catchStart, ins);
        }

        private void OnSuccess(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart, Instruction finallyEnd)
        {
            var successEnd = Instruction.Create(OpCodes.Nop);
            methodDef.Body.Instructions.InsertAfter(finallyStart, successEnd);

            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef));
            ins.Add(Instruction.Create(OpCodes.Brtrue_S, successEnd));
            if (finallyEnd.OpCode != OpCodes.Nop)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
                ins.Add(finallyEnd.Copy());
                if (methodDef.ReturnType.IsValueType || methodDef.ReturnType.IsEnum(out _) && !methodDef.ReturnType.IsArray)
                {
                    ins.Add(Instruction.Create(OpCodes.Box, methodDef.ReturnType.ImportInto(ModuleDefinition)));
                }
                ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertAfter(finallyStart, ins);
        }

        private void OnExit(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyEnd)
        {
            var ins = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertBefore(finallyEnd.Previous, ins);
        }
        #region LoadMosOnStack

        private VariableDefinition[] LoadMosOnStack(RouMethod rouMethod, List<Instruction> instructions)
        {
            var mos = new VariableDefinition[rouMethod.Mos.Count];
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                mos[i++] = LoadMoOnStack(mo, rouMethod.MethodDef.Body, instructions);
            }
            return mos;
        }

        private VariableDefinition LoadMoOnStack(Mo mo, MethodBody methodBody, List<Instruction> instructions)
        {
            VariableDefinition variable;
            if (mo.Attribute != null)
            {
                variable = methodBody.CreateVariable(mo.Attribute.AttributeType.ImportInto(ModuleDefinition));
                instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                instructions.Add(Instruction.Create(OpCodes.Newobj, mo.Attribute.Constructor.ImportInto(ModuleDefinition)));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable));
                }
            }
            else
            {
                variable = methodBody.CreateVariable(mo.TypeDef.ImportInto(ModuleDefinition));
                instructions.Add(Instruction.Create(OpCodes.Newobj, mo.TypeDef.GetZeroArgsCtor().ImportInto(ModuleDefinition)));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
            }
            return variable;
        }

        private Collection<Instruction> LoadAttributeArgumentIns(Collection<CustomAttributeArgument> arguments)
        {
            var ins = new Collection<Instruction>();
            foreach (var arg in arguments)
            {
                ins.Add(LoadValueOnStack(arg.Type, arg.Value));
            }
            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyIns(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties, VariableDefinition attributeDef)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, attributeDef));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
                ins.Add(Instruction.Create(OpCodes.Nop));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition InitMethodContext(MethodDefinition methodDef, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            //instructions.AddRange(LoadMethodArgumentsOnStack(methodDef, out var argumentsVariable)); // variable
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef)); // dup
            //instructions.Add(Instruction.Create(OpCodes.Ldloc, argumentsVariable)); // variable
            instructions.Add(Instruction.Create(OpCodes.Newobj, _methodMethodContextCtorRef));
            instructions.Add(Instruction.Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void ExecuteMoMethod(string methodName, VariableDefinition[] mos, VariableDefinition methodContext, List<Instruction> instructions)
        {
            foreach (var mo in mos)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, mo));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, methodContext));
                var methodRef = mo.VariableType.Resolve().RecursionImportMethod(ModuleDefinition, methodName, md => md.Parameters.Count == 1 && md.Parameters.First().ParameterType.Is(Constants.TYPE_MethodContext));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, methodRef));
            }
        }
    }
}
