using BasicUsage.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class Issue7
    {
        [Log]
        private async Task Sleep()
        {
            await Task.Delay(500);
        }

        [Log]
        private async Task<string> GenericSleep()
        {
            await Task.Delay(500);

            return "abc";
        }

        [Log]
        private async void VoidAsync()
        {
            await Task.Delay(500);
        }

        [Log]
        private async ValueTask ValueSleep()
        {
            await Task.Delay(500);
        }

        [Log]
        private async ValueTask<int> GenericValueSleep()
        {
            await Task.Delay(500);

            return 13;
        }

        [Log]
        private IEnumerable<string> Iterator()
        {
            yield break;
        }

        [Log]
        private async IAsyncEnumerable<string> IteratorAsync()
        {
            await Task.Delay(500);
            yield break;
        }
    }
}
