using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class OrderUseCase
    {
        public List<string> Executions { get; } = new List<string>();

        [Order]
        [Order1]
        [OrderM1]
        public string[] Buildin()
        {
            return new[]
            {
                nameof(OrderM1Attribute),
                nameof(OrderAttribute),
                nameof(Order1Attribute),
            };
        }

        [Order(Order = Math.PI)]
        [Order1]
        [OrderM1]
        public async Task<string[]> TempPropAsync()
        {
            await Task.Yield();
            return new[]
            {
                nameof(OrderM1Attribute),
                nameof(Order1Attribute),
                nameof(OrderAttribute),
            };
        }
    }
}
