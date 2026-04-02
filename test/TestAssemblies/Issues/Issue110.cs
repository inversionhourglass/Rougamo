using Issues.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Issues;

public class Issue110
{
    public int X { get; set; } = 1;

    public int Y { get; set; } = 2;

    public int Z { get; set; } = 3;

    [_110_]
    public IEnumerator Enumerator(List<string> logs)
    {
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public IEnumerator<int> GenericEnumerator(List<string> logs)
    {
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public IEnumerator<T> GenericEnumerator<T>(List<string> logs)
    {
        yield return default;
    }

    [_110_]
    public IEnumerable Enumerable(List<string> logs)
    {
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public IEnumerable<int> GenericEnumerable(List<string> logs)
    {
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public IEnumerable<T> GenericEnumerable<T>(List<string> logs)
    {
        yield return default;
    }

    [_110_]
    public async IAsyncEnumerator<int> EnumeratorAsync(List<string> logs)
    {
        await Task.Yield();
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public async IAsyncEnumerator<T> EnumeratorAsync<T>(List<string> logs)
    {
        await Task.Yield();
        yield return default;
    }

    [_110_]
    public async IAsyncEnumerable<int> EnumerableAsync(List<string> logs)
    {
        await Task.Yield();
        yield return X;
        yield return Y;
        yield return Z;
    }

    [_110_]
    public async IAsyncEnumerable<T> EnumerableAsync<T>(List<string> logs)
    {
        await Task.Yield();
        yield return default;
    }
}
