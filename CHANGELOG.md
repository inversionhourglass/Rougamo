- [x] 重构`MethodContext`以支持对象池
- [ ] 内建对象池，`MethodContext`默认池化，新增XxxAttribute可应用于`IMo`子类，用于指定Mo的生命周期，可指定为单例、瞬态、对象池
- [ ] 将`IMo`编译时用到的属性提取到Attribute中，减少`IMo`的成员，

    - 将`IMo`的`Flags`属性和`Pattern`属性提取到`PointcutAttribute`中，同时新增`IFlexibleModifierPointcut`和`IFlexiblePatternPointcut`用于支持Attribute应用时指定切入点。当实现Flexible接口后，可以使用之前直接初始化属性赋值，优先级：应用Attribute时指定 > PointcutAttribute指定 > 属性初始化
    - 将`IMo`的`Features`属性提取到`AdviceAttribute`中
    - 将`IMo`的`MethodContextOmits`属性和`ForceSync`属性提取到`OptimizationAttribute`中

- [x] AspectN新增`ctor`和`cctor`
- [x] [[#82](https://github.com/inversionhourglass/Rougamo/issues/82)]。可通过配置排除代理方法/类的调用堆栈信息，用于消除`Exception.StackTrace/Environment.StackTrace`中的冗余信息

        .NET6及以上版本通过`StackTraceHiddenAttribute`直接忽略Rougamo代理方法的调用堆栈信息，.NET5及以下版本通过`StackTraceHiddenAttribute`忽略Rougamo代理方法的调用堆栈信息，可以通过将配置项`pure-stacktrace`设置为false来改变这一默认行为。对于.NET6之前的版本及.NET Framework版本，可以通过`Exception`的扩展方法`GetNonRougamoStackTrace`获取没有Rougamo代理方法的堆栈信息，或者是`ToNonRougamoString`扩展方法直接获取没有Rougamo代理方法堆栈信息的异常信息。

- [ ] NuGet pack时自动写入`PrivateAssets`
- [ ] 支持配置化非侵入式织入

---
