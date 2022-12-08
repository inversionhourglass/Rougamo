
# Rougamo - 肉夹馍
> Translated by google

[中文](README.md) | English

Rougamo, a Chinese snack, perhaps somewhat similar to a hamburger. Wrapping important logic code to provide AOP is like wrapping delicious stuffing in buns.

## Quick start
```csharp
// 1. Install-Package Rougamo.Fody
// 2. Define the class to inherit MoAttribute, and define the code that needs to be woven
public class LoggingAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        Log.Info("Before the method execution");
    }

    public override void OnException(MethodContext context)
    {
        Log.Error("When an exception occurs when the method is executed", context.Exception);
    }

    public override void OnSuccess(MethodContext context)
    {
        Log.Info("After the method executes successfully");
    }

    public override void OnExit(MethodContext context)
    {
        Log.Info("After the method is executed, whether it succeeds or fails");
    }
}

// 3. Apply Attribute
public class Service
{
    [Logging]
    public static int Sync(Model model)
    {
        // ...
    }

    [Logging]
    public async Task<Data> Async(int id)
    {
        // ...
    }
}
```

## Batch apply based on method accessibility
In the quick start, we introduced how to weave the code into the specified method, but in actual use, it may be very cumbersome and intrusive to add
this attribute to each method or many methods in a huge project, so ` MoAttribute` is designed to be applied to methods, classes, assemblies, and modules,
while setting accessibility attributes for added flexibility.

```csharp
// 1. While inheriting MoAttribute, override the Flags attribute, and default to InstancePublic (public instance method) when not overridden.
public class LoggingAttribute : MoAttribute
{
    // Changing to all public methods is valid, whether it is an instance method or a static method.
    public override AccessFlags Flags => AccessFlags.Public;

    // Method override omitted.
}

// 2. Apply
// 2.1. apply to class
[Logging]
public class Service
{
    // Logging whill be apply.
    public static void M1() { }

    // Logging whill be apply.
    public void M2() { }

    // protected access level does not apply Logging code weaving.
    protected void M3() { }
}
// 2.2. Applied to an assembly, all public methods of the assembly will be statically woven into the application.
[assembly: Logging]
```

## Weaving by implementing an empty interface(IRougamo<>)
If marking each method via Attribute feels too cumbersome or intrusive, and batch weaving via method accessibility is too general and not custom enough,
weaving via an empty interface will provide intrusiveness and convenient way.

```csharp
// 1. Define the code that needs to be woven.(Because LoggingMo is used as a generic parameter of the IRougamo interface, you can simply implement the IMo interface here, and of course you can inherit MoAttribute like LoggingAttribute in Quick Start)
public class LoggingMo : IMo
{
    public override AccessFlags Flags => AccessFlags.All;

    public override void OnEntry(MethodContext context)
    {
        // Information including input parameters, class instances, method descriptions, etc. can be obtained from the context object.
        Log.Info("Before the method execution");
    }

    public override void OnException(MethodContext context)
    {
        Log.Error("When an exception occurs when the method is executed", context.Exception);
    }

    public override void OnExit(MethodContext context)
    {
        Log.Info("After the method is executed, whether it succeeds or fails");
    }

    public override void OnSuccess(MethodContext context)
    {
        Log.Info("After the method executes successfully");
    }
}

// 2. Define an empty interface. The ILoggingRougamo interface defined in this step can be skipped, and IRougamo<LoggingMo> can also be implemented directly in the next step.
public interface ILoggingRougamo : IRougamo<LoggingMo>
{
}

// 3. Apply an empty interface. If you are used to defining a parent interface/parent class for the same type/domain class when programming, you only need the parent interface/parent class to implement the interface.
public interface IRepository<TModel, TId> : ILoggingRougamo
{
    // ...
}
```

## Exception handling and modifying return values(v1.1.0)
In the `OnException` method, you can call the `HandledException` method of `MethodContext` to indicate that the exception has been handled and set the return value.
In the `OnEntry` and `OnSuccess` methods, you can modify the actual method by calling the `ReplaceReturnValue` method of `MethodContext` The return value of
`ReturnValue`, `ExceptionHandled` and other attributes should not be used to modify the return value and handle exceptions directly. `HandledException` and
`ReplaceReturnValue` contain some other logic, which may be updated in the future. Also note that `Iterator/AsyncIterator` does not have this functionality.

