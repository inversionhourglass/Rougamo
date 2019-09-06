namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行成功（无异常）上下文
    /// </summary>
    public sealed class SuccessContext : MethodContext
    {
        private readonly ExitContext _exit;
        private object _returnValue;

        /// <summary>
        /// </summary>
        public SuccessContext(ExitContext exit) : base(exit.Target, exit.Method, exit.Args)
        {
            _exit = exit;
        }

        /// <summary>
        /// 方法返回值
        /// </summary>
        public object ReturnValue
        {
            get => _returnValue;
            // todo: change to private
            set
            {
                _returnValue = value;
                _exit.ReturnValue = value;
            }
        }
    }
}
