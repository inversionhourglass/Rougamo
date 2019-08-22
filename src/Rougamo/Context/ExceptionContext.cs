using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行异常上下文
    /// </summary>
    public sealed class ExceptionContext : MethodContext
    {
        private readonly OutContext _out;
        private bool _isHandled;
        private Exception _originException;
        private object _handledReturn;
        private Exception _throw;

        /// <summary>
        /// </summary>
        public ExceptionContext(OutContext @out) : base(@out.Target, @out.Method, @out.Args)
        {
            _out = @out;
        }

        /// <summary>
        /// 原方法抛出的异常是否已处理，给<see cref="HandledReturn"/>,<see cref="Throw"/>赋值时会自动设置为true
        /// </summary>
        public bool IsHandled
        {
            get => _isHandled;
            set
            {
                _isHandled = value;
                _out.ExceptionHandled = value;
                if (!value)
                {
                    _handledReturn = UNSET;
                    _out.ActuallyReturn = UNSET;
                    _throw = null;
                    _out.Throw = null;
                }
            }
        }

        /// <summary>
        /// 方法执行抛出的异常
        /// </summary>
        public Exception OriginException
        {
            get => _originException;
            set
            {
                _originException = value;
                _out.OriginException = value;
            }
        }

        /// <summary>
        /// 手动设置异常处理后的返回值
        /// </summary>
        public object HandledReturn
        {
            get => _handledReturn;
            set
            {
                _handledReturn = value;
                IsHandled = true;
                _out.ActuallyReturn = value;
            }
        }

        /// <summary>
        /// 手动设置异常处理后重新抛出的异常
        /// </summary>
        public Exception Throw
        {
            get => _throw;
            set
            {
                _throw = value;
                IsHandled = true;
                _out.Throw = value;
            }
        }
    }
}
