- [#36](https://github.com/inversionhourglass/Rougamo/issues/36)
- [#37](https://github.com/inversionhourglass/Rougamo/issues/37)
- [#54](https://github.com/inversionhourglass/Rougamo/issues/54)

# 3.0版本对比之前版本差异
> 主要功能差异的原因都是采用代理调用后导致的，这种方式带来了一些优势，也具有一些劣势

1. 刷新参数值功能仅`out / ref`参数有效
2. `void async`方法无法确保方法切实执行完毕后再执行`OnSuccess`，也无法确保异常一定会进`OnException`
3. `ExMoAttribute`弃用，代理调用的方式不再区分是否使用`async / await`语法

---
