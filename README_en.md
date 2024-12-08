
# Rougamo - 肉夹馍

[中文](README.md) | English

## What is Rougamo

Rougamo is a static code weaving AOP component. Commonly used AOP components include Castle, Autofac, and AspectCore. Unlike these components, which typically implement AOP through dynamic proxy and IoC mechanisms at runtime, Rougamo achieves AOP by directly modifying the target method's IL code during compilation. Rougamo supports all types of methods, including synchronous/asynchronous methods, static/instance methods, constructors, and properties.

### The origin of the name.

Rougamo, a type of traditional Chinese street food, is somewhat similar to a hamburger, as both involve meat being sandwiched between bread. Every time I mention AOP, I think of Rougamo, because AOP is just like Rougamo, do aspects around the mehod.

### Rougamo's Advantages and Disadvantages

- **Advantages:**
  1. **Shorter Application Startup Time**, Static weaving occurs at compile time, whereas dynamic proxies are set up at application startup.
  2. **Support for All Methods**, Rougamo supports all methods, including constructors, properties, and static methods. Dynamic proxies often only handle instance methods due to their reliance on IoC.
  3. **Independent from Other Components**, Rougamo does not require initialization and is easy and quick to use. Dynamic AOP relies on IoC components, which often need initialization at application startup and vary between different IoC components. Rougamo does not require initialization; you just define the aspect type and apply it.

