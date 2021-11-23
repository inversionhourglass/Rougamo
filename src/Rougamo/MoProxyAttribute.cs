using System;

namespace Rougamo
{
    /// <summary>
    /// 使用一个已有Attribute作为代理类型进行织入
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module, AllowMultiple = true)]
    public class MoProxyAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public MoProxyAttribute(Type originAttributeType, Type moAttribtueType)
        {
            OriginAttributeType = originAttributeType;
            MoAttribtueType = moAttribtueType;
        }

        /// <summary>
        /// 被代理的Attribute类型
        /// </summary>
        public Type OriginAttributeType { get; set; }

        /// <summary>
        /// 继承自<see cref="MoAttribute"/>的代理Attribute
        /// </summary>
        public Type MoAttribtueType { get; set; }
    }
}
