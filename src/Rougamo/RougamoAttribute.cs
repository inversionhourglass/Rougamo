using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class RougamoAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public RougamoAttribute(Type moType)
        {
            MoType = moType;
        }

        /// <summary>
        /// </summary>
        public virtual Type MoType { get; }
    }


    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class RougamoAttribute<T> : RougamoAttribute where T : IMo
    {
        /// <summary>
        /// </summary>
        public RougamoAttribute() : base(typeof(T))
        {
        }
    }
}
