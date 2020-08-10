using System;

namespace Rougamo
{
    /// <summary>
    /// 多互斥类型，与<see cref="IRepulsionsRougamo{TMo, TRepulsion}"/>配合使用
    /// </summary>
    public abstract class MoRepulsion
    {
        /// <summary>
        /// 类型必须继承自<see cref="IMo"/>，同时实现该类时，该字段必须一次性初始化，不能包含逻辑处理
        /// </summary>
        /// <example>
        /// Repulsions = new [] { typeof(Abc), typeof(Bcd) };
        /// </example>
        public abstract Type[] Repulsions { get; }
    }
}
