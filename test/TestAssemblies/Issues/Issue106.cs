using Issues.Attributes;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Issues;
public class Issue106
{
    public void Test()
    {
        var method = MethodBase.GetCurrentMethod().DeclaringType.GetMethod(nameof(WithoutAsync));
        Attribute.GetCustomAttributes(method, typeof(StateMachineAttribute), inherit: false);
    }

    [_106_]
    public Task WithoutAsync() => Task.CompletedTask;
}
