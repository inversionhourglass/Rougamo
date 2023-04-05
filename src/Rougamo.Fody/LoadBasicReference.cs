using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _typeVoidRef = FindTypeDefinition(typeof(void).FullName).ImportInto(ModuleDefinition);
            _typeSystemRef = FindTypeDefinition(typeof(Type).FullName).ImportInto(ModuleDefinition);
            _typeMethodBaseRef = FindTypeDefinition(typeof(MethodBase).FullName).ImportInto(ModuleDefinition);
            _typeIntRef = FindTypeDefinition(typeof(int).FullName).ImportInto(ModuleDefinition);
            _typeBoolRef = FindTypeDefinition(typeof(bool).FullName).ImportInto(ModuleDefinition);
            _typeObjectRef = FindTypeDefinition(typeof(object).FullName).ImportInto(ModuleDefinition);
            _typeExceptionRef = FindTypeDefinition(typeof(Exception).FullName).ImportInto(ModuleDefinition);
            _typeListDef = FindTypeDefinition(typeof(List<>).FullName);
            _typeListRef = _typeListDef.ImportInto(ModuleDefinition);

            _methodGetTypeFromHandleRef = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
            _methodGetMethodFromHandleRef = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
            _methodListAddRef = _typeListDef.Methods.Single(x => x.Name == "Add" && x.Parameters.Count == 1 && x.Parameters[0].ParameterType == _typeListDef.GenericParameters[0]);
            _methodListToArrayRef = _typeListDef.Methods.Single(x => x.Name == "ToArray" && !x.HasParameters);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var rougamoAssembly = assemblies.Single(x => x.GetName().Name == nameof(Rougamo));
            var idiscoverer = Type.GetType($"{Constants.TYPE_IMethodDiscoverer}, {rougamoAssembly.FullName}");
            var isMatchMethod = idiscoverer.GetMethod(Constants.METHOD_IsMatch);
            var isMatch = isMatchMethod.CreateDelegate(typeof(Func<,,>).MakeGenericType(idiscoverer, typeof(MethodInfo), typeof(bool)));
        }
    }
}
