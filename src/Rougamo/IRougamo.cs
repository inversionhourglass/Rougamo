namespace Rougamo
{
    /// <summary>
    /// 根接口，用于查找
    /// </summary>
    public interface IRougamo<in T> where T : IMo, new()
    {
    }
}
