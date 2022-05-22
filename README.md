# Rougamo - 肉夹馍

### rougamo是什么
静态代码织入AOP，.NET最常用的AOP应该是Castle DynamicProxy，rougamo的功能与其类似，但是实现却截然不同，
DynamicProxy是运行时生成一个代理类，通过方法重写的方式执行织入代码，rougamo则是代码编译时直接修改IL代码，
.NET静态AOP方面有一个很好的组件PostSharp，rougamo的注入方式也是与其类似的。

### 快速开始(MoAttribute)
```csharp
// 1.NuGet引用Rougamo.Fody
// 2.定义类继承MoAttribute，同时定义需要织入的代码
public class LoggingAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // 从context对象中能取到包括入参、类实例、方法描述等信息
        Log.Info("方法执行前");
    }

    public override void OnException(MethodContext context)
    {
        Log.Error("方法执行异常", context.Exception);
    }

    public override void OnSuccess(MethodContext context)
    {
        Log.Info("方法执行成功后");
    }

    public override void OnExit(MethodContext context)
    {
        Log.Info("方法退出时，不论方法执行成功还是异常，都会执行");
    }
}

// 3.应用Attribute
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

##### 根据方法可访问性批量应用
在快速开始中介绍了如何将代码织入到指定方法上，但实际使用时，一个庞大的项目如果每个方法或很多方法都去加上这个Attribute
可能会很繁琐，所以`MoAttribute`设计为可以应用于方法(method)、类(class)、程序集(assembly)和模块(module)，同时设置了
可访问性属性，增加灵活性
```csharp
// 1.在继承MoAttribute的同时，重写Flags属性，未重写时默认InstancePublic(public实例方法)
public class LoggingAttribute : MoAttribute
{
    // 改为所有public方法有效，不论是实例方法还是静态方法
    public override AccessFlags Flags => AccessFlags.Public;

    // 方法重写省略
}

// 2.应用
// 2.1.应用于类上
[Logging]
public class Service
{
    // 静态织入将被应用
    public static void M1() { }

    // 静态织入将被应用
    public void M2() { }

    // 不会应用静态织入
    protected void M3() { }
}
// 2.2.应用于程序集上，该程序集所有public方法都将应用静态织入
[assembly: Logging]
```

### 异常处理和修改返回值（v1.1.0）
在`OnException`方法中可以通过调用`MethodContext`的`HandledException`方法表明异常已处理并设置返回值，
在`OnSuccess`方法中可以通过调用`MethodContext`的`ReplaceReturnValue`方法修改方法实际的返回值，需要注意的是，
不要直接通过`ReturnValue`、`ExceptionHandled`等这些属性来修改返回值和处理异常，`HandledException`和
`ReplaceReturnValue`包含一些其他逻辑，后续可能还会更新。
```csharp
public class TestAttribute : MoAttribute
{
    public override void OnException(MethodContext context)
    {
        // 处理异常并将返回值设置为newReturnValue，如果方法无返回值(void)，直接传入null即可
        context.HandledException(this, newReturnValue);
    }

    public override void OnSuccess(MethodContext context)
    {
        // 修改方法返回值
        context.ReplaceReturnValue(this, newReturnValue);
    }
}
```

### 忽略织入(IgnoreMoAttribute)
在快速开始中，我们介绍了如何批量应用，由于批量引用的规则只限定了方法可访问性，所以可能有些符合规则的方法并不想应用织入，
此时便可使用`IgnoreMoAttribute`对指定方法/类进行标记，那么该方法/类(的所有方法)都将忽略织入。如果将`IgnoreMoAttribute`
应用到程序集(assembly)或模块(module)，那么该程序集(assembly)/模块(module)将忽略所有织入。另外，在应用`IgnoreMoAttribute`
时还可以通过MoTypes指定忽略的织入类型。
```csharp
// 当前程序集忽略所有织入
[assembly: IgnoreMo]
// 当前程序集忽略TheMoAttribute的织入
[assembly: IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]

// 当前类忽略所有织入
[IgnoreMo]
class Class1
{
    // ...
}

