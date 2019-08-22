namespace Rougamo
{
    /// <summary>
    /// 全方法接入
    /// </summary>
    public interface IFullRougamo<in T> : IRougamo<T> where T : IMo, new()
    {
    }
}
