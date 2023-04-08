using BasicUsage.Attributes;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    [MethodOnly]
    [PropertyOnly]
    [MethodGetter]
    [MethodSetter]
    [MethodProperty]
    public class AccessableUseCase : IRecording
    {
        public static readonly string[] Expected_Property_Get = new[] { nameof(PropertyOnlyAttribute), nameof(MethodGetterAttribute), nameof(MethodPropertyAttribute) };
        public static readonly string[] Expected_Property_Set = new[] { nameof(PropertyOnlyAttribute), nameof(MethodSetterAttribute), nameof(MethodPropertyAttribute) };
        public static readonly string[] Expected_Method = new[] { nameof(MethodOnlyAttribute), nameof(MethodGetterAttribute), nameof(MethodSetterAttribute), nameof(MethodPropertyAttribute) };

        [IgnoreMo]
        public List<string> Recording { get; } = new List<string>();

        [IgnoreMo]
        public static List<string> StaticRecording { get; } = new List<string>();

        public int IntItem { get; set; }

        public static string StringItem { get; set; }

        public int Transfer(int value)
        {
            return value;
        }

        public static async Task<int> TransferAsync(int value)
        {
            await Task.Yield();
            return value;
        }
    }
}
