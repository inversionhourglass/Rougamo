- [[#71](https://github.com/inversionhourglass/Rougamo/issues/71)] 修复blazor项目在发布时illink对程序集做裁减优化时产生的异常
  > 程序集主要描述信息基本存储在ModuleDefinition的MetadataSystem字段中，各类型的原数据也基本从该对象中读取。然而通过删除/清空MethodDefinition的CustomDebugInformation并不会影响MetadataSystem中的数据，同时，在将修改写入程序集时，MetadataSystem中绝大部分数据都将直接清空后重新写入，然而CustomDebugInformation是个例外，这就导致MethodDefinition上对CustomDebugInformations的删除/清空操作无效。所以对CustomDebugInformation的删除操作还需要从MetadataSystem中的删除。参考 [MetadataSystem.Clear()](https://github.com/jbevain/cecil/blob/8e1ae7b4ea67ccc38cb8db3ded6802643109ffd7/Mono.Cecil/MetadataSystem.cs#L137)

---
