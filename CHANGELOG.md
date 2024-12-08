# 5.0.0

## 性能优化

### 切面类型属性成员缩减

成员缩减以减少切面类型实例化后内存占用，将目前`IMo`中定义的所有属性全部删除，分别提供相应的`Attribute`和接口，具体改动如下：

| 5.0 之前切面类型属性 | 5.0 对应的Attribute   | 5.0 对应的Interface       |
|:------------------:|:---------------------:|:-------------------------:|
| Flags              | PointcutAttribute     | IFlexibleModifierPointcut |
| Pattern            | PointcutAttribute     | IFlexiblePatternPointcut  |
| Features           | AdviceAttribute       |            /              |
| MethodContextOmits | OptimizationAttribute |            /              |
| ForceSync          | OptimizationAttribute |            /              |
| Order              |          /            | IFlexibleOrderable        |

**Q: 为什么升级后有的只有Attribute，有的只有接口，而有的两个都有**

**A:** 在5.0之前的版本可以通过`new`关键字为属性增加setter，然后在应用切面类型时通过属性动态配置（如下代码所示）。这种方式在5.0版本中默认不支持，但可以通过上表中的接口实现，对于没有提供相应接口的属性是经过考虑后认为不会在应用切面类型时指定的。对于Attribute，从上表中可以看到，只有`Order`属性没有提供对应的Attribute，这是因为考虑到`Order`一般都是应用切面类型时动态指定，当然，如果你希望指定默认值，同样可以实现`IFlexibleOrderable`接口后指定默认值。

```csharp
// 5.0之前的用法
public class TestAttribute : MoAttribute
{
    // 默认Pattern只有getter，通过new关键字为Pattern增加setter
    public new Pattern { get; set; }
}

[Test(Pattern = "method(* *.Try*(..))")] // 应用Attribute动态指定Pattern
public class Cls { }

// 5.0之后实现应用Attribute时动态指定Pattern属性
public class TestAttribute : MoAttribute, IFlexiblePatternPointcut
{
    public string Pattern { get; set; }
}
```

下面展示 5.0 升级前后的差异：

```csharp
// 5.0之前的切面类型定义
public class TestAttribute : MoAttribute
{
    public override AccessFlags Flags => AccessFlags.All | AccessFlags.Method;

    public override string Pattern => "method(* *(..))";

    public override Feature Features => Feature.OnEntry;

    public override ForceSync ForceSync => ForceSync.All;

    public override Omit MethodContextOmits => Omit.None;

    public override double Order => 2;
}

// 5.0的切面类型定义
[Pointcut("method(* *(..))")] // 由于pattern的优先级高于修饰符Flgas，所以这里只保留了pattern
[Advice(Feature.OnEntry)]
[Optimization(ForceSync = ForceSync.All, MethodContext = Omit.None)]
public class T1Attribute : MoAttribute, IFlexibleOrderable
{
    public double Order { get; set; } = 2;
}
```

#### Roslyn代码分析器

本次的属性成员变动较大，升级后手动修改会比较繁琐，同时还可能出现遗漏。虽然Rougamo在编译时会对切面类型进行检查，并在发现不符合规范的切面类型时产生一个编译错误。但 5.0 提供了更好的升级体验，新增 Roslyn 代码分析器和代码修复程序，可以在编写代码时直接发现问题并提供快捷修复。

