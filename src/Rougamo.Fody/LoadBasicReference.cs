using System;
using System.Reflection;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _typeSystemRef = FindTypeDefinition(typeof(Type).FullName).ImportInto(ModuleDefinition);
            _typeMethodBaseRef = FindTypeDefinition(typeof(MethodBase).FullName).ImportInto(ModuleDefinition);
            _typeIntRef = FindTypeDefinition(typeof(int).FullName).ImportInto(ModuleDefinition);
            _typeObjectRef = FindTypeDefinition(typeof(object).FullName).ImportInto(ModuleDefinition);
            _typeExceptionRef = FindTypeDefinition(typeof(Exception).FullName).ImportInto(ModuleDefinition);

            _methodGetTypeFromHandleRef = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
            _methodGetMethodFromHandleRef = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
        }
    }
}
