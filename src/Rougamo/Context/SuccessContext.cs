using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行成功（无异常）上下文
    /// </summary>
    public sealed class SuccessContext : MethodContext
    {
        private readonly OutContext _out;
        private bool _replaced;
        private object _replacedReturn;
        private Exception _throw;

        /// <summary>
        /// </summary>
        public SuccessContext(OutContext @out) : base(@out.Target, @out.Method, @out.Args)
        {
            _out = @out;
        }

        /// <summary>
        /// 是否已替换原方法返回值，给<see cref="ReplacedReturn"/>,<see cref="Throw"/>赋值时会自动设置为true
        /// </summary>
        public bool Replaced
        {
            get => _replaced;
            set
            {
                _replaced = value;
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
        /// 方法原始返回值，如需修改返回值，请通过<see cref="ReplacedReturn"/>进行修改
        /// </summary>
        public object OriginReturn { get; set; }

        /// <summary>
        /// 手动替换的返回值
        /// </summary>
        public object ReplacedReturn
        {
            get => _replacedReturn;
            set
            {
                _replacedReturn = value;
                Replaced = true;
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
                Replaced = true;
                _out.Throw = value;
            }
        }
    }
}
