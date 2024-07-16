- [[#75](https://github.com/inversionhourglass/Rougamo/issues/75)] 3.0版本采用代理调用方式后，应该由外层代理方法保持持有所有Attribute更为合适，而内部实际代码逻辑方法则应该移出所有相关Attribute，保证通过Attribute获取方法时不会出现重复或获取错误的情况
- 对于`virtual`和`override`这类可重写的方法，在克隆时克隆返回的方法上移除`virtual`属性

---