// 当前类忽略TheMoAttribute的织入
[IgnoreMo(MoTypes = new[] { typeof(TheMoAttribute))]
class Class2
{
    // ...
}
```

### 通过实现空接口的方式进行织入(IRougamo<>)
如果每个方法进行Attribute标记感觉太过繁琐，而通过方法可访问性进行批量织入又不够自定义，那么该方式或许能满足你的需求。
```csharp
// 1.定义需要织入的代码，也可以直接使用快速开始中定义的LoggingAttribute
public class LoggingMo : IMo
{
    public override AccessFlags Flags => AccessFlags.All;

    public override void OnEntry(MethodContext context)
    {
        // 从context对象中能取到包括入参、类实例、方法描述等信息
        Log.Info("方法执行前");
    }

    public override void OnException(MethodContext context)
    {
        Log.Error("方法执行异常", context.Exception);
    }

    public override void OnExit(MethodContext context)
    {
        Log.Info("方法退出时，不论方法执行成功还是异常，都会执行");
    }

    public override void OnSuccess(MethodContext context)
    {
        Log.Info("方法执行成功后");
    }
}

// 2.定义空接口
public interface ILoggingRougamo : IRougamo<LoggingMo>
{
}

// 3.应用空接口，如果你编程时习惯将同一类型/领域的类定义一个父接口/父类，那么只需要父接口/父类实现该接口即可
public interface IRepository<TModel, TId> : ILoggingRougamo
{
    // ...
}
```

### 织入互斥
##### 单类型互斥(IRougamo<,>)
由于我们有Attribute标记和接口实现两种织入方式，那么就可能出现同时应用的情况，而如果两种织入的内容是相同的，那就会出现
重复织入的情况，为了尽量避免这种情况，在接口定义时，可以定义互斥类型，也就是同时只有一个能生效，具体哪个生效，根据
[优先级](#Priority)来定
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
        // Mo2Attribute应用于方法上，优先级高于接口实现的Mo1Attribute，Mo2Attribute将被应用
    }

    [Mo3]
    public void M2()
    {
        // Mo1Attribute和Mo3Attribute不互斥，两个都将被应用
    }
}
```
##### 多类型互斥(IRepulsionsRougamo<,>)
`IRougamo<,>`只能与一个类型互斥，`IRepulsionsRougamo<,>`则可以与多个类型互斥
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
        // Mo1与Mo2、Mo3互斥，但由于Mo3优先级高于Mo1，所以Mo1不生效时，所有互斥类型都将生效
        // 所以最终Mo2Attribute、Mo3Attribute、Mo5Attribute将被应用
        Console.WriteLine("m1");
    }

    [Mo4]
    public void M2()
    {
        // Mo1与Mo2、Mo3互斥，但由于Mo1优先级高于Mo2，所以Mo2将不生效
        // 最终Mo1Attribute、Mo4Attribute、Mo5Attribute将被应用
        Console.WriteLine("m2");
    }
}
```
<font color=red>通过上面的例子，你可能注意到，这个多类型互斥并不是多类型之间互相互斥，而是第一个泛型与第二个泛型定义的类型互斥，
第二个泛型之间并不互斥，也就像上面的示例那样，当`Mo1Attribute`不生效时，与它互斥的`Mo2Attribute`、`Mo3Attribute`都将生效。
这里需要理解，定义互斥的原因是Attribute和空接口实现两种方式可能存在的重复应用，而并不是为了排除所有织入的重复。同时也不推荐使用
多互斥定义，这样容易出现逻辑混乱，建议在应用织入前仔细思考一套统一的规则，而不是随意定义，然后试图使用多互斥来解决问题</font>

### Attribute代理织入(MoProxyAttribute)
如果你已经使用一些第三方组件对一些方法进行了Attribute标记，现在你希望对这些标记过的方法进行aop操作，但又不想一个一个手动增加rougamo
的Attribute标记，此时便可以通过代理的方式一步完成aop织入。再比如你的项目现在有很多标记了`ObsoleteAttribute`的过时方法，你希望
在过期方法在被调用时输出调用堆栈日志，用来排查现在那些入口在使用这些过期方法，也可以通过该方式完成。
```csharp
public class ObsoleteProxyMoAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        Log.Warning("过期方法被调用了：" + Environment.StackTrace);
    }
}

[module: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]

public class Cls
{
    [Obsolete]
    private int GetId()
    {
        // 该方法将应用织入代码
        return 123;
    }
}
```

### 优先级(Priority)
1. `IgnoreMoAttribute`
2. Method `MoAttribute`
3. Method `MoProxyAttribute`
4. Type `MoAttribute`
5. Type `MoProxyAttribute`
6. Type `IRougamo<>`, `IRougamo<,>`, `IRepulsionsRougamo<,>`
7. Assembly & Module `MoAttribute`

### 开关(enable/disable)
Rougamo由个人开发，由于能力有限，对IL的研究也不是那么深入透彻，并且随着.NET的发展也会不断的出现一些新的类型、新的语意甚至新的IL指令，
也因此可能会存在一些BUG，而当IL层面的BUG可能无法快速定位到问题并修复，所以这里提供了一个开关可以在不去掉Rougamo引用的情况下不进行代码织入，
也因此推荐各位在使用Rougamo进行代码织入时，织入的代码是不影响业务的，比如日志和APM。如果希望使用稳定且在遇到问题能够快速得到支持的静态织入
组件，推荐使用[PostSharp](https://www.postsharp.net)

Rougamo是在[fody](https://github.com/Fody/Fody)的基础上研发的，引用Rougamo后首次编译会生成一个`FodyWeavers.xml`文件，默认内容如下
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo />
</Weavers>
```
在希望禁用Rougamo时，需要在配置文件的`Rougamo`节点增加属性`enabled`并设置值为`false`
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo enabled="false" />
</Weavers>
```

### 记录yield return IEnumerable/IAsyncEnumerable返回值
我们知道，使用`yield return`语法糖+`IEnumerable`返回值的方法，在调用方法结束后，该方法的代码实际并没有执行，代码的实际执行是在你访问
这个`IEnumerable`对象里的元素时候，比如你去foreach这个对象或者调用`ToList/ToArray`的时候，并且返回的这些元素并没有一个数组/链表进行
统一的保存（具体原理在这里不展开说明了），所以默认情况下是没有办法直接获取到`yield return IEnumerable`返回的所有元素集合的。

但可能有些对代码监控比较严格的场景需要记录所有返回值，所以在实现上我创建了一个数组保存了所有的返回元素，但是由于这个数组是额外创建的，会占
用额外的内存空间，同时又不清楚这个`IEnumerable`返回的元素集合有多大，所以为了避免可能额外产生过多的内存消耗，默认情况下是不会记录
`yield return IEnumerable`返回值的，如果需要记录返回值，需要在`FodyWeavers.xml`的`Rougamo`节点增加属性配置`enumerable-returns="true"`
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo enumerable-returns="true" />
</Weavers>
```

### todo
1. 测试代码
2. 英文文档