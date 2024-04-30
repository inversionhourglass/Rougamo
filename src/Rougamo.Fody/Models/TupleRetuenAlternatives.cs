using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal sealed class SimplifyGlobalMos
    {
        /// <summary>
        /// use this constructor if exists global IgnoreMoAttribute
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SimplifyGlobalMos() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public SimplifyGlobalMos(CustomAttribute[] directs, TypeReference[] generics, Dictionary<string, TypeReference> proxies, string[] ignores)
        {
            Directs = directs;
            Generics = generics;
            Proxies = proxies;
            Ignores = ignores;
        }

        public CustomAttribute[]? Directs { get; }

        public TypeReference[] Generics { get; }

        public Dictionary<string, TypeReference>? Proxies { get; }

        public string[]? Ignores { get; }

        public bool GlobalIgnore => Ignores == null;
    }

    internal sealed class GlobalMos
    {
        public GlobalMos(Dictionary<string, List<CustomAttribute>> directs, Dictionary<string, TypeReference> generics, Dictionary<string, TypeReference> proxies, Dictionary<string, TypeReference>? ignores)
        {
            Directs = directs;
            Generics = generics;
            Proxies = proxies;
            Ignores = ignores;
        }

        public Dictionary<string, List<CustomAttribute>> Directs { get; }

        public Dictionary<string, TypeReference> Generics { get; set; }

        public Dictionary<string, TypeReference> Proxies { get; }

        public Dictionary<string, TypeReference>? Ignores { get; }

        public bool GlobalIgnore => Ignores == null;
    }

    internal sealed class RepulsionMo
    {
        public RepulsionMo(TypeReference mo, TypeReference[] repulsions)
        {
            Mo = mo;
            Repulsions = repulsions;
        }

        public TypeReference Mo { get; }
        
        public TypeReference[] Repulsions { get; }
    }

    internal sealed class ExtractMos
    {
        public ExtractMos(CustomAttribute[] mos, TypeReference[] genericMos,  TypeReference[] proxied)
        {
            Mos = mos;
            GenericMos = genericMos;
            Proxied = proxied;
        }

        public CustomAttribute[] Mos { get; }

        public TypeReference[] GenericMos { get; set; }

        public TypeReference[] Proxied { get; }
    }

    internal sealed class ProxyReleation
    {
        public ProxyReleation(TypeReference origin, TypeReference proxy)
        {
            Origin = origin;
            Proxy = proxy;
        }

        public TypeReference Origin { get; }
        
        public TypeReference Proxy { get; }
    }
}
