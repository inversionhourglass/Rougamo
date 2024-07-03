using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal interface IParameterSimulation
    {
        Instruction[]? PrepareLoad(MethodSimulation method);

        Instruction[]? PrepareLoadAddress(MethodSimulation method);

        Instruction[] Load(MethodSimulation method);

        Instruction[] LoadAddress(MethodSimulation method);
    }
}
