using System.Reflection;

namespace Rougamo
{
    /// <summary>
    /// 自定义包含方法接入
    /// </summary>
    public interface IIncludeRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
        /// <summary>
        /// 包含的方法集合
        /// </summary>
        MethodInfo[] RougamoIncludes { get; }
    }
}
