using Mono.Cecil;

namespace Rougamo.Fody
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static class GlobalRefs
    {
        public static TypeReference TrObject { get; private set; }

        public static TypeReference TrBool { get; private set; }

        public static TypeReference TrInt { get; private set; }

        public static TypeReference TrType { get; private set; }

        public static TypeReference TrMethodBase { get; private set; }

        public static MethodReference MrGetTypeFromHandle { get; private set; }

        public static MethodReference MrGetMethodFromHandle {  get; private set; }

        public static void Init(ModuleWeaver weaver)
        {
            TrObject = weaver._typeObjectRef;
            TrBool = weaver._typeBoolRef;
            TrInt = weaver._typeIntRef;
            TrType = weaver._typeSystemRef;
            TrMethodBase = weaver._typeMethodBaseRef;

            MrGetTypeFromHandle = weaver._methodGetTypeFromHandleRef;
            MrGetMethodFromHandle = weaver._methodGetMethodFromHandleRef;
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
