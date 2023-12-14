
# Rougamo - 肉夹馍

[中文](README.md) | English

## What is Rougamo
Rougamo is an AOP component that takes effect at compile time. There are some well-known AOP components like Castle, Autofac and AspectCore. Unlike these components, which are implemented through dynamic proxying and IoC at runtime, Rougamo modifies target methods' IL during compilation. If you are familiar with the AOP component PostSharp, then Rougamo is similar to PostSharp.

# Weaving Methods

## Quick Start with Attribute
```csharp
// 1.Add Rougamo.Fody package via NuGet
// 2.Define a class that inherits MoAttribute
public class LoggingAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // We can get method arguments, target class instance and MethodBase throughs MethodContext
        Log.Info("before method execute");
    }

    public override void OnException(MethodContext context)
    {
        Log.Error("while a exception was throwing", context.Exception);
    }

    public override void OnSuccess(MethodContext context)
    {
        Log.Info("method executed successfully");
    }

    public override void OnExit(MethodContext context)
    {
        Log.Info("before method exits");
    }
}

// 3.Apply Attribute to methods
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

## Flag Matching
In the quick start section, I introduced how to weave codes into a specific method. However, in real business scenarios, there may be many methods in a project that require applying this Attribute. Then it will be tedious and invasive. So `MoAttribute` is designed to be applied to methods, classes, assemblies, and modules, and you can select methods that match the property `MoAttribute.Flags`. The following features can be filtered through `Flags`:
- Method accessibility (public or non-public)
- Is static method or not
- Method category：normal method / property / getter / setter / constructor

```csharp
// 1. Override Flags property, InstancePublic is the default value (public and instance method or property).
public class LoggingAttribute : MoAttribute
{
    // All public methods, both instance and static
    public override AccessFlags Flags => AccessFlags.Public | AccessFlags.Method;

    // All instance properties(include getters and setters)
    // public override AccessFlags Flags => AccessFlags.Instance | AccessFlags.Property;

    // ...
}

// 2. Apply
// 2.1. Apply to class
[Logging]
public class Service
{
    // ...
}

// 2.2. Apply to assembly
[assembly: Logging]
```

Attention, when `Flags` does not specify any of `Method/PropertyGetter/PropertySetter/Property/Constructor`, the default is the method and property (`Method|Property`). The reason is `Method/PropertyGetter/PropertySetter/Property/Constructor` cannot be specified before 2.0, and the default matches methods and properties. This logic will continue to be compatible in 2.0.

## Weaving Through Interface
In the previous way, we mainly applied the Attribute to methods, classes, or assemblies, which requires us to modify code. This is an intrusive weaving method, which is not friendly for frequent AOP embedding scenarios. At this time, weaving through interfaces can greatly reduce or even avoid invasive weaving.

```csharp
// 1. Interface weaving allows us to directly implement the IMo interface, and of course we can also inherit from MoAttribute.
public class LoggingMo : IMo
{
    // 1.1. Override Flags property
    public AccessFlags Flags => AccessFlags.All | AccessFlags.Method;

    public void OnEntry(MethodContext context)
    {
        // We can get method arguments, target class instance and MethodBase throughs MethodContext
        Log.Info("before method execute");
    }

    public void OnException(MethodContext context)
    {
        Log.Error("while a exception was throwing", context.Exception);
    }

    public void OnSuccess(MethodContext context)
    {
        Log.Info("method executed successfully");
    }

    public void OnExit(MethodContext context)
    {
        Log.Info("before method exits");
    }
}

// 2. Implement IRougamo interface
public class TheService : ITheService, IRougamo<LoggingMo>
{
    // ...
}
```

In the above example, we only needs to implement an empty interface `IRougamo<LoggingMo>`, and the methods that match the `Flags` defined by `LoggingMo` will be selected. You may have a question "Isn't this still about modifying the code? Isn't it invasive?" Yes, it is still invasive. But if your project has a common layer for encapsulation, such as all `Service` have to implement `IService` or inherit from `Service` which is define in common layer, then you can operate on the parent/base interface at this time:

```csharp
// Common interface implements IRougamo
public interface IService : IRougamo<LoggingMo>
{
    // ...
}

// Bussiness interface implements common interface
public interface ITheService : IService
{
    // ...
}

