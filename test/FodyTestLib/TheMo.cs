using Rougamo;
using Rougamo.Context;
using System;

namespace FodyTestLib
{
    public class TheMo : IMo
    {
        public void Before(InContext context)
        {
            Console.WriteLine(nameof(Before));
        }

        public void Exceptioned(ExceptionContext context)
        {
            Console.WriteLine(nameof(Exceptioned));
        }

        public void Exit(OutContext context)
        {
            Console.WriteLine(nameof(Exit));
        }

        public void Successed(SuccessContext context)
        {
            Console.WriteLine(nameof(Successed));
        }
    }

    public interface IBaseMo : IRougamo<TheMo> { }
}