- **Disadvantages:**
  **Larger Assembly Size**, Since Rougamo is a compile-time AOP, it weaves additional code into the assembly at compile time, which increases the assembly size. However, this overhead is generally minimal and can be evaluated in your project through [configuration options](https://github.com/inversionhourglass/Rougamo/wiki/%E9%85%8D%E7%BD%AE%E9%A1%B9).

## Basic Functionality Introduction

As an AOP component, Rougamo's primary function is to execute additional operations at key lifecycle points of a method. Rougamo supports four lifecycle points (or Join Points):
1. Before method execution;
2. After method execution successfully;
3. After method throws an exception;
4. When the method exits (regardless of whether it was successful or threw an exception, similar to `try..finally`).

Here's a simple example demonstrating how lifecycle points are expressed in code:

```csharp
// Define a type inheriting from MoAttribute
public class TestAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // OnEntry corresponds to before method execution
    }

    public override void OnException(MethodContext context)
    {
        // OnException corresponds to after method throws an exception
    }

    public override void OnSuccess(MethodContext context)
    {
        // OnSuccess corresponds to after method execution successfully
    }

    public override void OnExit(MethodContext context)
    {
        // OnExit corresponds to when method exits
    }
}
```

At each lifecycle point, in addition to performing operations like logging, measuring method execution time, and APM instrumentation that don't affect method execution, you can also:
1. [Modify method parameters](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E4%BF%AE%E6%94%B9%E6%96%B9%E6%B3%95%E5%8F%82%E6%95%B0)
2. [Intercept method execution](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C%E6%8B%A6%E6%88%AA)
3. [Modify method return values](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E4%BF%AE%E6%94%B9%E6%96%B9%E6%B3%95%E8%BF%94%E5%9B%9E%E5%80%BC)
4. [Handle method exceptions](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E5%A4%84%E7%90%86%E6%96%B9%E6%B3%95%E5%BC%82%E5%B8%B8)
5. [Retry method execution](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E9%87%8D%E8%AF%95%E6%89%A7%E8%A1%8C%E6%96%B9%E6%B3%95)

## Applying Rougamo to Methods

The simplest and most direct way is to apply the defined `Attribute` directly to methods. This can include synchronous and asynchronous methods, instance methods and static methods, properties and property getters/setters, as well as instance and static constructors:

```csharp
class Abc
{
    [Test]
    static Abc() { }

    [Test]
    public Abc() { }

    [Test]
    public int X { get; set; }

    public static Y
    {
        [Test]
        get;
        [Test]
        set;
    }

    [Test]
    public void M() { }

    [Test]
    public static async ValueTask MAsync() => await Task.Yield();
}
```

Applying attributes directly to methods is straightforward, but for common AOP types, adding the attribute to every method can be cumbersome. Rougamo also provides several bulk application methods:
1. [Class or assembly-level attributes](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#%E7%B1%BB%E6%88%96%E7%A8%8B%E5%BA%8F%E9%9B%86%E7%BA%A7attribute%E5%BA%94%E7%94%A8)
2. [Low-intrusive implementation of the empty interface IRougamo](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#%E5%AE%9E%E7%8E%B0%E7%A9%BA%E6%8E%A5%E5%8F%A3irougamo)
3. [Specify an attribute as a proxy attribute and apply it to methods with the proxy attribute](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#attribute%E4%BB%A3%E7%90%86)

When applying attributes in bulk, such as applying `TestAttribute` to a class, you typically don’t want every method in the class to receive `TestAttribute`. Instead, you may want to select methods that meet specific criteria. Rougamo offers two methods for method selection:
1. [Coarse-grained method feature matching](https://github.com/inversionhourglass/Rougamo/wiki/%E6%96%B9%E6%B3%95%E5%8C%B9%E9%85%8D#%E7%B2%97%E7%B2%92%E5%BA%A6%E7%9A%84%E6%96%B9%E6%B3%95%E7%89%B9%E6%80%A7%E5%8C%B9%E9%85%8D), which allows specifying static, instance, public, private, property, constructor methods, etc.
2. [AspectJ-style class expressions](https://github.com/inversionhourglass/Rougamo/wiki/%E6%96%B9%E6%B3%95%E5%8C%B9%E9%85%8D#%E7%B1%BBaspectj%E8%A1%A8%E8%BE%BE%E5%BC%8F%E5%8C%B9%E9%85%8D), which provide AspectJ-like expressions for finer-grained matching, such as method names, return types, parameter types, etc.

## Asynchronous Aspects

In modern development, asynchronous programming has become quite common. In the previous example, the method lifecycle nodes correspond to synchronous methods. If you need to perform asynchronous operations, you would have to manually block asynchronous operations to wait for the results, which can lead to resource wastage and performance loss. Rougamo provides asynchronous aspects in addition to synchronous aspects to better support asynchronous operations:

```csharp
// Define a type that inherits from AsyncMoAttribute
public class TestAttribute : AsyncMoAttribute
{
    public override async ValueTask OnEntryAsync(MethodContext context) { }

    public override async ValueTask OnExceptionAsync(MethodContext context) { }

    public override async ValueTask OnSuccessAsync(MethodContext context) { }

    public override async ValueTask OnExitAsync(MethodContext context) { }
}
```

However, if asynchronous operations are not needed, it's still recommended to use synchronous aspects. For more information on asynchronous aspects, you can refer to [Asynchronous Aspects](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BC%82%E6%AD%A5%E5%88%87%E9%9D%A2).

## Performance Optimization

Rougamo is a method-level AOP component. When applying Rougamo to methods, it instantiates related objects each time the method is called, which adds a burden to garbage collection (GC). Although this overhead is usually minimal and often negligible, Rougamo addresses performance impact by providing various optimization strategies:

1. **[Partial Weaving](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E9%83%A8%E5%88%86%E7%BC%96%E7%BB%87)**: If you only want to record a call log before method execution and do not need other lifecycle nodes or exception handling, you can use partial weaving to include only the required functionalities. This reduces the actual IL code woven and minimizes the number of instructions executed at runtime.
   
2. **[Structs](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E7%BB%93%E6%9E%84%E4%BD%93)**: One difference between classes and structs is that classes are allocated on the heap, while structs are allocated on the stack. Using structs can allocate some Rougamo types on the stack, reducing GC pressure.
   
3. **[Slimming MethodContext](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E7%98%A6%E8%BA%ABmethodcontext)**: `MethodContext` holds contextual information about the current method. This information requires additional objects and involves boxing and unboxing operations, such as for method parameters and return values. If you do not need this information, slimming down `MethodContext` can achieve certain optimization effects.
   
4. **[Forced Synchronization](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E5%BC%BA%E5%88%B6%E5%90%8C%E6%AD%A5)**: As discussed in [Asynchronous Aspects](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BC%82%E6%AD%A5%E5%88%87%E9%9D%A2), asynchronous aspects use `ValueTask` to optimize synchronous execution but still incur additional overhead. If asynchronous operations are not required, forcing synchronous aspects can avoid the extra costs of asynchronous aspects.

## Learn More

- [Configuration Options](https://github.com/inversionhourglass/Rougamo/wiki/%E9%85%8D%E7%BD%AE%E9%A1%B9)
- [Using IoC in Rougamo](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E8%82%89%E5%A4%B9%E9%A6%8D%E4%B8%AD%E4%BD%BF%E7%94%A8IoC)
- [Special Note on async void](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#async-void%E7%89%B9%E5%88%AB%E8%AF%B4%E6%98%8E)
- [Custom Execution Order When Using Multiple Rougamo Types](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E5%BA%94%E7%94%A8%E5%A4%9A%E4%B8%AA%E8%82%89%E5%A4%B9%E9%A6%8D%E7%B1%BB%E5%9E%8B%E6%97%B6%E8%87%AA%E5%AE%9A%E4%B9%89%E6%89%A7%E8%A1%8C%E9%A1%BA%E5%BA%8F)
- [Specifying Rougamo Built-in Attributes When Using Attributes](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E5%BA%94%E7%94%A8attribute%E6%97%B6%E6%8C%87%E5%AE%9A%E8%82%89%E5%A4%B9%E9%A6%8D%E5%86%85%E7%BD%AE%E5%B1%9E%E6%80%A7)
- [Special Parameters or Return Value Types (ref struct)](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E7%89%B9%E6%AE%8A%E5%8F%82%E6%95%B0%E6%88%96%E8%BF%94%E5%9B%9E%E5%80%BC%E7%B1%BB%E5%9E%8Bref-struct)
- [Considerations for Developing Middleware with Rougamo](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E4%BD%BF%E7%94%A8%E8%82%89%E5%A4%B9%E9%A6%8D%E5%BC%80%E5%8F%91%E4%B8%AD%E9%97%B4%E4%BB%B6%E6%B3%A8%E6%84%8F%E4%BA%8B%E9%A1%B9)
- [Overview of Functional Support for Different Method Types](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E4%B8%8D%E5%90%8C%E7%A7%8D%E7%B1%BB%E7%9A%84%E6%96%B9%E6%B3%95%E5%AF%B9%E5%8A%9F%E8%83%BD%E6%94%AF%E6%8C%81%E6%80%A7%E4%B8%80%E8%A7%88)
