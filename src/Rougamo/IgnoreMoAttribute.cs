using System;

namespace Rougamo
{
    /// <summary>
    /// 与<see cref="MoAttribute"/>以及<see cref="MoProxyAttribute"/>配合使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public sealed class IgnoreMoAttribute : Attribute
    {
    }
}
