using System;

namespace Rougamo
{
    /// <summary>
    /// Weaving using an existing Attribute as the proxy type.
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
        /// Attribute type being proxied.
        /// </summary>
        public Type OriginAttributeType { get; set; }

        /// <summary>
        /// Proxy Attribute inherited from <see cref="MoAttribute"/>.
        /// </summary>
        public Type MoAttribtueType { get; set; }
    }
}
