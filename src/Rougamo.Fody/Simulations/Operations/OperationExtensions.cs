using Rougamo.Fody.Simulations.PlainValues;

namespace Rougamo.Fody.Simulations.Operations
{
    internal static class OperationExtensions
    {
        public static Eq IsEqual(this ILoadable value1, ILoadable value2) => new(value1, value2);

        public static Eq IsEqual(this ILoadable value1, int value2) => value1.IsEqual(new Int32Value(value2, value1.ModuleWeaver));

        public static Eq IsNull(this ILoadable value) => new(value, new Null(value.ModuleWeaver));

        public static Gt Gt(this ILoadable value1, ILoadable value2) => new(value1, value2);

        public static Gt Gt(this ILoadable value1, int value2) => value1.Gt(new Int32Value(value2, value1.ModuleWeaver));

        public static Lt Lt(this ILoadable value1, ILoadable value2) => new(value1, value2);

        public static Lt Lt(this ILoadable value1, int value2) => value1.Lt(new Int32Value(value2, value1.ModuleWeaver));

        public static BitAnd And(this ILoadable value1, ILoadable value2) => new(value1, value2);

        public static BitAnd And(this ILoadable value1, int value2) => value1.And(new Int32Value(value2, value1.ModuleWeaver));
    }
}
