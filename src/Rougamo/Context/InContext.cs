using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行前上线文
    /// </summary>
    public sealed class InContext : MethodContext
    {
        private readonly OutContext _out;
        private bool _interrupt;
        private object _replacedReturn;
        private Exception _throw;

        /// <summary>
        /// </summary>
        public InContext(OutContext @out) : base(@out.Target, @out.Method, @out.Args)
        {
            _out = @out;
        }

        /// <summary>
        /// 终止方法执行，给<see cref="ReplacedReturn"/>,<see cref="Throw"/>赋值时会自动设置为true
        /// </summary>
        public bool Interrupt
        {
            get => _interrupt;
            set
            {
                _interrupt = value;
                _out.ReturnReplaced = value;
                if (!value)
                {
                    _replacedReturn = UNSET;
                    _out.ActuallyReturn = UNSET;
                    _throw = null;
                    _out.Throw = null;
                }
            }
        }

        /// <summary>
        /// 手动设置的返回值
        /// </summary>
        public object ReplacedReturn
        {
            get => _replacedReturn;
            set
            {
                _replacedReturn = value;
                Interrupt = true;
                _out.ActuallyReturn = value;
            }
        }

        /// <summary>
        /// 手动引发的异常
        /// </summary>
        public Exception Throw
        {
            get => _throw;
            set
            {
                _throw = value;
                Interrupt = true;
                _out.Throw = value;
            }
        }
    }
}
