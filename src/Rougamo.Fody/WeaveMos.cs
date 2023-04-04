using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Rougamo.Fody.Enhances;
using System.Collections.Generic;
using System.Linq;
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
                    if (rouMethod.MethodDef.IsEmpty())
                    {
                        EmptyMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncIterator)
                    {
                        AiteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncTaskOrValueTask)
                    {
                        AsyncTaskMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void EmptyMethodWeave(RouMethod rouMethod)
        {
            var bodyInstructions = rouMethod.MethodDef.Body.Instructions;
            var ret = bodyInstructions.Last();
            Instruction afterOnSuccessNop;
            if (bodyInstructions.Count > 1)
            {
                afterOnSuccessNop = ret.Previous;
            }
            else
            {
                afterOnSuccessNop = Create(OpCodes.Nop);
                bodyInstructions.Insert(bodyInstructions.Count - 1, afterOnSuccessNop);
            }

            var instructions = InitMoArrayVariable(rouMethod, out var mosVariable);
            var contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, mosVariable, false, false, instructions);

            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Length, mosVariable, contextVariable, instructions, false);

            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
            instructions.Add(Create(OpCodes.Brtrue_S, afterOnSuccessNop));

            ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.MethodDef, rouMethod.Mos.Length, mosVariable, contextVariable, instructions, _config.ReverseCallNonEntry);

            rouMethod.MethodDef.Body.Instructions.InsertBefore(afterOnSuccessNop, instructions);

            instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Length, mosVariable, contextVariable, instructions, _config.ReverseCallNonEntry);
            rouMethod.MethodDef.Body.Instructions.InsertAfter(afterOnSuccessNop, instructions);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        #region LoadMosOnStack

        private VariableDefinition[] LoadMosOnStack(RouMethod rouMethod, List<Instruction> instructions)
        {
            var mos = new VariableDefinition[rouMethod.Mos.Length];
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
                variable = methodBody.CreateVariable(Import(mo.Attribute.AttributeType));
                instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                instructions.Add(Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                instructions.Add(Create(OpCodes.Stloc, variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable));
                }
            }
            else
            {
                variable = methodBody.CreateVariable(Import(mo.TypeDef!));
                instructions.Add(Create(OpCodes.Newobj, Import(mo.TypeDef!.GetZeroArgsCtor())));
                instructions.Add(Create(OpCodes.Stloc, variable));
            }
            return variable;
        }

        private List<Instruction> InitMoArrayVariable(RouMethod rouMethod, out VariableDefinition mosVariable)
        {
            mosVariable = rouMethod.MethodDef.Body.CreateVariable(_typeIMoArrayRef);
            var instructions = InitMoArray(rouMethod.Mos);
            instructions.Add(Create(OpCodes.Stloc, mosVariable));

            return instructions;
        }

        private IList<Instruction> CreateTempMoArray(VariableDefinition[] moVariables)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, moVariables.Length),
                Create(OpCodes.Newarr, _typeIMoRef)
            };
            for (var i = 0; i < moVariables.Length; i++)
            {
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                instructions.Add(Create(OpCodes.Ldloc, moVariables[i]));
                instructions.Add(Create(OpCodes.Stelem_Ref));
            }

            return instructions;
        }

        private IList<Instruction> CreateTempMoArray(VariableDefinition stateMachineVariable, FieldReference[] moFields)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, moFields.Length),
                Create(OpCodes.Newarr, _typeIMoRef)
            };
            for (var i = 0; i < moFields.Length; i++)
            {
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                instructions.Add(stateMachineVariable.LdlocOrA());
                instructions.Add(Create(OpCodes.Ldfld, moFields[i]));
                instructions.Add(Create(OpCodes.Stelem_Ref));
            }

            return instructions;
        }

        private List<Instruction> InitMoArray(Mo[] mos)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, mos.Length),
                Create(OpCodes.Newarr, _typeIMoRef)
            };
            var i = 0;
            foreach (var mo in mos)
            {
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                instructions.AddRange(InitMo(mo));
                instructions.Add(Create(OpCodes.Stelem_Ref));
                i++;
            }

            return instructions;
        }

        private IList<Instruction> InitMo(Mo mo)
        {
            if (mo.Attribute != null)
            {
                var instructions = new List<Instruction>();
                instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                instructions.Add(Create(OpCodes.Newobj, Import(mo.Attribute.Constructor)));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyDup(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties));
                }

                return instructions;
            }
            
            return new Instruction[] { Create(OpCodes.Newobj, Import(mo.TypeDef!.GetZeroArgsCtor())) };
        }

        private Collection<Instruction> LoadAttributeArgumentIns(Collection<CustomAttributeArgument> arguments)
        {
            var instructions = new Collection<Instruction>();
            foreach (var arg in arguments)
            {
                instructions.Add(LoadValueOnStack(arg.Type, arg.Value));
            }
            return instructions;
        }

        private Collection<Instruction> LoadAttributePropertyIns(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties, VariableDefinition attributeDef)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Create(OpCodes.Ldloc, attributeDef));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyDup(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Create(OpCodes.Dup));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition CreateMethodContextVariable(MethodDefinition methodDef, VariableDefinition mosVariable, bool isAsync, bool isIterator, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            InitMethodContext(methodDef, isAsync, isIterator, mosVariable, null, null, instructions);
            instructions.Add(Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void InitMethodContext(MethodDefinition methodDef, bool isAsync, bool isIterator, VariableDefinition? mosVariable, VariableDefinition? stateMachineVariable, FieldReference? mosFieldRef, List<Instruction> instructions)
        {
            var isAsyncCode = isAsync ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var isIteratorCode = isIterator ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var mosNonEntryFIFO = _config.ReverseCallNonEntry ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1;
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.Add(Create(isAsyncCode));
            instructions.Add(Create(isIteratorCode));
            instructions.Add(Create(mosNonEntryFIFO));
            if (stateMachineVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldloc, mosVariable));
            }
            else
            {
                instructions.Add(stateMachineVariable.LdlocOrA());
                instructions.Add(Create(OpCodes.Ldfld, mosFieldRef));
            }
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef));
            instructions.Add(Create(OpCodes.Newobj, _methodMethodContextCtorRef));
        }

        private List<Instruction> InitMethodContext(MethodDefinition methodDef, bool isAsync, bool isIterator, VariableDefinition? moArrayVariable, VariableDefinition? stateMachineVariable, FieldReference? mosFieldRef)
        {
            var instructions = new List<Instruction>();

            var isAsyncCode = isAsync ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var isIteratorCode = isIterator ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var mosNonEntryFIFO = _config.ReverseCallNonEntry ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1;
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.Add(Create(isAsyncCode));
            instructions.Add(Create(isIteratorCode));
            instructions.Add(Create(mosNonEntryFIFO));
            if (mosFieldRef == null)
            {
                instructions.Add(Create(OpCodes.Ldloc, moArrayVariable));
            }
            else
            {
                instructions.Add(stateMachineVariable!.LdlocOrA());
                instructions.Add(Create(OpCodes.Ldfld, mosFieldRef));
            }
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef));
            instructions.Add(Create(OpCodes.Newobj, _methodMethodContextCtorRef));

            return instructions;
        }

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, VariableDefinition moArrayVariable, VariableDefinition contextVariable, List<Instruction> instructions, bool reverseCall)
        {
            var loopExit = Create(OpCodes.Nop);

            instructions.AddRange(ExecuteMoMethod(methodName, methodDef, mosCount, loopExit, moArrayVariable, contextVariable, null, null, reverseCall));

            instructions.Add(loopExit);
        }

        private List<Instruction> ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, Instruction loopExit, VariableDefinition? mosVariable, VariableDefinition? contextVariable, FieldReference? mosField, FieldReference? contextField, bool reverseCall)
        {
            var instructions = new List<Instruction>();
            var flagVariable = methodDef.Body.CreateVariable(_typeIntRef);

            Instruction loopFirst;

            if (reverseCall)
            {
                instructions.Add(Create(OpCodes.Ldc_I4, mosCount));
                instructions.Add(Create(OpCodes.Ldc_I4_1));
                instructions.Add(Create(OpCodes.Sub));
                instructions.Add(Create(OpCodes.Stloc, flagVariable));
                loopFirst = Create(OpCodes.Ldloc, flagVariable);
                instructions.Add(loopFirst);
                instructions.Add(Create(OpCodes.Ldc_I4_0));
                instructions.Add(Create(OpCodes.Clt));
                instructions.Add(Create(OpCodes.Brtrue, loopExit));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldc_I4_0));
                instructions.Add(Create(OpCodes.Stloc, flagVariable));
                loopFirst = Create(OpCodes.Ldloc, flagVariable);
                instructions.Add(loopFirst);
                instructions.Add(Create(OpCodes.Ldc_I4, mosCount));
                instructions.Add(Create(OpCodes.Clt));
                instructions.Add(Create(OpCodes.Brfalse_S, loopExit));
            }

            if (mosVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, mosField));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldloc, mosVariable));
            }
            instructions.Add(Create(OpCodes.Ldloc, flagVariable));
            instructions.Add(Create(OpCodes.Ldelem_Ref));
            if (contextVariable == null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, contextField));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
            instructions.Add(Create(OpCodes.Ldloc, flagVariable));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            if (reverseCall)
            {
                instructions.Add(Create(OpCodes.Sub));
            }
            else
            {
                instructions.Add(Create(OpCodes.Add));
            }
            instructions.Add(Create(OpCodes.Stloc, flagVariable));
            instructions.Add(Create(OpCodes.Br_S, loopFirst));

            return instructions;
        }

        private List<Instruction> ExecuteMoMethod(string methodName, int mosCount, VariableDefinition[]? moVariables, VariableDefinition? contextVariable, FieldReference[]? moFields, FieldReference? contextField, bool reverseCall)
        {
            var instructions = new List<Instruction>();

            for (int i = 0; i < mosCount; i++)
            {
                var j = reverseCall ? mosCount - i - 1 : i;
                
                if (moVariables == null)
                {
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, moFields![j]));
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, contextField));
                    instructions.Add(Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
                }
                else
                {
                    instructions.Add(Create(OpCodes.Ldloc, moVariables![j]));
                    instructions.Add(Create(OpCodes.Ldloc, contextVariable));
                    instructions.Add(Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
                }
            }

            return instructions;
        }

        private ExceptionHandler GetOuterExceptionHandler(MethodDefinition methodDef)
        {
            ExceptionHandler? exceptionHandler = null;
            int offset = methodDef.Body.Instructions.First().Offset;
            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                if (handler.HandlerType != ExceptionHandlerType.Catch) continue;
                if (handler.TryEnd.Offset > offset)
                {
                    exceptionHandler = handler;
                    offset = handler.TryEnd.Offset;
                }
            }
            return exceptionHandler ?? throw new RougamoException($"[{methodDef.FullName}] can not find outer exception handler");
        }

        private void SetTryCatchFinally(int features, MethodDefinition methodDef, ITryCatchFinallyAnchors anchors)
        {
            if (Feature.OnException.IsMatch(features))
            {
                var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    CatchType = _typeExceptionRef,
                    TryStart = anchors.TryStart,
                    TryEnd = anchors.CatchStart,
                    HandlerStart = anchors.CatchStart,
                    HandlerEnd = anchors.FinallyStart
                };
                methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            }

            if ((features & (int)(Feature.OnSuccess | Feature.OnExit)) != 0 || Feature.ExceptionHandle.IsMatch(features))
            {
                var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                {
                    TryStart = anchors.TryStart,
                    TryEnd = anchors.FinallyStart,
                    HandlerStart = anchors.FinallyStart,
                    HandlerEnd = anchors.FinallyEnd
                };
                methodDef.Body.ExceptionHandlers.Add(finallyHandler);
            }
        }
    }
}
