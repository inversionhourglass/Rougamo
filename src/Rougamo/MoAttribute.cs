using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Property)]
    public abstract class MoAttribute : Attribute, IMo
    {
        #region todo
        ///// <summary>
        ///// 默认构造方法
        ///// </summary>
        //protected MoAttribute() { }

        ///// <summary>
        ///// 通过构造方法传入的<see cref="AccessFlags"/>会覆盖类定义的<see cref="AccessFlags"/>
        ///// </summary>
        //protected MoAttribute(AccessFlags flags)
        //{
        //    Flags = flags;
        //}
        #endregion todo

        /// <inheritdoc/>
        public virtual AccessFlags Flags { get; }

        /// <inheritdoc/>
        public virtual Feature Features { get; set; }

        /// <inheritdoc/>
        public virtual Type? DiscovererType { get; set; }

        /// <inheritdoc/>
        public virtual double Order { get; set; }

        /// <inheritdoc/>
        public virtual void OnEntry(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnException(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnExit(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnSuccess(MethodContext context) { }
    }
}
