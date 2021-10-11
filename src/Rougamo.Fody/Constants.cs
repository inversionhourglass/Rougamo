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

        public const string TYPE_Void = "System.Void";
        public const string TYPE_Task = "System.Threading.Tasks.Task";
        public const string TYPE_ValueTask = "System.Threading.Tasks.ValueTask";
        public const string TYPE_MulticastDelegate = "System.MulticastDelegate";
        public const string TYPE_AsyncStateMachineAttribute = "System.Runtime.CompilerServices.AsyncStateMachineAttribute";
        public const string TYPE_IteratorStateMachineAttribute = "System.Runtime.CompilerServices.IteratorStateMachineAttribute";
        public const string TYPE_AsyncIteratorStateMachineAttribute = "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute";
        public const string TYPE_AsyncTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder";
        public const string TYPE_AsyncValueTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder";
        public const string TYPE_AsyncVoidMethodBuilder = "System.Runtime.CompilerServices.AsyncVoidMethodBuilder";

        public const string TYPE_ARRAY_Type = "System.Type[]";

        public const string PROP_Flags = "Flags";
        public const string PROP_Repulsions = "Repulsions";
        public const string PROP_MoTypes = "MoTypes";
        public const string PROP_Exception = "Exception";
        public const string PROP_HasException = "HasException";
        public const string PROP_ReturnValue = "ReturnValue";

        public const string FIELD_Flags = "<Flags>k__BackingField";
        public const string FIELD_Repulsions = "<Repulsions>k__BackingField";
        public const string FIELD_RougamoMos = ">_<rougamo_mos";
        public const string FIELD_RougamoContext = ">_<rougamo_context";
        public const string FIELD_IteratorReturnList = ">_<returns";

        public const string METHOD_OnEntry = "OnEntry";
        public const string METHOD_OnSuccess = "OnSuccess";
        public const string METHOD_OnException = "OnException";
        public const string METHOD_OnExit = "OnExit";
        public const string METHOD_Create = "Create";
        public const string METHOD_MoveNext = "MoveNext";
        public const string METHOD_SetResult = "SetResult";
    }
}
