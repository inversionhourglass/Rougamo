﻿using Rougamo.Context;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <summary>
    /// Basic interface for code weaving.
    /// </summary>
    public interface IMo
    {
        /// <summary>
        /// Execution order.
        /// </summary>
        double Order { get; }

        /// <summary>
        /// If you declare a struct that implment IMo, then you can use this property to minimize boxing operation.
        /// </summary>
        Omit MethodContextOmits { get; }

        /// <summary>
        /// Which methods should be executed synchronously in an async method
        /// </summary>
        ForceSync ForceSync { get; }

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

        /// <summary>
        /// Before the method executing.
        /// </summary>
        ValueTask OnEntryAsync(MethodContext context);

        /// <summary>
        /// After the method executes successfully
        /// </summary>
        ValueTask OnSuccessAsync(MethodContext context);

        /// <summary>
        /// When an exception occurs when the method is executed.
        /// </summary>
        ValueTask OnExceptionAsync(MethodContext context);

        /// <summary>
        /// After the method is executed, whether it succeeds or an exception occurs.
        /// </summary>
        ValueTask OnExitAsync(MethodContext context);
    }
}