![](https://raw.githubusercontent.com/inversionhourglass/Rougamo/82840776d1bc1fab5576c261899458b95cf22630/member_migration.gif)

### ref struct参数及返回值处理

由于`ref struct`无法进行装箱/拆箱操作，所以`ref struct`参数和返回值无法保存到`MethodContext`中，5.0 之前推荐通过`Omit.Arguments`和`Omit.ReturnValue`处理该问题，详见 [#61](https://github.com/inversionhourglass/Rougamo/issues/61#issuecomment-1985508754).

#### SkipRefStructAttribute

在 5.0 版本中新增`SkipRefStructAttribute`用于处理`ref struct`：

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

在5.0版本中，可以通过`LifetimeAttribute`指定切面类型的生命周期，通过`Pooled`和`Singleton`生命周期可以优化Rougamo产生的GC.

```csharp
[Lifetime(Lifetime.Transient)] // 临时，每次创建都是直接new。在没有应用LifetimeAttribute时，默认为Transient
public class Test1Attribute : MoAttribute { }

[Lifetime(Lifetime.Pooled)] // 对象池，每次创建都从对象池中获取
public class Test2Attribute : MoAttribute { }

[Lifetime(Lifetime.Singleton)] // 单例
public class Test3Attribute : MoAttribute { }
```

需要注意的是，`Pooled`和`Singleton`生命周期要求切面类型必须包含无参构造方法，另外`Singleton`还要求不能在应用切面类型Attribute时动态指定属性，避免并发问题。

#### IResettable

`Pooled`生命周期使用对象池，要求对象在返回对象池之前要重置自身状态。重置操作可以直接在`OnExit`中做，或者更明确的实现`IResettable`，在`TryReset`中进行重置操作。

```csharp
[Lifetime(Lifetime.Pooled)]
public class PooledAttribute : MoAttribute, IResettable
{
    public SingletonAttribute() { }

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
```

### MethodContext对象池化

在 5.0 版本中，`MethodContext`将默认从对象池中获取，这一默认行为将在较大程度上优化Rougamo产生的GC。

`MethodContext`的对象池和切面类型的对象池用的是同一个，可以通过环境变量设置对象池最大持有数量，默认为`CPU逻辑核心数 * 2`（不同类型分开）。

| 环境变量                                      | 说明                                                         |
|:--------------------------------------------:|:-------------------------------------------------------------|
| `NET_ROUGAMO_POOL_MAX_RETAIN`                | 对象池最大持有对象数量，对所有类型生效                           |
| `NET_ROUGAMO_POOL_MAX_RETAIN_<TypeFullName>` | 指定类型对象池最大持有对象数量，覆盖`NET_ROUGAMO_POOL_MAX_RETAIN`配置，`<TypeFullName>`为指定类型的全名称，命名空间分隔符`.`替换为`_` |

## 异常堆栈信息优化

关联 [[#82](https://github.com/inversionhourglass/Rougamo/issues/82)]

Rougamo 自 4.0 版本开始全面使用代理织入的方式，由于该方式会为被拦截方法额外生成一个代理方法，所以在堆栈信息中会额外产生一层调用堆栈，在程序抛出异常时，调用堆栈会显得复杂且冗余：

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

这种异常堆栈优化在.NET6.0及之后的.NET版本中是默认的，不需要任何操作，但对于 .NET 6.0 之前的版本，需要调用`Exception`的扩展方法`ToNonRougamoString`来获取优化后的ToString字符串，或者调用`Exception`的扩展方法`GetNonRougamoStackTrace`获取优化后的调用堆栈。

之所以.NET6.0之后默认支持异常堆栈优化，是因为.NET6.0之后调用堆栈会默认排除应用了`StackTraceHiddenAttribute`的方法。

**关于优化后堆栈信息默认方法名自带`$Rougamo_`前缀的说明**  

代理织入使得实际方法全部增加`$Rougamo_`前缀，只有实际方法与PDB信息对应，可以获取行号等信息。不做额外处理去除前缀是因为没有必要，前缀固定不会影响阅读分析，额外操作去除前缀影响性能，同时也会导致 .NET 6.0 及以上版本无法无感知完成优化。如果确实想要去除该前缀，请自行处理。

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

5.0 可以通过配置`FodyWeavers.xml`完成切面类型应用，而不必再添加/修改C#代码。

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo>
    <Mos>
      <Mo assembly="Rougamo.OpenTelemetry" type="Rougamo.OpenTelemetry.OtelAttribute" pattern="method(* *Service.*(..))"/>
    </Mos>
  </Rougamo>
</Weavers>
```

在上面的例子中，`OtelAttribute`将被应用到类名以`Service`结尾的所有方法上。

上面的配置中，每一个`Mo`节点为一条应用规则，`Mos`节点下可以定义多个`Mo`节点，下面是`Mo`节点的属性说明：
- `type`，切面类型全名称
- `assembly`，切面类型所在程序集名称（不要.dll后缀）
- `pattern`，AspectN表达式，`type`将应用到该表达式匹配的方法上。该配置可选，未设置时将采用切面类型`type`自身的配置

配置结合 [Cli4Fody](https://github.com/inversionhourglass/Cli4Fody) 可以实现零侵入式的代码织入，详细可参考 [Pooling](https://github.com/inversionhourglass/Pooling) 与 [Cli4Fody](https://github.com/inversionhourglass/Cli4Fody) 的 [零侵入实践](https://github.com/inversionhourglass/Cli4Fody?tab=readme-ov-file#pooling%E9%9B%B6%E4%BE%B5%E5%85%A5%E5%AE%9E%E8%B7%B5).

## 其他

- **删除配置项`moarray-threshold`**

    该配置项是用数组优化大量切面类型应用到方法时，用遍历数组执行切面方法的方式代替每个切面类型单独执行切面方法，以达到精简MSIL的目的。
    
    但随着Rougamo的功能完善，在4.0版本中因异步切面的加入，使得异步方法无法使用数组达到预期优化。在5.0版本中，随着对象池的加入，同步方法也难以使用数组完成预期优化。
    
    综合复杂度和实际优化效果考虑，最终决定在5.0版本中移除配置项`moarray-threshold`。

- **新增ISyncMo和IAsyncMo接口**

    由于结构体无法继承父类/父结构体，所以在定义结构体切面类型时只能直接实现`IMo`接口，但该接口包含全部同步/异步切面方法，全部实现比较繁琐。

    Rougamo在 5.0 版本中新增`ISyncMo`和`IAsyncMo`，通过 [默认接口方法](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/proposals/csharp-8.0/default-interface-methods) 对部分方法提供默认实现。

    默认接口方法需要 SDK 最低 .NET Core 3.0 的版本，所以只有 .NET Core 3.0 及以上版本才有`ISyncMo`和`IAsyncMo`两个接口。

    ```csharp
    // 实现ISyncMo接口可以不用实现异步切面方法
    [Pointcut("method(* *(..))")]
    public struct SyncMo : ISyncMo
    {
        public void OnEntry(MethodContext context) { }

        public void OnException(MethodContext context) { }

        public void OnExit(MethodContext context) { }

        public void OnSuccess(MethodContext context) { }
    }

    // 实现IAsyncMo接口可以不用实现同步切面方法
    [Pointcut("method(* *(..))")]
    public struct AsyncMo : IAsyncMo
    {
        public ValueTask OnEntryAsync(MethodContext context) => default;

        public ValueTask OnExceptionAsync(MethodContext context) => default;

        public ValueTask OnExitAsync(MethodContext context) => default;

        public ValueTask OnSuccessAsync(MethodContext context) => default;
    }

    // 应用切面类型
    [assembly: Rougamo<SyncMo>]
    [assembly: Rougamo(typeof(AsyncMo))]
    ```
