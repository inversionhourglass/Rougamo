using ExUsage.Attributes;
using System.Threading.Tasks;

namespace ExUsage
{
    public static class SuccessReplacement
    {
        [SucceedReplaceInt]
        public static int Sync()
        {
            return 1;
        }

        [SucceedReplaceInt]
        public static async Task<int> Async()
        {
            await Task.Yield();
            return 1;
        }

        [SucceedReplaceInt]
        public static Task<int> NonAsync()
        {
            return Task.FromResult(1);
        }

        [SucceedReplaceInt]
        public static Task<string> UnmatchTypeNonAsync()
        {
            return Task.FromResult("1");
        }
    }
}
