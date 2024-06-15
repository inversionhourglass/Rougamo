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

        private RougamoRefImporter _importer;

        private TypeDefinition _typeListDef;
        private TypeDefinition _typeSystemDef;
        private TypeDefinition _typeMethodBaseDef;

        private TypeReference _typeVoidRef;
        private TypeReference _typeSystemRef;
        private TypeReference _typeIntRef;
        private TypeReference _typeBoolRef;
        private TypeReference _typeObjectRef;
        private TypeReference _typeExceptionRef;
        private TypeReference _typeCancellationTokenRef;
        private TypeReference _typeListRef;
        private TypeReference _typeIMoRef;
        private TypeReference _typeMethodContextRef;
        private TypeReference _typeIMoArrayRef;
        private TypeReference _typeDebuggerStepThroughAttributeRef;

        private TsAsyncable _typeValuetask;

        private MethodReference _methodGetTypeFromHandleRef;
        private MethodReference _methodGetMethodFromHandleRef;
        private MethodReference _methodListAddRef;
        private MethodReference _methodListToArrayRef;
        private MethodReference _methodIEnumeratorMoveNextRef;
        private MethodReference _methodDebuggerStepThroughCtorRef;
        private MethodReference _methodMethodContextCtorRef;
        private MethodReference _methodMethodContextSetExceptionRef;
        private MethodReference _methodMethodContextSetReturnValueRef;
        private MethodReference _methodMethodContextGetReturnValueRef;
        private MethodReference _methodMethodContextGetHasExceptionRef;
        private MethodReference _methodMethodContextGetExceptionHandledRef;
        private MethodReference _methodMethodContextGetReturnValueReplacedRef;
        private MethodReference _methodMethodContextGetArgumentsRef;
        private MethodReference _methodMethodContextGetRewriteArgumentsRef;
        private MethodReference _methodMethodContextGetRetryCountRef;
        private Dictionary<string, MethodReference> _methodIMosRef;
        private Dictionary<string, MethodReference> _stateMachineCtorRefs;

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
