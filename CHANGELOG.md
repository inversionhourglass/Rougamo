# 5.0.0

## 性能优化

### 切面类型属性成员缩减

目前的切面类型包含众多属性，这些属性均为配置项，很多的配置项只在编译时供Rougamo使用，在运行时并不需要，切面类型在实例化时这些属性都会占用内存。

现在将这些属性提取到Attribute中，以类型元数据的形式进行配置，减少切面类型的成员数量，从而优化切面类型实例化对象的内存占用。这样做的另一个好处在于，对于自定义结构体实现`IMo`的场景，可以减少需要实现的成员数量，提高编码效率，因为很多时候这些配置采用默认配置即可。

在介绍成员缩减及对应的Attribute之前，先再次回顾一下切面类型的基础情况。所有切面类型都实现`IMo`接口，切面类型的属性成员缩减会影响下面是所有的切面类型：
```
IMo                        # 切面类型基础接口，所有切面类型都需要实现该接口
├── RawMoAttribute         # 继承Attribute，可自定义同步和异步切面方法
│   ├── MoAttribute        # 仅可自定义同步切面方法，异步切面方法使用调用同步切面方法的默认实现
│   └── AsyncMoAttribute   # 仅可自定义异步切面方法，同步切面方法使用调用异步切面方法的默认实现
└── RawMo                  # 与RawMoAttribute功能相同，唯一差别是RawMo未继承Attribute
    ├── Mo                 # 与MoAttribute功能相同，唯一差别是Mo未继承Attribute
    └── AsyncMo            # 与AsyncMoAttribute功能相同，唯一差别是AsyncMo未继承Attribute
```

1. `Flags`属性和`Pattern`属性提取到`PointcutAttribute`中

    ```csharp
    [Pointcut("execution(* *(..))")] // 现在的用法
    public class Test1Attribute : MoAttribute
    {
        // 之前的用法
        public override string Pattern => "execution(* *(..))";
    }

    [Pointcut(AccessFlags.Public)] // 现在的用法
    public class Test2Attribute : MoAttribute
    {
        // 之前的用法
        public override AccessFlags Flags => AccessFlags.Public;
    }
    ```
    `Flags`和`Pattern`作为属性时还可以在应用Attribute时指定，比如下面的例子：
    ```csharp
    public class TestAttribute : MoAttribute
    {
        public new string Pattern { get; set; }
    }

    // 在应用TestAttribute时指定Pattern
    [assembly: Test(Pattern = "method(* *(..))")]
    ```
    应用Attribute时指定`Flags`和`Pattern`的方式更具灵活性，为了继续保持这种灵活的用法，新增`IFlexibleModifierPointcut`和`IFlexiblePatternPointcut`接口：
    ```csharp
    public class Test1Attribute : MoAttribute, IFlexiblePatternPointcut
    {
        // 实现IFlexiblePatternPointcut接口后可以用以前的方式直接给Pattern赋值，当然也可以使用PointcutAttribute指定
        public string Pattern { get; set; } = "execution(* *(..))";
    }

    [Pointcut(AccessFlags.Public)]] // 实现IFlexibleModifierPointcut接口后可以继续使用PointcutAttribute指定，同时也可以用以前的方式直接给Flags赋值
    public class Test2Attribute : MoAttribute, IFlexibleModifierPointcut
    {
        public AccessFlags Flags { get; set; }
    }
    ```

2. `Features`属性提取到`AdviceAttribute`中

    ```csharp
    [Advice(Feature.OnEntry)] // 现在的用法
    public class TestAttribute : MoAttribute
    {
        // 之前的用法
        public override Feature Features => Feature.OnEntry;
    }
    ```
    切面类型所具有的功能一般在定义Attribute时就已经确定，所以没有提供类似`IFlexibleFeaturePointcut`的接口。

