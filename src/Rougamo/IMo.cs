using Rougamo.Context;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    public interface IMo
    {
        /// <summary>
        /// Weavable method type, this attribute has no effect when applied at the method level.
        /// </summary>
        AccessFlags Flags { get; }

        /// <summary>
        /// Method matched with this pattern will be weaving. This attribute has no effect when applied at the method level. It has a higher priority than <see cref="Flags"/>
        /// </summary>
        string? Pattern { get; }

        /// <summary>
        /// Which features will be weaving.
        /// </summary>
        Feature Features { get; }

        /// <summary>
        /// Execution order.
        /// </summary>
        double Order { get; }

        /// <summary>
        /// If you declare a struct that implment IMo, and you do not need the Mos property of MethodContext,
        /// then you should set it to true, or Rougamo will boxing your struct as a IMo object and save it into MethodContext.Mos.
        /// </summary>
        Omit MethodContextOmits { get; }

        /// <summary>
        /// Before the method executing.
        /// </summary>
        void OnEntry(MethodContext context);

        /// <summary>
        /// After the method executes successfully
        /// </summary>
        void OnSuccess(MethodContext context);

        /// <summary>
        /// When an exception occurs when the method is executed.
        /// </summary>
        void OnException(MethodContext context);

        /// <summary>
        /// After the method is executed, whether it succeeds or an exception occurs.
        /// </summary>
        void OnExit(MethodContext context);
    }
}