public class TheService : ITheService
{
    // ...
}
```

## Pattern Matching
[Flag Matching](#flag-matching) is easy to use, but the matching granularity is too large. In this section, we will introduce the pattern matching introduced in version 2.0, which has more precise matching rules. The syntax refers to aspectj, but it is not exactly the same. So it is recommended to understand it before using it.

```csharp
public class PatternAttribute : MoAttribute
{
    // Match all methods that method name starts with Get
    public override string? Pattern => "method(* *.Get*(..))";

    // Match all public static methods
    public override string? Pattern => "method(public static * *(..))";

    // Match all property getter
    public override string? Pattern => "getter(* *)";

    // Match all methods that return type is int[] or implements System.Collections.Generic.IEnumerable<int>
    public override string? Pattern => "method(int[]||System.Collections.Generic.IEnumerable<int>+ *(..))";
}
```

### Basic Concepts
Flag matching overrides the `Flags` property, and pattern matching overrides the `Pattern` property. Since both flag matching and pattern matching are used for filtering methods, they cannot be used at the same time. Pattern matching has higher priority, `Pattern` is used when it is not null, otherwise `Flags` is used.

Pattern matching support seven matching rules:
- `method([modifier] returnType declaringType.methodName([parameters]))`
- `getter([modifier] propertyType declaringType.propertyName)`
- `setter([modifier] propertyType declaringType.propertyName)`
- `property([modifier] propertyType declaringType.propertyName)`
- `execution([modifier] returnType declaringType.methodName([parameters]))`
- `attr(position [index] attributeType)`
- `regex(REGEX)`

`getter`, `setter` and `property` means property getter, property setter, property getter and setter. `method` means all normal methods(except getter, setter, constructor). `execution` means all method(execpt constructor). `regex` is a special case and will be introduced in [Regex Matching](#regex-matching).

Except for the special format of `regex`, the contents of the other five matching rules mainly include the following five (or below) parts:
- `[modifier]`，the access modifier can be omitted. When omitted, it means matching all. The access modifier includes the following seven:
    - `private`
    - `internal`
    - `protected`
    - `public`
    - `privateprotected`，即`private protected`
    - `protectedinternal`，即`protected internal`
    - `static`，it should be noted that omitting this access modifier means matching both static and instances. If you want to match only instances, you can use it with the logical modifier `!` like `!static`
- `returnType`，the method returns the value type or property type. The format of the type is more complex. For details, see [Type Matching Format](#type-matching-format)
- `declaringType`，the type of the class in which the method/property is declared, For details, see [ype Matching Format](#type-matching-format)
- `methodName/propertyName`，the name of the method/property. The name can use `*` for fuzzy matching, such as `*Async`, `Get*`, `Get*V2`, etc. `*` matches 0 or more characters
- `[parameters]`，method parameter list, Rougamo's parameter list matching is relatively simple, not as complicated as aspectj, and only supports arbitrary matching and full matching.
    - Use `..` to match any number of parameters of any type.
    - Specify the number and type of parameters for matching. Parameter types are matched according to [Type Matching Format](#type-matching-format). Rougamo cannot perform fuzzy matching on the number of parameters like aspectj. For example, `int,..,double` is not supported.
- `position` Where the attributes apply to, uses `*` to match anywhere.
	- `type` Applies to types.
	- `exec` Applies to methods, properties, properties getter, properties setter.
	- `para` Applies to parameters. Must be used together with `[index]`.
	- `ret` Applies to return value.
    - `*` Match all positions.
- `[index]` Only needed when `position` is `para`. Usually, `index` is an integer, starts at `0`, which means the first parameter, and uses `*` to match any parameter.

### Type Matching Format

#### Type Format
First of all, we make it clear that there are several ways to express a certain type: type name; namespace + type name; assembly + namespace + type name. Since Rogamo's application limit is assembly, so Rogamo chooses to use namespace + type name to express a type. The connection between namespace and type name adopts our common point connection method, namely `namespace.type name`.

#### Nested Type
Rougamo uses `/` as the nested class connector, which is inconsistent with the connector `+` in usual programming habits. The main reason is that `+` is a special character, which means [subclass](#subclass-matching). For easier reading, another symbol is used. For example, `a.b.c.D/E` means the namespace is `a.b.c` and the outer class is `D` and the nested class is `E`. Of course nested classes support multiple levels of nesting.

#### Generic Type
What needs to be declared first is that generics are the same as `static`. They match all when not declared, it means match both non-generic types and generic types. If you want to match only non-generic types or only generic types, Additional definitions are required, and the relevant definitions of generics are expressed using `<>`.
- Only match non-generic types: `a.b.C<!>`, use logical not `!` to indicate not matching any generics
- Match any generic: `a.b.C<..>`, use two dots `..` to match at least one generics of any type
- Matches a specified number of generics of any type: `a.b.C<,,>`. The example means matching three generics of any type. Each added `,` means matching an additional generic of any type. `a.b.C< >` means matching a generic of any type
- Open and closed generic types: those with undetermined generic types are called open generic types, such as `List<T>`, and those with determined generic types are called closed generic types, such as `List<int> >`. When writing a matching expression, if you want to specify a specific generic type instead of arbitrary matching as described above, then for open undetermined generic types, you can use our commonly used `T1, T2, TA, TX`, etc. indicate that for closed and determined generic types, the determined type can be used directly.
    ```csharp
    public class Generic<T1, T2>
    {
        public static void M(T1 t1, int x, T2 t2) { }
    }

    public class TestAttribute : MoAttribute
    {
        // When defining a matching expression, for an open generic type, it does not need to be consistent with the generic name defined by the type. For example, it is called T1, T2 above, and TA, TB is used in the pattern.
        public override string? Pattern => "method(* *<TA,TB>.*(TA,int,TB))";
    }
    ```
- Generic methods: In addition to classes that can define generic parameters, methods can also define generic parameters. The method of using generic parameters of a method is the same as that of a type, so there will be no additional introduction.
    ```csharp
    public class Generic<T1, T2>
    {
        public static void M<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) { }
    }

    public class TestAttribute : MoAttribute
    {
        public override string? Pattern => "method(* *<TA,TB>.*<TX, TY>(TA,TB,TX,TY))";

        // You can also use non-generic matching, any matching and any type matching
        // public override string? Pattern => "method(* *<TA,TB>.*<..>(TA,TB,*,*))";
    }
    ```

#### Fuzzy Matching
Two types of fuzzy matching were introduced earlier, one is name fuzzy matching `*`, and the other is parameter/generic arbitrary matching `..`. These two symbols are still used for type fuzzy matching.

As introduced in [Type Format](#type-format), the type format consists of two parts `namespace.type name`, so type fuzzy matching can be divided into: namespace matching, type name matching, generic matching, Subclass matching. [Generic Type](#generic-type) was just introduced in the previous section, [Subclass Matching](#subclass-matching) will be introduced in the next section. This section mainly describes the basic fuzzy matching rules of types.
- Type name matching: Fuzzy matching of type names is very simple. You can use `*` to match 0 or more characters, such as `*Service`, `Mock*`, `Next*Repo*V2`, etc. It should be noted that `*` cannot directly match any nested type. For example, it is not feasible to use `*Service*` to match `AbcService+Xyz`. The nested type needs to be clearly stated, such as `*Service/* `, matching the nested class whose name ends with `Service`. If it is a second-level nested class, it also needs to be clearly pointed out `*Service/*/*`
- Namespace Matching
    - Absent matching: When the namespace is absent, it means matching any namespace. For example, the expression `Abc` can match `l.m.n.Abc` or `x.y.z.Abc`
    - Exact match: Do not use any wildcards, write a complete namespace
    - Fuzzy name: The namespace has one or more segments, and each segment is connected with `.`. Just like the type name matching, the characters in each segment can be matched by themselves using `*`, such as `*.x*z.ab*.vv `
    - Multi-segment fuzzy: Use `..` to match 0 or multiple namespaces. For example, `*..xyz.Abc` can match `a.b.xyz.Abc` or `lmn.xyz.Abc`, `..` can also match Used multiple times, such as using `a..internal..t*..Ab` to match `a.internal.tk.Ab` and `a.b.internal.c.t.u.Ab`

#### Subclass Matching
We can use `+` to match itself and its subclasses. For example, `method(a.b.c.IService+ *(..))` matches methods which return a type that implement interface `a.b.c.IService`. In addition, subclass matching can also be used with wildcards. For example, `method(* *(*Provider+))` indicates that the matching method parameter has only one subclass and the parameter type is a subclass of a type ending with `Provider`.

### Special Syntax

#### Primitive Types
For primitive types, Rougamo supports type abbreviations to make pattern look more concise and clear. The currently supported abbreviated types are `bool`, `byte`, `short`, `int`, `long`, `sbyte`, `ushort`, `uint`, `ulong`, `char`, `string`, `float`, `double`, `decimal`, `object`, `void`.

#### Nullable
Just like our usual programming, we can use `?` to represent the `Nullable` type, for example `int?` is `Nullable<int>`. It should be noted that the Nullable syntax of reference types should not be regarded as the `Nullable` type. For example, `string?` is actually `string`. In Rougamo, write `string` directly instead of `string?`.

#### Tuple ValueTuple
When we write C# code, we can directly use brackets to represent `ValueTuple`, which is also supported in Rougamo. For example, `(int, string)` means `ValueTuple<int, string>` or `Tuple<int, string>`.

#### Task ValueTask
Now asynchronous programming is a basic programming method, so there will be many methods whose return value is `Task` or `ValueTask`. At the same time, if you want to be compatible with the two return values ​​of `Task` and `ValueTask`, patterns are need to use the logical operator `||` to connect, which will greatly increase the complexity of the pattern. Rougamo adds the familiar `async` keyword to match `Task` and `ValueTask` return values. For example, `Task<int>` and `ValueTask<int>` can be written as `async int`. For non-generic Type `Task` and `ValueTask` are written as `async null`. It should be noted that there is currently no way to match `async void` alone, `void` will match `void` and `async void`.

#### Simplify Type and Method Name
As mentioned earlier, the pattern of type consists of `namespace.typeName`. If we want to match any type, the standard writing method should be `*..*`, where `*..` represents any namespace, followed by `*` represents any type name. For any type, we can simplify it as `*`. Similarly, the standard writing method of any method of any type should be `*..*.*`, we can also simplify this as `*`. So `method(*..* *..*.*(..))` and `method(* *(..))` express the same meaning.

### Regex Matching
For each method, Rougamo will generate a string signature for it, and the regex match is the match for this string of signatures. Its signature format is `modifiers returnType declaringType.methodName([parameters])`.

- `modifiers` contains two parts. One part is the accessibility modifier, namely `private/protected/internal/public/privateprotected/protectedinternal`, and the other part is whether the static method `static` is used. The `static` keyword is omitted for non-static methods. Separate the two parts with a space.
- `returnType/declaringType` are all full names of `namespace.typeName`. It should be noted that all types in the regular matching signature are full names, and similar `int` cannot be used to match `System.Int32`
- Generics, types and methods may contain generics. For closed generic types, just use the full name of the type. For open generic types, we abide by the following rules. Generics start from `T1` and increase backward. , that is, `T1/T2/T3...`, the order of increase is in the order of `declaringType` first and then `method`. For details, please see the following examples.
- `parameters`, parameters can be expanded by the full name of each parameter
- Nested types, nested types are connected using `/`

```csharp
namespace a.b.c;

