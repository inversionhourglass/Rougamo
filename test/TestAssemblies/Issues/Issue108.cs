using Issues.Attributes;
using System;

namespace Issues;

public class Issue108<T>
{
    [_108_]
    public string CatchAndGetStackTrace(int code = 108, string message = "mono-stacktrace")
    {
        try
        {
            ThrowCore(code, message);
            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.StackTrace ?? string.Empty;
        }
    }

    private void ThrowCore(int code, string message)
    {
        throw new InvalidOperationException($"{typeof(T).Name}:{code}:{message}");
    }
}
