
# Rougamo - 肉夹馍

[中文](README.md) | English

## What is Rougamo
Rougamo is a AOP component that takes effect at compile time. There are some well-known AOP components like Castle, Autofac and AspectCore. Unlike these components, which are implemented through dynamic proxying and IoC at runtime, Rougamo modifies target methods' IL during compilation. If you are familiar with the AOP component PostSharp, then Rougamo is similar to PostSharp.

# Weaving Methods

## Quick start with Attribute
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

// 3.Apply Attribute to assembly or class or method
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

## Flag Match
在快速开始中介绍了如何将代码织入到指定方法上，但实际使用时，一个项目中可能有很多方法都需要应用该Attribute，如果手动到每个方法上增加这样一个Attribute，那么将会很繁琐且侵入性比较大，所以`MoAttribute`设计为可以应用于方法(method)、类(class)、程序集(assembly)和模块(module)，同时可以通过`MoAttribute`的`Flags`属性选择符合特征的方法，`Flags`目前可以设置以下特征：
- 方法可访问性（public或者非public）
- 是否静态方法
- 方法类型：方法/属性/getter/setter/构造方法

```csharp
// 在继承MoAttribute的同时，重写Flags属性，未重写时默认InstancePublic(可访问性为public的实例方法或属性)
public class LoggingAttribute : MoAttribute
{
    // 所有的public方法，不论是静态还是实例
    public override AccessFlags Flags => AccessFlags.Public | AccessFlags.Method;

    // 所有的实例方法属性方法（getter和setter），不论是静态还是实例，普通方法不会被选择
    // public override AccessFlags Flags => AccessFlags.Instance | AccessFlags.Property;

    // 方法重写省略
}

// 2.应用
// 2.1.应用于类上
[Logging]
public class Service
{
    // ...
}

// 2.2.应用于程序集上
[assembly: Logging]
```

需要注意的是，在2.0版本之前`Flags`仅支持设置方法可访问性和是否静态方法，并不支持区分方法/属性/构造方法，在2.0版本前如果将`Attribute`应用于`class/module/assembly`，那么默认会匹配方法和属性的getter/setter。在2.0版本以后，为了兼容老的逻辑，在`Flags`不指定`Method/PropertyGetter/PropertySetter/Property/Constructor`中的任意一个时，那么默认就是方法和属性（`Method|Property`），所以如果在批量应用时你希望仅仅应用于方法时，就需要手动为`Flags`指定`AccessFlags.Method`。

## 接口织入
在前面介绍的方式中，我们主要是通过`Attribute`或应用到方法上或应用到类或程序集上，这种方式会需要我们修改项目代码，这是一种侵入式的织入方式，这种方式对于AOP埋点频繁的场景并不友好。此时通过接口织入的方式，可以大大降低甚至是避免侵入式织入。

```csharp
// 1.定义需要织入的代码（接口织入的方式可以直接实现IMo，当然也可以继续继承MoAttribute）
public class LoggingMo : IMo
{
    // 1.1.特征过滤
    public override AccessFlags Flags => AccessFlags.All | AccessFlags.Method;

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

// 2.应用空接口
public class TheService : ITheService, IRougamo<LoggingMo>
{
    // ...
}
```

在上面的示例中，需要织入代码的类只需要实现`IRougamo<LoggingMo>`这样一个空接口，与`LoggingMo`定义的`Flags`匹配的方法就会被织入`LoggingMo`的代码。看完上面的示例你可能会有个疑问，这不是还是要修改代码，不还是侵入式的吗。是的，上面的示例还是具侵入式的，但如果你们的项目会对一些通用层进行封装，比如上面Service层的`ITheService`你们有统一的基础接口或者`Service`具有统一的父类，那么此时就可以在父类/基础接口上操作了：

```csharp
// 在统一基础接口上直接应用IRougamo空接口
public interface IService : IRougamo<LoggingMo>
{
    // ...
}

// 业务接口实现基础接口
public interface ITheService : IService
{
    // ...
}

public class TheService : ITheService
{
    // ...
}
```

这种场景其实是很常见的，很多公司都会有自己的一套基础框架，在基础框架中对不同的层会定义一套基础父类/接口，比如`Service/Repository/Cache`等，这时候我们便可以直接在基础框架中通过`IRougamo`空接口的方式直接定义AOP，这样对于业务来说便是完全无侵入的了。

