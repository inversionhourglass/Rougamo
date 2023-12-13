- [#46](https://github.com/inversionhourglass/Rougamo/issues/46) Unity接入及优化
- [#48](https://github.com/inversionhourglass/Rougamo/issues/48) Pattern支持Attribute匹配
	- 基本格式`attr([pos] TYPE)`
		- `pos` 限定Attribute应用的位置
			- `type` 应用于类型上
			- `exec` 应用于方法/属性/属性getter/属性setter
			- `para x` 应用于某个参数上，`x`一般为整数，表示参数的位置，`0`表示第一个参数，`x`为`*`时表示任意参数
			- `ret` 应用于返回值上
		- `TYPE` 为Attribute类型名称，匹配规则与普通类型匹配规则一致
- [#53](https://github.com/inversionhourglass/Rougamo/issues/53) 在`OnException`, `OnSuccess`和`OnExit`执行前，支持更新方法参数值到`MethodContext.Arguments`中

---

