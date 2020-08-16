using Rougamo;
using Rougamo.ImplAssembly;
using System;

[assembly: PublicMo]
[module: StaticMo]
[module: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]