`IRougamo`能够带来更好的无侵入式体验，但也有其局限性，静态类无法实现接口，灵活性也不如`Attribute`，所以两者结合使用，才是最佳的使用方式。

## 表达式匹配
前面介绍了[特征匹配](#特征匹配)，这种方式简单易上手，对于粒度较大的匹配模式是很合适的，但这种方式的主要缺点就是粒度过大，无法进行更为精确的匹配，即使是后续的更新，也只是支持区分普通方法/属性/构造方法，很难做更为精确的匹配规则。

在2.0版本，Rougamo决定参考java一个广为人知的aop组件aspectj，使用字符串表达式进行匹配。字符串扩展性强，任意的组合格式可以完成绝大部分匹配需求。需要强调的是，由于作者对aspectj也不是完全熟练，所以Rougamo的表达式规则和aspectj可能存在一定出入，同时也针对C#的语法特性增加了一些语法支持，推荐有aspectj经验的朋友也简单了解一下再进行使用。

```csharp
public class PatternAttribute : MoAttribute
{
    // 使用表达式匹配，可以轻松进行方法名称匹配
    // 匹配所有方法名以Get开头的方法
    public override string? Pattern => "method(* *.Get*(..))";

    // 覆盖了特征匹配功能（除了构造方法）
    // 匹配所有public静态方法
    public override string? Pattern => "method(public static * *(..))";

    // 匹配所有getter
    public override string? Pattern => "getter(* *)";

    // 还能进行子类匹配
    // 匹配所有返回值是int集合的方法
    public override string? Pattern => "method(int[]||System.Collections.Generic.IEnumerable<int>+ *(..))";

    // 更多匹配规则，请查看后面的介绍
}
```

### 基础概念
特征匹配是重写`Flags`属性，对应的表达式匹配是重写`Pattern`属性，由于表达式匹配和特征匹配都是用于过滤/匹配方法的，所以两个不能同时使用，`Pattern`优先级高于`Flags`，当`Pattern`不为`null`时使用`Pattern`，否则使用`Flags`。

表达式共支持六种匹配规则，表达式必须是六种的其中一种：
- `method([modifier] returnType declaringType.methodName([parameters]))`
- `getter([modifier] propertyType declaringType.propertyName)`
- `setter([modifier] propertyType declaringType.propertyName)`
- `property([modifier] propertyType declaringType.propertyName)`
- `execution([modifier] returnType declaringType.methodName([parameters]))`
- `regex(REGEX)`

上面的六种规则中，`getter`, `setter`, `property`分别表示匹配属性的`getter`, `setter`和全部匹配（`getter`+`setter`），`method`表示匹配普通方法（非`getter/setter/constructor`），`execution`表示匹配所有方法，包含`getter/setter`。`regex`是个特例，将在[正则匹配](#正则匹配)中进行单独介绍。在表达式内容格式上，`method`和`execution`比`getter/setter/property`多一个`([parameters])`，这是因为属性的类型即可表示属性`getter`的返回值类型和`setter`的参数类型，所以相对于`method`和`execution`，省略了参数列表。

上面列出的六种匹配规则，除了`regex`的格式特殊，其他的五种匹配规则的内容主要包含以下五个（或以下）部分：
- `[modifier]`，访问修饰符，可以省略，省略时表示匹配所有，访问修饰符包括以下七个：
    - `private`
    - `internal`
    - `protected`
    - `public`
    - `privateprotected`，即`private protected`
    - `protectedinternal`，即`protected internal`
    - `static`，需要注意的是，省略该访问修饰符表示既匹配静态也匹配实例，如果希望仅匹配实例，可以与逻辑修饰符`!`一起使用：`!static`
- `returnType`，方法返回值类型或属性类型，类型的格式较为复杂，详见[类型匹配格式](#类型匹配格式)
- `declaringType`，声明该方法/属性的类的类型，[类型匹配格式](#类型匹配格式)
- `methodName/propertyName`，方法/属性的名称，名称可以使用`*`进行模糊匹配，比如`*Async`,`Get*`,`Get*V2`等，`*`匹配0或多个字符
- `[parameters]`，方法参数列表，Rougamo的参数列表匹配相对简单，没有aspectj那么复杂，仅支持任意匹配和全匹配
    - 使用`..`表示匹配任意参数，这里说的任意是指任意多个任意类型的参数
    - 如果不进行任意匹配，那么就需要指定参数的个数及类型，当然类型是按照[类型匹配格式](#类型匹配格式)进行匹配的。Rougamo不能像aspectj一样进行参数个数模糊匹配，比如`int,..,double`是不支持的

在上面列出的六种匹配规则中不包含构造方法的匹配，主要原因在于构造方法的特殊性。对构造方法进行AOP操作其实是很容易出现问题的，比较常见的就是在AOP时使用了还未初始化的字段/属性，所以我一般认为，对构造方法进行AOP时一般是指定特定构造方法的，一般不会进行批量匹配织入。所以目前对于构造方法的织入，推荐直接在构造方法上应用`Attribute`进行精确织入。另外由于`Flags`对构造方法的支持和表达式匹配都是在`2.0`新增的功能，目前并没有想好构造方法的表达式格式，等大家使用一段时间后，可以综合大家的建议再考虑，也为构造方法的表达式留下更多的操作空间。

### 类型匹配格式

#### 类型格式
首先我们明确，我们表达某一个类型时有这样几种方式：类型名称；命名空间+类型名称；程序集+命名空间+类型名称。由于Rougamo的应用上限是程序集，同时为了严谨，Rogamo选择使用命名空间+类型名称来表达一个类型。命名空间和类型名称之间的连接采用我们常见的点连接方式，即`命名空间.类型名称`。

#### 嵌套类
嵌套类虽然使用不多，但该支持的还是要支持到。Rougamo使用`/`作为嵌套类连接符，这里与平时编程习惯里的连接符`+`不一致，主要是考虑到`+`是一个特殊字符，表示[子类](#子类匹配)，为了方便阅读，所以采用了另一个符号。比如`a.b.c.D/E`就表示命名空间为`a.b.c`，外层类为`D`的嵌套类`E`。当然嵌套类支持多层嵌套。

#### 泛型
需要首先声明的是，泛型和`static`一样，在不声明时匹配全部，也就是既匹配非泛型类型也匹配泛型类型，如果希望仅匹配非泛型类型或仅匹配泛型类型时需要额外定义，泛型的相关定义使用`<>`表示。
- 仅匹配非泛型类型：`a.b.C<!>`，使用逻辑非`!`表示不匹配任何泛型
- 匹配任意泛型：`a.b.C<..>`，使用两个点`..`表示匹配任意多个任意类型的泛型
- 匹配指定数量任意类型泛型：`a.b.C<,,>`，示例表示匹配三个任意类型泛型，每添加一个`,`表示额外匹配一个任意类型的泛型，你可能已经想到了`a.b.C<>`表示匹配一个任意类型的泛型
- 开放式与封闭式泛型类型：未确定泛型类型的称为开放式泛型类型，比如`List<T>`，确定了泛型类型的称为封闭式泛型类型，比如`List<int>`，那么在编写匹配表达式时，如果希望指定具体的泛型，而不是像上面介绍的那种任意匹配，那么对于开放式未确定的泛型类型，可以使用我们常用的`T1,T2,TA,TX`等表示，对于封闭式确定的泛型类型直接使用确定的类型即可。
    ```csharp
    // 比如我们有如下泛型类型
    public class Generic<T1, T2>
    {
        public static void M(T1 t1, int x, T2 t2) { }
    }

    // 定义匹配表达式时，对于开放式泛型类型，并不需要与类型定义的泛型名称一致，比如上面叫T1,T2，表达式里用TA,TB
    public class TestAttribute : MoAttribute
    {
        public override string? Pattern => "method(* *<TA,TB>.*(TA,int,TB))";
    }
    ```
- 泛型方法：除了类可以定义泛型参数，方法也可以定义泛型参数，方法的泛型参数与类型的泛型参数使用方法一致，就不再额外介绍了
    ```csharp
    // 比如我们有如下泛型类型
    public class Generic<T1, T2>
    {
        public static void M<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) { }
    }

    // 定义匹配表达式时，对于开放式泛型类型，并不需要与类型定义的泛型名称一致，比如上面叫T1,T2，表达式里用TA,TB
    public class TestAttribute : MoAttribute
    {
        public override string? Pattern => "method(* *<TA,TB>.*<TX, TY>(TA,TB,TX,TY))";

        // 同样可以使用非泛型匹配、任意匹配和任意类型匹配
        // public override string? Pattern => "method(* *<TA,TB>.*<..>(TA,TB,*,*))";
    }
    ```

#### 模糊匹配
在前面介绍过两种模糊匹配，一种是名称模糊匹配`*`，一种是参数/泛型任意匹配`..`。在类型的模糊匹配上依旧使用的是这两个符号。

在[类型格式](#类型格式)中介绍到，类型格式由两部分组成`命名空间.类型名称`，所以类型的模糊匹配可以分为：命名空间匹配、类型名称匹配、泛型匹配、子类匹配，其中[泛型匹配](#泛型)在上一节刚介绍过，[子类匹配](#子类匹配)将在下一节介绍，本节主要讲述类型基本的模糊匹配规则。
- 类型名称匹配：类型名称的模糊匹配很简单，可以使用`*`匹配0或多个字符，比如`*Service`,`Mock*`,`Next*Repo*V2`等。需要注意的是，`*`并不能直接匹配任意嵌套类型，比如期望使用`*Service*`来匹配`AbcService+Xyz`是不可行的，嵌套类型需要明确指出，比如`*Service/*`，匹配名称以`Service`结尾的类型的嵌套类，如果是二层嵌套类，也需要明确指出`*Service/*/*`
- 命名空间匹配
    - 缺省匹配：在命名空间缺省的情况下表示匹配任意命名空间，也就是只要类型名称即可，比如表达式`Abc`可以匹配`l.m.n.Abc`也可以匹配`x.y.z.Abc`
    - 完全匹配：不使用任何通配符，编写完全的命名空间，即可进行完全匹配
    - 名称模糊：命名空间有一或多段，每一段之间用`.`连接，和类型名称匹配一样，每一段的字符都可以使用`*`自行匹配，比如`*.x*z.ab*.vv`
    - 多段模糊：使用`..`可以匹配0或多段命名空间，比如`*..xyz.Abc`可以匹配`a.b.xyz.Abc`也可以匹配`lmn.xyz.Abc`，`..`也可以多次使用，比如使用`a..internal..t*..Ab`匹配`a.internal.tk.Ab`和`a.b.internal.c.t.u.Ab`

#### 子类匹配
在前面介绍[接口织入](#接口织入)时有聊到，我们可以在父类/基础接口实现一个空接口`IRougamo<>`，这样继承/实现了父类/基础接口的类型的方法在条件匹配的情况下就会进行代码织入。那么这种方式是需要修改父类/基础接口才行，如果父类/基础接口是引用的第三方库或者由于流程原因不能直接修改，又该如何优化操作呢。此时就可以结合`assembly attribute`和子类匹配表达式来完成匹配织入了，定义匹配表达式`method(* a.b.c.IService+.*(..))`，这段表达式表示可匹配所有`a.b.c.IService`子类的所有方法，然后再通过`[assembly: Xx]`将`XxAttribute`应用到整个程序集即可。

如上面的示例所示，我们使用`+`表示进行子类匹配。除了方法的声明类型，返回值类型、参数类型都可以使用子类匹配。另外子类匹配还可以与通配符一起使用，比如`method(* *(*Provider+))`表示匹配方法参数仅一个且参数类型是以`Provider`结尾的类型的子类。

### 特殊语法

#### 基础类型简写
对于常用基础类型，Rougamo支持类型简写，让表达式看起来更简洁清晰。目前之前简写的类型有`bool`, `byte`, `short`, `int`, `long`, `sbyte`, `ushort`, `uint`, `ulong`, `char`, `string`, `float`, `double`, `decimal`, `object`, `void`。

#### Nullable简写
正如我们平时编程一样，我们可以使用`?`表示`Nullable`类型，比如`int?`即为`Nullable<int>`。需要注意的是，不要将引用类型的Nullable语法也当做`Nullable`类型，比如`string?`其实就是`string`，在Rougamo里面直接写`string`，而不要写成`string?`。

#### ValueTuple简写
我们在编写C#代码时，可以直接使用括号表示`ValueTuple`，在Rougamo中同样支持该比如，比如`(int,string)`即表示`ValueTuple<int, string>`或`Tuple<int, string>`。

#### Task简写
现在异步编程已经是基础的编程方式了，所以方法返回值为`Task`或`ValueTask`的方法将会非常之多，同时如果要兼容`Task`和`ValueTask`两种返回值，表达式还需要使用逻辑运算符`||`进行连接，那将大大增加表达式的复杂性。Rougamo增加了熟悉的`async`关键字用来匹配`Task`和`ValueTask`返回值，比如`Task<int>`和`ValueTask<int>`可以统一写为`async int`，那么对于非泛型的`Task`和`ValueTask`则写为`async null`。需要注意的是，目前没有单独匹配`async void`的方式，`void`会匹配`void`和`async void`。

#### 类型及方法简写
前面有介绍到，类型的表达由`命名空间.类型名称`组成，如果我们希望匹配任意类型时，标准的写法应该是`*..*`，其中`*..`表示任意命名空间，后面的`*`表示任意类型名称，对于任意类型，我们可以简写为`*`。同样的，任意类型的任意方法的标准写法应该是`*..*.*`，其中前面的`*..*`表示任意类型，之后的`.`是连字符，最后的`*`表示任意方法，这种我们同样可以简写为`*`。所以`method(*..* *..*.*(..))`和`method(* *(..))`表达的意思相同。

### 正则匹配
对于每个方法，Rougamo都会为其生成一个字符串签名，正则匹配即是对这串签名的正则匹配。其签名格式与`method/execution`的格式类似`modifiers returnType declaringType.methodName([parameters])`。

- `modifiers`包含两部分，一部分是可访问性修饰符，即`private/protected/internal/public/privateprotected/protectedinternal`，另一部分是是否静态方法`static`，非静态方法省略`static`关键字，两部分中间用空格分隔。
- `returnType/declaringType`均为`命名空间.类型名称`的全写，需要注意的是，在正则匹配的签名中所有的类型都是全名称，不可使用类似`int`去匹配`System.Int32`
- 泛型，类型和方法都可能包含泛型，对于封闭式泛型类型，直接使用类型全名称即可；对于开放式泛型类型，我们遵守以下的规定，泛型从`T1`开始向后增加，即`T1/T2/T3...`，增加的顺序按`declaringType`先`method`后的顺序，详细可看后续的示例
- `parameters`，参数按每个参数的全名称展开即可
- 嵌套类型，嵌套类型使用`/`连接

```csharp
namespace a.b.c;

public class Xyz
{
    // 签名：public System.Int32 a.b.c.Xyz.M1(System.String)
    public int M1(string s) => default;

    // 签名：public static System.Void a.b.c.Xyz.M2<T1>(T1)
    public static void M2<T>(T value) { }

    public class Lmn<TU, TV>
    {
        // 签名：internal System.Threading.Tasks.Task<System.DateTime> a.b.c.Xyz/Lmn<T1,T2>.M3<T3,T4>(T1,T2,T3,T4)
        internal Task<DateTime> M3<TO, TP>(TU u, TV v, TO o, TP p) => Task.FromResult(DateTime.Now);

        //签名：private static System.Threading.Tasks.ValueTask a.b.c.Xyz/Lmn<T1,T2>.M4()
        private static async ValueTask M4() => await Task.Yeild();
    }
}
```

正则匹配存在编写复杂的问题，同时也不支持子类匹配，所以一般不编写正则匹配规则，其主要是作为其他匹配规则的一种补充，可以支持一些更为复杂的名称匹配。由于Rougamo支持逻辑运算法，所以也给到正则更多辅助的空间，比如我们想要查找方法名不以`Async`结尾的`Task/ValueTask`返回值方法`method(async null *(..)) && regex(^\S+ (static )?\S+ \S+?(?<!Async)\()`。

# 编织功能

## 重写方法参数
在`OnEntry`中可以通过修改`MethodContext.Arguments`中的元素来修改方法的参数值，为了明确你是想要修改方法参数，还需要将`MethodContext.RewriteArguments`设置为`true`来确认重写参数。
```csharp
public class DefaultValueAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        context.RewriteArguments = true;

        // 判断参数类型最好通过下面ParameterInfo来判断，而不要通过context.Arguments[i].GetType()，因为context.Arguments[i]可能为null
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
    // 当传入null值时将返回空字符串
    [DefaultValue]
    public string EmptyIfNull(string value) => value;
}
```

## 修改返回值
在`OnEntry`和`OnSuccess`方法中可以通过调用`MethodContext`的`ReplaceReturnValue`方法修改方法实际的返回值，需要特别注意的是，不要直接通过`ReturnValue`属性来修改返回值，`ReplaceReturnValue`包含一些其他逻辑，后续可能还会更新。另外还需要注意，`Iterator/AsyncIterator`不可修改返回值。
```csharp
public class TestAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // 在OnEntry中修改返回值可以在执行织入方法前直接返回，常用于参数/上下文验证拦截
        context.ReplaceReturnValue(this, newReturnValue);
    }

    public override void OnSuccess(MethodContext context)
    {
        // 方法执行成功后，修改方法返回值
        context.ReplaceReturnValue(this, newReturnValue);
    }
}
```

## 异常处理
在`OnException`方法中可以通过调用`MethodContext`的`HandledException`方法表明异常已处理并设置返回值，同样的，不要直接通过`ReturnValue`、`ExceptionHandled`这些属性直接设置异常已处理和设置返回值。
```csharp
public class TestAttribute : MoAttribute
{
    public override void OnException(MethodContext context)
    {
        // 处理异常并将返回值设置为newReturnValue，如果方法无返回值(void)，直接传入null即可
        context.HandledException(this, newReturnValue);
    }
}
```

## 重试
重试功能可以在遇到指定异常或者返回值非预期值的情况下重新执行当前方法，实现方式是在`OnException`和`OnSuccess`中设置`MethodContext.RetryCount`值，在`OnException`和`OnSuccess`执行完毕后如果`MethodContext.RetryCount`值大于0那么就会重新执行当前方法。
```csharp
internal class RetryAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        // 初始化重试次数
        context.RetryCount = 3;
    }

    public override void OnException(MethodContext context)
    {
        // 出现异常减少一次重试次数，当RetryCount减到0时，表示达到重试次数上限，不再重试
        context.RetryCount--;
    }

    public override void OnSuccess(MethodContext context)
    {
        if (context.ReturnValue != ABC)
        {
            // 结果非预期减少一次重试次数，当RetryCount减到0时，表示达到重试次数上限，不再重试
            context.RetryCount--;
        }
        else
        {
            // 结果达到预期，直接将重试次数设置为0，不再进行重试
            context.RetryCount = 0;
        }
    }
}

// 应用RetryAttribute后，Test方法将会重试3次
[Retry]
public void Test()
{
    throw new Exception();
}
```
**针对异常处理重试的场景，作者创建了一个独立项目 [Rougamo.Retry](https://github.com/inversionhourglass/Rougamo.Retry) ，如果只是针对某种异常进行重试操作可以直接使用 [Rougamo.Retry](https://github.com/inversionhourglass/Rougamo.Retry)**

使用重试功能需要注意以下几点：
- 在通过`MethodContext.HandledException()`处理异常或通过`MethodContext.ReplaceReturnValue()`修改返回值时会直接将`MethodContext.RetryCount`置为0，因为手动处理异常和修改返回值就表示你已经决定了该方法的最终结果，所以就不再需要重试了
- `MoAttribute`的`OnEntry`和`OnExit`只会执行一次，不会因为重试而多次执行
- 尽量不要在`ExMoAttribute`中使用重试功能，除非你真的知道实际的处理逻辑。思考下面这段代码，`ExMoAttribute`无法在`Task`内部报错后重新执行整个外部方法
  ```csharp
  public Task Test()
  {
    DoSomething();

    return Task.Run(() => DoOtherThings());
  }
  ```

## 部分织入
Rougamo有四个方法可以重写`OnEntry`、`OnSucess`、`OnException`、`OnExit`，分别对应着方法生命周期的四个时间点，随着版本的更新，在不同的时间点还可以做一些额外的编织功能，也就是上面介绍的[重写方法参数](#重写方法参数)、[修改返回值](#修改返回值)、[异常处理](#异常处理)、[重试](#重试)。但实际上，我们一般用不上这所有的功能，比如我们为参数设置默认值只用得上`OnEntry`和重写方法参数，再比如我们再方法异常时记录异常并返回特定值只用得上`OnException`和异常处理。

在之前的版本，无论功能是否用得上，对应的代码都会织入到方法中，这样是会增加最终程序集大小的，特别是对于仅使用`OnEntry`功能的，在指定仅织入`OnEntry`后连try..catch..finally都不会织入。

部分织入时为了进一步减小Rougamo的织入体积和减少运行时运行指令而设计的，属于细节优化，默认是功能全启用的，可以通过重写`MoAttribute.Features`来设置，`Features`属性的类型为枚举，将织入该属性值对应的功能。
|      枚举值      |                           功能                                    |
|:---------------:|:------------------------------------------------------------------|
|       All       |包含全部功能，默认值                                                 |
|     OnEntry     |仅OnEntry，不可修改参数值，不可修改返回值                             |
|   OnException   |仅OnException，不可处理异常                                          |
|    OnSuccess    |仅OnSuccess，不可修改返回值                                          |
|     OnExit      |OnExit                                                             |
|   RewriteArgs   |包含OnEntry，同时可以在OnEntry中修改参数值                            |
|  EntryReplace   |包含OnEntry，同时可以在OnEntry中修改返回值                            |
| ExceptionHandle |包含OnException，同时可以在OnEntry中处理异常                          |
|  SuccessReplace |包含OnSuccess，同时可以在OnSuccess中修改返回值                        |
|  ExceptionRetry |包含OnException，同时可以在OnException中进行重试                      |
|  SuccessRetry   |包含OnSuccess，同时可以在OnSuccess中进行重试                          |
|     Retry       |包含OnException和OnSuccess，同时可以在OnException和OnSuccess中进行重试 |
|     Observe     |包含OnEntry、OnException、OnSuccess和OnExit，常用于日志、APM埋点等操作 |
| NonRewriteArgs  |包含除修改参数外的所有功能                                            |
|    NonRetry     |包含除重试外的所有功能                                                |

## 忽略织入
Rougamo是具有批量织入能力的，虽然目前版本提供了较为丰富的匹配规则，但是如果为了排除某个或某几个方法而让匹配表达式过于复杂，那就得不偿失了。对于排除特定某个或几个方法时，可以直接使用`IgnoreMoAttribute`，将其应用到方法上就是该方法忽略织入，应用到类上就是该类的所有方法忽略织入，应用到程序集上就是整个程序集的所有方法忽略织入。

另外，忽略织入时可以指定忽略的类型，比如一个方法应用了多个`MoAttribute`，可以单独忽略某一个，在不指定时表示忽略全部。
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

## Attribute代理织入
如果你已经使用一些第三方组件对一些方法进行了Attribute标记，现在你希望对这些标记过的方法进行aop操作，但又不想一个一个手动增加Rougamo的Attribute标记，此时便可以通过代理的方式一步完成aop织入。再比如你的项目现在有很多标记了`ObsoleteAttribute`的过时方法，你希望在过期方法在被调用时输出调用堆栈日志，用来排查现在那些入口在使用这些过期方法，也可以通过该方式完成。
```csharp
public class ObsoleteProxyMoAttribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        Log.Warning("过期方法被调用了：" + Environment.StackTrace);
    }
}

// 在程序集级别进行代理，所有标记了ObsoleteAttribute的方法都将应用ObsoleteProxyMoAttribute
[assembly: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]

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

## async/non-async统一处理
我们在实现`MoAttribute`时，在`OnEntry/OnSuccess/OnException/OnExit`方法中，通过`MethodContext.ReturnValue`获取返回值类型时，对于`Task/ValueTask`返回值的方法在使用和不使用`async/await`语法时存在不同的表现，比如`Task<int> M()`在使用async语法时，`MethodContext.ReturnValue`的类型为`int`，而在没有使用async语法时类型为`Task<int>`。

这个并不是bug，而是最初的设定，正如我们在使用async语法时代码return的类型为`int`，而在没有使用async语法时代码return的类型为`Task<int>`那样。再者，使用和不使用`async`语法在编译后的表现也不一样，使用`async`语法会在编译时生成状态机类型，这里具体就不展开了。所以这种设定不仅仅是基于写代码时的习惯，也是基于实际编译后的代码结构。

虽然说设定如此，但很多时候我们对一些简易的方法重载不会使用`async/await`语法，但是我们又会希望Rougamo能够像有`async/await`语法的方法一样处理。那么此时就推荐使用`ExMoAttribute`，无论是否使用`async`语法，都能够像使用了`async`语法一样进行处理。

**需要注意`ExMoAttribute`和`MoAttribute`有以下区别：**
- `ExMoAttribute`可重写的方法名为`ExOnEntry/ExOnException/ExOnSuccess/ExOnExit`
- `ExMoAttribute`的返回值通过`MethodContext.ExReturnValue`获取，通过`MethodContext.ReturnValue`获取到的会是`Task/ValueTask`值
- `ExMoAttribute`返回值是否被替换/设置，通过`MethodContext.ExReturnValueReplaced`获取，通过`MethodContext.ReturnValueReplaced`获取到的一般都为true（因为替换为ContinueWith返回的Task了）
- `ExMoAttribute`的返回值类型通过`MethodContext.ExReturnType`获取，`ReturnType/RealReturnType/ExReturnType`三者之间有什么区别可以看各自的属性文档描述或`ExMoAttribute`的类型文档描述

```csharp
// 测试代码
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

## 织入互斥
### 单类型互斥(IRougamo<,>)
由于我们有Attribute标记和接口实现两种织入方式，那么就可能出现同时应用的情况，而如果两种织入的内容是相同的，那就会出现重复织入的情况，为了尽量避免这种情况，在接口定义时，可以定义互斥类型，也就是同时只有一个能生效，具体哪个生效，根据[优先级](#优先级)来定。
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

### 多类型互斥(IRepulsionsRougamo<,>)
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
通过上面的例子，你可能注意到，这个多类型互斥并不是多类型之间互相互斥，而是第一个泛型与第二个泛型定义的类型互斥，第二个泛型之间并不互斥，也就像上面的示例那样，当`Mo1Attribute`不生效时，与它互斥的`Mo2Attribute`、`Mo3Attribute`都将生效。这里需要理解，定义互斥的原因是Attribute和空接口实现两种方式可能存在的重复应用，而并不是为了排除所有织入的重复。同时也不推荐使用多互斥定义，这样容易出现逻辑混乱，建议在应用织入前仔细思考一套统一的规则，而不是随意定义，然后试图使用多互斥来解决问题。

# 其他

## 优先级
1. `IgnoreMoAttribute`
2. Method `MoAttribute`
3. Method `MoProxyAttribute`
4. Type `MoAttribute`
5. Type `MoProxyAttribute`
6. Type `IRougamo<>`, `IRougamo<,>`, `IRepulsionsRougamo<,>`
7. Assembly & Module `MoAttribute`

## 配置项
在引用Rougamo之后编译时会在项目根目录生成一个`FodyWeavers.xml`文件，格式如下：
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Rougamo />
</Weavers>
```
添加配置的方式就是直接在`Rougamo`节点添加属性和值，比如`<Rougamo enabled="false" />`。

下表为全部配置项：

|                 名称(~~曾用名~~)                  | 默认值 |            说明            |
|:------------------------------------------------:|:-----:|:---------------------------|
|  enabled                                         | true  | 是否开启rougamo             |
|  composite-accessibility                         | false | 是否使用类+方法综合可访问性进行匹配，默认仅按方法可访问性进行匹配。比如类的可访问性为internal，方法的可访问性为public，那么默认情况下该方法的可访问性认定为public，将该配置设置为true后，该方法的可访问性认定为internal |
|  moarray-threshold                               |   4   | 当方法上实际生效的`MoAttribute`达到该值时将使用数组保存。该配置用于优化织入代码，大多数情况下一个方法上仅一个`MoAttribute`，这时候使用数组保存在调用其方法时会产生更多的IL代码 |
|  iterator-returns(~~enumerable-returns~~)        | false | 是否保存iterator的返回值到`MethodContext.ReturnValue`，谨慎使用该功能，如果迭代器产生大量数据，启用该功能将占用等量内存 |
|  reverse-call-nonentry(~~reverse-call-ending~~)  | true  | 当一个方法上有多个`MoAttribute`时，是否在执行`OnException/OnSuccess/OnExit`时按照`OnEntry`的倒序执行，默认倒序执行 |
|  except-type-patterns                            |       | 类型全名称的正则表达式，符合该表达式的类型将被全局排除，多个正则表达式之间用英文逗号或者分号分隔 |

## FAQ
**Q: 首次接触Rougamo，但发现不生效**  
A: 首次接触时需要注意，项目必须直接以来`Rougamo.Fody`才能生效，间接依赖是无效的。比如项目A依赖B，B引用了`Rougamo.Fody`，此时项目A中的织入是无法生效的，需要项目A直接引用`Rougamo.Fody`。

**Q: 使用Rougamo开发组件后，其他项目引用我的组件还必须引用`Rougamo.Fody`吗**  
A: 组件在引用`Rougamo.Fody`后可以手动修改项目文件里的引用节点，添加` PrivateAssets="contentfiles;analyzers"`，修改后如下（复制记得修改版本号），之后其他人使用你打包的组件时就可以不用直接依赖`Rougamo.Fody`，但需要直接依赖你打包的组件。
```xml
<PackageReference Include="Rougamo.Fody" Version="2.0.0" PrivateAssets="contentfiles;analyzers" />
```