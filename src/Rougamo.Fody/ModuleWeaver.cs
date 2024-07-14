using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Models;
using Rougamo.Fody.Simulations.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
        private readonly bool _testRun;

        private RougamoRefImporter _importer;

        internal TypeDefinition _typeListDef;
        internal TypeDefinition _typeSystemDef;
        internal TypeDefinition _typeMethodBaseDef;

        internal TypeReference _typeVoidRef;
        internal TypeReference _typeSystemRef;
        internal TypeReference _typeMethodBaseRef;
        internal TypeReference _typeIntRef;
        internal TypeReference _typeBoolRef;
        internal TypeReference _typeObjectRef;
        internal TypeReference _typeExceptionRef;
        internal TypeReference _typeCancellationTokenRef;
        internal TypeReference _typeListRef;
        internal TypeReference _typeIMoRef;
        internal TypeReference _typeMethodContextRef;
        internal TypeReference _typeIMoArrayRef;
        internal TypeReference _typeDebuggerStepThroughAttributeRef;

        private TsAsyncable _typeValuetask;

        internal MethodReference _methodGetTypeFromHandleRef;
        internal MethodReference _methodGetMethodFromHandleRef;
        internal MethodReference _methodListAddRef;
        internal MethodReference _methodListToArrayRef;
        internal MethodReference _methodIEnumeratorMoveNextRef;
        internal MethodReference _methodDebuggerStepThroughCtorRef;
        internal MethodReference _methodExceptionDispatchInfoCaptureRef;
        internal MethodReference _methodExceptionDispatchInfoThrowRef;
        internal MethodReference _methodMethodContextCtorRef;
        internal MethodReference _methodMethodContext3CtorRef;
        internal MethodReference _methodMethodContextSetExceptionRef;
        internal MethodReference _methodMethodContextSetReturnValueRef;
        internal MethodReference _methodMethodContextGetReturnValueRef;
        internal MethodReference _methodMethodContextGetHasExceptionRef;
        internal MethodReference _methodMethodContextGetExceptionHandledRef;
        internal MethodReference _methodMethodContextGetReturnValueReplacedRef;
        internal MethodReference _methodMethodContextGetArgumentsRef;
        internal MethodReference _methodMethodContextGetRewriteArgumentsRef;
        internal MethodReference _methodMethodContextGetRetryCountRef;
        internal Dictionary<string, MethodReference> _methodIMosRef;
        internal Dictionary<string, MethodReference> _stateMachineCtorRefs;

        private List<RouType> _rouTypes;
        private Config _config;

        private readonly Instruction[] EmptyInstructions = [];

        public ModuleWeaver() : this(false) { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ModuleWeaver(bool testRun)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _testRun = testRun;
        }

        public override void Execute()
        {
            _importer = new(this);

            try
            {
                ReadConfig();
                if (!_config.Enabled) return;

                LoadBasicReference();
                GlobalRefs.Init(this);
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
    }
}