```csharp
public class TestAttribute : MoAttribute
{
    public override void OnException(MethodContext context)
    {
        // Handle exceptions and set the return value to newReturnValue. If the method has no return value (void), simply pass in null
        context.HandledException(this, newReturnValue);
    }

    public override void OnSuccess(MethodContext context)
    {
        // Modify method return value
        context.ReplaceReturnValue(this, newReturnValue);
    }
}
```

## rewrite method arguments(v1.3.0)
In `OnEntry`, you can modify the arguments of the method by modifying the elements of `MethodContext.Arguments`. In order to be compatible with the situation where `MethodContext.Arguments` may be used to store some temporary values in the old version (although the possibility is very small),
so you also need to set `MethodContext.RewriteArguments` to `true` to confirm the rewrite arguments.
```csharp
public class DefaultValueAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        context.RewriteArguments = true;

        var parameters = context.Method.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType == typeof(string) && context.Arguments[i] == null)
            {
                context.Arguments[i] = string.Empty;
            }
        }
    }
}

public class Test
{
    // When the value is null, an empty string will be returned
    [DefaultValue]
    public string EmptyIfNull(string value) => value;
}
```

## Unified processing logic for async/non-async methods(v1.2.0)
Before version 1.2.0, for methods with similar method signatures `Task<int> M()`, when using async syntax, the type of the return value obtained through `MethodContext.ReturnValue`
is `int`, while in The type of the return value obtained when the async syntax is not used is `Task<int>`. This is not a bug, but the initial setting. Just as when we use the sync
syntax, the return value of the code is `int`, but without the When using the async syntax, the return value of the code is like `Task<int>`. But when you first came into contact
with the project, it may be difficult to notice this setting and in the process of using it, you also hope that there is no method using async syntax to execute `OnSuccess/OnExit`
after the Task is executed, so 1.2.0 introduced ExMoAttribute `, Use the processing logic of the previous async syntax method for the `Task/ValueTask` return value method with and
without async syntax.

**Note that `ExMoAttribute` and `MoAttribute` have the following differences:**
- `ExMoAttribute` overridable method named `ExOnEntry/ExOnException/ExOnSuccess/ExOnExit`
- The return value of `ExMoAttribute` is obtained through `MethodContext.ExReturnValue`, and the value obtained through `MethodContext.ReturnValue` will be the value of `Task/ValueTask`
- Whether the return value of `ExMoAttribute` is replaced/set, it is obtained through `MethodContext.ExReturnValueReplaced`, and the value obtained through `MethodContext.ReturnValueReplaced` is generally true (because it is replaced with the Task returned by ContinueWith)
- The return value type of `ExMoAttribute` is obtained through `MethodContext.ExReturnType`. What is the difference between `ReturnType/RealReturnType/ExReturnType` can be seen in the description of the respective attribute document or the type document description of `ExMoAttribute`

```csharp
[Fact]
public async Task Test()
{
    Assert.Equal(1, Sync());
    Assert.Equal(-1, SyncFailed1());
    Assert.Throws<InvalidOperationException>(() => SyncFailed3());

    Assert.Equal(1, await NonAsync());
    Assert.Equal(-1, await NonAsyncFailed1());
    Assert.Equal(-1, await NonAsyncFailed2());
    await Assert.ThrowsAsync<InvalidOperationException>(() => NonAsyncFailed3());
    await Assert.ThrowsAsync<InvalidOperationException>(() => NonAsyncFailed4());

    Assert.Equal(1, await Async());
    Assert.Equal(-1, await AsyncFailed1());
    Assert.Equal(-1, await AsyncFailed2());
    await Assert.ThrowsAsync<InvalidOperationException>(() => AsyncFailed3());
    await Assert.ThrowsAsync<InvalidOperationException>(() => AsyncFailed4());
}

[FixedInt]
static int Sync()
{
    return int.MaxValue;
}

[FixedInt]
static int SyncFailed1()
{
    throw new NotImplementedException();
}

[FixedInt]
static int SyncFailed3()
{
    throw new InvalidOperationException();
}

[FixedInt]
static Task<int> NonAsync()
{
    return Task.FromResult(int.MinValue);
}

[FixedInt]
static Task<int> NonAsyncFailed1()
{
    throw new NotImplementedException();
}

