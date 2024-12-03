using Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        protected override void LoadBasicReference()
        {
            base.LoadBasicReference();

            _tListRef = FindAndImportType(typeof(List<>).FullName);
            _tExceptionRef = FindAndImportType(typeof(Exception).FullName);
            _tCancellationTokenRef = FindAndImportType(typeof(CancellationToken).FullName);
            _tIAsyncStateMachineRef = FindAndImportType(typeof(IAsyncStateMachine).FullName);
            _tAsyncTaskMethodBuilderRef = FindAndImportType(typeof(AsyncTaskMethodBuilder).FullName);
            _tAsyncTaskMethodBuilder1Ref = FindAndImportType(typeof(AsyncTaskMethodBuilder<>).FullName);
            _tObjectArrayRef = new ArrayType(_tObjectRef);
            _tPoolRef = FindAndImportType(Constants.TYPE_RougamoPool);

            var typeDebuggerStepThroughAttributeRef = FindAndImportType(typeof(DebuggerStepThroughAttribute).FullName);
            var typeAsyncStateMachineAttributeRef = FindAndImportType(typeof(AsyncStateMachineAttribute).FullName);
            var typeIteratorStateMachineAttributeRef = FindAndImportType(typeof(IteratorStateMachineAttribute).FullName);
            var typeExceptionDispatchInfoDef = FindTypeDefinition(typeof(ExceptionDispatchInfo).FullName);
            var typeCompilerGeneratedAttributeDef = FindTypeDefinition(typeof(CompilerGeneratedAttribute).FullName);
            var typeDebuggerHiddenAttributeDef = FindTypeDefinition(typeof(DebuggerHiddenAttribute).FullName);
            TryFindTypeDefinition(Constants.TYPE_StackTraceHiddenAttribute, out var typeStackTraceHiddenAttributeDef);

            _ctorObjectRef = _tObjectRef.GetCtor(0).ImportInto(this);
            _ctorDebuggerStepThroughRef = typeDebuggerStepThroughAttributeRef.GetCtor(0).ImportInto(this);
            _ctorCompilerGeneratedAttributeRef = typeCompilerGeneratedAttributeDef.GetCtor(0).ImportInto(this);
            _ctorDebuggerHiddenAttributeRef = typeDebuggerHiddenAttributeDef.GetCtor(0).ImportInto(this);
            _ctorAsyncStateMachineAttributeRef = typeAsyncStateMachineAttributeRef.GetMethod(false, x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type))!.ImportInto(this);
            if (typeStackTraceHiddenAttributeDef != null) _ctorStackTraceHiddenAttributeRef = typeStackTraceHiddenAttributeDef.GetCtor(0).ImportInto(this);

            _mPoolGetRef = this.Import(_tPoolRef.GetMethod(Constants.METHOD_Get, false));
            _mPoolReturnRef = this.Import(_tPoolRef.GetMethod(Constants.METHOD_Return, false));
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
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            foreach (var item in base.GetAssembliesForScanning())
            {
                yield return item;
            }

            yield return "Rougamo";

            var assemblies = Configuration.Mos.Select(x => x.Assembly).Distinct();
            foreach (var assembly in assemblies)
            {
                yield return assembly;
            }
        }
    }
}
