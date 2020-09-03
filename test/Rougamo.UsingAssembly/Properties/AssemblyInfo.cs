using Rougamo;
using Rougamo.ImplAssembly;
using System;

[assembly: PublicMo(123, new[] { 1.2, 2.3 }, StringValue = "aha", IntArray = new[] { 9, 0, 9 })]
[module: StaticMo]
[module: MoProxy(typeof(ObsoleteAttribute), typeof(ObsoleteProxyMoAttribute))]