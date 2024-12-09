
# Rougamo - 肉夹馍

中文 | [English](https://github.com/inversionhourglass/Rougamo/blob/master/README_en.md)

## Rougamo是什么

Rougamo是一个静态代码编织的AOP组件，同为AOP组件较为常用的有Castle、Autofac、AspectCore等，与这些组件不同的是，这些组件基本都是通过动态代理+IoC的方式实现AOP，是运行时完成的，而Rougamo是编译时直接修改目标方法织入IL代码实现AOP的。肉夹馍支持包括同步/异步方法、静态/实例方法、构造方法、属性内的所有种类的方法。

### 名字的由来

也没什么由来，方法级AOP在我看来就是两面包夹芝士，在方法执行前后添加切面，将整个方法夹在中间。肉夹馍大家都吃过的，虽然叫肉夹馍，但其实是肉夹于馍，外层为馍中间为肉，Rougamo同理，外层切面我称为`Mo`，中间实际方法称之为`Rou`，希望Rougamo的馍能够为你们的肉增香添色。

### Rougamo的优劣势

- **优势**
  1. **更短的应用启动时间**，静态编织发生在编译期，而动态代理发生在应用启动时；
  2. **支持所有方法**，支持包括构造方法、属性、静态方法之内的所有方法。动态代理由于依赖IoC，往往只能实现对实例方法的切面操作；
  3. **独立于其他组件，无需初始化**，动态AOP依赖于IoC组件，往往需要在应用启动时进行初始化，且不同的IoC组件还有着不同的初始化方式，肉夹馍无需初始化，定义好切面类型直接应用即可；
- **劣势**
  **更大的程序集大小**，由于肉夹馍时编译时AOP，在编译时会将额外的代码织入当前程序集中，所以不可避免的会增加程序集的大小。但这种额外开销其实很小，可以在自己的项目中通过 [配置项](https://github.com/inversionhourglass/Rougamo/wiki/%E9%85%8D%E7%BD%AE%E9%A1%B9) 启用和禁止肉夹馍对比评估实际影响；

## 基础功能介绍

肉夹馍作为一款AOP组件，其基本功能就是在方法几个主要生命周期节点执行额外操作，肉夹馍支持四个生命周期节点（或称为连接点 Join Point）：
1. 方法执行前；
2. 方法执行成功后；
3. 方法抛出异常后；
4. 方法退出时（无论方法执行成功还是抛出异常，都会进入该生命周期节点，类似`try..finally..`）。

用一个简单示例进行说明生命周期节点在代码中的表现形式：

```csharp
// 定义一个类型继承MoAttribute
public class TestAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // OnEntry对应方法执行前
    }

    public override void OnException(MethodContext context)
    {
        // OnException对应方法抛出异常后
    }

    public override void OnSuccess(MethodContext context)
    {
        // OnSuccess对应方法执行成功后
    }

    public override void OnExit(MethodContext context)
    {
        // OnExit对应方法退出时
    }
}
```

在方法的各个生命周期节点，除了能够执行类似记录日志、统计方法耗时、APM埋点等不影响方法执行的操作，还可以进行以下这些控制方法执行的操作：
1. [修改方法参数](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E4%BF%AE%E6%94%B9%E6%96%B9%E6%B3%95%E5%8F%82%E6%95%B0)
2. [方法执行拦截](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C%E6%8B%A6%E6%88%AA)
3. [修改方法返回值](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E4%BF%AE%E6%94%B9%E6%96%B9%E6%B3%95%E8%BF%94%E5%9B%9E%E5%80%BC)
4. [处理方法异常](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E5%A4%84%E7%90%86%E6%96%B9%E6%B3%95%E5%BC%82%E5%B8%B8)
5. [重试执行方法](https://github.com/inversionhourglass/Rougamo/wiki/%E6%8E%A7%E5%88%B6%E6%96%B9%E6%B3%95%E6%89%A7%E8%A1%8C#%E9%87%8D%E8%AF%95%E6%89%A7%E8%A1%8C%E6%96%B9%E6%B3%95)

## 如何应用肉夹馍到方法上

最简单直接的方式便是将定义的`Attribute`直接应用到方法上，这个方法可以是同步的也可以是异步的，可以是实例方法也可以是静态方法，可以是属性也可以是属性getter/setter，可以是示例构造方法也可以是静态构造方法：

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
    public static async ValueTask MAsync() => await Task.Yeild();
}
```

直接应用到方法上的方式简单明了，但对于一些非常常用的AOP类型，每个方法都需要添加Attribute就显得非常麻烦，所以肉夹馍还提供了多种批量应用的方式：
1. [类或程序集级别的Attribute的方式](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#%E7%B1%BB%E6%88%96%E7%A8%8B%E5%BA%8F%E9%9B%86%E7%BA%A7attribute%E5%BA%94%E7%94%A8)
2. [低侵入性的实现空接口IRougamo的方式](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#%E5%AE%9E%E7%8E%B0%E7%A9%BA%E6%8E%A5%E5%8F%A3irougamo)
3. [指定某Attribute为代理Attribute，寻找应用了代理Attribute方法的方式](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BA%94%E7%94%A8%E6%96%B9%E5%BC%8F#attribute%E4%BB%A3%E7%90%86)

我们在进行批量应用的时候，比如将`TestAttribute`直接应用到一个类上，我们一般并不是希望这个类的所有方法都应用`TestAttribute`，而是希望选定那些满足特定特征的部分方法。肉夹馍在方法筛选上提供了两种方案：
1. [粗粒度的方法特征匹配](https://github.com/inversionhourglass/Rougamo/wiki/%E6%96%B9%E6%B3%95%E5%8C%B9%E9%85%8D#%E7%B2%97%E7%B2%92%E5%BA%A6%E7%9A%84%E6%96%B9%E6%B3%95%E7%89%B9%E6%80%A7%E5%8C%B9%E9%85%8D)，可以指定匹配静态、实例、共有、私有、属性、构造方法等一个或多个粗粒度特征；
2. [类AspectJ表达式匹配](https://github.com/inversionhourglass/Rougamo/wiki/%E6%96%B9%E6%B3%95%E5%8C%B9%E9%85%8D#%E7%B1%BBaspectj%E8%A1%A8%E8%BE%BE%E5%BC%8F%E5%8C%B9%E9%85%8D)，提供了类似AspectJ格式的表达式，可以进行更为细致的匹配，比如方法名称、返回值类型、参数类型等。

## 异步切面

现在异步编程在日常开发中已经是非常常见的了，在最上面的示例中，方法生命周期节点对应的方法全是同步的，如果希望进行一些异步操作，就不得不手动阻塞异步操作等待返回结果了，这是存在一定的资源浪费和性能损耗的。肉夹馍除了同步切面，还提供了异步切面，更好的支持异步操作：

```csharp
// 定义一个类型继承AsyncMoAttribute
public class TestAttribute : AsyncMoAttribute
{
    public override async ValueTask OnEntryAsync(MethodContext context) { }

    public override async ValueTask OnExceptionAsync(MethodContext context) { }

    public override async ValueTask OnSuccessAsync(MethodContext context) { }

    public override async ValueTask OnExitAsync(MethodContext context) { }
}
```

不过需要注意的是，如果并不需要做异步操作，那么还是推荐使用同步切面，更多关于异步切面的说明，请跳转 [异步切面](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BC%82%E6%AD%A5%E5%88%87%E9%9D%A2)。

## 性能优化

肉夹馍是方法级AOP组件，在对方法应用肉夹馍后，每次调用方法都会实例化肉夹馍的相关对象，增加GC的负担。虽然这些负担是轻微的，很多时候是可以忽略不计的，但肉夹馍关注其对性能的影响，所以从不同的方向提供了各种优化方式：
1. [部分编织](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E9%83%A8%E5%88%86%E7%BC%96%E7%BB%87)，如果你只是想在执行方法前记录一下调用日志，那么其他生命周期节点以及异常处理等功能实际并用不上，此时通过部分编织功能仅选择你需要功能进行编织，这样可以缩减实际编织的IL代码，同时减少运行时实际执行的指令数量；
2. [结构体](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E7%BB%93%E6%9E%84%E4%BD%93)，类与结构体的其中一个区别便是，类分配在堆中，而结构体在栈中，使用结构体可以使肉夹馍的部分类型分配到栈中，减少GC压力；
3. [瘦身MethodContext](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E7%98%A6%E8%BA%ABmethodcontext)，`MethodContext`中保存了当前方法的一些上下文信息，这些信息需要使用额外的对象进行保存，同时伴随着一些装箱拆箱操作，比如方法参数、返回值等，如果确定不需要这些信息，可以通过瘦身`MethodContext`达到一定的优化效果；
4. [强制同步](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E5%BC%BA%E5%88%B6%E5%90%8C%E6%AD%A5)，在[异步切面](https://github.com/inversionhourglass/Rougamo/wiki/%E5%BC%82%E6%AD%A5%E5%88%87%E9%9D%A2)中介绍了异步切面与同步切面的关系，异步切面虽然使用了`ValueTask`来优化同步执行，但终究还是存在额外的开销，在编写异步切面时，如果确定不需要异步操作，可以通过强制调用同步切面来避免异步切面的额外开销。

## 了解更多

- [配置项](https://github.com/inversionhourglass/Rougamo/wiki/%E9%85%8D%E7%BD%AE%E9%A1%B9)
- [肉夹馍中使用IoC](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E8%82%89%E5%A4%B9%E9%A6%8D%E4%B8%AD%E4%BD%BF%E7%94%A8IoC)
- [async void特别说明](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#async-void%E7%89%B9%E5%88%AB%E8%AF%B4%E6%98%8E)
- [应用多个肉夹馍类型时自定义执行顺序](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E5%BA%94%E7%94%A8%E5%A4%9A%E4%B8%AA%E8%82%89%E5%A4%B9%E9%A6%8D%E7%B1%BB%E5%9E%8B%E6%97%B6%E8%87%AA%E5%AE%9A%E4%B9%89%E6%89%A7%E8%A1%8C%E9%A1%BA%E5%BA%8F)
- [应用Attribute时指定肉夹馍内置属性](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E5%BA%94%E7%94%A8attribute%E6%97%B6%E6%8C%87%E5%AE%9A%E8%82%89%E5%A4%B9%E9%A6%8D%E5%86%85%E7%BD%AE%E5%B1%9E%E6%80%A7)
- [特殊参数或返回值类型ref struct](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E7%89%B9%E6%AE%8A%E5%8F%82%E6%95%B0%E6%88%96%E8%BF%94%E5%9B%9E%E5%80%BC%E7%B1%BB%E5%9E%8Bref-struct)
- [使用肉夹馍开发中间件注意事项](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E4%BD%BF%E7%94%A8%E8%82%89%E5%A4%B9%E9%A6%8D%E5%BC%80%E5%8F%91%E4%B8%AD%E9%97%B4%E4%BB%B6%E6%B3%A8%E6%84%8F%E4%BA%8B%E9%A1%B9)
- [不同种类的方法对功能支持性一览](https://github.com/inversionhourglass/Rougamo/wiki/%E5%85%B6%E4%BB%96#%E4%B8%8D%E5%90%8C%E7%A7%8D%E7%B1%BB%E7%9A%84%E6%96%B9%E6%B3%95%E5%AF%B9%E5%8A%9F%E8%83%BD%E6%94%AF%E6%8C%81%E6%80%A7%E4%B8%80%E8%A7%88)
