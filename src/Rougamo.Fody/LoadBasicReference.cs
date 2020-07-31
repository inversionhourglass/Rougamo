using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        TypeDefinition _rougamoTd;

        void LoadBasicReference()
        {
            _rougamoTd = FindType("Rougamo.IMo");
        }
    }
}
