using Fody;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
        private TypeReference _typeSystemRef;
        private TypeReference _typeMethodBaseRef;
        private TypeReference _typeIntRef;
        private TypeReference _typeObjectRef;
        private TypeReference _typeObjectArrayRef;
        private TypeReference _typeExceptionRef;
        private TypeReference _typeIMoRef;
        private TypeReference _typeMethodContextRef;
        private TypeReference _typeIMoArrayRef;

        private MethodReference _methodGetTypeFromHandleRef;
        private MethodReference _methodGetMethodFromHandleRef;
        private MethodReference _methodMethodContextCtorRef;
        private MethodReference _methodMethodContextSetExceptionRef;
        private MethodReference _methodMethodContextSetReturnValueRef;
        private MethodReference _methodMethodContextGetHasExceptionRef;
        private Dictionary<string, MethodReference> _methodIMosRef;

        private List<RouType> _rouTypes;

        public override void Execute()
        {
            if (Config != null && string.Equals(Config.Attributes("enabled").Select(x => x.Value).SingleOrDefault(), "false", System.StringComparison.OrdinalIgnoreCase)) return;

            LoadBasicReference();
            FindRous();
            if (_rouTypes.Count == 0) return;
            WeaveMos();
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "netstandard";
            yield return "mscorlib";
            yield return "System";
            yield return "System.Runtime";
            yield return "System.Core";
        }
    }
}
