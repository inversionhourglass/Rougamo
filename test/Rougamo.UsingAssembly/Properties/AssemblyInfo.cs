using Rougamo;
using Rougamo.ImplAssembly;
using Rougamo.UsingAssembly;

[assembly: TheMo]
[module: TheMo]

[assembly: MoProxy(typeof(AbcAttribute), typeof(TheMoAttribute))]