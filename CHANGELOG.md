- [#36](https://github.com/inversionhourglass/Rougamo/issues/36) 应用Rougamo的方法支持步入调试
- [#60](https://github.com/inversionhourglass/Rougamo/issues/60) 支持自定义`AsyncMethodBuilder`
- [#63](https://github.com/inversionhourglass/Rougamo/issues/63) 支持泛型`Attribute<T>`
- [#65](https://github.com/inversionhourglass/Rougamo/issues/65) 修复特定`Type`类型无法作为`MoAttribute`构造方法参数

# 3.0版本对比之前版本差异
3.0版本在代码织入的方式上进行了改变，从原方法内部进行代码织入的方式改为代理方法织入，这样的改变会使Rougamo之前提供的部分功能发生变化：

1. 在 [#53](https://github.com/inversionhourglass/Rougamo/issues/53) 中支持的功能“刷新参数值”现在仅对`out / ref`参数有效
2. `void async`方法无法确保方法切实执行完毕后再执行`OnSuccess`，也无法确保异常一定会进`OnException`
3. `ExMoAttribute`弃用，代理调用的方式不再区分是否使用`async / await`语法

---