public class Xyz
{
    // signature：public System.Int32 a.b.c.Xyz.M1(System.String)
    public int M1(string s) => default;

    // signature：public static System.Void a.b.c.Xyz.M2<T1>(T1)
    public static void M2<T>(T value) { }

    public class Lmn<TU, TV>
    {
        // signature：internal System.Threading.Tasks.Task<System.DateTime> a.b.c.Xyz/Lmn<T1,T2>.M3<T3,T4>(T1,T2,T3,T4)
        internal Task<DateTime> M3<TO, TP>(TU u, TV v, TO o, TP p) => Task.FromResult(DateTime.Now);

        // signature：private static System.Threading.Tasks.ValueTask a.b.c.Xyz/Lmn<T1,T2>.M4()
        private static async ValueTask M4() => await Task.Yeild();
    }
}
```


Regex Matching has the problem of being complicated to write, and it does not support subclass matching, so Regex Matching are generally not written. They are mainly used as a supplement to other matching rules to support some more complex name matching. Since Rougamo supports logical operations, it also gives more auxiliary space for Regex Matching. For example, we want to find the return value method of `Task/ValueTask` whose method name does not end with `Async` `method(async null *(..) ) && regex(^\S+ (static )?\S+ \S+?(?<!Async)\()`.

# Weaving Ability

## Overwrite Method Arguments
In `OnEntry`, you can modify the parameter values of the method by modifying the elements in `MethodContext.Arguments`. In order to make it clear that you want to modify the method parameters, you also need to set `MethodContext.RewriteArguments` to `true` to confirm the rewriting. parameter.
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
    [DefaultValue]
    public string EmptyIfNull(string value) => value;
}
```

## Modify Return Value
In the `OnEntry` and `OnSuccess` methods, you can modify the actual return value of the method by calling the `ReplaceReturnValue` method of `MethodContext`. Special attention should be paid to not modifying the return value directly through the `ReturnValue` property. `ReplaceReturnValue` contains some other logic may be updated later. Also note that the return value of `Iterator/AsyncIterator` cannot be modified.
```csharp
public class TestAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        context.ReplaceReturnValue(this, newReturnValue);
    }

    public override void OnSuccess(MethodContext context)
    {
        context.ReplaceReturnValue(this, newReturnValue);
    }
}
```

## Exception Handle
In the `OnException` method, you can indicate that the exception has been handled and set the return value by calling the `HandledException` method of `MethodContext`. Similarly, do not directly set the exception to be handled and set the return value directly through the properties of `ReturnValue` and `ExceptionHandled`. .
```csharp
public class TestAttribute : MoAttribute
{
    public override void OnException(MethodContext context)
    {
        context.HandledException(this, newReturnValue);
    }
}
```

## Retry
The retry function can re-execute the current method when a specified exception is encountered or the return value is unexpected. To do this, you need set `MethodContext.RetryCount` value. After `OnException` and `OnSuccess`, if the value of `MethodContext.RetryCount` is greater than 0, the current method will be re-executed.
```csharp
internal class RetryAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // Initialize the number of retries
        context.RetryCount = 3;
    }

    public override void OnException(MethodContext context)
    {
        // If an exception occurs, the number of retries will be reduced by one. When RetryCount is reduced to 0, it means that the upper limit of retries has been reached and no more attempts will be made.
        context.RetryCount--;
    }

    public override void OnSuccess(MethodContext context)
    {
        if (context.ReturnValue != ABC)
        {
            // The result is an unexpected reduction in the number of retries. When RetryCount decreases to 0, it means that the upper limit of retries has been reached and no more attempts will be made.
            context.RetryCount--;
        }
        else
        {
            // The result is as expected, directly set the number of retries to 0 and no longer retry.
            context.RetryCount = 0;
        }
    }
}

// The Test method will retry 3 times
[Retry]
public void Test()
{
    throw new Exception();
}
```
**For exception handling retry scenarios, I created an independent project [Rougamo.Retry](https://github.com/inversionhourglass/Rougamo.Retry). If you only want to retry a certain exception, you can use [Rougamo directly. .Retry](https://github.com/inversionhourglass/Rougamo.Retry)**

Please note the following points when using the retry function:
- When handling exceptions through `MethodContext.HandledException()` or modifying the return value through `MethodContext.ReplaceReturnValue()`, `MethodContext.RetryCount` will be set to 0 directly, because manually handling exceptions and modifying the return value means that you have decided the final result of the method, so there is no need to retry
- `OnEntry` and `OnExit` of `MoAttribute` will only be executed once and will not be executed multiple times due to retries.
- Try not to use the retry in `ExMoAttribute` unless you really know the actual processing logic. Think about the following code, `ExMoAttribute` cannot re-execute the entire external method after reporting an error inside `Task`
  ```csharp
  public Task Test()
  {
    DoSomething();

    return Task.Run(() => DoOtherThings());
  }
  ```

## Partially Weaving
Rougamo will weave all functions into the code by default, but generally not all functions are used. And with the iteration of the version, more and more functions are supported and more and more code is woven into it, which will increase the final assembly size. By specifying partial weaving, you can weave in only the functionality you need. Partial weaving is set up by overriding `MoAttribute.Features`.

|      Value      |                                                  functions                                                      |
|:---------------:|:----------------------------------------------------------------------------------------------------------------|
|       All       | Contains all functions, default value                                                                           |
|     OnEntry     | Only OnEntry, parameter values cannot be modified, and return values cannot be modified                         |
|   OnException   | OnException only, exceptions cannot be handled                                                                  |
|    OnSuccess    | OnSuccess only, the return value cannot be modified                                                             |
|     OnExit      | OnExit                                                                                                          |
|   RewriteArgs   | Contains OnEntry, and parameter values can be modified in OnEntry                                               |
|  EntryReplace   | Contains OnEntry, and the return value can be modified in OnEntry                                               |
| ExceptionHandle | Contains OnException, and exceptions can be handled in OnEntry                                                  |
|  SuccessReplace | Contains OnSuccess, and the return value can be modified in OnSuccess                                           |
|  ExceptionRetry | Contains OnException, and can retry in OnException                                                              |
|  SuccessRetry   | Contains OnSuccess, and can retry in OnSuccess                                                                  |
|     Retry       | Contains OnException and OnSuccess, and can retry in OnException and OnSuccess                                  |
|     Observe     | Contains OnEntry, OnException, OnSuccess and OnExit                                                             |
| NonRewriteArgs  | Contains all functions except modifying parameters                                                              |
|    NonRetry     | Contains all functionality except retry                                                                         |
|    FreshArgs    | Update the newest parameters value into MethodContext.Arguments before calls OnException, OnSuccess and OnExit  |

## Ignore Weaving
Rougamo has the ability to weave in batches. Although the current version provides richer matching rules, if the matching pattern is too complicated in order to exclude one or several methods, it will not be worth the gain. When excluding a specific method or methods, you can directly use `IgnoreMoAttribute`. When applied to a method, the method will ignore weaving. When applied to a class, all methods of the class will ignore weaving. When applied to the assembly, all methods in the assembly ignore weaving.

In addition, you can specify the ignored type when ignoring weaving. For example, if a method applies multiple `MoAttribute`, you can ignore one individually. If not specified, it means ignoring all.
```csharp
// Current assembly ignores all weaving
[assembly: IgnoreMo]
// Current assembly ignores weaving of TheMoAttribute
[assembly: IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]

// Current class ignores all weaving
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

## Proxy Weaving
If you have used some third-party components to Attribute mark some methods, and now you want to perform aop operations on these marked methods, but do not want to manually add Rougamo's Attribute marks one by one. You can use a proxy in one step to complete aop weaving. If your project now has many obsolete methods marked with `ObsoleteAttribute`. You want to output the call stack log when the expired methods are called to check which entries are currently using these expired methods. This can also be done in this way.
```csharp
public class ObsoleteProxyMoAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        Log.Warning("The obsolete method was called：" + Environment.StackTrace);
    }
}

