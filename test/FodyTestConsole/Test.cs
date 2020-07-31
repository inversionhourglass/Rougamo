using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FodyTestConsole
{
    public class Test
    {
        public int Sync()
        {
            Console.WriteLine("1");
            var value = 233;
            Console.WriteLine("2");
            return value;
        }

        public Task<int> AsyncNotWait()
        {
            Console.WriteLine("1");
            var task = Task.FromResult(233);
            Console.WriteLine("2");
            return task;
        }

        public async Task<int> Async()
        {
            Console.WriteLine("1");
            var value = await Task.FromResult(233);
            Console.WriteLine("2");
            return value;
        }
    }
}
