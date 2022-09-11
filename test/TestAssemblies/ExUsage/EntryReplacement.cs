using ExUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace ExUsage
{
    public static class EntryReplacement
    {
        [EntryReplaceString]
        public static string Sync(int num1, int num2)
        {
            unchecked
            {
                return ((num1 << 16) | (num2 >> 16)).ToString();
            }
        }

        [EntryReplaceString]
        public static async Task<string> Async()
        {
            await Task.Yield();
            return nameof(EntryReplacement);
        }

        [EntryReplaceString]
        public static Task<string> NonAsync()
        {
            return Task.Run(string () => throw new InvalidOperationException(nameof(NonAsync)));
        }

        [EntryReplaceString]
        public static Task<int> UnmatchTypeNonAsync()
        {
            return Task.Run(() => nameof(UnmatchTypeNonAsync).GetHashCode());
        }
    }
}
