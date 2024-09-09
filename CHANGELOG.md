- 修复 [[#79](https://github.com/inversionhourglass/Rougamo/issues/79)]。同步方法变量包含类型为当前方法的声明类型时会在编译时抛出ArgumentNullException
- 修复 [[#80](https://github.com/inversionhourglass/Rougamo/issues/80)]。默认排除P/Invoke方法，这些方法本身也无法通过.NET进行AOP操作
- AspectJ-Like Pattern, method表达式匹配默认不再匹配构造方法，仅匹配普通方法（非属性方法和构造方法）

---