// Proxy at the assembly level, all methods marked with ObsoleteAttribute will have ObsoleteProxyMoAttribute applied
[assembly: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]
```

## ExMoAttribute
When we implement `MoAttribute`, in the `OnEntry/OnSuccess/OnException/OnExit` method, when we obtain the return value type through `MethodContext.ReturnValue`, for the `Task/ValueTask` method that returns a value, there are different performances when using and not using `async/await` syntax. For example, when using async syntax for `Task<int> M()`, the type of `MethodContext.ReturnValue` is `int`, but when not using async syntax, the type is `Task <int>`.

This is not a bug, but an initial design. Just as when we use async syntax, we will return like `return 123` for `Task<int>`, and when we do not use async syntax, we will return like `Task.FromResult(123)`. Furthermore, the performance after compilation is different between using and not using `async` syntax. Using `async` syntax will generate a state machine type at compile time, which will not be discussed in details here. So this design is not only based on the habits when writing code, but also based on the actual compiled code structure.

Although the design is like this, many times we do not use `async/await` syntax for some simple method overloading, but we also hope that Rougamo can handle it like methods with `async/await` syntax. Then it is recommended to use `ExMoAttribute` at this time. Regardless of whether `async` syntax is used, it can be processed just like using `async` syntax.

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
static int Sync() => int.MaxValue;

[FixedInt]
static int SyncFailed1() => throw new NotImplementedException();

[FixedInt]
static int SyncFailed3() => throw new InvalidOperationException();

[FixedInt]
static Task<int> NonAsync() => Task.FromResult(int.MinValue);

[FixedInt]
static Task<int> NonAsyncFailed1() => throw new NotImplementedException();

[FixedInt]
static Task<int> NonAsyncFailed2() => Task.Run(int () => throw new NotImplementedException());

[FixedInt]
static Task<int> NonAsyncFailed3() => throw new InvalidOperationException();

[FixedInt]
static Task<int> NonAsyncFailed4() => Task.Run(int () => throw new InvalidOperationException());

[FixedInt]
static async Task<int> Async()
{
    await Task.Yield();
    return int.MaxValue / 2;
}

[FixedInt]
static async Task<int> AsyncFailed1() => throw new NotImplementedException();

[FixedInt]
static async Task<int> AsyncFailed2()
{
    await Task.Yield();
    throw new NotImplementedException();
}

[FixedInt]
static async Task<int> AsyncFailed3() => throw new InvalidOperationException();

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

## Mutex Weaving
### Single Type Mutex
Since we have two weaving methods, Attribute tag and interface implementation, it may be applied at the same time, and if the content of the two weaving is the same, there will be repeated weaving. In order to avoid this as much as possible In this case, when the interface is defined, mutually exclusive types can be defined, that is, only one can take effect at the same time, and which one takes effect is determined according to [Priority](#priority).
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

### Multiple Type Mutex
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
Through the above example, you may notice that this multi-type mutual exclusion is not mutual exclusion between multiple types, but the mutual exclusion of the first generic type and the type defined by the second generic type, and the second generic type is mutually exclusive. They are not mutually exclusive. Just like the above example, when `Mo1Attribute` does not take effect, the mutually exclusive `Mo2Attribute` and `Mo3Attribute` will take effect. It needs to be understood here that the reason for defining mutual exclusion is the possible repeated application of Attribute and empty interface implementation, not to exclude all weaving repetitions. At the same time, it is not recommended to use multiple mutual exclusion definitions, which is prone to logical confusion. It is recommended to carefully consider a set of unified rules before application weaving, rather than random definitions, and then try to use multiple mutual exclusions to solve problems.

# Other

## Priority
1. `IgnoreMoAttribute`
2. Method `MoAttribute`
3. Method `MoProxyAttribute`
4. Type `MoAttribute`
5. Type `MoProxyAttribute`
6. Type `IRougamo<>`, `IRougamo<,>`, `IRepulsionsRougamo<,>`
7. Assembly & Module `MoAttribute`

## Configuration
After referencing Rougamo, a `FodyWeavers.xml` file will be generated in the project root directory during compilation, with the following format:
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo />
</Weavers>
```
The way to add configuration is to add attributes and values directly to the `Rougamo` node, such as `<Rougamo enabled="false" />`.

