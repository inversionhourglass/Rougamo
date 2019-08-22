using System.Reflection;

namespace Rougamo
{
    /// <summary>
    /// 自定义排除方法接入
    /// </summary>
    public interface IExcludeRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
        /// <summary>
        /// 排除的方法集合
        /// </summary>
        MethodInfo[] RougamoExcludes { get; }
    }
}
