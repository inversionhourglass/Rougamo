using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMethodContext(TypeReference typeRef, IHost? host, ModuleDefinition moduldeDef) : TypeSimulation(typeRef, host, moduldeDef)
    {
    }
}
