using Rougamo.Metadatas;
using System;

namespace Rougamo
{
    /// <summary>
    /// Ref structs cannot be boxed or unboxed, so ref struct parameters cannot be saved into MethodContext.Arguments, and ref struct return values cannot be saved into MethodContext.ReturnValue.
    /// You can use <see cref="OptimizationAttribute"/> to indicate that the aspect type does not require parameters or return values. If you don't, you must use this attribute to skip ref struct parameters and return value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class SkipRefStructAttribute : Attribute
    {
    }
}
