using System;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void LoadBasicReference()
        {
            _systemTypeRef = FindTypeDefinition(typeof(Type).FullName).ImportInto(ModuleDefinition);
            _getTypeFromHandleRef = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }).ImportInto(ModuleDefinition);
        }
    }
}
