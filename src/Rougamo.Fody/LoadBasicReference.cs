using Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _tValueTypeRef = FindAndImportType(typeof(ValueType).FullName);
            _tObjectRef = FindAndImportType(typeof(object).FullName);
            _tVoidRef = FindAndImportType(typeof(void).FullName);
            _tInt32Ref = FindAndImportType(typeof(int).FullName);
            _tBooleanRef = FindAndImportType(typeof(bool).FullName);
            _tTypeRef = FindAndImportType(typeof(Type).FullName);
            _tMethodBaseRef = FindAndImportType(typeof(MethodBase).FullName);
            _tListRef = FindAndImportType(typeof(List<>).FullName);
            _tExceptionRef = FindAndImportType(typeof(Exception).FullName);
            _tCancellationTokenRef = FindAndImportType(typeof(CancellationToken).FullName);
            _tIAsyncStateMachineRef = FindAndImportType(typeof(IAsyncStateMachine).FullName);
            _tAsyncTaskMethodBuilderRef = FindAndImportType(typeof(AsyncTaskMethodBuilder).FullName);
            _tAsyncTaskMethodBuilder1Ref = FindAndImportType(typeof(AsyncTaskMethodBuilder<>).FullName);
            _tObjectArrayRef = new ArrayType(_tObjectRef);

            var typeDebuggerStepThroughAttributeRef = FindAndImportType(typeof(DebuggerStepThroughAttribute).FullName);
            var typeAsyncStateMachineAttributeRef = FindAndImportType(typeof(AsyncStateMachineAttribute).FullName);
            var typeIteratorStateMachineAttributeRef = FindAndImportType(typeof(IteratorStateMachineAttribute).FullName);
            var typeExceptionDispatchInfoDef = FindTypeDefinition(typeof(ExceptionDispatchInfo).FullName);
            var typeCompilerGeneratedAttributeDef = FindTypeDefinition(typeof(CompilerGeneratedAttribute).FullName);
            var typeDebuggerHiddenAttributeDef = FindTypeDefinition(typeof(DebuggerHiddenAttribute).FullName);

            _ctorObjectRef = _tObjectRef.GetCtor(0).ImportInto(this);
            _ctorDebuggerStepThroughRef = typeDebuggerStepThroughAttributeRef.GetCtor(0).ImportInto(this);
            _ctorCompilerGeneratedAttributeRef = typeCompilerGeneratedAttributeDef.GetCtor(0).ImportInto(this);
            _ctorDebuggerHiddenAttributeRef = typeDebuggerHiddenAttributeDef.GetCtor(0).ImportInto(this);
            _ctorAsyncStateMachineAttributeRef = typeAsyncStateMachineAttributeRef.GetMethod(false, x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type))!.ImportInto(this);

            _mGetTypeFromHandleRef = _tTypeRef.GetMethod(Constants.METHOD_GetTypeFromHandle, false).ImportInto(this);
            _mGetMethodFromHandleRef = _tMethodBaseRef.GetMethod(false, x => x.Name == Constants.METHOD_GetMethodFromHandle && x.Parameters.Count == 2)!.ImportInto(this);
            _mExceptionDispatchInfoCaptureRef = typeExceptionDispatchInfoDef.GetStaticMethod(Constants.METHOD_Capture, false).ImportInto(this);
            _mExceptionDispatchInfoThrowRef = typeExceptionDispatchInfoDef.GetMethod(false, x => x.Parameters.Count == 0 && x.Name == Constants.METHOD_Throw)!.ImportInto(this);
            _mIAsyncStateMachineMoveNextRef = _tIAsyncStateMachineRef.GetMethod(Constants.METHOD_MoveNext, false)!.ImportInto(this);
            _mIAsyncStateMachineSetStateMachineRef = _tIAsyncStateMachineRef.GetMethod(Constants.METHOD_SetStateMachine, false)!.ImportInto(this);
            
            var iteratorStateMachineAttributeCtorRef = typeIteratorStateMachineAttributeRef.GetMethod(false, x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type))!.ImportInto(this);

            _stateMachineCtorRefs = new()
            {
                { Constants.TYPE_AsyncStateMachineAttribute, _ctorAsyncStateMachineAttributeRef },
                { Constants.TYPE_IteratorStateMachineAttribute, iteratorStateMachineAttributeCtorRef }
            };

#if DEBUG
            var debuggerTypeRef = this.Import(FindTypeDefinition(typeof(Debugger).FullName));
            _methodDebuggerBreakRef = this.Import(debuggerTypeRef.GetMethod(false, x => x.IsStatic && x.Name == "Break")!);
#endif
        }

        private TypeReference FindAndImportType(string fullName)
        {
            return ModuleDefinition.ImportReference(FindTypeDefinition(fullName));
        }
    }
}
