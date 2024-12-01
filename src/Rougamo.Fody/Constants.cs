namespace Fody
{
    internal static partial class Constants
    {
        public const string TYPE_StackTraceHiddenAttribute = "System.Diagnostics.StackTraceHiddenAttribute";
        public const string TYPE_IMo = "Rougamo.IMo";
        public const string TYPE_RougamoAttribute = "Rougamo.RougamoAttribute";
        public const string TYPE_MoProxyAttribute = "Rougamo.MoProxyAttribute";
        public const string TYPE_IgnoreMoAttribute = "Rougamo.IgnoreMoAttribute";
        public const string TYPE_SkipRefStructAttribute = "Rougamo.SkipRefStructAttribute";
        public const string TYPE_MoRepulsion = "Rougamo.MoRepulsion";
        public const string TYPE_AccessFlags = "Rougamo.AccessFlags";
        public const string TYPE_Feature = "Rougamo.Feature";
        public const string TYPE_Omit = "Rougamo.Context.Omit";
        public const string TYPE_ForceSync = "Rougamo.ForceSync";
        public const string TYPE_IRougamo = "Rougamo.IRougamo";
        public const string TYPE_IRepulsionsRougamo = "Rougamo.IRepulsionsRougamo";
        public const string TYPE_PointcutAttribute = "Rougamo.Metadatas.PointcutAttribute";
        public const string TYPE_AdviceAttribute = "Rougamo.Metadatas.AdviceAttribute";
        public const string TYPE_OptimizationAttribute = "Rougamo.Metadatas.OptimizationAttribute";
        public const string TYPE_LifetimeAttribute = "Rougamo.Metadatas.LifetimeAttribute";
        public const string TYPE_IFlexibleModifierPointcut = "Rougamo.Flexibility.IFlexibleModifierPointcut";
        public const string TYPE_IFlexiblePatternPointcut = "Rougamo.Flexibility.IFlexiblePatternPointcut";
        public const string TYPE_IFlexibleOrderable = "Rougamo.Flexibility.IFlexibleOrderable";
        public const string TYPE_RougamoPool = "Rougamo.RougamoPool`1";

        public const string PROP_Flags = "Flags";
        public const string PROP_Pattern = "Pattern";
        public const string PROP_Features = "Features";
        public const string PROP_ForceSync = "ForceSync";
        public const string PROP_Order = "Order";
        public const string PROP_Repulsions = "Repulsions";
        public const string PROP_MoTypes = "MoTypes";
        public const string PROP_Exception = "Exception";
        public const string PROP_ReturnValue = "ReturnValue";
        public const string PROP_ExceptionHandled = "ExceptionHandled";
        public const string PROP_ReturnValueReplaced = "ReturnValueReplaced";
        public const string PROP_Arguments = "Arguments";
        public const string PROP_RewriteArguments = "RewriteArguments";
        public const string PROP_RetryCount = "RetryCount";
        public const string PROP_MethodContextOmits = "MethodContextOmits";
        public const string PROP_Target = "Target";
        public const string PROP_TargetType = "TargetType";
        public const string PROP_Method = "Method";
        public const string PROP_Mos = "Mos";
        public const string PROP_MethodContext = "MethodContext";

        public const string FIELD_Repulsions = "<Repulsions>k__BackingField";
        public const string FIELD_RougamoMo_Prefix = ">_<rougamo_mo_";
        public const string FIELD_RougamoMos = ">_<rougamo_mos";
        public const string FIELD_RougamoContext = ">_<rougamo_context";
        public const string FIELD_Singleton = ">_<Singleton";

        public const string METHOD_OnEntry = "OnEntry";
        public const string METHOD_OnSuccess = "OnSuccess";
        public const string METHOD_OnException = "OnException";
        public const string METHOD_OnExit = "OnExit";
        public const string METHOD_OnEntryAsync = "OnEntryAsync";
        public const string METHOD_OnSuccessAsync = "OnSuccessAsync";
        public const string METHOD_OnExceptionAsync = "OnExceptionAsync";
        public const string METHOD_OnExitAsync = "OnExitAsync";
        public const string METHOD__Singleton = "$Singleton";
        public const string METHOD_Get = "Get";
        public const string METHOD_Return = "Return";
    }
}
