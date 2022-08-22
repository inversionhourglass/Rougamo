using Issues.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue16
    {
        public const string Flag1 = nameof(Issue16);
        public const string Flag2 = nameof(TestAsync);

        [_16_MultiApply(Flag1, _16_MultiApplyAttribute.Em.A, "1", "2")]
        [_16_MultiApply(Flag2, _16_MultiApplyAttribute.Em.B, "2", "3")]
        [_16_MultiApply(Flag2, _16_MultiApplyAttribute.Em.B, "3", "4")]
        [_16_MultiApply(Flag1, _16_MultiApplyAttribute.Em.B, "1", "2")]
        [_16_MultiApply(Flag1, _16_MultiApplyAttribute.Em.A, "1", "2")]
        public async Task TestAsync(List<string> items)
        {
            await Task.Yield();
        }
    }
}
