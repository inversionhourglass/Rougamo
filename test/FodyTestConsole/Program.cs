using FodyTestLib;
using System;

namespace FodyTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ccls = new ConsoleCls();
            var lcls = new LibCls();
            ccls.Id = 123;
            lcls.Name = "abc";

            Console.Write("Press any key to continue...");
            Console.ReadLine();
        }
    }
}
