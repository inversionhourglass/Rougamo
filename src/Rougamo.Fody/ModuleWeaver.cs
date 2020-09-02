using Fody;
using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
        private TypeReference _systemTypeRef;
        private TypeReference _methodBaseTypeRef;
        private TypeReference _intTypeRef;
        private TypeReference _objectTypeRef;
        private TypeReference _imoTypeRef;
        private TypeReference _entryContextTypeRef;
        private TypeReference _successContextTypeRef;
        private TypeReference _exceptionContextTypeRef;
        private TypeReference _exitContextTypeRef;
        private TypeReference _objectArrayTypeRef;

        private MethodReference _getTypeFromHandleRef;

        private List<RouType> _rouTypes;

        public override void Execute()
        {
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
