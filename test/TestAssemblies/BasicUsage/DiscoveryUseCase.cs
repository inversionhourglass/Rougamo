using BasicUsage.Attributes;
using Discoverers;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    [Discovery(Order = 1)]
    [Discovery(Order = 2, DiscovererType = typeof(GetXxxDiscoverer))]
    [SuffixMatch("Async", Order = 3)]
    [PrefixMatch(Order = 4)]
    [PrefixMatch(Order = 5, Prefix = "Random")]
    [PublicDiscovery(Order = 6)]
    public class DiscoveryUseCase : IRecording
    {
        private readonly Random _random = new Random();

        [IgnoreMo]
        public List<string> Recording { get; } = new List<string>();

        public static readonly string[] Expected_Name = new[] { "6" };
        public string Name { get; set; }

        public static readonly string[] Expected_Salary = new[] { "1", "6" };
        public Number Salary { get; set; }

        public static readonly string[] Expected_GetNameSalary = new[] { "2", "6" };
        public string GetNameSalary() => "NameSalary";

        public static readonly string[] Expected_GetSalaryName = new[] { "2", "6" };
        public string GetSalaryName() => "SalaryName";

        public static readonly string[] Expected_GetRawSalaryAsync = new[] { "2", "3", "6" };
        public async Task<int> GetRawSalaryAsync()
        {
            await Task.Yield();
            return (int)Number.Seven;
        }

        public static readonly string[] Expected_RandomStringAsync = new[] { "3", "5", "6" };
        public async Task<string> RandomStringAsync()
        {
            await Task.Yield();
            return Guid.NewGuid().ToString();
        }

        public static readonly string[] Expected_RandomInt = new[] { "5", "6" };
        public int RandomInt() => _random.Next();

        public enum Number
        {
            Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine
        }
    }
}
