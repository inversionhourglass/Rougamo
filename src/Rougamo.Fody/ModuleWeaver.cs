using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
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

        private readonly Code[] _brs = new[] {
            Code.Leave, Code.Leave_S, Code.Br, Code.Br_S,
            Code.Beq, Code.Beq_S, Code.Bne_Un, Code.Bne_Un_S,
            Code.Bge, Code.Bge_S, Code.Bge_Un, Code.Bge_Un_S,
            Code.Bgt, Code.Bgt_S, Code.Bgt_Un, Code.Bgt_Un_S,
            Code.Ble, Code.Ble_S, Code.Ble_Un, Code.Ble_Un_S,
            Code.Blt, Code.Blt_S, Code.Blt_Un, Code.Blt_Un_S,
            Code.Brtrue, Code.Brtrue_S, Code.Brfalse, Code.Brfalse_S
        };

        private List<RouType> _rouTypes;

        public override void Execute()
        {
            if (Config != null && string.Equals(Config.Attributes("enabled").Select(x => x.Value).SingleOrDefault(), "false", System.StringComparison.OrdinalIgnoreCase)) return;

            LoadLocalAssemblies();
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

        private void LoadLocalAssemblies()
        {
            var folderPath = System.IO.Path.GetDirectoryName(AssemblyFilePath);
            var assemblyPaths = System.IO.Directory.GetFiles(folderPath, "*.dll");
            foreach (var assemblyPath in assemblyPaths)
            {
                if (assemblyPath == this.AssemblyFilePath)
                {
                    continue;
                }
                try
                {
                    //var binary = System.IO.File.ReadAllBytes(assemblyPath);
                    //System.Reflection.Assembly.Load(binary);
                    System.Reflection.Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
