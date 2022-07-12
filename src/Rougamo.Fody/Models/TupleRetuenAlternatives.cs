using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal sealed class SimplifyGlobalMos
    {
        /// <summary>
        /// use this constructor if exists global IgnoreMoAttribute
        /// </summary>
        public SimplifyGlobalMos() { }

        public SimplifyGlobalMos(CustomAttribute[] directs, Dictionary<string, TypeDefinition> proxies, string[] ignores)
        {
            Directs = directs;
            Proxies = proxies;
            Ignores = ignores;
        }

        public CustomAttribute[]? Directs { get; }

        public Dictionary<string, TypeDefinition>? Proxies { get; }

        public string[]? Ignores { get; }

        public bool GlobalIgnore => Ignores == null;
    }

    internal sealed class GlobalMos
    {
        public GlobalMos(Dictionary<string, CustomAttribute> directs, Dictionary<string, TypeDefinition> proxies, Dictionary<string, TypeDefinition>? ignores)
        {
            Directs = directs;
            Proxies = proxies;
            Ignores = ignores;
        }

        public Dictionary<string, CustomAttribute> Directs { get; }

        public Dictionary<string, TypeDefinition> Proxies { get; }

        public Dictionary<string, TypeDefinition>? Ignores { get; }

        public bool GlobalIgnore => Ignores == null;
    }

    internal sealed class RepulsionMo
    {
        public RepulsionMo(TypeDefinition mo, TypeDefinition[] repulsions)
        {
            Mo = mo;
            Repulsions = repulsions;
        }

        public TypeDefinition Mo { get; }
        
        public TypeDefinition[] Repulsions { get; }
    }

    internal sealed class ExtractMos
    {
        public ExtractMos(CustomAttribute[] mos, TypeDefinition[] proxied)
        {
            Mos = mos;
            Proxied = proxied;
        }

        public CustomAttribute[] Mos { get; }
        
        public TypeDefinition[] Proxied { get; }
    }

    internal sealed class ProxyReleation
    {
        public ProxyReleation(TypeDefinition origin, TypeDefinition proxy)
        {
            Origin = origin;
            Proxy = proxy;
        }

        public TypeDefinition Origin { get; }
        
        public TypeDefinition Proxy { get; }
    }
}
