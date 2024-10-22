- [x] 重构`MethodContext`以支持Pooling
- [ ] 将`IMo`编译时用到的属性提取到Attribute中，减少`IMo`的成员，
- [x] AspectN新增`ctor`和`cctor`
- [x] [[#82](https://github.com/inversionhourglass/Rougamo/issues/82)]。可通过配置排除代理方法/类的调用堆栈信息，用于消除`Exception.StackTrace/Environment.StackTrace`中的冗余信息

        .NET6及以上版本通过`StackTraceHiddenAttribute`直接忽略Rougamo代理方法的调用堆栈信息，.NET5及以下版本通过`StackTraceHiddenAttribute`忽略Rougamo代理方法的调用堆栈信息，可以通过将配置项`pure-stacktrace`设置为false来改变这一默认行为。对于.NET6之前的版本及.NET Framework版本，可以通过`Exception`的扩展方法`GetNonRougamoStackTrace`获取没有Rougamo代理方法的堆栈信息，或者是`ToNonRougamoString`扩展方法直接获取没有Rougamo代理方法堆栈信息的异常信息。

- [ ] NuGet pack时自动写入`PrivateAssets`
- [ ] 支持配置化非侵入式织入

---
