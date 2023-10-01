using System;

#if FODY
namespace Rougamo.Fody
#else
namespace Rougamo
#endif
{
    /// <summary>
    /// accessable flags
    /// </summary>
    [Flags]
#if FODY
    internal enum AccessFlags : int
#else
    public enum AccessFlags : int
#endif
    {
        /// <summary>
        /// Static
        /// </summary>
        Static = 0b1001,
        /// <summary>
        /// Instance
        /// </summary>
        Instance = 0b0110,
        /// <summary>
        /// Public
        /// </summary>
        Public = 0b0011,
        /// <summary>
        /// NonPublic
        /// </summary>
        NonPublic = 0b1100,
        /// <summary>
        /// Static and Public
        /// </summary>
        StaticPublic = Static & Public,
        /// <summary>
        /// Static and NonPublic
        /// </summary>
        StaticNonPublic = Static & NonPublic,
        /// <summary>
        /// Instance and Public (default)
        /// </summary>
        InstancePublic = Instance & Public,
        /// <summary>
        /// Instance and NonPublic
        /// </summary>
        InstanceNonPublic = Instance & NonPublic,
        /// <summary>
        /// Static and instance, public and nonpublic
        /// </summary>
        All = Static | Instance,
        /// <summary>
        /// Method only(except property get set methods)
        /// </summary>
        Method = 0b1_0000,
        /// <summary>
        /// Property getter only
        /// </summary>
        PropertyGetter = 0b10_0000,
        /// <summary>
        /// Property setter only
        /// </summary>
        PropertySetter = 0b100_0000,
        /// <summary>
        /// Property getter and setter
        /// </summary>
        Property = PropertyGetter | PropertySetter,
        /// <summary>
        /// constructor
        /// </summary>
        Constructor = 0b1000_0000,
    }
}
