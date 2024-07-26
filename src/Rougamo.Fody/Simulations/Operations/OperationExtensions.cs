using Rougamo.Fody.Simulations.PlainValues;

namespace Rougamo.Fody.Simulations.Operations
{
    internal static class OperationExtensions
    {
        public static Eq IsEqual(this ILoadable value1, ILoadable value2) => new(value1, value2);

        public static Eq IsNull(this ILoadable value) => new(value, new Null(value.ModuleWeaver));

        public static Gt Gt(this ILoadable value1, ILoadable value2) => new(value1, value2);
    }
}
