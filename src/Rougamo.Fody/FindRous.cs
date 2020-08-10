using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void FindRous()
        {
            _rouTypes = new List<RouType>();
            FullScan();
        }

        private void FullScan()
        {
            (var directs, var proxies) = FindGlobalAttributes();

            foreach (var typeDef in ModuleDefinition.Types)
            {
                if (!typeDef.IsClass || typeDef.IsValueType || typeDef.IsDelegate() || !typeDef.HasMethods) continue;

                var rouType = new RouType(typeDef);
                var ignore = typeDef.CustomAttributes.Any(ca => ca.Is(Constants.TYPE_IgnoreMoAttribute));
                var implementations = ExtractClassImplementations(typeDef);
                var classAttributes = ExtractClassAttributes(typeDef);

                foreach (var methodDef in typeDef.Methods)
                {
                    if ((methodDef.Attributes & MethodAttributes.Abstract) != 0) continue;


                }
            }
        }

        /// <summary>
        /// 查找程序集级别继承自MoAttribute以及使用MoProxyAttribute代理的Attribute，module级别会覆盖assembly级别
        /// </summary>
        /// <returns>
        /// directs: 继承自MoAttribute的类型
        /// proxies: 通过MoProxyAttribute代理的类型
        /// </returns>
        private (Dictionary<string, CustomAttribute> directs, Dictionary<string, (TypeDefinition origin, TypeDefinition proxy)> proxies) FindGlobalAttributes()
        {
            var (directs, proxies) = FindGlobalAttributes(ModuleDefinition.Assembly.CustomAttributes, "assembly");
            var (moduleDirects, moduleProxies) = FindGlobalAttributes(ModuleDefinition.CustomAttributes, "module");

            foreach (var direct in moduleDirects)
            {
                if (directs.ContainsKey(direct.Key))
                {
                    WriteInfo($"module replaces assembly MoAttribute: {direct.Key}");
                }
                directs[direct.Key] = direct.Value;
            }

            foreach (var proxy in moduleProxies)
            {
                if (proxies.ContainsKey(proxy.Key))
                {
                    WriteInfo($"module replaces assembly MoProxyAttribute: {proxy.Key}");
                }
                proxies[proxy.Key] = proxy.Value;
            }

            return (directs, proxies.Values.ToDictionary(x => x.origin.FullName));
        }

        /// <summary>
        /// 从attributes中查找继承自MoAttribute以及使用MoProxyAttribute代理的Attribute
        /// </summary>
        /// <param name="attributes">给定查找范围</param>
        /// <param name="locationName">全局范围名称</param>
        /// <returns>
        /// directs: 继承自MoAttribute的类型
        /// proxies: 通过MoProxyAttribute代理的类型
        /// </returns>
        private (Dictionary<string, CustomAttribute> directs, Dictionary<string, (TypeDefinition origin, TypeDefinition proxy)> proxies) FindGlobalAttributes(Collection<CustomAttribute> attributes, string locationName)
        {
            var directs = new Dictionary<string, CustomAttribute>();
            var proxies = new Dictionary<string, (TypeDefinition origin, TypeDefinition proxy)>();

            foreach (var attribute in attributes)
            {
                var attrType = attribute.AttributeType;
                if (attrType.DerivesFrom(Constants.TYPE_MoAttribute))
                {
                    var key = attrType.FullName;
                    if (directs.TryAdd(key, attribute))
                    {
                        WriteInfo($"{locationName} MoAttribute found: {key}");
                    }
                    else
                    {
                        WriteError($"duplicate {locationName} MoAttribute found: {key}");
                    }
                }
                else if (attrType.Is(Constants.TYPE_MoProxyAttribute))
                {
                    var arg1 = attribute.ConstructorArguments[0].Value;
                    var arg2 = attribute.ConstructorArguments[1].Value;
                    var origin = arg1 is TypeDefinition ? (TypeDefinition)arg1 : ((TypeReference)arg1).Resolve();
                    var proxy = arg2 is TypeDefinition ? (TypeDefinition)arg2 : ((TypeReference)arg2).Resolve();
                    if (!proxy.DerivesFrom(Constants.TYPE_MoAttribute))
                    {
                        WriteError($"Mo proxy type has to inherit from Rougamo.MoAttribute");
                    }
                    else if (!proxy.GetConstructors().Any(ctor => !ctor.HasParameters))
                    {
                        WriteError($"Mo proxy type has to contains non-parameters constructor");
                    }
                    else
                    {
                        var key = $"{origin.FullName}|{proxy.FullName}";
                        if (proxies.TryAdd(key, (origin, proxy)))
                        {
                            WriteInfo($"{locationName} MoProxyAttribute found: {key}");
                        }
                        else
                        {
                            WriteError($"duplicate {locationName} MoProxyAttribute found: {key}");
                        }
                    }
                }
            }

            return (directs, proxies);
        }

        /// <summary>
        /// 从接口继承的方式中提取IMo已经对应的互斥类型
        /// </summary>
        /// <param name="typeDef">程序集中的类型</param>
        /// <returns>
        ///         mo: 实现IMo接口的类型
        /// repulsions: 与mo互斥的类型
        /// </returns>
        private (TypeDefinition mo, TypeDefinition[] repulsions)[] ExtractClassImplementations(TypeDefinition typeDef)
        {
            var mos = new List<(TypeDefinition, TypeDefinition[])>();
            var mosInterfaces = typeDef.GetGenericInterfaces(Constants.TYPE_IRougamo_1);
            var repMosInterfaces = typeDef.GetGenericInterfaces(Constants.TYPE_IRougamo_2);
            var multiRepMosInterfaces = typeDef.GetGenericInterfaces(Constants.TYPE_IRepulsionsRougamo);

            mos.AddRange(mosInterfaces.Select(x => (x.GenericArguments[0].Resolve(), new TypeDefinition[0])));
            mos.AddRange(repMosInterfaces.Select(x => (x.GenericArguments[0].Resolve(), new TypeDefinition[] { x.GenericArguments[1].Resolve() })));
            mos.AddRange(multiRepMosInterfaces.Select(x => (x.GenericArguments[0].Resolve(), ExtractRepulsionFromIl(x.GenericArguments[1].Resolve()))));

            return mos.ToArray();
        }

        /// <summary>
        /// 从IRepulsionsRougamo的泛型类型IL代码中提取互斥类型
        /// </summary>
        /// <param name="typeDef">IRepulsionsRougamo</param>
        /// <returns>互斥类型</returns>
        private TypeDefinition[] ExtractRepulsionFromIl(TypeDefinition typeDef)
        {
            return ExtractRepulsionFromProp(typeDef) ?? ExtractRepulsionFromCtor(typeDef) ?? new TypeDefinition[0];
        }

        /// <summary>
        /// 从IRepulsionsRougamo泛型类型的属性Get方法中提取互斥类型
        /// </summary>
        /// <param name="typeDef">IRepulsionsRougamo</param>
        /// <returns>互斥类型</returns>
        private TypeDefinition[] ExtractRepulsionFromProp(TypeDefinition typeDef)
        {
            do
            {
                var property = typeDef.Properties.FirstOrDefault(prop => prop.Name == Constants.PROP_Repulsions);
                if(property != null)
                {
                    Dictionary<string, TypeDefinition> repulsions = null;
                    foreach (var instruction in property.GetMethod.Body.Instructions)
                    {
                        if(instruction.OpCode == OpCodes.Newarr)
                        {
                            repulsions = new Dictionary<string, TypeDefinition>();
                        }
                        else if(repulsions != null && instruction.IsLdtoken(Constants.TYPE_IMo, out var def) && !repulsions.ContainsKey(def.FullName))
                        {
                            repulsions.Add(def.FullName, def);
                        }
                    }
                    return repulsions == null ? null : repulsions.Values.ToArray();
                }
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return null;
        }

        /// <summary>
        /// 从IRepulsionsRougamo泛型类型的构造方法中提取互斥类型
        /// </summary>
        /// <param name="typeDef">IRepulsionsRougamo</param>
        /// <returns>互斥类型</returns>
        private TypeDefinition[] ExtractRepulsionFromCtor(TypeDefinition typeDef)
        {
            do
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if (nonCtor != null)
                {
                    Dictionary<string, TypeDefinition> repulsions = null;
                    var instructions = nonCtor.Body.Instructions;
                    for (int i = instructions.Count - 1; i >= 0; i--)
                    {
                        if (instructions[i].IsStfld(Constants.FIELD_Repulsions, Constants.TYPE_ARRAY_Type))
                        {
                            repulsions = new Dictionary<string, TypeDefinition>();
                        }
                        else if(repulsions != null && instructions[i].IsLdtoken(Constants.TYPE_IMo, out var def) && !repulsions.ContainsKey(def.FullName))
                        {
                            repulsions.Add(def.FullName, def);
                        }
                        else if(instructions[i].OpCode == OpCodes.Newarr && repulsions != null)
                        {
                            return repulsions.Values.ToArray();
                        }
                    }
                }
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return null;
        }

        /// <summary>
        /// 从class级别查找继承自MoAttribute的Attribute（仅当前类，父类级别无效）
        /// </summary>
        /// <param name="typeDef">程序集中的类型</param>
        /// <returns>继承自MoAttribute的类型</returns>
        private CustomAttribute[] ExtractClassAttributes(TypeDefinition typeDef)
        {
            return typeDef.CustomAttributes.Where(ca => ca.DerivesFrom(Constants.TYPE_MoAttribute)).ToArray();
        }
    }
}
