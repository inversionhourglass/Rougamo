namespace Rougamo
{
    /// <summary>
    /// public且实例方法接入
    /// </summary>
    public interface IPublicInstanceRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
    }
}
