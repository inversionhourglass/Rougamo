namespace Rougamo
{
    /// <summary>
    /// 实例方法接入
    /// </summary>
    public interface IInstanceRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
    }
}
