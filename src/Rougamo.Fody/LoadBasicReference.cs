using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _typeListDef = FindTypeDefinition(typeof(List<>).FullName);

            _typeVoidRef = FindTypeDefinition(typeof(void).FullName).ImportInto(ModuleDefinition);
            _typeSystemRef = FindTypeDefinition(typeof(Type).FullName).ImportInto(ModuleDefinition);
            _typeMethodBaseRef = FindTypeDefinition(typeof(MethodBase).FullName).ImportInto(ModuleDefinition);
            _typeIntRef = FindTypeDefinition(typeof(int).FullName).ImportInto(ModuleDefinition);
            _typeBoolRef = FindTypeDefinition(typeof(bool).FullName).ImportInto(ModuleDefinition);
            _typeObjectRef = FindTypeDefinition(typeof(object).FullName).ImportInto(ModuleDefinition);
            _typeExceptionRef = FindTypeDefinition(typeof(Exception).FullName).ImportInto(ModuleDefinition);
            _typeDebuggerStepThroughAttributeRef = FindTypeDefinition(typeof(DebuggerStepThroughAttribute).FullName).ImportInto(ModuleDefinition);
            _typeListRef = _typeListDef.ImportInto(ModuleDefinition);
            var typeAsyncStateMachineAttributeRef = FindTypeDefinition(typeof(AsyncStateMachineAttribute).FullName).ImportInto(ModuleDefinition);
            var typeIteratorStateMachineAttributeRef = FindTypeDefinition(typeof(IteratorStateMachineAttribute).FullName).ImportInto(ModuleDefinition);

            _methodGetTypeFromHandleRef = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
            _methodGetMethodFromHandleRef = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
            _methodListAddRef = _typeListDef.Methods.Single(x => x.Name == "Add" && x.Parameters.Count == 1 && x.Parameters[0].ParameterType == _typeListDef.GenericParameters[0]);
            _methodListToArrayRef = _typeListDef.Methods.Single(x => x.Name == "ToArray" && !x.HasParameters);
            _methodDebuggerStepThroughCtorRef = _typeDebuggerStepThroughAttributeRef.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic && !x.Parameters.Any()).ImportInto(ModuleDefinition);
            var asyncStateMachineAttributeCtorRef = typeAsyncStateMachineAttributeRef.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type)).ImportInto(ModuleDefinition);
            var iteratorStateMachineAttributeCtorRef = typeIteratorStateMachineAttributeRef.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type)).ImportInto(ModuleDefinition);

            _stateMachineCtorRefs = new()
            {
                { Constants.TYPE_AsyncStateMachineAttribute, asyncStateMachineAttributeCtorRef },
                { Constants.TYPE_IteratorStateMachineAttribute, iteratorStateMachineAttributeCtorRef }
            };
        }
    }
}
