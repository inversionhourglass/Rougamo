namespace Rougamo.Fody
{
    internal static class Constants
    {
        public const string TYPE_IMo = "Rougamo.IMo";
        public const string TYPE_MoAttribute = "Rougamo.MoAttribute";
        public const string TYPE_RougamoAttribute = "Rougamo.RougamoAttribute";
        public const string TYPE_RougamoAttribute_1 = "Rougamo.RougamoAttribute`1";
        public const string TYPE_MoProxyAttribute = "Rougamo.MoProxyAttribute";
        public const string TYPE_IgnoreMoAttribute = "Rougamo.IgnoreMoAttribute";
        public const string TYPE_MoRepulsion = "Rougamo.MoRepulsion";
        public const string TYPE_AccessFlags = "Rougamo.AccessFlags";
        public const string TYPE_Feature = "Rougamo.Feature";
        public const string TYPE_Omit = "Rougamo.Context.Omit";
        public const string TYPE_ForceSync = "Rougamo.ForceSync";
        public const string TYPE_IRougamo = "Rougamo.IRougamo";
        public const string TYPE_IRougamo_1 = "Rougamo.IRougamo`1";
        public const string TYPE_IRougamo_2 = "Rougamo.IRougamo`2";
        public const string TYPE_IRepulsionsRougamo = "Rougamo.IRepulsionsRougamo";
        public const string TYPE_MethodContext = "Rougamo.Context.MethodContext";

        public const string TYPE_Void = "System.Void";
        public const string TYPE_String = "System.String";
        public const string TYPE_Boolean = "System.Boolean";
        public const string TYPE_Byte = "System.Byte";
        public const string TYPE_Int16 = "System.Int16";
        public const string TYPE_Int32 = "System.Int32";
        public const string TYPE_Int64 = "System.Int63";
        public const string TYPE_SByte = "System.SByte";
        public const string TYPE_UInt16 = "System.UInt16";
        public const string TYPE_UInt32 = "System.UInt32";
        public const string TYPE_UInt64 = "System.UInt64";
        public const string TYPE_Single = "Single";
        public const string TYPE_Double = "System.Double";
        public const string TYPE_Type = "System.Type";
        public const string TYPE_Task = "System.Threading.Tasks.Task";
        public const string TYPE_ValueTask = "System.Threading.Tasks.ValueTask";
        public const string TYPE_MulticastDelegate = "System.MulticastDelegate";
        public const string TYPE_ObsoleteAttribute = "System.ObsoleteAttribute";
        public const string TYPE_IEnumerable = "System.Collections.Generic.IEnumerable";
        public const string TYPE_IAsyncEnumerable = "System.Collections.Generic.IAsyncEnumerable";
        public const string TYPE_CompilerGeneratedAttribute = "System.CompilerGeneratedAttribute";
        public const string TYPE_IsByRefLikeAttribute = "System.Runtime.CompilerServices.IsByRefLikeAttribute";
        public const string TYPE_DebuggableAttribute = "System.Diagnostics.DebuggableAttribute";
        public const string TYPE_Runtime_CompilerGeneratedAttribute = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";
        public const string TYPE_AsyncStateMachineAttribute = "System.Runtime.CompilerServices.AsyncStateMachineAttribute";
        public const string TYPE_IteratorStateMachineAttribute = "System.Runtime.CompilerServices.IteratorStateMachineAttribute";
        public const string TYPE_AsyncIteratorStateMachineAttribute = "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute";
        public const string TYPE_AsyncMethodBuilderAttribute = "System.Runtime.CompilerServices.AsyncMethodBuilderAttribute";
        public const string TYPE_AsyncMethodBuilder = "System.Runtime.CompilerServices.AsyncMethodBuilder";
        public const string TYPE_AsyncTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder"; // async Task
        public const string TYPE_AsyncValueTaskMethodBuilder = "System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder"; // async ValueTask
        public const string TYPE_AsyncVoidMethodBuilder = "System.Runtime.CompilerServices.AsyncVoidMethodBuilder"; // async void
        public const string TYPE_ManualResetValueTaskSourceCore = "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore"; // async IAsyncEnumerable
        public const string TYPE_ManualResetValueTaskSourceCore_Bool = "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1<System.Boolean>";

        public const string TYPE_ARRAY_Type = "System.Type[]";

        public static string Setter(string propertyName) => $"set_{propertyName}";
        public static string Getter(string propertyName) => $"get_{propertyName}";
        public const string PROP_Flags = "Flags";
        public const string PROP_Pattern = "Pattern";
        public const string PROP_Features = "Features";
        public const string PROP_ForceSync = "ForceSync";
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
        public const string PROP_MethodContextOmits = "MethodContextOmits";
        public const string PROP_IsCompleted = "IsCompleted";
        public const string PROP_Current = "Current";
        public const string PROP_Task = "Task";

        public const string FIELD_Format = "<{0}>k__BackingField";
        public const string FIELD_Repulsions = "<Repulsions>k__BackingField";
        // Private fields cannot be accessed externally even using IL
        //public const string FIELD_Exception = "<Exception>k__BackingField";
        //public const string FIELD_ReturnValue = "<ReturnValue>k__BackingField";
        public const string FIELD_RougamoMo_Prefix = ">_<rougamo_mo_";
        public const string FIELD_RougamoMos = ">_<rougamo_mos";
        public const string FIELD_RougamoContext = ">_<rougamo_context";
        public const string FIELD_Builder = "<>t__builder";
        public const string FIELD_Promise = "<>v__promiseOfValueOrEnd";
        public const string FIELD_State = "<>1__state";
        public const string FIELD_This = "<>4__this";
        public const string FIELD_Awaiter = "<>u__1";
        public const string FIELD_Result = ">_<result";
        public const string FIELD_Current_Suffix = "current";
        public const string FIELD_Iterator = ">_<iterator";
        public const string FIELD_IteratorReturnList = ">_<items";
        public const string FIELD_MoAwaiter = ">_<moAwaiter";
        public const string FIELD_InitialThreadId = "<>l__initialThreadId";
        public const string FIELD_Disposed = "<>w__disposeMode";

        public const string METHOD_OnEntry = "OnEntry";
        public const string METHOD_OnSuccess = "OnSuccess";
        public const string METHOD_OnException = "OnException";
        public const string METHOD_OnExit = "OnExit";
        public const string METHOD_OnEntryAsync = "OnEntryAsync";
        public const string METHOD_OnSuccessAsync = "OnSuccessAsync";
        public const string METHOD_OnExceptionAsync = "OnExceptionAsync";
        public const string METHOD_OnExitAsync = "OnExitAsync";
        public const string METHOD_Ctor = ".ctor";
        public const string METHOD_IsMatch = "IsMatch";
        public const string METHOD_Create = "Create";
        public const string METHOD_MoveNext = "MoveNext";
        public const string METHOD_SetStateMachine = "SetStateMachine";
        public const string METHOD_Start = "Start";
        public const string METHOD_SetResult = "SetResult";
        public const string METHOD_SetException = "SetException";
        public const string METHOD_GetAwaiter = "GetAwaiter";
        public const string METHOD_GetResult = "GetResult";
        public const string METHOD_AwaitUnsafeOnCompleted = "AwaitUnsafeOnCompleted";
        public const string METHOD_Complete = "Complete";
        public const string METHOD_GetEnumerator = "GetEnumerator";
        public const string METHOD_GetAsyncEnumerator = "GetAsyncEnumerator";
        public const string METHOD_MoveNextAsync = "MoveNextAsync";
        public const string METHOD_GetTypeFromHandle = "GetTypeFromHandle";
        public const string METHOD_GetMethodFromHandle = "GetMethodFromHandle";
        public const string METHOD_Add = "Add";
        public const string METHOD_ToArray = "ToArray";
        public const string METHOD_Capture = "Capture";
        public const string METHOD_Throw = "Throw";

        public static string GenericPrefix(string name) => $"{name}<";
        public static string GenericSuffix(string name) => $">.{name}";
    }
}
