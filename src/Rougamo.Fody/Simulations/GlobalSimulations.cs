﻿namespace Rougamo.Fody.Simulations
{
    internal static class GlobalSimulations
    {
        public static TypeSimulation Object = GlobalRefs.TrObject.Simulate(null!);
        public static TypeSimulation Bool = GlobalRefs.TrInt.Simulate(null!);
        public static TypeSimulation Type = GlobalRefs.TrType.Simulate(null!);
        public static TypeSimulation MethodBase = GlobalRefs.TrMethodBase.Simulate(null!);
    }
}