[FixedInt]
static Task<int> NonAsyncFailed2()
{
    return Task.Run(int () => throw new NotImplementedException());
}

[FixedInt]
static Task<int> NonAsyncFailed3()
{
    throw new InvalidOperationException();
}

[FixedInt]
static Task<int> NonAsyncFailed4()
{
    return Task.Run(int () => throw new InvalidOperationException());
}

[FixedInt]
static async Task<int> Async()
{
    await Task.Yield();
    return int.MaxValue / 2;
}

[FixedInt]
static async Task<int> AsyncFailed1()
{
    throw new NotImplementedException();
}

[FixedInt]
static async Task<int> AsyncFailed2()
{
    await Task.Yield();
    throw new NotImplementedException();
}

[FixedInt]
static async Task<int> AsyncFailed3()
{
    throw new InvalidOperationException();
}

[FixedInt]
static async Task<int> AsyncFailed4()
{
    await Task.Yield();
    throw new InvalidOperationException();
}

class FixedIntAttribute : ExMoAttribute
{
    protected override void ExOnException(MethodContext context)
    {
        if (context.Exception is NotImplementedException)
        {
            context.HandledException(this, -1);
        }
    }

    protected override void ExOnSuccess(MethodContext context)
    {
        context.ReplaceReturnValue(this, 1);
    }
}
```

## Ignore weaving(IgnoreMoAttribute)
In the quick start, we introduced how to apply in batches. Since the rules of batch references only limit the accessibility of methods, there may be some methods that
meet the rules and do not want to apply weaving. At this time, you can use `IgnoreMoAttribute` to specify method/ class, then that method/class (all methods) will
ignore weaving. If `IgnoreMoAttribute` is applied to an assembly or module, all weaving will be ignored for that assembly/module. Additionally, it is possible to
specify ignored weaving types via MoTypes when applying the `IgnoreMoAttribute`.

```csharp
// The current assembly ignores all weaving
[assembly: IgnoreMo]
// The current assembly ignores weaving of TheMoAttribute
[assembly: IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]

// The current class ignores all weaving
[IgnoreMo]
class Class1
{
    // ...
}

// The current class ignores weaving of TheMoAttribute
[IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]
class Class2
{
    // ...
}
```

## Attribute proxy weaving(MoProxyAttribute)
If you have used some third-party components to mark some methods with Attribute, and now you want to perform aop operations on these marked methods, but do not want to
manually add the Attribute mark of rougamo one by one, you can step by proxy. Complete aop weaving. Another example is that your project now has a lot of obsolete methods
marked with `ObsoleteAttribute`. You want to output the call stack log when the expired method is called, to check which entries are using these expired methods. You can
also do this in this way.

```csharp
public class ObsoleteProxyMoAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        Log.Warning("expired method was called." + Environment.StackTrace);
    }
}

[module: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]

public class Cls
{
    [Obsolete]
    private int GetId()
    {
        // This method weaves the application into the code
        return 123;
    }
}
```

## Weave Mutex
### Single type mutex(IRougamo<,>)
Since we have two weaving methods, Attribute tag and interface implementation, it may be applied at the same time, and if the content of the two weaving is the same,
there will be repeated weaving. In order to avoid this as much as possible In this case, when the interface is defined, mutually exclusive types can be defined,
that is, only one can take effect at the same time, and which one takes effect is determined according to [Priority] (#Priority).

```csharp
public class Mo1Attribute : MoAttribute
{
    // ...
}
public class Mo2Attribute : MoAttribute
{
    // ...
}
public class Mo3Attribute : MoAttribute
{
    // ...
}

public class Test : IRougamo<Mo1Attribute, Mo2Attribute>
{
    [Mo2]
    public void M1()
    {
        // Mo2Attribute is applied to the method, the priority is higher than the Mo1Attribute implemented by the interface, and the Mo2Attribute will be applied
    }

    [Mo3]
    public void M2()
    {
        // Mo1Attribute and Mo3Attribute are not mutually exclusive, both will be applied
    }
}
```

### Multitype Mutual Exclusion(IRepulsionsRougamo<,>)
`IRougamo<,>` can only be mutually exclusive with one type, `IRepulsionsRougamo<,>` can be mutually exclusive with multiple types.

```csharp
public class Mo1Attribute : MoAttribute
{
}
public class Mo2Attribute : MoAttribute
{
}
public class Mo3Attribute : MoAttribute
{
}
public class Mo4Attribute : MoAttribute
{
}
public class Mo5Attribute : MoAttribute
{
}

