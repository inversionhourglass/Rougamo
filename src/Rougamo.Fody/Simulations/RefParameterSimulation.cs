using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal class RefParameterSimulation(ParameterSimulation parameter) : ParameterSimulation(parameter.Module)
    {
        public override Instruction[] Load() => parameter.LoadAddress();

        public override Instruction[] LoadAddress() => parameter.LoadAddress();
    }
}
