using System;
using System.Reflection;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _systemTypeRef = FindTypeDefinition(typeof(Type).FullName).ImportInto(ModuleDefinition);
            _methodBaseTypeRef = FindTypeDefinition(typeof(MethodBase).FullName).ImportInto(ModuleDefinition);
            _intTypeRef = FindTypeDefinition(typeof(int).FullName).ImportInto(ModuleDefinition);
            _objectTypeRef = FindTypeDefinition(typeof(object).FullName).ImportInto(ModuleDefinition);

            _getTypeFromHandleRef = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
        }
    }
}
