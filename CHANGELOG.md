- 修复 [[#77](https://github.com/inversionhourglass/Rougamo/issues/77)]

### 修复描述

异步方法的调试信息不仅需要对应的DebugInformation和CustomDebugInformation以及一些Attribute，生成的StateMachine类名以及变量对应的字段名也有一定格式要求，具体参考：
- 类名生成：https://github.com/dotnet/roslyn/blob/5cba0ce666766b1db7cd75009575c7e12c4be72c/src/Compilers/CSharp/Portable/Symbols/Synthesized/GeneratedNames.cs#L78-L84
- 字段名生成：https://github.com/dotnet/roslyn/blob/5cba0ce666766b1db7cd75009575c7e12c4be72c/src/Compilers/CSharp/Portable/Symbols/Synthesized/GeneratedNames.cs#L203-L244
- 字段solt计算：https://github.com/dotnet/roslyn/blob/5cba0ce666766b1db7cd75009575c7e12c4be72c/src/Compilers/CSharp/Portable/Lowering/StateMachineRewriter/StateMachineRewriter.cs#L194-L197

---
