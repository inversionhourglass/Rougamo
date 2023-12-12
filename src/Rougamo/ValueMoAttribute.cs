using System;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class ValueMoAttribute : Attribute
    {
        /// <summary>
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ValueMoAttribute() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// </summary>
        /// <param name="valueMoType">A type that implements <see cref="IValueMo"/></param>
        public ValueMoAttribute(Type valueMoType)
        {
            if (!valueMoType.IsValueType) throw new ArgumentException("valueMoType must be a struct type.");

            ValueMoType = valueMoType;
        }

        /// <summary>
        /// A type that implements <see cref="IValueMo"/>
        /// </summary>
        public virtual Type ValueMoType { get; }
    }

    /// <summary>
    /// </summary>
    public class ValueMoAttribute<T> : ValueMoAttribute where T : struct, IValueMo
    {
        /// <inheritdoc/>
        public override Type ValueMoType => typeof(T);
    }
}
