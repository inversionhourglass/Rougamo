using Issues.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue7
    {
        [_7_8_Log]
        private async Task Sleep()
        {
            await Task.Delay(500);
        }

        [_7_8_Log]
        private async Task<string> GenericSleep()
        {
            await Task.Delay(500);

            return "abc";
        }

        [_7_8_Log]
        private async void VoidAsync()
        {
            await Task.Delay(500);
        }

        [_7_8_Log]
        private async ValueTask ValueSleep()
        {
            await Task.Delay(500);
        }

        [_7_8_Log]
        private async ValueTask<int> GenericValueSleep()
        {
            await Task.Delay(500);

            return 13;
        }

        [_7_8_Log]
        private IEnumerable<string> Iterator()
        {
            yield break;
        }

        [_7_8_Log]
        private async IAsyncEnumerable<string> IteratorAsync()
        {
            await Task.Delay(500);
            yield break;
        }
    }
}