3. `MethodContextOmits`属性和`ForceSync`属性提取到`OptimizationAttribute`中

    ```csharp
    [Optimization(MethodContext = Omit.All, ForceSync = ForceSync.All)] // 现在的用法
    public class TestAttribute : MoAttribute
    {
        // 之前的用法
        public override Omit MethodContextOmits => Omit.All;
        public override ForceSync ForceSync => ForceSync.All;
    }
    ```
    是否强制执行同步切面方法和`MethodContext`成员优化一般在定义切面类型时就已经确定，所以`ForceSync`和`Omit`没有提供类似`IFlexibleOptimizationPointcut`的接口。

4. `Order`属性提取到`IFlexibleOrderable`中

    ```csharp
    public class TestAttribute : MoAttribute, IFlexibleOrderable
    {
        // 实现IFlexibleOrderable接口后，Order用法和以前一样，可以在定义类时直接指定，也可以在应用Attribute时通过属性指定
        public double Order { get; set; }
    }
    ```

### ref struct参数及返回值处理

通过`MethodContext.Arguments`和`MethodContext.ReturnValue`可以分别获取到当前方法的参数列表和返回值，但由于`ref struct`无法进行装箱/拆箱操作（详见 [#61](https://github.com/inversionhourglass/Rougamo/issues/61)），所以在5.0.0版本之前，可以在应用Attribute时通过`MethodContextOmits`指定忽略参数或返回值：

```csharp
[Test(MethodContextOmits = Omit.Arguments | Omit.ReturnValue)]
public ReadOnlySpan<char> M(ReadOnlySpan<char> value) => default;
```

这里再次说明一下，为什么不默认忽略`ref struct`，这是因为这一默认行为可能会让你在不知情的情况下无法达成预期目标，编译时发现`ref struct`参数/返回值进行报错提示，由开发者决定是忽略`ref struct`参数/返回值或者是采用其他处理方案。

#### SkipRefStructAttribute

在5.0.0版本中，`MethodContextOmits`被提取到`OptimizationAttribute`中，同时没有提供类似`IFlexibleOptimizationPointcut`的接口，所以无法在应用Attribute时指定`Omit`，但在5.0.0版本中新增`SkipRefStructAttribute`用于处理这种情况：

```csharp
[SkipRefStruct]
[Test]
public ReadOnlySpan<char> M(ReadOnlySpan<char> value) => default;
```

这样的优势在于，如果方法上应用了多个切面类型，不再需要为每个切面类型指定`MethodContextOmits`，同时`SkipRefStructAttribute`还可以应用在类和程序集上，可以在更大范围上声明忽略`ref struct`。

#### 配置项skip-ref-struct

在确定当前程序集默认忽略`ref struct`的情况下，可以通过配置项`skip-ref-struct`为当前程序集应用这个设定，配合 [Cli4Fody](https://github.com/inversionhourglass/Cli4Fody) 可以实现非侵入式配置，`skip-ref-struct`设置为`true`的效果等同于`[assembly: SkipRefStruct]`。

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
	<Rougamo skip-ref-struct="true" />
</Weavers>
```

### 自定义切面类型生命周期

在2.2版本中，肉夹馍新增了[结构体](https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E7%BB%93%E6%9E%84%E4%BD%93)，用于优化切面类型GC。前段时间又编写了一个[Pooling](https://github.com/inversionhourglass/Pooling)组件，通过该组件可以在编译时将切面类型的new操作替换为对象池操作（详见博客：[.NET无侵入式对象池解决方案](https://www.cnblogs.com/nigture/p/18468831#rougamo%E9%9B%B6%E4%BE%B5%E5%85%A5%E5%BC%8F%E4%BC%98%E5%8C%96%E6%A1%88%E4%BE%8B)），原本计划使用[Pooling](https://github.com/inversionhourglass/Pooling)完成对切面类型的GC优化，但思来想去，还是决定将对象池功能内置，同时新增单例模式。

在5.0版本中，可以通过`LifetimeAttribute`指定切面类型的生命周期：

```csharp
[Lifetime(Lifetime.Transient)] // 临时，每次创建都是直接new。在没有应用LifetimeAttribute时，默认为Transient
public class Test1Attribute : MoAttribute { }

[Lifetime(Lifetime.Pooled)] // 对象池，每次创建都从对象池中获取
public class Test2Attribute : MoAttribute { }

[Lifetime(Lifetime.Singleton)] // 单例
public class Test3Attribute : MoAttribute { }
```

需要注意的是，不是所有切面类型无脑设置为对象池或单例模式即可完成优化。选择生命周期时需要注意以下几点：
1. 推荐程度：单例 > 对象池 > 临时
2. 单例要求切面类型必须是无状态的，必须包含无参构造方法，且应用切面类型时不可调用有参构造方法或设置属性

    ```csharp
    [Lifetime(Lifetime.Singleton)]
    public class SingletonAttribute : MoAttribute
    {
        public SingletonAttribute() { }

        public SingletonAttribute(int value) { }

        public int X { get; set; }
    }

    [Singleton(1)]     // 编译时报错，不可调用有参构造方法
    [Singleton(X = 2)] // 编译时报错，不可设置属性
    ```

3. 对象池要求切面类型必须包含无参构造方法，且应用切面类型时不可调用有参构造方法。如果切面类型有状态可实现`Rougamo.IResettable`接口，并在`TryReset`方法中完成状态重置，或在`OnExit / OnExitAsync`中完成状态重置

    ```csharp
    [Lifetime(Lifetime.Pooled)]
    public class PooledAttribute : MoAttribute, IResettable
    {
        public SingletonAttribute() { }

        public SingletonAttribute(int value) { }

        public int X { get; set; }

        public override void OnExit(MethodContext context)
        {
            // 可以在OnExit中状态重置，比如将X重置为0
            X = 0;
        }

        public bool TryReset()
        {
            // 也可以实现IResettable接口，在该方法中完成状态重置
            X = 0;

            // 返回true表示重置成功，返回false，当前对象将会直接抛弃，不会返回到对象池中
            return true;
        }
    }

    [Pooled(1)]     // 编译时报错，不可调用有参构造方法
    [Pooled(X = 2)] // 支持的操作，所以如果需要在应用时设置一些状态，可以用属性的方式而不要用构造方法参数的方式
    ```

总结来说，如果可以将切面类型设计为无状态的，推荐使用单例；如果无法保证无状态，但可以管理好状态，推荐使用对象池；如果无法很好的管理状态，还可以使用结构体；最后，如果对GC优化要求没那么严格，使用默认的临时策略即可。

### MethodContext对象池化

在5.0版本中，`MethodContext`将默认从对象池中获取，这一默认行为将在较大程度上优化Rougamo产生的GC。

`MethodContext`的对象池和切面类型的对象池用的是同一个，可以通过环境变量设置对象池最大持有数量，默认为`CPU逻辑核心数 * 2`（不同类型分开）。

| 环境变量 | 说明 |
|:-------:|:-----|
| `NET_ROUGAMO_POOL_MAX_RETAIN` | 对象池最大持有对象数量，对所有类型生效 |
| `NET_ROUGAMO_POOL_MAX_RETAIN_<TypeFullName>` | 指定类型对象池最大持有对象数量，覆盖`NET_ROUGAMO_POOL_MAX_RETAIN`配置，`<TypeFullName>`为指定类型的全名称，命名空间分隔符`.`替换为`_` |

## 异常堆栈信息优化

[[#82](https://github.com/inversionhourglass/Rougamo/issues/82)]。Rougamo自4.0版本开始全面使用代理织入的方式，由于该方式会为被拦截方法额外生成一个代理方法，所以在堆栈信息中会额外产生一层调用堆栈，在程序抛出异常时，调用堆栈会显得复杂且冗余：

```csharp
// 测试代码
public class TestAttribute : MoAttribute { }

try
{
    await M1();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

[Test]
public static async Task M1() => await M2();

[Test]
public static async ValueTask M2()
{
    await Task.Yield();
    M3();
}

[Test]
public static void M3() => throw new NotImplementedException();
```

Rougamo 5.0之前，上面代码在.NET6.0中运行的结果为（不同.NET版本堆栈信息可能有些差异）：
```
System.NotImplementedException: The method or operation is not implemented.
   at X.Program.$Rougamo_M3() in D:\X\Y\Z\Program.cs:line 49
   at X.Program.M3()
   at X.Program.$Rougamo_M2() in D:\X\Y\Z\Program.cs:line 43
   at X.Program.M2()
   at X.Program.M2()
   at X.Program.$Rougamo_M1() in D:\X\Y\Z\Program.cs:line 36
   at X.Program.M1()
   at X.Program.M1()
   at X.Program.Main(String[] args) in D:\X\Y\Z\Program.cs:line 13
```

Rougamo 5.0版本之后，运行结果为：
```
System.NotImplementedException: The method or operation is not implemented.
   at X.Program.$Rougamo_M3() in D:\X\Y\Z\Program.cs:line 49
   at X.Program.$Rougamo_M2() in D:\X\Y\Z\Program.cs:line 43
   at X.Program.$Rougamo_M1() in D:\X\Y\Z\Program.cs:line 36
   at X.Program.Main(String[] args) in D:\X\Y\Z\Program.cs:line 13
```

这种异常堆栈优化在.NET6.0及之后的.NET版本中是默认的，不需要任何操作，但对于.NET6.0之前的版本，需要调用`Exception`的扩展方法`ToNonRougamoString`来获取优化后的ToString字符串，或者调用`Exception`的扩展方法`GetNonRougamoStackTrace`获取优化后的调用堆栈。

之所以.NET6.0之后默认支持异常堆栈优化，是因为.NET6.0之后调用堆栈会默认排除应用了`StackTraceHiddenAttribute`的方法。

你可能注意到优化后的堆栈信息中的方法名都增加`$Rougamo_`前缀，而不是原方法名，这是因为代理织入后拥有`$Rougamo_`前缀的才是原方法，原方法名对应的方法现在时代理方法。正如前面未优化的堆栈信息所示，只有拥有`$Rougamo_`前缀的方法才有对应的行号信息，同时考虑到这种固定前缀并不会影响阅读分析，所以没有做额外的处理。

另外，如果你有特殊需求不需要这种堆栈信息优化，可以将配置项`pure-stacktrace`设置为`false`。
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
	<Rougamo pure-stacktrace="false" />
</Weavers>
```

## AspectN新语法

新增`ctor`和`cctor`，用于用于快速匹配构造方法和静态构造方法。

- `ctor(<declaring-type>([<parameter-type>..]))`

    ```csharp
    // 匹配所有构造方法
    [Pointcut("ctor(*(..))")]

    // 匹配所有非泛型类型的构造方法
    [Pointcut("ctor(*<!>(..))")]

    // 匹配IService子类的构造方法
    [Pointcut("ctor(IService+(..))")]

    // 匹配所有无参构造方法
    [Pointcut("ctor(*())")]

    // 匹配所有包含三个参数（任意类型）的构造方法
    [Pointcut("ctor(*(,,))")]

    // 匹配两个参数分别为int和Guid的构造方法
    [Pointcut("ctor(*(int,System.Guid))")]
    ```

- `cctor(<declaring-type>)`

    ```csharp
    // 匹配所有静态构造方法
    [Pointcut("cctor(*)")]

    // 匹配类名包含Singleton的静态构造方法
    [Pointcut("cctor(*Singleton*)")]
    ```

## 配置化非侵入式织入

**todo**

## NuGet优化

**todo**

## 其他配置项变更

除了前面在[配置项skip-ref-struct](#配置项skip-ref-struct)中介绍的`skip-ref-struct`，在[异常堆栈信息优化](#异常堆栈信息优化)中介绍的`pure-stacktrace`，以及在[配置化非侵入式织入](#配置化非侵入式织入)中介绍的系列新配置之外，还有以下配置项变更：

- **删除配置项`moarray-threshold`**

    该配置项是用数组优化大量切面类型应用到方法时，用遍历数组执行切面方法的方式代替每个切面类型单独执行切面方法，以此达到精简MSIL的目的。但随着Rougamo的功能完善，在4.0版本中因异步切面的加入，使得异步方法无法使用数组达到预期优化。在5.0版本中，随着对象池的加入，同步方法也难以使用数组完成预期优化。综合复杂度和实际优化效果考虑，最终决定在5.0版本中移除配置项`moarray-threshold`。
