using IndirectDependency1;
using IndirectDependency1.Attributes;
using IndirectDependency1.Mos;
using IndirectDependency2;
using IndirectDependency2.Attributes;
using IndirectDependency2.Mos;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class IndirectDependencyUseCase
    {
        [Id1Mo]
        [Id1Mo<Id2Cls>]
        [Id1Mo<Id2Cls<Id1Struct>>]
        [Id1Mo<Id2Struct>]
        [Id1Mo<Id2Struct<Id1Cls>>]
        [Id2Mo]
        [Id2Mo<Id1Cls>]
        [Id2Mo<Id1Cls<Id2Struct>>]
        [Id2Mo<Id1Struct>]
        [Id2Mo<Id1Struct<Id2Cls>>]
        [Rougamo<Id1ValueMo>]
        [Rougamo<Id2ValueMo<Id1Cls>>]
        [Rougamo(typeof(Id1ValueMo<Id2Cls<int>>))]
        [Rougamo(typeof(Id2ValueMo<Id1Struct<Id2Cls>>))]
        public string[] Sync(List<string> mos)
        {
            return GetMos(MethodBase.GetCurrentMethod()).ToArray();
        }

        [Id1Mo]
        [Id1Mo<Id2Cls>]
        [Id1Mo<Id2Cls<Id1Struct>>]
        [Id1Mo<Id2Struct>]
        [Id1Mo<Id2Struct<Id1Cls>>]
        [Id2Mo]
        [Id2Mo<Id1Cls>]
        [Id2Mo<Id1Cls<Id2Struct>>]
        [Id2Mo<Id1Struct>]
        [Id2Mo<Id1Struct<Id2Cls>>]
        [Rougamo<Id1ValueMo>]
        [Rougamo<Id2ValueMo<Id1Cls>>]
        [Rougamo(typeof(Id1ValueMo<Id2Cls<int>>))]
        [Rougamo(typeof(Id2ValueMo<Id1Struct<Id2Cls>>))]
        public static async Task<string[]> Async(List<string> mos)
        {
            await Task.Yield();
            return GetMos(MethodBase.GetCurrentMethod()).ToArray();
        }

        private static IEnumerable<string> GetMos(MethodBase method)
        {
            foreach(var attribute in method.CustomAttributes)
            {
                var attributeType = attribute.AttributeType;
                if (typeof(IMo).IsAssignableFrom(attributeType))
                {
                    yield return attributeType.FullName;
                }
                else if (attributeType == typeof(RougamoAttribute))
                {
                    yield return ((Type)attribute.ConstructorArguments[0].Value).FullName;
                }
                else if (attributeType.IsGenericType && attributeType.GetGenericTypeDefinition() == typeof(RougamoAttribute<>))
                {
                    yield return attributeType.GetGenericArguments()[0].FullName;
                }
            }
        }
    }
}
