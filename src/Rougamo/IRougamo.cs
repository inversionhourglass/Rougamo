namespace Rougamo
{
    /// <summary>
    /// For types that implement this interface, use <typeparamref name="T"/> for code weaving.
    /// </summary>
    /// <typeparam name="T">The type implements <see cref="IMo"/>.</typeparam>
    public interface IRougamo<in T> where T : IMo, new()
    {
    }

    /// <summary>
    /// For types that implement this interface, use <typeparamref name="TMo"/> for code weaving.
    /// </summary>
    /// <typeparam name="TMo">The type implements <see cref="IMo"/>.</typeparam>
    /// <typeparam name="TRepulsion">
    /// The type implements <see cref="IMo"/>, mutually exclusive with <typeparamref name="TMo"/>, 
    /// only one takes effect at the same time according to the priority relationship.
    /// </typeparam>
    public interface IRougamo<in TMo, in TRepulsion> where TMo : IMo, new() where TRepulsion : IMo
    {
    }

    /// <summary>
    /// For types that implement this interface, use <typeparamref name="TMo"/> for code weaving.
    /// </summary>
    /// <typeparam name="TMo">The type implements <see cref="IMo"/>.</typeparam>
    /// <typeparam name="TRepulsion">
    /// The type implements <see cref="MoRepulsion"/>, its <see cref="MoRepulsion.Repulsions"/> defines
    /// multiple mutually exclusive types with <typeparamref name="TMo"/>, and only one takes effect at
    /// the same time according to the priority relationship.
    /// </typeparam>
    public interface IRepulsionsRougamo<in TMo, in TRepulsion> where TMo : IMo, new() where TRepulsion : MoRepulsion
    {
    }
}
