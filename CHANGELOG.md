- [#61](https://github.com/inversionhourglass/Rougamo/issues/51) `Omit`新增枚举项`ReturnValue`
  - `Omit.ReturnValue`可以让`MethodContext`忽略记录方法返回值，但同时也会导致无法设置返回值，包括异常处理设置返回值
  - 将`Omit`的优先级调整为高于`Feature`，这也就意味着，如果设置了`Omit.Arguments`，那么即使设置了`Feature.RewriteArgs`也无法重写方法参数
  - 由于`ref struct`类型限制，无法将其装箱为`object`，也无法从`object`拆箱得到，所以如果方法使用该类型作为参数或者返回值，那么必须使用`Omit`表明不需要使用到方法参数或返回值，否则将在编译时报错

---