The following table shows all configuration items:

|             Name(~~former name~~)                | default value |            description            |
|:------------------------------------------------:|:-------------:|:----------------------------------|
|  enabled                                         |      true     | Whether to enable rougamo         |
|  composite-accessibility                         |      false    | Whether to use class+method comprehensive accessibility for matching. By default, only method accessibility is used for matching. For example, if the accessibility of a class is internal and the accessibility of a method is public, then by default the accessibility of the method is considered public. After setting this configuration to true, the accessibility of the method is considered internal |
|  moarray-threshold                               |        4      | When the `MoAttribute` actually effective on the method reaches this value, it will be saved in an array. This configuration is used to optimize weaving code. In most cases, there is only one `MoAttribute` on a method. In this case, using an array to save it will generate more IL code when calling its method |
|  iterator-returns(~~enumerable-returns~~)        |      false    | Whether to save the return value of the iterator to `MethodContext.ReturnValue`. Use this function with caution. If the iterator generates a large amount of data, enabling this function will occupy the same amount of memory |
|  reverse-call-nonentry(~~reverse-call-ending~~)  |      true     | When there are multiple `MoAttribute` on a method, whether to execute `OnException/OnSuccess/OnExit` in the reverse order of `OnEntry`. By default, it is executed in reverse order                                             |
|  except-type-patterns                            |               | Regular expression of the full name of the type. Types matching this expression will be globally excluded. Multiple regular expressions should be separated by commas or semicolons                                             |

## FAQ
**Q: First time contacting Rougamo, but it doesn’t work.**  
A: When contacting for the first time, please note that the project must directly depend on `Rougamo.Fody` to take effect. Indirect dependencies are invalid. For example, project A depends on B, and B references `Rougamo.Fody`. At this time, the weaving in project A cannot take effect, and project A needs to directly reference `Rougamo.Fody`.

**Q: After using Rougamo to develop libraries, do other projects that reference my libraries still have to reference `Rougamo.Fody`?**  
A: After the library references `Rougamo.Fody`, you can manually modify the reference node in the project file and add `PrivateAssets="contentfiles;analyzers"`. The modification is as follows (remember to modify the version number when copying). When others use the library, you don't need to depend on `Rougamo.Fody` directly, but you need to directly depend on the your libraries.
```xml
<PackageReference Include="Rougamo.Fody" Version="2.0.0" PrivateAssets="contentfiles;analyzers" />
```