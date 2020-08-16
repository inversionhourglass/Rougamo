using Fody;
using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
        private TypeReference _systemTypeRef;

        private MethodReference _getTypeFromHandleRef;

        private List<RouType> _rouTypes;

        public override void Execute()
        {
            LoadBasicReference();
            FindRous();
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
