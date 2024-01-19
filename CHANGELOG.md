- [#46](https://github.com/inversionhourglass/Rougamo/issues/46) 新的结构体，优化GC
    - 新增`RougamoAttribute`支持自定义结构体实现`IMo`接口，通过结构体优化GC
    - 优化`MethodContext`内部字段，通过延迟初始化、全局空缓存的方式优化GC
    - 优化织入代码，使用原始类型来存储`IMo`对象，而不使用`IMo`接口类型存储，在实现类型为结构体时可以避免装箱操作。当然，如果织入的`IMo`对象过多，导致使用数组进行存储，那也是没有办法的
- [#55](https://github.com/inversionhourglass/Rougamo/issues/55) 屏蔽`MoAttribute`定义的属性
    - `MoAttribute`的所有属性改为仅getter为public，如果希望公开`MoAttribute`定义的属性，可以通过`new`的方式覆盖并修改可访问性，例：`public new virtual double Order { get; set; }`

---
