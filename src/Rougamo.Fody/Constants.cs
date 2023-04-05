namespace Rougamo.Fody
{
    internal static class Constants
    {
        public const string TYPE_IMo = "Rougamo.IMo";
        public const string TYPE_MoAttribute = "Rougamo.MoAttribute";
        public const string TYPE_MoProxyAttribute = "Rougamo.MoProxyAttribute";
        public const string TYPE_IgnoreMoAttribute = "Rougamo.IgnoreMoAttribute";
        public const string TYPE_MoRepulsion = "Rougamo.MoRepulsion";
        public const string TYPE_AccessFlags = "Rougamo.AccessFlags";
        public const string TYPE_IRougamo_1 = "Rougamo.IRougamo`1";
        public const string TYPE_IRougamo_2 = "Rougamo.IRougamo`2";
        public const string TYPE_IRepulsionsRougamo = "Rougamo.IRepulsionsRougamo`2";
        public const string TYPE_MethodContext = "Rougamo.Context.MethodContext";
        public const string TYPE_IMethodDiscoverer = "Rougamo.IMethodDiscoverer";

        public const string TYPE_Void = "System.Void";
        public const string TYPE_Int32 = "System.Int32";
        public const string TYPE_Double = "System.Double";
        public const string TYPE_Type = "System.Type";
        public const string TYPE_Task = "System.Threading.Tasks.Task";
        public const string TYPE_ValueTask = "System.Threading.Tasks.ValueTask";
        public const string TYPE_MulticastDelegate = "System.MulticastDelegate";
        public const string TYPE_ObsoleteAttribute = "System.ObsoleteAttribute";
        public const string TYPE_AsyncStateMachineAttribute = "System.Runtime.CompilerServices.AsyncStateMachineAttribute";
        public const string TYPE_IteratorStateMachineAttribute = "System.Runtime.CompilerServices.IteratorStateMachineAttribute";
        public const string TYPE_AsyncIteratorStateMachineAttribute = "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute";
        public const string TYPE_AsyncTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"; // async Task
        public const string TYPE_AsyncValueTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder"; // async ValueTask
        public const string TYPE_AsyncVoidMethodBuilder = "System.Runtime.CompilerServices.AsyncVoidMethodBuilder"; // async void
        public const string TYPE_ManualResetValueTaskSourceCore = "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore"; // async IAsyncEnumerable
        public const string TYPE_ManualResetValueTaskSourceCore_Bool = "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1<System.Boolean>";

        public const string TYPE_ARRAY_Type = "System.Type[]";

        public const string PROP_Flags = "Flags";
        public const string PROP_DiscovererType = "DiscovererType";
        public const string PROP_Features = "Features";
        public const string PROP_Order = "Order";
        public const string PROP_Repulsions = "Repulsions";
        public const string PROP_MoTypes = "MoTypes";
        public const string PROP_Exception = "Exception";
        public const string PROP_HasException = "HasException";
        public const string PROP_ReturnValue = "ReturnValue";
        public const string PROP_ExceptionHandled = "ExceptionHandled";
        public const string PROP_ReturnValueReplaced = "ReturnValueReplaced";
        public const string PROP_Arguments = "Arguments";
        public const string PROP_RewriteArguments = "RewriteArguments";
        public const string PROP_RetryCount = "RetryCount";

        public const string FIELD_Format = "<{0}>k__BackingField";
        public const string FIELD_Repulsions = "<Repulsions>k__BackingField";
        // Private fields cannot be accessed externally even using IL
        //public const string FIELD_Exception = "<Exception>k__BackingField";
        //public const string FIELD_ReturnValue = "<ReturnValue>k__BackingField";
        public const string FIELD_RougamoMo_Prefix = ">_<rougamo_mo_";
        public const string FIELD_RougamoMos = ">_<rougamo_mos";
        public const string FIELD_RougamoContext = ">_<rougamo_context";
        public const string FIELD_Builder = "<>t__builder";
        public const string FIELD_State = "<>1__state";
        public const string FIELD_Current_Suffix = "current";
        public const string FIELD_IteratorReturnList = ">_<returns";

        public const string METHOD_OnEntry = "OnEntry";
        public const string METHOD_OnSuccess = "OnSuccess";
        public const string METHOD_OnException = "OnException";
        public const string METHOD_OnExit = "OnExit";
        public const string METHOD_IsMatch = "IsMatch";
        public const string METHOD_Create = "Create";
        public const string METHOD_MoveNext = "MoveNext";
        public const string METHOD_SetResult = "SetResult";
        public const string METHOD_SetException = "SetException";
        public const string METHOD_GetEnumerator_Prefix = "System.Collections.Generic.IEnumerable<";
        public const string METHOD_GetEnumerator_Suffix = ">.GetEnumerator";
        public const string METHOD_GetAsyncEnumerator_Prefix = "System.Collections.Generic.IAsyncEnumerable<";
        public const string METHOD_GetAsyncEnumerator_Suffix = ">.GetAsyncEnumerator";
    }
}
