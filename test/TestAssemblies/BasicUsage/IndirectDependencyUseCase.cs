using IndirectDependency1;
using IndirectDependency1.Attributes;
using IndirectDependency1.Mos;
using IndirectDependency2;
using IndirectDependency2.Attributes;
using IndirectDependency2.Mos;
using Rougamo;
using System.Collections.Generic;
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
            return [
                typeof(Id1MoAttribute).FullName,
                typeof(Id1MoAttribute<Id2Cls>).FullName,
                typeof(Id1MoAttribute<Id2Cls<Id1Struct>>).FullName,
                typeof(Id1MoAttribute<Id2Struct>).FullName,
                typeof(Id1MoAttribute<Id2Struct<Id1Cls>>).FullName,
                typeof(Id2MoAttribute).FullName,
                typeof(Id2MoAttribute<Id1Cls>).FullName,
                typeof(Id2MoAttribute<Id1Cls<Id2Struct>>).FullName,
                typeof(Id2MoAttribute<Id1Struct>).FullName,
                typeof(Id2MoAttribute<Id1Struct<Id2Cls>>).FullName,
                typeof(Id1ValueMo).FullName,
                typeof(Id2ValueMo<Id1Cls>).FullName,
                typeof(Id1ValueMo<Id2Cls<int>>).FullName,
                typeof(Id2ValueMo<Id1Struct<Id2Cls>>).FullName
                ];
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
            return [
                typeof(Id1MoAttribute).FullName,
                typeof(Id1MoAttribute<Id2Cls>).FullName,
                typeof(Id1MoAttribute<Id2Cls<Id1Struct>>).FullName,
                typeof(Id1MoAttribute<Id2Struct>).FullName,
                typeof(Id1MoAttribute<Id2Struct<Id1Cls>>).FullName,
                typeof(Id2MoAttribute).FullName,
                typeof(Id2MoAttribute<Id1Cls>).FullName,
                typeof(Id2MoAttribute<Id1Cls<Id2Struct>>).FullName,
                typeof(Id2MoAttribute<Id1Struct>).FullName,
                typeof(Id2MoAttribute<Id1Struct<Id2Cls>>).FullName,
                typeof(Id1ValueMo).FullName,
                typeof(Id2ValueMo<Id1Cls>).FullName,
                typeof(Id1ValueMo<Id2Cls<int>>).FullName,
                typeof(Id2ValueMo<Id1Struct<Id2Cls>>).FullName
            ];
        }
    }
}