public class TestRepulsion : MoRepulsion
{
    public override Type[] Repulsions => new[] { typeof(Mo2Attribute), typeof(Mo3Attribute) };
}

[assembly: Mo2]
[assembly: Mo5]

public class Class2 : IRepulsionsRougamo<Mo1Attribute, TestRepulsion>
{
    [Mo3]
    public void M1()
    {
        // Mo1 is mutually exclusive with Mo2 and Mo3, but since Mo3 has a higher priority than Mo1, when Mo1 does not take effect, all mutually exclusive types will take effect.
        // So eventually Mo2Attribute, Mo3Attribute, Mo5Attribute will be applied.
        Console.WriteLine("m1");
    }

    [Mo4]
    public void M2()
    {
        // Mo1 is mutually exclusive with Mo2 and Mo3, but since Mo1 has a higher priority than Mo2, Mo2 will not take effect
        // Eventually Mo1Attribute, Mo4Attribute, Mo5Attribute will be applied
        Console.WriteLine("m2");
    }
}
```
<font color=red>Through the above example, you may notice that this multi-type mutual exclusion is not mutual exclusion between multiple types, but the mutual exclusion of
the first generic type and the type defined by the second generic type, and the second generic type is mutually exclusive. They are not mutually exclusive. Just like the
above example, when `Mo1Attribute` does not take effect, the mutually exclusive `Mo2Attribute` and `Mo3Attribute` will take effect. It needs to be understood here that the
reason for defining mutual exclusion is the possible repeated application of Attribute and empty interface implementation, not to exclude all weaving repetitions. At the
same time, it is not recommended to use multiple mutual exclusion definitions, which is prone to logical confusion. It is recommended to carefully consider a set of
unified rules before application weaving, rather than random definitions, and then try to use multiple mutual exclusions to solve problems.</font>

## Priority
1. `IgnoreMoAttribute`
2. Method `MoAttribute`
3. Method `MoProxyAttribute`
4. Type `MoAttribute`
5. Type `MoProxyAttribute`
6. Type `IRougamo<>`, `IRougamo<,>`, `IRepulsionsRougamo<,>`
7. Assembly & Module `MoAttribute`

## Switch
Rougamo is developed by individuals. Due to limited capabilities, the research on IL is not so thorough, and with the development of .NET, some new types, new semantics and
even new IL instructions will continue to appear. Therefore, there may exist Some bugs, and the bugs at the IL level may not be able to quickly locate the problem and fix it,
so here is a switch to avoid code weaving without removing the Rougamo reference. Therefore, it is recommended that you use Rougamo for code weaving When the woven code
does not affect the business, such as logs and APM. If you want to use static weaving components that are stable and can be quickly supported when you encounter problems,
it is recommended to use [PostSharp](https://www.postsharp.net)

Rougamo is developed on the basis of [fody](https://github.com/Fody/Fody). After referencing Rougamo, the first compilation will generate a `FodyWeavers.xml` file. The default content is as follows
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo />
</Weavers>
```

When you want to disable Rougamo, you need to add the attribute `enabled` to the `Rougamo` node of the configuration file and set the value to `false`
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo enabled="false" />
</Weavers>
```

## Record yield return IEnumerable/IAsyncEnumerable return value
We know that using `yield return` syntax sugar + `IEnumerable` return value method, after calling the method, the code of the method is not actually executed, the actual
execution of the code is when you access the elements in the `IEnumerable` object When, for example, you go to foreach this object or call `ToList/ToArray`, and the
returned elements are not stored in an array/linked list (the specific principle is not explained here), so by default there is no The method directly obtains the
collection of all elements returned by `yield return IEnumerable`.

But there may be some scenarios with strict code monitoring that need to record all return values, so in the implementation, I created an array to save all the returned elements,
but since this array is created additionally, it will take up additional memory space, and at the same time It is not clear how big the set of elements returned by this
`IEnumerable` is, so in order to avoid excessive memory consumption, the return value of `yield return IEnumerable` will not be recorded by default. The `Rougamo` node of
FodyWeavers.xml` adds attribute configuration `enumerable-returns="true"`.
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo enumerable-returns="true" />
</Weavers>
```
