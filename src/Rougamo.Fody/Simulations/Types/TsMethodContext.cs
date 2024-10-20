using Fody;
using Fody.Simulations;
using Fody.Simulations.Types;
using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMethodContext(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        public PropertySimulation P_Mos => PropertySimulate(Constants.PROP_Mos, false);

        public PropertySimulation P_Target => PropertySimulate(Constants.PROP_Target, false);

        public PropertySimulation P_TargetType => PropertySimulate(Constants.PROP_TargetType, false);

        public PropertySimulation P_Method => PropertySimulate(Constants.PROP_Method, false);

        public PropertySimulation P_ReturnValueReplaced => PropertySimulate(Constants.PROP_ReturnValueReplaced, false);

        public PropertySimulation P_ReturnValue => PropertySimulate(Constants.PROP_ReturnValue, false);

        public PropertySimulation P_RewriteArguments => PropertySimulate(Constants.PROP_RewriteArguments, false);

        public PropertySimulation<TsArray> P_Arguments => PropertySimulate<TsArray>(Constants.PROP_Arguments, false);

        public PropertySimulation P_Exception => PropertySimulate(Constants.PROP_Exception, false);

        public PropertySimulation P_RetryCount => PropertySimulate(Constants.PROP_RetryCount, false);

        public PropertySimulation P_ExceptionHandled => PropertySimulate(Constants.PROP_ExceptionHandled, false);
    }
}
