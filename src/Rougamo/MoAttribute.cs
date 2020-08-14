using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method)]
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
        public abstract void OnEntry(EntryContext context);

        /// <inheritdoc/>
        public abstract void OnException(ExceptionContext context);

        /// <inheritdoc/>
        public abstract void OnExit(ExitContext context);

        /// <inheritdoc/>
        public abstract void OnSuccess(SuccessContext context);
    }
}
