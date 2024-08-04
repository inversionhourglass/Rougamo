using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Models;
using Rougamo.Fody.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
        private readonly bool _testRun;
        private bool _debugMode;

        internal TypeReference _tValueTypeRef;
        internal TypeReference _tObjectRef;
        internal TypeReference _tVoidRef;
        internal TypeReference _tInt32Ref;
        internal TypeReference _tBooleanRef;
        internal TypeReference _tObjectArrayRef;
        internal TypeReference _tTypeRef;
        internal TypeReference _tMethodBaseRef;
        internal TypeReference _tListRef;
        internal TypeReference _tExceptionRef;
        internal TypeReference _tCancellationTokenRef;
        internal TypeReference _tIAsyncStateMachineRef;
        internal TypeReference _tAsyncTaskMethodBuilderRef;
        internal TypeReference _tAsyncTaskMethodBuilder1Ref;
        internal TypeReference _tValueTaskRef;
        internal TypeReference _tValueTaskAwaiterRef;
        internal TypeReference _tIMoArrayRef;
        internal TypeReference _tMethodContextRef;

        internal MethodReference _ctorObjectRef;
        internal MethodReference _ctorDebuggerStepThroughRef;
        internal MethodReference _ctorCompilerGeneratedAttributeRef;
        internal MethodReference _ctorDebuggerHiddenAttributeRef;
        internal MethodReference _ctorAsyncStateMachineAttributeRef;

        internal MethodReference _mGetTypeFromHandleRef;
        internal MethodReference _mGetMethodFromHandleRef;
        internal MethodReference _mExceptionDispatchInfoCaptureRef;
        internal MethodReference _mIAsyncStateMachineMoveNextRef;
        internal MethodReference _mIAsyncStateMachineSetStateMachineRef;
        internal MethodReference _mExceptionDispatchInfoThrowRef;
        internal Dictionary<string, MethodReference> _stateMachineCtorRefs;

#if DEBUG
        internal MethodReference _methodDebuggerBreakRef;
#endif

        internal GlobalSimulations _simulations;

        private List<RouType> _rouTypes;
        private Config _config;


        public ModuleWeaver() : this(false) { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ModuleWeaver(bool testRun)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _testRun = testRun;
        }

        public override void Execute()
        {
            _debugMode = IsDebugMode();

            try
            {
                ReadConfig();
                if (!_config.Enabled) return;

                LoadBasicReference();
                _simulations = new(this);
                FindRous();
                if (_rouTypes.Count == 0) return;
                WeaveMos();
            }
            catch (RougamoException e)
            {
                if (_testRun) throw;

                if (e.MethodDef == null)
                {
                    WriteError(e.Message);
                }
                else
                {
                    WriteError(e.Message, e.MethodDef);
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "netstandard";
            yield return "mscorlib";
            yield return "System";
            yield return "System.Runtime";
            yield return "System.Core";
        }

        private void ReadConfig()
        {
            var enabled = "true".Equals(GetConfigValue("true", "enabled"), StringComparison.OrdinalIgnoreCase);
            var compositeAccessibility = "true".Equals(GetConfigValue("false", "composite-accessibility"), StringComparison.OrdinalIgnoreCase);
            var moArrayThreshold = int.Parse(GetConfigValue("4", "moarray-threshold"));
#if DEBUG
            var recordingIteratorReturns = true;
#else
            var recordingIteratorReturns = "true".Equals(GetConfigValue("false", "iterator-returns", "enumerable-returns"), StringComparison.OrdinalIgnoreCase);
#endif
            var reverseCallNonEntry = "true".Equals(GetConfigValue("true", "reverse-call-nonentry", "reverse-call-ending"), StringComparison.OrdinalIgnoreCase);
            var proxyCalling = "true".Equals(GetConfigValue("true", "proxy-calling"), StringComparison.OrdinalIgnoreCase);
            var forceAsyncSyntax = "true".Equals(GetConfigValue("true", "force-async-syntax"), StringComparison.OrdinalIgnoreCase);
            var exceptTypePatterns = GetConfigValue(string.Empty, "except-type-patterns").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            _config = new Config(enabled, compositeAccessibility, moArrayThreshold, recordingIteratorReturns, reverseCallNonEntry, proxyCalling, forceAsyncSyntax, exceptTypePatterns);
        }

        private string GetConfigValue(string defaultValue, params string[] configKeys)
        {
            if (Config == null) return defaultValue;

            foreach (var configKey in configKeys)
            {
                var configAttribute = Config.Attributes(configKey).SingleOrDefault();
                if (configAttribute != null) return configAttribute.Value;
            }

            return defaultValue;
        }

        public bool IsDebugMode()
        {
            var debuggableAttribute = ModuleDefinition.Assembly.CustomAttributes.SingleOrDefault(x => x.Is(Constants.TYPE_DebuggableAttribute));
            if (debuggableAttribute == null) return false;
            if (debuggableAttribute.ConstructorArguments.Count == 1 && debuggableAttribute.ConstructorArguments[0].Value is int modes) return (modes & 0x100) != 0;
            if (debuggableAttribute.ConstructorArguments.Count == 2 && debuggableAttribute.ConstructorArguments[1].Value is bool isJITOptimizerDisabled) return isJITOptimizerDisabled;

            return false;
        }

        private void Debugger(IList<Instruction> instructions, RouMethod rouMethod, string methodName)
        {
#if DEBUG
            if (rouMethod.MethodDef.Name == methodName) instructions.Add(Instruction.Create(OpCodes.Call, _methodDebuggerBreakRef));
#endif
        }
    }
}
