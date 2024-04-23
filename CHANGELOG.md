- [#66](https://github.com/inversionhourglass/Rougamo/issues/66) lambda编译时生成的方法，如果方法参数包含泛型嵌套类，在使用Pattern时将在编译时报错
  - Rougamo排除编译器生成的方法，但包含编译器生成的属性getter/setter，如果有其他常用的编译器生成方法，后续再处理
  - 修复泛型嵌套类签名转换失败的问题

---
