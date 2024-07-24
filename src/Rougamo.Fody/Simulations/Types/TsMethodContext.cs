using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMethodContext(TypeReference typeRef, IHost? host, ModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        public PropertySimulation P_ReturnValueReplaced => PropertySimulate(Constants.PROP_ReturnValueReplaced, false);

        public PropertySimulation P_ReturnValue => PropertySimulate(Constants.PROP_ReturnValue, false);

        public PropertySimulation P_RewriteArguments => PropertySimulate(Constants.PROP_RewriteArguments, false);

        public PropertySimulation<TsArray> P_Arguments => PropertySimulate<TsArray>(Constants.PROP_Arguments, false);

        public PropertySimulation P_Exception => PropertySimulate(Constants.PROP_Exception, false);

        public PropertySimulation P_HasException => PropertySimulate(Constants.PROP_HasException, false);

        public PropertySimulation P_RetryCount => PropertySimulate(Constants.PROP_RetryCount, false);

        public PropertySimulation P_ExceptionHandled => PropertySimulate(Constants.PROP_ExceptionHandled, false);
    }
}
