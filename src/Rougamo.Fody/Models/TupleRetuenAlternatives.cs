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

        public SimplifyGlobalMos(CustomAttribute[] directs, TypeDefinition[] generics, Dictionary<string, TypeDefinition> proxies, string[] ignores)
        {
            Directs = directs;
            Generics = generics;
            Proxies = proxies;
            Ignores = ignores;
        }

        public CustomAttribute[]? Directs { get; }

        public TypeDefinition[] Generics { get; }

        public Dictionary<string, TypeDefinition>? Proxies { get; }

        public string[]? Ignores { get; }

        public bool GlobalIgnore => Ignores == null;
    }

    internal sealed class GlobalMos
    {
        public GlobalMos(Dictionary<string, List<CustomAttribute>> directs, Dictionary<string, TypeDefinition> generics, Dictionary<string, TypeDefinition> proxies, Dictionary<string, TypeDefinition>? ignores)
        {
            Directs = directs;
            Generics = generics;
            Proxies = proxies;
            Ignores = ignores;
        }

        public Dictionary<string, List<CustomAttribute>> Directs { get; }

        public Dictionary<string, TypeDefinition> Generics { get; set; }

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
        public ExtractMos(CustomAttribute[] mos, TypeDefinition[] genericMos,  TypeDefinition[] proxied)
        {
            Mos = mos;
            GenericMos = genericMos;
            Proxied = proxied;
        }

        public CustomAttribute[] Mos { get; }

        public TypeDefinition[] GenericMos { get; set; }

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
