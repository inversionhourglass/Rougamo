namespace Rougamo
{
    /// <summary>
    /// public方法接入
    /// </summary>
    public interface IPublicRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
    }
}
