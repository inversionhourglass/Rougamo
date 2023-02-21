using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rougamo.Fody
{
    public partial class ModuleWeaver : BaseModuleWeaver
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private TypeReference _typeVoidRef;
        private TypeReference _typeSystemRef;
        private TypeReference _typeMethodBaseRef;
        private TypeReference _typeIntRef;
        private TypeReference _typeBoolRef;
        private TypeReference _typeObjectRef;
        private TypeReference _typeObjectArrayRef;
        private TypeReference _typeExceptionRef;
        private TypeDefinition _typeListDef;
        private TypeReference _typeListRef;
        private TypeReference _typeIMoRef;
        private TypeReference _typeMethodContextRef;
        private TypeReference _typeIMoArrayRef;

        private MethodReference _methodGetTypeFromHandleRef;
        private MethodReference _methodGetMethodFromHandleRef;
        private MethodReference _methodListAddRef;
        private MethodReference _methodListToArrayRef;
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

        // Private fields cannot be accessed externally even using IL
        //private FieldReference _fieldMethodContextExceptionRef;
        //private FieldReference _fieldMethodContextReturnValueRef;

        private List<RouType> _rouTypes;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public override void Execute()
        {
            if (!this.ConfigEnabled()) return;

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
                    System.Reflection.Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                    // ignore
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeReference Import(TypeReference typeRef)
        {
            return ModuleDefinition.ImportReference(typeRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MethodReference Import(MethodReference methodRef)
        {
            return ModuleDefinition.ImportReference(methodRef);
        }
    }
}
